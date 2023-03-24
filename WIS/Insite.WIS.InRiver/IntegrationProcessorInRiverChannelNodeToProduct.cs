namespace Insite.WIS.InRiver;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.WIS.Broker;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.InRiver.Models;

[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.InRiver)]
public class IntegrationProcessorInRiverChannelNodeToProduct : IntegrationProcessorInRiver
{
    public IntegrationProcessorInRiverChannelNodeToProduct(
        IntegrationProcessorFileHelper fileHelper
    )
        : base(fileHelper) { }

    public override DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep integrationJobStep
    )
    {
        this.InRiverObjectName = "ChannelNode";
        return base.Execute(siteConnection, integrationJob, integrationJobStep);
    }

    public override IEnumerable<InRiverGenericObject> ParseXmlFile(
        string processingFileName,
        IntegrationConnection integrationConnection
    )
    {
        using (
            var fileStream = this.FileHelper.StreamFromFilePath(
                processingFileName,
                integrationConnection
            )
        )
        {
            using (
                var reader =
                    fileStream != null
                        ? new StreamReader(fileStream, true)
                        : new StreamReader(processingFileName, true)
            )
            {
                var document = XDocument.Load(reader);
                var rootElement = document.Root;
                var xmlNamespace = rootElement.GetDefaultNamespace();

                var inRiverObjects = new List<InRiverGenericObject>();
                var xmlNodes = rootElement.Elements();
                foreach (var node in xmlNodes)
                {
                    var inRiverObject = new InRiverGenericObject
                    {
                        UniqueIdentifier = IntegrationProcessorXmlHelper.GetUniqueIdentifier(
                            node,
                            xmlNamespace
                        ),
                        Action = IntegrationProcessorXmlHelper.GetAttributeValue("Action", node),
                        Fields = new Dictionary<string, IEnumerable<TranslationDictionary>>(),
                        Parents = new List<InRiverGenericObject>(),
                        Children = new List<InRiverGenericObject>(),
                    };

                    IntegrationProcessorXmlHelper.ProcessFields(
                        node,
                        xmlNamespace,
                        this.InRiverObjectName,
                        this.DefaultLanguage,
                        inRiverObject
                    );

                    var linksNode = node.Element(xmlNamespace + this.InRiverObjectName + "Links");
                    if (linksNode != null)
                    {
                        IntegrationProcessorXmlHelper.ProcessParentNode(
                            linksNode,
                            xmlNamespace,
                            this.InRiverObjectName,
                            inRiverObject
                        );
                        IntegrationProcessorXmlHelper.ProcessChildrenNodes(
                            linksNode,
                            xmlNamespace,
                            this.InRiverObjectName,
                            inRiverObject
                        );
                    }

                    inRiverObjects.Add(inRiverObject);
                }

                return inRiverObjects;
            }
        }
    }

    public override DataTable ProcessInRiverCollection(
        IEnumerable<InRiverGenericObject> inRiverCollection,
        DataTable dataTableSchema,
        IntegrationJob integrationJob,
        SiteConnection siteConnection,
        JobDefinitionStep integrationJobStep
    )
    {
        var integrationTable = dataTableSchema.Clone();
        this.AddRequiredColumnsToTable(integrationTable);

        foreach (var collectionObject in inRiverCollection)
        {
            this.LogHelper.LogProcessingInRiverObject(
                this.InRiverObjectName,
                collectionObject.UniqueIdentifier
            );

            var fields = dataTableSchema.Columns
                .Cast<DataColumn>()
                .Select(
                    column =>
                        this.GetTranslatedFieldValue(
                            collectionObject.Fields,
                            column.ColumnName,
                            this.DefaultLanguage
                        )
                )
                .ToList();

            var categoryName = this.GetCategoryName(collectionObject, inRiverCollection);

            if (!string.IsNullOrEmpty(categoryName))
            {
                fields.AddRange(
                    new List<string> { collectionObject.UniqueIdentifier, categoryName }
                );

                // Force the category creation if there are no products in the category OR ELSE it's an orphan category name
                var forceChildObjectAdd =
                    collectionObject.Children.Count == 0
                    || collectionObject.Children.Count(o => o.EntityType.Equals("Product")) == 0;

                if (forceChildObjectAdd)
                {
                    collectionObject.Children.Add(
                        new InRiverGenericObject
                        {
                            UniqueIdentifier = string.Empty,
                            EntityType = string.Empty
                        }
                    );
                }

                foreach (
                    var child in collectionObject.Children.Where(
                        o =>
                            o.EntityType.Equals("Product")
                            || (
                                forceChildObjectAdd
                                && o.UniqueIdentifier == string.Empty
                                && o.EntityType == string.Empty
                            )
                    )
                )
                {
                    var channelNodeFieldsWithProduct = fields.Select(o => o.Clone()).ToList();
                    channelNodeFieldsWithProduct.Add(child.UniqueIdentifier);
                    channelNodeFieldsWithProduct.Add(child.Action == "Deleted");
                    integrationTable.LoadDataRow(channelNodeFieldsWithProduct.ToArray(), true);
                }
            }
            else
            {
                this.LogHelper.LogObjectSkipped(
                    "Channel Node",
                    collectionObject.UniqueIdentifier,
                    collectionObject.Parents.FirstOrDefault()?.UniqueIdentifier
                );
            }
        }

        return integrationTable;
    }

    protected void AddRequiredColumnsToTable(DataTable integrationTable)
    {
        integrationTable.Columns.Add("UniqueIdentifier");
        integrationTable.Columns.Add("CategoryName");
        integrationTable.Columns.Add("ProductEntityId");
        integrationTable.Columns.Add("DeltaDeleted");
    }

    private string GetCategoryName(
        InRiverGenericObject channelNode,
        IEnumerable<InRiverGenericObject> channelNodes
    )
    {
        var categoryName = this.CategoryName(channelNode, this.DefaultLanguage);
        var node = channelNodes.SingleOrDefault(
            o => o.UniqueIdentifier.Equals(channelNode.Parents.FirstOrDefault()?.UniqueIdentifier)
        );
        if (node != null)
        {
            return this.GetCategoryName(node, channelNodes) + ":" + categoryName;
        }

        if (channelNode.Parents.FirstOrDefault()?.UniqueIdentifier == null)
        {
            return null;
        }

        var parentCategoryName = this.CategoryLookupTable.Rows
            .Cast<DataRow>()
            .Where(
                row =>
                    row["UniqueIdentifier"].Equals(
                        channelNode.Parents.FirstOrDefault()?.UniqueIdentifier
                    )
            )
            .Select(row => (string)row["CategoryName"])
            .FirstOrDefault();
        if (parentCategoryName != null)
        {
            return parentCategoryName + ":" + categoryName;
        }

        var websiteName = this.WebsiteLookupTable.Rows
            .Cast<DataRow>()
            .Where(
                row =>
                    row["UniqueIdentifier"].Equals(
                        channelNode.Parents.FirstOrDefault()?.UniqueIdentifier
                    )
            )
            .Select(row => (string)row["WebsiteName"])
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(websiteName))
        {
            return websiteName + ":" + categoryName;
        }

        return categoryName.IsNotBlank() && !categoryName.Contains(":") ? categoryName : null;
    }

    protected string CategoryName(InRiverGenericObject channelNode, string defaultLanguage)
    {
        if (
            !channelNode.Fields.Any(
                o => o.Key.Equals("ChannelNodeName", StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return string.Empty;
        }

        var translationDictionaries = channelNode.Fields
            .First(o => o.Key.Equals("ChannelNodeName", StringComparison.OrdinalIgnoreCase))
            .Value.ToList();
        var categoryNameDictionary = translationDictionaries.FirstOrDefault(
            o => o.LanguageCode.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase)
        );

        if (categoryNameDictionary == null && defaultLanguage.Contains("-"))
        {
            var alternateLanguageCode = defaultLanguage.Split('-').First();
            categoryNameDictionary = translationDictionaries.FirstOrDefault(
                o =>
                    o.LanguageCode.Equals(alternateLanguageCode, StringComparison.OrdinalIgnoreCase)
            );
        }

        return categoryNameDictionary != null
            ? categoryNameDictionary.TranslatedValue
            : string.Empty;
    }
}
