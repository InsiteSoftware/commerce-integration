﻿namespace Insite.WIS.InRiver;

using System.Collections.Generic;
using System.Data;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.WIS.Broker;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.InRiver.Models;

[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.InRiver)]
internal class IntegrationProcessorInRiverProduct : IntegrationProcessorInRiver
{
    public IntegrationProcessorInRiverProduct(IntegrationProcessorFileHelper fileHelper)
        : base(fileHelper) { }

    public override DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep integrationJobStep
    )
    {
        this.InRiverObjectName = "Product";
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
        var integrationTable = dataTableSchema.Clone();
        this.AddRequiredColumnsToTable(integrationTable);

        foreach (var collectionObject in inRiverCollection)
        {
            this.LogHelper.LogProcessingInRiverObject(
                this.InRiverObjectName,
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
                translatedAttributeValue = this.LookupCommonVocabularyKeyValue(
                    "Product",
                    column.ColumnName,
                    translatedAttributeValue
                );
                fields.Add(translatedAttributeValue);
            }

            fields.AddRange(
                new List<string>
                {
                    collectionObject.UniqueIdentifier,
                    collectionObject.Action.Equals("Delete") ? "false" : "true"
                }
            );
            integrationTable.LoadDataRow(fields.ToArray(), true);
        }

        return integrationTable;
    }

    protected void AddRequiredColumnsToTable(DataTable integrationTable)
    {
        integrationTable.Columns.Add("UniqueIdentifier");
        integrationTable.Columns.Add("Active");
    }
}
