namespace Insite.WIS.InRiver;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.WIS.Broker;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.InRiver.Models;

[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.InRiver)]
public class IntegrationProcessorInRiverResource : IntegrationProcessorInRiver
{
    public IntegrationProcessorInRiverResource(IntegrationProcessorFileHelper fileHelper)
        : base(fileHelper) { }

    public override DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep integrationJobStep
    )
    {
        this.InRiverObjectName = "Resource";
        return base.Execute(siteConnection, integrationJob, integrationJobStep);
    }

    public override DataTable ProcessInRiverCollection(
        IEnumerable<InRiverGenericObject> inRiverCollection,
        DataTable dataTableSchema,
        IntegrationJob integrationJob,
        SiteConnection siteConnection,
        JobDefinitionStep integrationJobStep
    )
    {
        // Get path where inRiver resources are stored on the IIS server
        var resourceImagePath = this.ResourceImagePath(integrationJob, integrationJobStep);
        var resourceTable = dataTableSchema.Clone();
        this.AddRequiredColumnsToTable(resourceTable);

        foreach (var collectionObject in inRiverCollection)
        {
            this.LogHelper.LogProcessingInRiverObject(
                this.InRiverObjectName,
                collectionObject.UniqueIdentifier
            );

            var resourceFields = new List<string>();
            foreach (DataColumn column in dataTableSchema.Columns)
            {
                var attributeValue = this.GetTranslatedFieldValue(
                    collectionObject.Fields,
                    column.ColumnName,
                    this.DefaultLanguage
                );
                var translatedAttributeValue = this.LookupCommonVocabularyKeyValue(
                    this.InRiverObjectName,
                    column.ColumnName,
                    attributeValue
                );

                resourceFields.Add(translatedAttributeValue);
            }

            var fullResourceName =
                this.GetTranslatedFieldValue(
                    collectionObject.Fields,
                    "ResourceFileId",
                    this.DefaultLanguage
                )
                + "_"
                + this.GetTranslatedFieldValue(
                    collectionObject.Fields,
                    "ResourceFilename",
                    this.DefaultLanguage
                );
            var fullResourceNameExtension = Path.GetExtension(fullResourceName);

            var smallImageExtension = this.GetStepParameter(
                integrationJob,
                integrationJobStep.Sequence,
                "SmallImageExtension",
                fullResourceNameExtension
            );
            var mediumImageExtension = this.GetStepParameter(
                integrationJob,
                integrationJobStep.Sequence,
                "MediumImageExtension",
                fullResourceNameExtension
            );
            var resourceType = this.GetStepParameter(
                integrationJob,
                integrationJobStep.Sequence,
                "ResourceType",
                "Product"
            );

            if (resourceType is not "Product" and not "Category")
            {
                throw new DataException(
                    "The step parameter 'ResourceType' is not valid.  Please specify 'Product' or 'Category'."
                );
            }

            foreach (var parent in collectionObject.Parents)
            {
                string parentIdentifier;
                if (resourceType.Equals("Product", StringComparison.InvariantCultureIgnoreCase))
                {
                    parentIdentifier = parent.UniqueIdentifier;
                }
                else
                {
                    parentIdentifier = this.CategoryLookupTable.Rows
                        .Cast<DataRow>()
                        .Where(row => row["UniqueIdentifier"].Equals(parent.UniqueIdentifier))
                        .Select(row => (string)row["CategoryName"])
                        .FirstOrDefault();
                }

                // For now we can only ignore records that are not associated to a product or category since that is the unique key for the fieldmapper.
                if (parentIdentifier != null)
                {
                    var clonedResourceFields = resourceFields.Select(o => o.Clone()).ToList();
                    clonedResourceFields.AddRange(
                        new List<string>
                        {
                            parentIdentifier,
                            resourceImagePath
                                + @"Thumbnail/"
                                + Path.GetFileNameWithoutExtension(fullResourceName)
                                + "."
                                + smallImageExtension,
                            resourceImagePath
                                + @"Preview/"
                                + Path.GetFileNameWithoutExtension(fullResourceName)
                                + "."
                                + mediumImageExtension,
                            resourceImagePath + @"Original/" + fullResourceName
                        }
                    );
                    resourceTable.LoadDataRow(clonedResourceFields.ToArray(), true);
                }
            }
        }

        return resourceTable;
    }

    protected virtual string GetStepParameter(
        IntegrationJob integrationJob,
        int stepNumber,
        string parameterName,
        string defaultValue
    )
    {
        if (integrationJob.JobDefinition == null)
        {
            throw new ArgumentNullException("integrationJob");
        }

        var parameter = integrationJob.IntegrationJobParameters.SingleOrDefault(
            o =>
                o.JobDefinitionStepParameter != null
                && o.JobDefinitionStepParameter.Name.Equals(
                    parameterName,
                    StringComparison.OrdinalIgnoreCase
                )
                && o.JobDefinitionStepParameter.JobDefinitionStep.Sequence == stepNumber
        );

        return parameter == null ? defaultValue : parameter.Value;
    }

    public void AddRequiredColumnsToTable(DataTable integrationTable)
    {
        integrationTable.Columns.Add("UniqueIdentifier");
        integrationTable.Columns.Add("SmallImagePath");
        integrationTable.Columns.Add("MediumImagePath");
        integrationTable.Columns.Add("LargeImagePath");
    }

    protected internal string ResourceImagePath(
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep
    )
    {
        var integrationJobDefinitionStepParameter =
            integrationJob.IntegrationJobParameters.SingleOrDefault(
                o =>
                    o.JobDefinitionStepParameter != null
                    && o.JobDefinitionStepParameter.Name.Equals(
                        "inRiverResourceLocation",
                        StringComparison.OrdinalIgnoreCase
                    )
                    && o.JobDefinitionStepParameter.JobDefinitionStepId == jobStep.Id
            );

        if (integrationJobDefinitionStepParameter == null)
        {
            throw new ArgumentException(
                "inRiverResourceLocation parameter is not defined in the jobstep parameters. Please specify the location of the resource files in this parameter."
            );
        }

        var resourceImagePath = integrationJobDefinitionStepParameter.Value;
        if (!resourceImagePath.EndsWith(@"/"))
        {
            resourceImagePath += @"/";
        }

        return resourceImagePath;
    }
}
