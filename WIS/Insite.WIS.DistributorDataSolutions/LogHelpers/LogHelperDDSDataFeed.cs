namespace Insite.WIS.DistributorDataSolutions.LogHelpers;

using System.Collections.Generic;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.WebIntegrationService;

public class LogHelperDDSDataFeed
{
    private readonly IntegrationJobLogger jobLogger;

    public LogHelperDDSDataFeed(SiteConnection siteConnection, IntegrationJob integrationJob)
    {
        this.jobLogger = new IntegrationJobLogger(siteConnection, integrationJob);
    }

    public void LogDebug(string message)
    {
        this.jobLogger.Debug(message);
    }

    public void LogInfo(string message)
    {
        this.jobLogger.Info(message);
    }

    public void LogError(string message)
    {
        this.jobLogger.Error(message);
    }

    public void LogWarn(string message)
    {
        this.jobLogger.Warn(message);
    }

    public void LogMinifyDataSetStart()
    {
        this.LogDebug($"Starting minifying dataSet");
    }

    public void LogMinifyDataSetFinish()
    {
        this.LogDebug($"Finished minifying dataSet");
    }

    public void LogMinifyDataSetRemovingColumns(
        string dataTableName,
        IEnumerable<string> columnNames
    )
    {
        this.LogDebug(
            $"Removing columns from dataTable {dataTableName}: {string.Join(",", columnNames)}"
        );
    }

    public void LogPopulateCategoryDataTableStart()
    {
        this.LogDebug($"Starting populating category dataTable");
    }

    public void LogPopulateCategoryDataTableFinish()
    {
        this.LogDebug($"Finished populating category dataTable");
    }

    public void LogSkippingPopulateCategoryDataTable()
    {
        this.LogDebug(
            $"Job definition parameter Websites either doesn't exist or isn't populated. "
                + $"Skipping populating category dataTable"
        );
    }

    public void LogPopulateCategoryProductDataTableStart()
    {
        this.LogDebug($"Starting populating category product dataTable");
    }

    public void LogPopulateCategoryProductDataTableFinish()
    {
        this.LogDebug($"Finished populating category product dataTable");
    }

    public void LogSkippingPopulateCategoryProductDataTable()
    {
        this.LogDebug(
            $"Job definition parameter Websites either doesn't exist or isn't populated. "
                + $"Skipping populating category product dataTable"
        );
    }

    public void LogPopulateAttributeTypeDataTableStart()
    {
        this.LogDebug($"Starting populating attribute type dataTable");
    }

    public void LogPopulateAttributeTypeDataTableFinish()
    {
        this.LogDebug($"Finished populating attribute type dataTable");
    }

    public void LogPopulateAttributeValueDataTableStart()
    {
        this.LogDebug($"Starting populating attribute value dataTable");
    }

    public void LogPopulateAttributeValueDataTableFinish()
    {
        this.LogDebug($"Finished populating attribute value dataTable");
    }

    public void LogPopulateProductImageDataTableStart()
    {
        this.LogDebug($"Starting populating product image dataTable");
    }

    public void LogPopulateProductImageDataTableFinish()
    {
        this.LogDebug($"Finished populating product image dataTable");
    }
}
