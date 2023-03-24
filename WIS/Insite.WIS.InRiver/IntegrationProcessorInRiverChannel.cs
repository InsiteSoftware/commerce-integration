namespace Insite.WIS.InRiver;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Insite.Common.Helpers;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.WIS.InRiver.Models;
using Insite.WIS.Broker;
using Insite.WIS.Broker.WebIntegrationService;

[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.InRiver)]
public class IntegrationProcessorInRiverChannel : IntegrationProcessorInRiver
{
    public IntegrationProcessorInRiverChannel(IntegrationProcessorFileHelper fileHelper)
        : base(fileHelper) { }

    public override DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep integrationJobStep
    )
    {
        this.InRiverObjectName = "Channel";
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
            this.LogHelper.LogObjectProcessed(
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
                fields.Add(translatedAttributeValue);
            }

            fields.AddRange(new List<string> { collectionObject.UniqueIdentifier });

            integrationTable.LoadDataRow(fields.ToArray(), true);

            var website = this.WebsiteLookupTable
                .AsEnumerable()
                .SingleOrDefault(
                    o =>
                        o.Field<string>("UniqueIdentifier")
                            .Equals(
                                collectionObject.UniqueIdentifier,
                                StringComparison.OrdinalIgnoreCase
                            )
                );
            if (website == null)
            {
                var initialDataset = XmlDatasetManager.ConvertXmlToDataset(
                    integrationJob.InitialData
                );
                var channelName = this.GetTranslatedFieldValue(
                    collectionObject.Fields,
                    "ChannelName",
                    this.DefaultLanguage
                );
                this.WebsiteLookupTable.Rows.Add(collectionObject.UniqueIdentifier, channelName);
                initialDataset.Tables.Remove("WebsiteLookup");
                var websiteLookupTable = this.WebsiteLookupTable.Clone();
                initialDataset.Tables.Add(websiteLookupTable);
            }
        }

        return integrationTable;
    }

    protected void AddRequiredColumnsToTable(DataTable integrationTable)
    {
        integrationTable.Columns.Add("UniqueIdentifier");
    }
}
