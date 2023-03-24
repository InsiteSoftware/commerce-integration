namespace Insite.WIS.AffiliatedDistributors.LogHelpers;

using System.Collections.Generic;
using Insite.WIS.AffiliatedDistributors.Constants;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.WebIntegrationService;

public class LogHelperADDataFeed
{
    private readonly IntegrationJobLogger jobLogger;

    public LogHelperADDataFeed(SiteConnection siteConnection, IntegrationJob integrationJob)
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
        this.LogDebug("Starting minifying dataSet");
    }

    public void LogMinifyDataSetFinish()
    {
        this.LogDebug("Finished minifying dataSet");
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

    public void LogPopulateStyleTraitDataTableStart()
    {
        this.LogDebug("Starting populating style trait dataTable");
    }

    public void LogPopulateStyleTraitDataTableFinish()
    {
        this.LogDebug("Finished populating style trait dataTable");
    }

    public void LogPopulateStyleClassDataTableStart()
    {
        this.LogDebug("Starting populating style class dataTable");
    }

    public void LogPopulateStyleClassDataTableFinish()
    {
        this.LogDebug("Finished populating style class dataTable");
    }

    public void LogPopulateAttributeTypeDataTableStart()
    {
        this.LogDebug("Starting populating attribute type dataTable");
    }

    public void LogPopulateStyleTraitValueDataTableStart()
    {
        this.LogDebug("Starting populating style trait value dataTable");
    }

    public void LogPopulateStyleTraitValueDataTableFinish()
    {
        this.LogDebug("Finished populating style trait value dataTable");
    }

    public void LogPopulateStyleTraitValueProductDataTableStart()
    {
        this.LogDebug("Starting populating style trait product value dataTable");
    }

    public void LogPopulateStyleTraitValueProductDataTableFinish()
    {
        this.LogDebug("Finished populating style trait product value dataTable");
    }

    public void LogPopulateAttributeTypeDataTableFinish()
    {
        this.LogDebug("Finished populating attribute type dataTable");
    }

    public void LogAddNewProductRecordsTableStart()
    {
        this.LogDebug("Starting addition of new product records");
    }

    public void LogAddNewProductRecordsTableFinish()
    {
        this.LogDebug("Finished addition of new product records");
    }

    public void LogAddLanguageCodeProductRecordsTableStart()
    {
        this.LogDebug("Starting addition of Language Code records");
    }

    public void LogAddLanguageCodeProductRecordsTableFinish()
    {
        this.LogDebug("Finished addition of Language Code records");
    }

    public void LogPopulateAttributeValueDataTableStart()
    {
        this.LogDebug("Starting populating attribute value dataTable");
    }

    public void LogPopulateAttributeValueDataTableFinish()
    {
        this.LogDebug("Finished populating attribute value dataTable");
    }

    public void LogPopulateProductSpecificationDataTableStart()
    {
        this.LogDebug("Starting populating product specification dataTable");
    }

    public void LogPopulateProductSpecificationDataTableFinish()
    {
        this.LogDebug("Finished populating product specification dataTable");
    }

    public void LogPopulateProductImageDataTableStart()
    {
        this.LogDebug("Starting populating product image dataTable");
    }

    public void LogPopulateProductImageDataTableFinish()
    {
        this.LogDebug("Finished populating product image dataTable");
    }

    public void LogPopulateDocumentDataTableStart()
    {
        this.LogDebug("Starting populating document dataTable");
    }

    public void LogPopulateDocumentDataTableFinish()
    {
        this.LogDebug("Finished populating document dataTable");
    }

    public void LogPopulateProductThreeSixtyImageDataTableStart()
    {
        this.LogDebug("Starting populating product 360 image dataTable");
    }

    public void LogPopulateProductThreeSixtyImageDataTableFinish()
    {
        this.LogDebug("Finished populating product 360 image dataTable");
    }

    public void LogSkippingPopulateProductThreeSixtyImageDataTable()
    {
        this.LogDebug(
            $"Column {ADDataFeedSourceFile.ThreeSixtyImageColumn} not found in data source. "
                + "Skipping populating product 360 image dataTable"
        );
    }

    public void LogPopulateCategoryDataTableStart()
    {
        this.LogDebug("Starting populating category dataTable");
    }

    public void LogPopulateCategoryDataTableFinish()
    {
        this.LogDebug("Finished populating category dataTable");
    }

    public void LogPopulateCategoryProductDataTableStart()
    {
        this.LogDebug("Starting populating category product dataTable");
    }

    public void LogPopulateCategoryProductDataTableFinish()
    {
        this.LogDebug("Finished populating category product dataTable");
    }

    public void LogPopulateCategoryImageDataTableStart()
    {
        this.LogDebug("Starting populating category image dataTable");
    }

    public void LogPopulateCategoryImageDataTableFinish()
    {
        this.LogDebug("Finished populating category image dataTable");
    }

    public void LogSkippingPopulateCategoryImageDataTables()
    {
        this.LogDebug(
            $"Column {ADDataFeedSourceFile.CategoryImageName} not found in data source."
                + "Skipping populating category image dataTables"
        );
    }

    public void LogPopulateAttributeTypeTranslationDataTableStart()
    {
        this.LogDebug("Starting populating attribute type translation dataTable");
    }

    public void LogPopulateAttributeTypeTranslationDataTableFinish()
    {
        this.LogDebug("Finished populating attribute type translation dataTable");
    }

    public void LogSkippingPopulateAttributeTypeTranslationDataTables()
    {
        this.LogDebug(
            "The amount attribute types from the base file and translation do not match. "
                + "Skipping populating attribute type translation dataTables"
        );
    }

    public void LogPopulateAttributeValueTranslationDataTableStart()
    {
        this.LogDebug("Starting populating attribute value translation dataTable");
    }

    public void LogPopulateAttributeValueTranslationDataTableFinish()
    {
        this.LogDebug("Finished populating attribute value translation dataTable");
    }

    public void LogSkippingPopulateAttributeValueTranslationDataTables()
    {
        this.LogDebug(
            "The amount attribute values from the base file and translation do not match. "
                + "Skipping populating attribute value translation dataTables"
        );
    }

    public void LogPopulateCategoryAttributeTypeDataTableStart()
    {
        this.LogDebug("Starting populating category attribute type dataTable");
    }

    public void LogPopulateCategoryAttributeTypeDataTableFinish()
    {
        this.LogDebug("Finished populating category attribute type dataTable");
    }

    public void LogSkippingPopulateCategoryDataTables()
    {
        this.LogDebug(
            "Job definition parameter Websites either doesn't exist or isn't populated. "
                + "Skipping populating category dataTables"
        );
    }

    internal void LogPopulatingBrandDataSetStart()
    {
        this.LogDebug("Being populating Brand dataset");
    }

    internal void LogPopulatingBrandDataSetFinish()
    {
        this.LogDebug("Finished populating the Brand dataset");
    }

    internal void LogPopulatePriceMatricies()
    {
        this.LogDebug("Begin creating and populating price matrices.");
    }

    internal void LogCompletePopulatingPriceMatricies()
    {
        this.LogDebug("Finished populating and creating price matrices");
    }
}
