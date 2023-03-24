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
using TranslationDictionary = Insite.WIS.InRiver.Models.TranslationDictionary;

[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.InRiver)]
internal class IntegrationProcessorInRiverCommonVocabulary : IntegrationProcessorInRiver
{
    public IntegrationProcessorInRiverCommonVocabulary(IntegrationProcessorFileHelper fileHelper)
        : base(fileHelper) { }

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
                        UniqueIdentifier = string.Empty,
                        Fields = new Dictionary<string, IEnumerable<TranslationDictionary>>()
                    };

                    var fields = node.Elements();
                    foreach (var field in fields)
                    {
                        var dataElement = field.Element(xmlNamespace + "string");
                        if (dataElement != null)
                        {
                            inRiverObject.Fields.Add(
                                field.Name.LocalName,
                                this.GetTranslatedFieldValue(dataElement, xmlNamespace)
                            );
                        }
                        else
                        {
                            if (
                                field.Name.LocalName.Equals(
                                    "Id",
                                    StringComparison.InvariantCultureIgnoreCase
                                )
                            )
                            {
                                inRiverObject.UniqueIdentifier = field.Value;
                            }

                            inRiverObject.Fields.Add(
                                field.Name.LocalName,
                                this.GetTranslatedFieldValue(field, xmlNamespace)
                            );
                        }
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

        foreach (var collectionObject in inRiverCollection)
        {
            this.LogHelper.LogObjectProcessed(
                "Common Vocabulary",
                collectionObject.UniqueIdentifier
            );

            var fields = new List<string>();
            foreach (DataColumn column in dataTableSchema.Columns)
            {
                var translatedAttributeValue = this.GetTranslatedFieldValue(
                    collectionObject.Fields,
                    column.ColumnName,
                    this.DefaultLanguage
                );
                fields.Add(translatedAttributeValue);
            }

            integrationTable.LoadDataRow(fields.ToArray(), true);
        }

        return integrationTable;
    }

    protected internal IEnumerable<TranslationDictionary> GetTranslatedFieldValue(
        XElement dataElement,
        XNamespace xmlNamespace
    )
    {
        var translationDictionary = new List<TranslationDictionary>();

        var localeNodes = dataElement.Elements();
        if (localeNodes.Any())
        {
            foreach (var localeNode in localeNodes)
            {
                var translationEntry = new TranslationDictionary();
                var languageElement = localeNode.Attribute("language");
                if (languageElement != null)
                {
                    translationEntry.LanguageCode = languageElement.Value.ToUpper();
                    translationEntry.TranslatedValue = localeNode.Value;
                }

                translationDictionary.Add(translationEntry);
            }
        }
        else
        {
            var translationEntry = new TranslationDictionary
            {
                LanguageCode = this.DefaultLanguage,
                TranslatedValue = dataElement.Value
            };

            translationDictionary.Add(translationEntry);
        }

        return translationDictionary;
    }
}
