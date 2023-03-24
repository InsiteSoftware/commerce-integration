namespace Insite.WIS.AffiliatedDistributors;

using Insite.Common.Helpers;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.WIS.AffiliatedDistributors.Constants;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Plugins;
using Schema.NET;
using System;
using System.Data;
using System.Web.WebPages;
using Data = Insite.WIS.Broker.Plugins.Constants.Data;

[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.AffiliatedDistributors)]
internal class IntegrationProcessorADPricingRefresh : IntegrationProcessorPricingRefreshBase
{
    internal IntegrationJobLogger JobLogger;
    internal IntegrationProcessorFlatFile FlatFileProcessor;

    public IntegrationProcessorADPricingRefresh() { }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    internal IntegrationProcessorADPricingRefresh(
        IntegrationProcessorFlatFile? flatFileProcessor = null
    )
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        this.FlatFileProcessor = flatFileProcessor;
    }

    public override DataSet Execute(
        SiteConnection siteConnection,
        Broker.WebIntegrationService.IntegrationJob integrationJob,
        Broker.WebIntegrationService.JobDefinitionStep jobStep
    )
    {
        this.JobLogger = new IntegrationJobLogger(siteConnection, integrationJob);

        this.JobLogger.Info("Processing price matrix data");

        this.FlatFileProcessor ??= new IntegrationProcessorFlatFile();

        var dataSet = this.FlatFileProcessor.Execute(siteConnection, integrationJob, jobStep);

        // Check and convert column names to a predetermined name to make processing easier
        dataSet = Helpers.ADHelper.ConvertColumnNames(dataSet);
        var lookupDataSet = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        var finishedDataSet = new DataSet();
        finishedDataSet.Tables.Add(
            this.PopulateCADPriceMatrix(dataSet, lookupDataSet, jobStep.Sequence)
        );
        return finishedDataSet;
    }

    private DataTable PopulateCADPriceMatrix(DataSet source, DataSet lookup, int sequence)
    {
        var priceMatrixDataTable = this.BuildPriceMatrixDataTable(sequence);
        if (source.Tables[0].Columns.Contains(ADDataFeedSourceFile.CAPriceColumn))
        {
            var rowNumber = 0;
            foreach (DataRow row in source.Tables[0].Rows)
            {
                var productId = this.GetProductId(
                    row[ADDataFeedSourceFile.MyPartNumberColumn].ToString(),
                    lookup
                );

                if (!productId.IsBlank())
                {
                    var dataRow = priceMatrixDataTable.NewRow();
                    dataRow[Data.ProductKeyPartColumn] = productId;
                    dataRow[Data.Amount01Column] = row[
                        ADDataFeedSourceFile.CAPriceColumn
                    ].ToString();
                    dataRow[Data.UnitOfMeasureColumn] = row[
                        ADDataFeedSourceFile.SalesUnitOfMeasureColumn
                    ].ToString();

                    try
                    {
                        dataRow[Data.ActivateOnColumn] = DateTime.Parse(
                            row[ADDataFeedSourceFile.CAPriceActivationColumn].ToString()
                        );
                    }
                    catch (Exception)
                    {
                        this.JobLogger.Error(
                            $"An error occurred when parsing the price matrix start date. This value is required. Row {rowNumber} was skipped."
                        );
                        rowNumber++;
                        continue;
                    }

                    try
                    {
                        dataRow[Data.DeactivateOnColumn] = DateTime.Parse(
                            row[ADDataFeedSourceFile.CAPriceDeactivationColumn].ToString()
                        );
                    }
                    catch (Exception)
                    {
                        this.JobLogger.Warn(
                            "An error occurred when parsing the price matrix end date. One has not been set."
                        );
                        rowNumber++;
                        continue;
                    }

                    priceMatrixDataTable.Rows.Add(dataRow);
                }
                else
                {
                    this.JobLogger.Error(
                        $"Product {row[ADDataFeedSourceFile.MyPartNumberColumn]} in row {rowNumber} not found within the lookup table."
                    );
                }

                rowNumber++;
            }
        }

        return priceMatrixDataTable;
    }
}
