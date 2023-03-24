namespace Insite.WIS.DistributorDataSolutions;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.DistributorDataSolutions.LogHelpers;
using Insite.WIS.DistributorDataSolutions.Provider;
using Insite.WIS.DistributorDataSolutions.Resources;
using Insite.WIS.DistributorDataSolutions.Services;

[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.DistributorDataSolutions)]
public class IntegrationProcessorDDSDataFeed : IntegrationProcessorFlatFile
{
    private const string ProductTable = "1Product";

    private const string CategoryTable = "2Category";

#pragma warning disable SA1306
    protected LogHelperDDSDataFeed LogHelperDDSDataFeed;
#pragma warning restore SA1306

    public override DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobDefinitionStep
    )
    {
        this.LogHelperDDSDataFeed = new LogHelperDDSDataFeed(siteConnection, integrationJob);

        if (jobDefinitionStep.Sequence is not 1 and not 2)
        {
            return new DataSet();
        }

        var dataSet = base.Execute(siteConnection, integrationJob, jobDefinitionStep);
        if (dataSet.Tables.Count == 0)
        {
            return dataSet;
        }

        if (jobDefinitionStep.Sequence == 1)
        {
            this.PopulateCategoryProductDataTable(integrationJob, dataSet);
            this.PopulateAttributeTypeDataTable(dataSet);
            this.PopulateAttributeValueDataTable(dataSet);
            this.PopulateProductImageDataTable(dataSet);

            this.MinifyProductDataSet(dataSet);
        }
        else
        {
            this.PopulateCategoryDataTable(integrationJob, dataSet);
        }

        return dataSet;
    }

    protected void MinifyProductDataSet(DataSet dataSet)
    {
        this.LogHelperDDSDataFeed.LogMinifyDataSetStart();

        var productDataTable = dataSet.Tables[ProductTable];

        var columnNamesToRemove = new List<string>
        {
            DDSDataFeedProductSourceFile.CategoryColumnPrefix,
            DDSDataFeedProductSourceFile.FacetColumnPrefix,
            DDSDataFeedProductSourceFile.ImageColumnPrefix,
            DDSDataFeedProductSourceFile.SectionColumnPrefix
        };

        var columnsToRemove = productDataTable.Columns
            .Cast<DataColumn>()
            .Where(
                o =>
                    columnNamesToRemove.Any(
                        p => o.ColumnName.StartsWith(p, StringComparison.OrdinalIgnoreCase)
                    )
            )
            .ToArray();

        this.LogHelperDDSDataFeed.LogMinifyDataSetRemovingColumns(
            productDataTable.TableName,
            columnsToRemove.Select(o => o.ColumnName)
        );

        foreach (var columnToRemove in columnsToRemove)
        {
            productDataTable.Columns.Remove(columnToRemove);
        }

        this.LogHelperDDSDataFeed.LogMinifyDataSetFinish();
    }

    protected void PopulateCategoryDataTable(IntegrationJob integrationJob, DataSet dataSet)
    {
        this.LogHelperDDSDataFeed.LogPopulateCategoryDataTableStart();

        var websiteName = GetWebsiteName(integrationJob);
        if (string.IsNullOrEmpty(websiteName))
        {
            this.LogHelperDDSDataFeed.LogSkippingPopulateCategoryDataTable();
            return;
        }

        var sourceCategoryDataTable = dataSet.Tables[CategoryTable];
        var destinationCategoryDataTable = DDSDataFeedDataTableProvider.CreateCategoryDataTable();

        foreach (DataRow dataRow in sourceCategoryDataTable.Rows)
        {
            var categoryCode = dataRow[
                DDSDataFeedCategorySourceFile.MainCategoryCodeColumn
            ].ToString();
            var categoryName = dataRow[
                DDSDataFeedCategorySourceFile.MainCategoryNameColumn
            ].ToString();
            var categoryCodes = new HashSet<string> { categoryCode };
            var categoryIndex = 1;

            AddCategoryToCategoryDataTable(
                destinationCategoryDataTable,
                websiteName,
                categoryCode,
                categoryName,
                categoryCodes
            );

            while (true)
            {
                categoryCode = ParseColumnValue(
                    dataRow,
                    string.Format(
                        DDSDataFeedCategorySourceFile.SubCategoryCodeColumnFormat,
                        categoryIndex
                    )
                );
                if (string.IsNullOrEmpty(categoryCode))
                {
                    break;
                }

                categoryName = ParseColumnValue(
                    dataRow,
                    string.Format(
                        DDSDataFeedCategorySourceFile.SubCategoryNameColumnFormat,
                        categoryIndex
                    )
                );
                categoryCodes.Add(categoryCode);
                categoryIndex++;

                AddCategoryToCategoryDataTable(
                    destinationCategoryDataTable,
                    websiteName,
                    categoryCode,
                    categoryName,
                    categoryCodes
                );
            }
        }

        dataSet.Tables.Remove(sourceCategoryDataTable);
        dataSet.Tables.Add(destinationCategoryDataTable);

        this.LogHelperDDSDataFeed.LogPopulateCategoryProductDataTableFinish();
    }

    private static void AddCategoryToCategoryDataTable(
        DataTable categoryDataTable,
        string websiteName,
        string categoryCode,
        string categoryName,
        HashSet<string> categoryCodes
    )
    {
        var websiteCategoryName = $"{websiteName}:{string.Join(":", categoryCodes)}";
        if (CategoryExistsInCategoryDataTable(categoryDataTable, websiteCategoryName))
        {
            return;
        }

        categoryDataTable.Rows.Add(websiteCategoryName, categoryCode, categoryName);
    }

    private static bool CategoryExistsInCategoryDataTable(
        DataTable categoryDataTable,
        string websiteCategoryName
    )
    {
        return categoryDataTable.Rows.Find(websiteCategoryName) != null;
    }

    protected void PopulateCategoryProductDataTable(IntegrationJob integrationJob, DataSet dataSet)
    {
        this.LogHelperDDSDataFeed.LogPopulateCategoryProductDataTableStart();

        var websiteName = GetWebsiteName(integrationJob);
        if (string.IsNullOrEmpty(websiteName))
        {
            this.LogHelperDDSDataFeed.LogSkippingPopulateCategoryProductDataTable();
            return;
        }

        var productDataTable = dataSet.Tables[ProductTable];
        var categoryProductDataTable =
            DDSDataFeedDataTableProvider.CreateCategoryProductDataTable();

        foreach (DataRow dataRow in productDataTable.Rows)
        {
            var distributorProductId = dataRow[
                DDSDataFeedProductSourceFile.DistributorProductIdColumn
            ].ToString();
            var categoryIndex = 1;

            while (true)
            {
                var categoryCodes = new HashSet<string>();
                var subCategoryIndex = 1;

                while (true)
                {
                    var categoryCode = ParseColumnValue(
                        dataRow,
                        string.Format(
                            DDSDataFeedProductSourceFile.CategoryCodeColumnFormat,
                            categoryIndex,
                            subCategoryIndex
                        )
                    );
                    if (string.IsNullOrEmpty(categoryCode))
                    {
                        break;
                    }

                    categoryCodes.Add(categoryCode);
                    subCategoryIndex++;
                }

                if (!categoryCodes.Any())
                {
                    break;
                }

                categoryProductDataTable.Rows.Add(
                    $"{websiteName}:{string.Join(":", categoryCodes)}",
                    distributorProductId
                );
                categoryIndex++;
            }
        }

        dataSet.Tables.Add(categoryProductDataTable);

        this.LogHelperDDSDataFeed.LogPopulateCategoryProductDataTableFinish();
    }

    protected void PopulateAttributeTypeDataTable(DataSet dataSet)
    {
        this.LogHelperDDSDataFeed.LogPopulateAttributeTypeDataTableStart();

        var productDataTable = dataSet.Tables[ProductTable];
        var attributeTypeDataTable = DDSDataFeedDataTableProvider.CreateAttributeTypeDataTable();
        var attributeTypes = new HashSet<string>();

        foreach (DataRow dataRow in productDataTable.Rows)
        {
            var facetIndex = 1;

            while (true)
            {
                var facetName = ParseColumnValue(
                    dataRow,
                    string.Format(DDSDataFeedProductSourceFile.FacetNameColumnFormat, facetIndex)
                );
                if (string.IsNullOrEmpty(facetName))
                {
                    break;
                }

                if (!attributeTypes.Contains(facetName))
                {
                    attributeTypes.Add(facetName);
                }

                facetIndex++;
            }
        }

        foreach (var attributeType in attributeTypes)
        {
            attributeTypeDataTable.Rows.Add(attributeType);
        }

        dataSet.Tables.Add(attributeTypeDataTable);

        this.LogHelperDDSDataFeed.LogPopulateAttributeTypeDataTableFinish();
    }

    protected void PopulateAttributeValueDataTable(DataSet dataSet)
    {
        this.LogHelperDDSDataFeed.LogPopulateAttributeValueDataTableStart();

        var productDataTable = dataSet.Tables[ProductTable];
        var attributeValueDataTable = DDSDataFeedDataTableProvider.CreateAttributeValueDataTable();

        foreach (DataRow dataRow in productDataTable.Rows)
        {
            var distributorProductId = dataRow[
                DDSDataFeedProductSourceFile.DistributorProductIdColumn
            ].ToString();
            var facetIndex = 1;

            while (true)
            {
                var facetName = ParseColumnValue(
                    dataRow,
                    string.Format(DDSDataFeedProductSourceFile.FacetNameColumnFormat, facetIndex)
                );
                if (string.IsNullOrEmpty(facetName))
                {
                    break;
                }

                var facetValueIndex = 1;

                while (true)
                {
                    var facetValue = ParseColumnValue(
                        dataRow,
                        string.Format(
                            DDSDataFeedProductSourceFile.FacetValueColumnFormat,
                            facetIndex,
                            facetValueIndex
                        )
                    );
                    if (string.IsNullOrEmpty(facetValue))
                    {
                        break;
                    }

                    attributeValueDataTable.Rows.Add(
                        distributorProductId,
                        facetName,
                        facetValue,
                        ++facetValueIndex
                    );
                    facetValueIndex++;
                }

                facetIndex++;
            }
        }

        dataSet.Tables.Add(attributeValueDataTable);

        this.LogHelperDDSDataFeed.LogPopulateAttributeValueDataTableFinish();
    }

    protected void PopulateProductImageDataTable(DataSet dataSet)
    {
        this.LogHelperDDSDataFeed.LogPopulateProductImageDataTableStart();

        var productDataTable = dataSet.Tables[ProductTable];
        var productImagesDataTable = DDSDataFeedDataTableProvider.CreateProductImageDataTable();

        foreach (DataRow dataRow in productDataTable.Rows)
        {
            var distributorProductId = dataRow[
                DDSDataFeedProductSourceFile.DistributorProductIdColumn
            ].ToString();
            var imageIndex = 1;

            while (true)
            {
                var name = ParseColumnValue(
                    dataRow,
                    string.Format(DDSDataFeedProductSourceFile.ImageNameColumnFormat, imageIndex)
                );
                if (string.IsNullOrEmpty(name))
                {
                    break;
                }

                var uriThumb = ParseColumnValue(
                    dataRow,
                    string.Format(
                        DDSDataFeedProductSourceFile.ImageUriThumbColumnFormat,
                        imageIndex
                    )
                );
                var uriSmall = ParseColumnValue(
                    dataRow,
                    string.Format(
                        DDSDataFeedProductSourceFile.ImageUriSmallColumnFormat,
                        imageIndex
                    )
                );
                var uriMedium = ParseColumnValue(
                    dataRow,
                    string.Format(
                        DDSDataFeedProductSourceFile.ImageUriMediumColumnFormat,
                        imageIndex
                    )
                );
                var uriLarge = ParseColumnValue(
                    dataRow,
                    string.Format(
                        DDSDataFeedProductSourceFile.ImageUriLargeColumnFormat,
                        imageIndex
                    )
                );

                productImagesDataTable.Rows.Add(
                    distributorProductId,
                    uriThumb,
                    uriSmall,
                    uriMedium,
                    uriLarge,
                    name,
                    ++imageIndex
                );
                imageIndex++;
            }
        }

        dataSet.Tables.Add(productImagesDataTable);

        this.LogHelperDDSDataFeed.LogPopulateProductImageDataTableFinish();
    }

    protected override IFlatFileService GetFlatFileService(
        IntegrationJob integrationJob,
        JobDefinitionStep jobDefinitionStep
    )
    {
        return new FlatFileServiceDDS(integrationJob, jobDefinitionStep);
    }

    private static string GetWebsiteName(IntegrationJob integrationJob)
    {
        var websiteParameter = integrationJob.IntegrationJobParameters.FirstOrDefault(
            o => o.JobDefinitionParameter.Name.Equals("Website", StringComparison.OrdinalIgnoreCase)
        );

        return websiteParameter != null
            ? !string.IsNullOrEmpty(websiteParameter.Value)
                ? websiteParameter.Value
                : websiteParameter.JobDefinitionParameter.DefaultValue
            : string.Empty;
    }

    private static string ParseColumnValue(DataRow dataRow, string columnName)
    {
        return dataRow.Table.Columns.Contains(columnName) ? dataRow[columnName].ToString() : null;
    }
}
