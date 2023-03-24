namespace Insite.WIS.AffiliatedDistributors;

using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.WIS.AffiliatedDistributors.LogHelpers;
using Insite.WIS.AffiliatedDistributors.Providers;
using Insite.WIS.AffiliatedDistributors.Constants;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.AffiliatedDistributors.Helpers;

[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.AffiliatedDistributors)]
public class IntegrationProcessorADDataFeed : IntegrationProcessorFlatFile
{
    private const string HttpPrefix = "http:";
    private const string HttpsPrefix = "https:";
    private const string JpgSuffix = ".jpg";

    private bool usePathMappingForImagesAndDocuments;
    private string imageAndDocumentSourceFolder;
    private string smallImageFolder;
    private string mediumImageFolder;
    private string largeImageFolder;
    private string documentFolder;
    private DataSet styleDataSet;

#pragma warning disable SA1306
    protected LogHelperADDataFeed LogHelperADDataFeed;
#pragma warning restore SA1306

    public override DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobDefinitionStep
    )
    {
        this.LogHelperADDataFeed = new LogHelperADDataFeed(siteConnection, integrationJob);
        var objectFileNameLowered = jobDefinitionStep.ObjectName.ToLowerInvariant();
        var stepName = jobDefinitionStep.Name.ToLowerInvariant();
        var baseLanguageFileName = GetBaseLanguageFileName(integrationJob);
        var isCategoryImageRefresh = IsCategoryImageRefresh(objectFileNameLowered, stepName);
        var isAttributeTranslation = !baseLanguageFileName.IsBlank();

        if (
            !objectFileNameLowered.Equals("styleclass")
            && !objectFileNameLowered.Equals("product")
            && !isCategoryImageRefresh
        )
        {
            return new DataSet();
        }

        var dataSet = new DataSet();

        if (isAttributeTranslation)
        {
            dataSet = this.ExecuteTranslation(siteConnection, integrationJob, jobDefinitionStep);
        }
        else
        {
            dataSet = base.Execute(siteConnection, integrationJob, jobDefinitionStep);
        }

        if (dataSet.Tables.Count == 0)
        {
            return dataSet;
        }

        var parameters = integrationJob.IntegrationJobParameters;
        string GetStepParameterValue(string parameterName)
        {
            var parameter = parameters.FirstOrDefault(
                p =>
                    p.JobDefinitionStepParameter != null
                    && p.JobDefinitionStepParameter.Name.EqualsIgnoreCase(parameterName)
            );
            if (parameter == null)
            {
                return null;
            }

            return parameter.Value;
        }

        var isEnterworks =
            integrationJob.IntegrationJobParameters
                .FirstOrDefault(
                    o =>
                        o.JobDefinitionStepParameter != null
                        && o.JobDefinitionStepParameter.Name.Equals(ADDataFeedSourceFile.Images)
                )
                ?.Value == "Enterworks";
        this.CleanDataSet(dataSet);
        dataSet = ADHelper.ConvertColumnNames(dataSet);

        var excludedAttributes = GetExcludedAttributes(integrationJob)
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(o => o.Trim())
            .ToArray();
        if (objectFileNameLowered.Equals("styleclass"))
        {
            this.PopulateStyleClassDataTable(dataSet, excludedAttributes);
            this.PopulateStyleTraitDataTable(dataSet, excludedAttributes);

            if (this.styleDataSet == null)
            {
                this.styleDataSet = dataSet.Copy();
            }

            this.MinifyDataSet(dataSet, objectFileNameLowered);

            if (dataSet.Tables.Count == 0)
            {
                return dataSet;
            }
            else
            {
                dataSet.Tables.Remove(dataSet.Tables[0]);
            }

            return dataSet;
        }
        else if (objectFileNameLowered.Equals("product"))
        {
            if (
                dataSet.Tables[0].Columns.Contains(ADDataFeedSourceFile.ProductNameColumn)
                && dataSet.Tables[0].Columns.Contains(ADDataFeedSourceFile.ProductGroupColumn)
                && dataSet.Tables[0].Columns.Contains(ADDataFeedSourceFile.CategoryCodeColumn)
                && dataSet.Tables[0].Columns.Contains(
                    ADDataFeedSourceFile.SkuShortDescriptionColumn
                )
                && this.styleDataSet != null
                && this.styleDataSet.Tables[0].Rows.Count != 0
            )
            {
                this.AddVariantColumnProduct(dataSet);
                this.PopulateStyleTraitValueDataTable(dataSet, this.styleDataSet);
                this.PopulateStyleTraitValueProductDataTable(dataSet, this.styleDataSet);
            }

            var languageCode = GetLanguageCode(integrationJob);
            this.AddLanguageCodeColumnProduct(dataSet, languageCode);
            this.EnsureMetaDescriptionColumnProduct(dataSet, jobDefinitionStep);
            this.PopulateAttributeTypeDataTable(dataSet, excludedAttributes);
            this.PopulateAttributeValueDataTable(dataSet, excludedAttributes);
            this.PopulateProductSpecificationDataTable(dataSet);
            this.MutateProductData(dataSet, jobDefinitionStep.JobDefinitionStepFieldMaps);

            if (dataSet.Tables[0].Columns.Contains(ADDataFeedSourceFile.BrandNameColumn))
            {
                this.PopulateBrandDataTable(dataSet);
            }

            this.imageAndDocumentSourceFolder = GetStepParameterValue("SourceFolder");
            this.smallImageFolder = GetStepParameterValue("SmallFolder");
            this.mediumImageFolder = GetStepParameterValue("MediumFolder");
            this.largeImageFolder = GetStepParameterValue("LargeFolder");
            this.documentFolder = GetStepParameterValue("DocumentFolder");
            this.usePathMappingForImagesAndDocuments =
                this.smallImageFolder.IsNotBlank()
                || this.mediumImageFolder.IsNotBlank()
                || this.largeImageFolder.IsNotBlank()
                || this.documentFolder.IsNotBlank();
            if (isEnterworks)
            {
                this.PopulateEnterworksProductImageDataTable(dataSet);
            }
            else
            {
                this.PopulateProductImageDataTable(dataSet);
            }

            this.PopulateDocumentDataTable(dataSet);
            this.PopulateProductThreeSixtyImageDataTable(dataSet);

            var websiteName = GetWebsiteName(integrationJob);
            if (string.IsNullOrEmpty(websiteName))
            {
                this.LogHelperADDataFeed.LogSkippingPopulateCategoryDataTables();
            }
            else
            {
                this.PopulateCategoryDataTable(dataSet, websiteName);
                this.PopulateCategoryProductDataTable(dataSet, websiteName);
                this.PopulateCategoryAttributeTypeDataTable(
                    dataSet,
                    websiteName,
                    excludedAttributes
                );
            }
        }

        if (stepName.Equals("refresh category images"))
        {
            var websiteName = GetWebsiteName(integrationJob);
            if (string.IsNullOrEmpty(websiteName))
            {
                this.LogHelperADDataFeed.LogSkippingPopulateCategoryImageDataTables();
            }

            this.PopulateCategoryImageDataTable(dataSet, websiteName);
        }

        if (isAttributeTranslation)
        {
            var baseAttributeTypeDataTable = new DataTable();
            var translationAttrubuteTypeDataTable = new DataTable();
            var baseAttributeValueDataTable = new DataTable();
            var translationAttributeValueDataTable = new DataTable();
            var translationTables = new DataSet();

            var languageCode = GetLanguageCode(integrationJob);

            var baseTable = dataSet.Tables[1].Copy();
            baseTable.TableName = baseLanguageFileName;
            translationTables.Tables.Add(dataSet.Tables[0].Copy());
            translationTables.Tables.Add(baseTable);

            foreach (DataTable table in translationTables.Tables)
            {
                if (table.TableName.Contains(baseLanguageFileName))
                {
                    baseAttributeValueDataTable = this.GetAttributesValuesForTranslation(
                        table,
                        excludedAttributes,
                        string.Empty
                    );
                    baseAttributeTypeDataTable = this.GetAttributeTypesForTranslation(
                        table,
                        excludedAttributes,
                        string.Empty
                    );
                }
                else
                {
                    translationAttributeValueDataTable = this.GetAttributesValuesForTranslation(
                        table,
                        excludedAttributes,
                        languageCode
                    );
                    translationAttrubuteTypeDataTable = this.GetAttributeTypesForTranslation(
                        table,
                        excludedAttributes,
                        languageCode
                    );
                }
            }

            this.PopulateAttributeTypeTranslationDataTable(
                dataSet,
                baseAttributeTypeDataTable,
                translationAttrubuteTypeDataTable
            );

            this.PopulateAttributeValueTranslationDataTable(
                dataSet,
                baseAttributeValueDataTable,
                translationAttributeValueDataTable
            );

            dataSet.Tables.RemoveAt(1);
            dataSet.Tables[0].TableName = "4product";
        }

        this.MinifyDataSet(dataSet, objectFileNameLowered);
        return dataSet;
    }

    private void PopulateAttributeValueTranslationDataTable(
        DataSet dataSet,
        DataTable baseDataTable,
        DataTable translationDataTable
    )
    {
        this.LogHelperADDataFeed.LogPopulateAttributeValueTranslationDataTableStart();
        if (baseDataTable.Rows.Count != translationDataTable.Rows.Count)
        {
            this.LogHelperADDataFeed.LogSkippingPopulateAttributeValueTranslationDataTables();
            return;
        }

        var common =
            from c in baseDataTable.AsEnumerable()
            join x in translationDataTable.AsEnumerable()
                on c.Field<string>("Index") equals x.Field<string>("Index")
            select new object[]
            {
                c["Index"],
                c["Source"],
                c["Keyword"],
                x["Keyword"],
                x["LanguageCode"]
            };

        var attributeValueDataTable =
            ADDataFeedDataTableProvider.CreateTranslationDictionaryDataTable(
                "7TranslationDictionary"
            );

        foreach (var item in common)
        {
            attributeValueDataTable.LoadDataRow(item.ToArray(), true);
        }

        dataSet.Tables.Add(attributeValueDataTable);

        this.LogHelperADDataFeed.LogPopulateAttributeValueTranslationDataTableFinish();
    }

    private DataTable GetAttributesValuesForTranslation(
        DataTable table,
        string[] excludedAttributes,
        string languageCode
    )
    {
        var attributeValueDataTable =
            ADDataFeedDataTableProvider.CreateTranslationDictionaryDataTable("Value");
        var translationIndex = 0;
        foreach (DataRow dataRow in table.Rows)
        {
            var attributes = GetAttributesForDataRow(dataRow, excludedAttributes);

            foreach (var attribute in attributes)
            {
                attributeValueDataTable.Rows.Add(
                    translationIndex++,
                    "AttributeValue",
                    attribute.attributeValue,
                    string.Empty,
                    languageCode
                );
            }
        }

        return attributeValueDataTable;
    }

    private void PopulateAttributeTypeTranslationDataTable(
        DataSet dataSet,
        DataTable baseDataTable,
        DataTable translationDataTable
    )
    {
        this.LogHelperADDataFeed.LogPopulateAttributeTypeTranslationDataTableStart();

        if (baseDataTable.Rows.Count != translationDataTable.Rows.Count)
        {
            this.LogHelperADDataFeed.LogSkippingPopulateAttributeTypeTranslationDataTables();
            return;
        }

        var common =
            from c in baseDataTable.AsEnumerable()
            join x in translationDataTable.AsEnumerable()
                on c.Field<string>("Index") equals x.Field<string>("Index")
            select new object[]
            {
                c["Index"],
                c["Source"],
                c["Keyword"],
                x["Keyword"],
                x["LanguageCode"]
            };

        var attributeTypeDataTable =
            ADDataFeedDataTableProvider.CreateTranslationDictionaryDataTable(
                "6TranslationDictionary"
            );

        foreach (var item in common)
        {
            attributeTypeDataTable.LoadDataRow(item.ToArray(), true);
        }

        dataSet.Tables.Add(attributeTypeDataTable);

        this.LogHelperADDataFeed.LogPopulateAttributeTypeTranslationDataTableFinish();
    }

    private DataTable GetAttributeTypesForTranslation(
        DataTable table,
        string[] excludedAttributes,
        string languageCode
    )
    {
        var attributeTypeDataTable =
            ADDataFeedDataTableProvider.CreateTranslationDictionaryDataTable("Type");
        var attributeTypes = new HashSet<string>();
        var index = 0;

        foreach (DataRow dataRow in table.Rows)
        {
            var attributes = GetAttributesForDataRow(dataRow, excludedAttributes);
            foreach (var attribute in attributes)
            {
                if (attributeTypes.Contains(attribute.attributeType))
                {
                    continue;
                }

                attributeTypeDataTable.Rows.Add(
                    index++,
                    "Attribute",
                    attribute.attributeType,
                    string.Empty,
                    languageCode
                );
                attributeTypes.Add(attribute.attributeType);
                index++;
            }
        }

        return attributeTypeDataTable;
    }

    private static bool IsCategoryImageRefresh(string objectFileNameLowered, string stepName)
    {
        if (objectFileNameLowered.Equals("category") && stepName.Equals("refresh category images"))
        {
            return true;
        }

        return false;
    }

    private sealed class DataRowComparer : IEqualityComparer<DataRow>
    {
        private readonly DataColumn[] columns;

        public DataRowComparer(DataTable table)
        {
            this.columns = table.Columns.Cast<DataColumn>().ToArray();
        }

        public bool Equals(DataRow x, DataRow y)
        {
            foreach (var column in this.columns)
            {
                var xc = x[column];
                var yc = y[column];

                if (xc is not null && yc is not null)
                {
                    if (!xc.Equals(yc))
                    {
                        return false;
                    }
                }
                else if (xc != yc)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(DataRow obj)
        {
            var result = 0;
            foreach (var column in this.columns)
            {
                var c = obj[column];

                if (c is not null)
                {
                    result ^= c.GetHashCode();
                }
            }

            return result;
        }
    }

    private void PopulateCategoryImageDataTable(DataSet dataSet, string websiteName)
    {
        this.LogHelperADDataFeed.LogPopulateCategoryImageDataTableStart();

        var dataTable = dataSet.Tables[0];
        var imageColumn = dataTable.Columns.Contains(ADDataFeedSourceFile.CategoryImageName)
            ? ADDataFeedSourceFile.CategoryImageName
            : null;
        var nameColumn = dataTable.Columns.Contains(ADDataFeedSourceFile.NumericCodeColumn)
            ? ADDataFeedSourceFile.NumericCodeColumn
            : null;

        this.LogHelperADDataFeed.LogDebug(imageColumn);
        this.LogHelperADDataFeed.LogDebug(nameColumn);

        if (imageColumn == null || nameColumn == null)
        {
            this.LogHelperADDataFeed.LogSkippingPopulateCategoryImageDataTables();
            return;
        }

        var categoryImageDataTable = ADDataFeedDataTableProvider.CreateCategoryImageDataTable();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var numericCode = dataRow[ADDataFeedSourceFile.NumericCodeColumn]?.ToString();
            var categoryImage = dataRow[imageColumn]?.ToString();

            this.LogHelperADDataFeed.LogDebug(numericCode);
            this.LogHelperADDataFeed.LogDebug(categoryImage);
            if (string.IsNullOrEmpty(categoryImage) || string.IsNullOrEmpty(numericCode))
            {
                continue;
            }

            var websiteCategoryName = GetWebsiteCategoryName(dataRow, websiteName);

            categoryImageDataTable.Rows.Add(websiteCategoryName, categoryImage);
        }

        dataSet.Tables.Add(categoryImageDataTable);
        // We need to remove the initial table since we are only using the one we are creating.
        dataSet.Tables.Remove(dataSet.Tables[0]);

        this.LogHelperADDataFeed.LogPopulateCategoryImageDataTableFinish();
    }

    private static string GetWebsiteCategoryName(DataRow dataRow, string websiteName)
    {
        var levelNumber = Convert.ToInt64(dataRow[ADDataFeedSourceFile.LevelNumber]);
        var websiteCategoryName = websiteName;

        for (var i = 1; i <= levelNumber; i++)
        {
            var categoryCode = ParseColumnValue(
                dataRow,
                string.Format(ADDataFeedSourceFile.LevelCodeColumnFormat, i)
            );

            websiteCategoryName += $":{categoryCode}";
        }

        return websiteCategoryName;
    }

    protected void AddVariantColumnProduct(DataSet dataSet)
    {
        this.LogHelperADDataFeed.LogAddNewProductRecordsTableStart();
        var variantTable = dataSet.Tables[0];
        if (variantTable != null)
        {
            variantTable.Columns.Add(ADDataFeedSourceFile.VariantType);
            variantTable.Columns.Add(ADDataFeedSourceFile.VariantParent);
            var update = false;

            var tempTable = variantTable.Clone();
            foreach (DataRow row in variantTable.Rows)
            {
                if (!string.IsNullOrEmpty(row[ADDataFeedSourceFile.ProductNameColumn].ToString()))
                {
                    var newProductRow = tempTable.NewRow();
                    newProductRow[ADDataFeedSourceFile.VariantType] = row[
                        ADDataFeedSourceFile.CategoryCodeColumn
                    ];
                    for (var categoryLevel = 1; true; categoryLevel++)
                    {
                        var categoryCode = ParseColumnValue(
                            row,
                            string.Format(
                                ADDataFeedSourceFile.CategoryCodeColumnFormatNoCategoryNumber,
                                categoryLevel
                            )
                        );
                        if (categoryCode.IsBlank())
                        {
                            break;
                        }

                        var categoryName = ParseColumnValue(
                            row,
                            string.Format(
                                ADDataFeedSourceFile.CategoryNameColumnFormatNoCategoryNumber,
                                categoryLevel
                            )
                        );
                        newProductRow[
                            string.Format(
                                ADDataFeedSourceFile.CategoryNameColumnFormatNoCategoryNumber,
                                categoryLevel
                            )
                        ] = row[
                            string.Format(
                                ADDataFeedSourceFile.CategoryNameColumnFormatNoCategoryNumber,
                                categoryLevel
                            )
                        ];
                        newProductRow[
                            string.Format(
                                ADDataFeedSourceFile.CategoryCodeColumnFormatNoCategoryNumber,
                                categoryLevel
                            )
                        ] = row[
                            string.Format(
                                ADDataFeedSourceFile.CategoryCodeColumnFormatNoCategoryNumber,
                                categoryLevel
                            )
                        ];
                    }

                    newProductRow[ADDataFeedSourceFile.CategoryCodeColumn] = row[
                        ADDataFeedSourceFile.CategoryCodeColumn
                    ];
                    newProductRow[ADDataFeedSourceFile.SkuShortDescriptionColumn] = row[
                        ADDataFeedSourceFile.ProductGroupColumn
                    ];
                    newProductRow[ADDataFeedSourceFile.MyPartNumberColumn] = row[
                        ADDataFeedSourceFile.ProductNameColumn
                    ];
                    row[ADDataFeedSourceFile.VariantParent] = row[
                        ADDataFeedSourceFile.ProductNameColumn
                    ];
                    update = true;
                    tempTable.Rows.Add(newProductRow);
                }

                tempTable.ImportRow(row);
            }

            var uniqueRows = tempTable.Rows
                .Cast<DataRow>()
                .ToHashSet(new DataRowComparer(tempTable));

            var rowsToRemove = tempTable.Rows.Cast<DataRow>().Except(uniqueRows).ToHashSet();

            foreach (var row in rowsToRemove)
            {
                tempTable.Rows.Remove(row);
            }

            if (update)
            {
                dataSet.Tables.Remove(variantTable);
                var variantDistinctTable = tempTable.DefaultView.ToTable();
                dataSet.Tables.Add(variantDistinctTable);
            }
        }

        this.LogHelperADDataFeed.LogAddNewProductRecordsTableFinish();
    }

    protected void AddLanguageCodeColumnProduct(DataSet dataSet, string languageCode)
    {
        this.LogHelperADDataFeed.LogAddLanguageCodeProductRecordsTableStart();
        var productTable = dataSet.Tables[0];
        if (productTable != null)
        {
            productTable.Columns.Add(ADDataFeedSourceFile.LanguageCode);
            foreach (DataRow row in productTable.Rows)
            {
                row[ADDataFeedSourceFile.LanguageCode] = languageCode;
            }

            productTable.AcceptChanges();
        }

        this.LogHelperADDataFeed.LogAddLanguageCodeProductRecordsTableFinish();
    }

    protected void EnsureMetaDescriptionColumnProduct(
        DataSet dataSet,
        JobDefinitionStep jobDefinitionStep
    )
    {
        var productTable = dataSet.Tables[0];
        if (
            productTable != null
            && !productTable.Columns.Contains(ADDataFeedSourceFile.MetaDescription)
        )
        {
            productTable.Columns.Add(ADDataFeedSourceFile.MetaDescription);
            productTable.AcceptChanges();
        }
    }

    protected void CleanDataSet(DataSet dataSet)
    {
        var dataTable = dataSet.Tables[0];
        var stringColumns = dataTable.Columns
            .Cast<DataColumn>()
            .Where(o => o.DataType == typeof(string))
            .ToArray();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            foreach (var dataColumn in stringColumns)
            {
                var original = dataRow[dataColumn].ToString();
                var decoded = HttpUtility.HtmlDecode(dataRow[dataColumn].ToString());

                if (decoded != original)
                {
                    dataRow[dataColumn] = decoded;
                }
            }
        }
    }

    protected void MinifyDataSet(DataSet dataSet, string objectName)
    {
        this.LogHelperADDataFeed.LogMinifyDataSetStart();

        var dataTable = dataSet.Tables[0];

        var columnNamesToRemove = new List<string>();

        if (objectName.Equals("styleclass"))
        {
            columnNamesToRemove = new List<string>
            {
                ADDataFeedSourceFile.TaxonomyNameColumn,
                ADDataFeedSourceFile.DataTypeColumn,
                ADDataFeedSourceFile.CategoryAttributeValueColumn,
                ADDataFeedSourceFile.ValueDelimeterColumn,
                ADDataFeedSourceFile.IsDifferentiatorColumn,
                ADDataFeedSourceFile.DisplaySequenceColumn,
                ADDataFeedSourceFile.EntityTypeColumn,
                ADDataFeedSourceFile.IsFilterableColumn,
                ADDataFeedSourceFile.FilterSequenceColumn,
                ADDataFeedSourceFile.PrintColumn,
                ADDataFeedSourceFile.StatusColumn
            };
        }
        else if (objectName.Equals("product"))
        {
            columnNamesToRemove = new List<string>
            {
                ADDataFeedSourceFile.AttributeNameColumn,
                ADDataFeedSourceFile.AttributeValueColumn,
                ADDataFeedSourceFile.AttributeUomColumn,
                ADDataFeedSourceFile.DifferentiatorColumn,
                ADDataFeedSourceFile.DifferentiatorColumn,
                ADDataFeedSourceFile.ItemImageItemImageColumn,
                ADDataFeedSourceFile.ItemImageDetailImageColumn,
                ADDataFeedSourceFile.ItemImageEnlargeImageColumn,
                ADDataFeedSourceFile.ItemImageCaptionColumn,
                ADDataFeedSourceFile.ItemDocumentNameColumn,
                ADDataFeedSourceFile.ItemDocumentTypeColumn,
                ADDataFeedSourceFile.DocCaptionColumn,
                ADDataFeedSourceFile.CategoryColumn
            };
        }

        var columnsToRemove = dataTable.Columns
            .Cast<DataColumn>()
            .Where(
                o =>
                    columnNamesToRemove.Any(
                        p => o.ColumnName.StartsWith(p, StringComparison.OrdinalIgnoreCase)
                    )
            )
            .ToArray();

        this.LogHelperADDataFeed.LogMinifyDataSetRemovingColumns(
            dataTable.TableName,
            columnsToRemove.Select(o => o.ColumnName)
        );

        foreach (var columnToRemove in columnsToRemove)
        {
            dataTable.Columns.Remove(columnToRemove);
        }

        this.LogHelperADDataFeed.LogMinifyDataSetFinish();
    }

    protected void PopulateBrandDataTable(DataSet dataset)
    {
        this.LogHelperADDataFeed.LogPopulatingBrandDataSetStart();

        var dataTable = dataset.Tables[0];
        var brandDataTable = ADDataFeedDataTableProvider.CreateBrandDataTable();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var brandName = dataRow[ADDataFeedSourceFile.BrandNameColumn];
            var memberPartNumber = dataRow[ADDataFeedSourceFile.MyPartNumberColumn];
            brandDataTable.Rows.Add(brandName, memberPartNumber);
        }

        dataset.Tables.Add(brandDataTable);

        this.LogHelperADDataFeed.LogPopulatingBrandDataSetFinish();
    }

    protected void PopulateStyleTraitValueDataTable(DataSet dataSet, DataSet tempDataSet)
    {
        this.LogHelperADDataFeed.LogPopulateStyleTraitValueProductDataTableStart();

        var dataTable = dataSet.Tables[0];
        var styleTraitValueTable = ADDataFeedDataTableProvider.CreateStyleTraitValueDataTable();
        var styleTraitValues = new HashSet<string>();
        List<(string styleClass, string styleTrait, string styleTraitValue)> styleValues = new();
        foreach (DataRow dataRow in dataTable.Rows)
        {
            GetStyleValueForDataRow(dataRow, tempDataSet, styleValues);
        }

        foreach (var styleValueProduct in styleValues)
        {
            if (styleTraitValues.Contains(styleValueProduct.styleTraitValue))
            {
                continue;
            }

            styleTraitValueTable.Rows.Add(
                styleValueProduct.styleClass,
                styleValueProduct.styleTrait,
                styleValueProduct.styleTraitValue
            );
        }

        dataSet.Tables.Add(styleTraitValueTable);
        this.LogHelperADDataFeed.LogPopulateStyleTraitValueProductDataTableFinish();
    }

    protected void PopulateStyleTraitValueProductDataTable(DataSet dataSet, DataSet tempDataSet)
    {
        this.LogHelperADDataFeed.LogPopulateStyleTraitValueProductDataTableStart();

        var dataTable = dataSet.Tables[0];
        var styleTraitValueProductTable =
            ADDataFeedDataTableProvider.CreateStyleTraitValueProductDataTable();
        var styleTraitValue = new HashSet<string>();
        List<(
            string styleClass,
            string styleTrait,
            string styleTraitValue,
            string myPartNumber
        )> styleValueProducts = new();
        foreach (DataRow dataRow in dataTable.Rows)
        {
            GetStyleValueProductsForDataRow(dataRow, tempDataSet, styleValueProducts);
        }

        foreach (var styleValueProduct in styleValueProducts)
        {
            if (styleTraitValue.Contains(styleValueProduct.styleTraitValue))
            {
                continue;
            }

            styleTraitValueProductTable.Rows.Add(
                styleValueProduct.styleClass,
                styleValueProduct.styleTrait,
                styleValueProduct.styleTraitValue,
                styleValueProduct.myPartNumber
            );
        }

        dataSet.Tables.Add(styleTraitValueProductTable);
        this.LogHelperADDataFeed.LogPopulateStyleTraitValueProductDataTableFinish();
    }

    protected void PopulateAttributeTypeDataTable(DataSet dataSet, string[] excludedAttributes)
    {
        this.LogHelperADDataFeed.LogPopulateAttributeTypeDataTableStart();

        var dataTable = dataSet.Tables[0];
        var attributeTypeDataTable = ADDataFeedDataTableProvider.CreateAttributeTypeDataTable();
        var attributeTypes = new HashSet<string>();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var attributes = GetAttributesForDataRow(dataRow, excludedAttributes);

            foreach (var attribute in attributes)
            {
                if (attributeTypes.Contains(attribute.attributeType))
                {
                    continue;
                }

                attributeTypeDataTable.Rows.Add(attribute.attributeType);
                attributeTypes.Add(attribute.attributeType);
            }
        }

        dataSet.Tables.Add(attributeTypeDataTable);

        this.LogHelperADDataFeed.LogPopulateAttributeTypeDataTableFinish();
    }

    protected void PopulateStyleTraitDataTable(DataSet dataSet, string[] excludedAttributes)
    {
        this.LogHelperADDataFeed.LogPopulateStyleTraitDataTableStart();

        var dataTable = dataSet.Tables[0];
        var styleTraitDataTable = ADDataFeedDataTableProvider.CreateStyleTraitDataTable();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var styles = GetStyleTraitForDataRow(dataRow, excludedAttributes);

            foreach (var style in styles)
            {
                styleTraitDataTable.Rows.Add(
                    style.numericCode,
                    style.attributeName,
                    style.displaySequence
                );
            }
        }

        dataSet.Tables.Add(styleTraitDataTable);

        this.LogHelperADDataFeed.LogPopulateStyleTraitDataTableFinish();
    }

    protected void PopulateStyleClassDataTable(DataSet dataSet, string[] excludedAttributes)
    {
        this.LogHelperADDataFeed.LogPopulateStyleClassDataTableStart();

        var dataTable = dataSet.Tables[0];
        var styleClassDataTable = ADDataFeedDataTableProvider.CreateStyleClassDataTable();
        var styleTrait = new HashSet<string>();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var styles = GetStyleclassForDataRow(dataRow);

            foreach (var style in styles)
            {
                if (styleTrait.Contains(style.numericCode))
                {
                    continue;
                }

                if (styleTrait.Contains(style.nodeName))
                {
                    continue;
                }

                styleClassDataTable.Rows.Add(style.numericCode, style.nodeName);
                styleTrait.Add(style.numericCode);
                styleTrait.Add(style.nodeName);
            }
        }

        dataSet.Tables.Add(styleClassDataTable);

        this.LogHelperADDataFeed.LogPopulateStyleClassDataTableFinish();
    }

    protected void PopulateAttributeValueDataTable(DataSet dataSet, string[] excludedAttributes)
    {
        this.LogHelperADDataFeed.LogPopulateAttributeValueDataTableStart();

        var dataTable = dataSet.Tables[0];
        var attributeValueDataTable = ADDataFeedDataTableProvider.CreateAttributeValueDataTable();
        var productAttributeValueDataTable =
            ADDataFeedDataTableProvider.CreateProductAttributeValueDataTable();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var partNumber = dataRow[ADDataFeedSourceFile.MyPartNumberColumn].ToString();
            var attributes = GetAttributesForDataRow(dataRow, excludedAttributes);
            var attributeIndex = 1;

            foreach (var attribute in attributes)
            {
                attributeValueDataTable.Rows.Add(
                    attribute.attributeType,
                    attribute.attributeValue,
                    attributeIndex++
                );
                productAttributeValueDataTable.Rows.Add(
                    partNumber,
                    attribute.attributeType,
                    attribute.attributeValue
                );
            }
        }

        dataSet.Tables.Add(attributeValueDataTable);
        dataSet.Tables.Add(productAttributeValueDataTable);

        this.LogHelperADDataFeed.LogPopulateAttributeValueDataTableFinish();
    }

    protected void PopulateProductSpecificationDataTable(DataSet dataSet)
    {
        this.LogHelperADDataFeed.LogPopulateProductSpecificationDataTableStart();

        var dataTable = dataSet.Tables[0];
        var productSpecificationDataTable =
            ADDataFeedDataTableProvider.CreateSpecificationDataTable();
        var itemFeatureColumns = dataTable.Columns
            .Cast<DataColumn>()
            .Where(
                o =>
                    o.ColumnName.StartsWith(
                        ADDataFeedSourceFile.ItemFeaturesColumn,
                        StringComparison.OrdinalIgnoreCase
                    )
                    || o.ColumnName.Equals(
                        ADDataFeedSourceFile.VideoLinksColumn,
                        StringComparison.OrdinalIgnoreCase
                    )
            )
            .OrderBy(o => o.ColumnName)
            .ToList();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var partNumber = dataRow[ADDataFeedSourceFile.MyPartNumberColumn].ToString();

            var itemFeatures = itemFeatureColumns
                .Select(o => dataRow[o.ColumnName].ToString())
                .Where(p => !string.IsNullOrEmpty(p));

            var languageCode = ParseColumnValue(dataRow, ADDataFeedSourceFile.LanguageCode);

            var itemApplication = ParseColumnValue(dataRow, ADDataFeedSourceFile.ApplicationColumn);
            var itemIncludes = ParseColumnValue(dataRow, ADDataFeedSourceFile.IncludesColumn);
            var itemStandardsApprovals = ParseColumnValue(
                dataRow,
                ADDataFeedSourceFile.StandardsApprovalsColumn
            );
            var itemWarranty = ParseColumnValue(dataRow, ADDataFeedSourceFile.WarrantyColumn);

            if (itemFeatures.Any())
            {
                var itemFeatureBullets =
                    $"<ul>{string.Join(string.Empty, itemFeatures.Select(o => $"<li>{o}</li>"))}</ul>";
                productSpecificationDataTable.Rows.Add(
                    partNumber,
                    "Item Features",
                    itemFeatureBullets,
                    languageCode
                );
            }

            if (!string.IsNullOrWhiteSpace(itemApplication))
            {
                productSpecificationDataTable.Rows.Add(
                    partNumber,
                    "Application",
                    itemApplication,
                    languageCode
                );
            }

            if (!string.IsNullOrWhiteSpace(itemIncludes))
            {
                productSpecificationDataTable.Rows.Add(
                    partNumber,
                    "Includes",
                    itemIncludes,
                    languageCode
                );
            }

            if (!string.IsNullOrWhiteSpace(itemStandardsApprovals))
            {
                productSpecificationDataTable.Rows.Add(
                    partNumber,
                    "Standards",
                    itemStandardsApprovals,
                    languageCode
                );
            }

            if (!string.IsNullOrWhiteSpace(itemIncludes))
            {
                productSpecificationDataTable.Rows.Add(
                    partNumber,
                    "Warranty",
                    itemWarranty,
                    languageCode
                );
            }
        }

        dataSet.Tables.Add(productSpecificationDataTable);

        this.LogHelperADDataFeed.LogPopulateProductSpecificationDataTableFinish();
    }

    protected void PopulateProductImageDataTable(DataSet dataSet)
    {
        this.LogHelperADDataFeed.LogPopulateProductImageDataTableStart();

        var dataTable = dataSet.Tables[0];
        var productImagesDataTable = ADDataFeedDataTableProvider.CreateProductImageDataTable();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var partNumber = dataRow[ADDataFeedSourceFile.MyPartNumberColumn].ToString();

            for (var imageIndex = 1; true; imageIndex++)
            {
                var itemImageItemImage = GetImagePath(
                    ParseColumnValue(
                        dataRow,
                        $"{ADDataFeedSourceFile.ItemImageItemImageColumn}{imageIndex}"
                    )
                );
                if (itemImageItemImage.IsBlank())
                {
                    break;
                }

                var itemImageDetailImage = GetImagePath(
                    dataRow[
                        $"{ADDataFeedSourceFile.ItemImageDetailImageColumn}{imageIndex}"
                    ].ToString()
                );
                var itemImageEnlargeImage = GetImagePath(
                    dataRow[
                        $"{ADDataFeedSourceFile.ItemImageEnlargeImageColumn}{imageIndex}"
                    ].ToString()
                );
                var itemImageCaption = dataRow[
                    $"{ADDataFeedSourceFile.ItemImageCaptionColumn}_{imageIndex}"
                ];

                productImagesDataTable.Rows.Add(
                    partNumber,
                    itemImageItemImage,
                    itemImageDetailImage,
                    itemImageEnlargeImage,
                    itemImageCaption,
                    imageIndex
                );
            }
        }

        dataSet.Tables.Add(productImagesDataTable);

        this.LogHelperADDataFeed.LogPopulateProductImageDataTableFinish();
    }

    protected void PopulateEnterworksProductImageDataTable(DataSet dataSet)
    {
        this.LogHelperADDataFeed.LogPopulateProductImageDataTableStart();

        var dataTable = dataSet.Tables[0];
        var productImagesDataTable =
            ADDataFeedDataTableProvider.CreateEnterworksProductImageDataTable();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var partNumber = dataRow[ADDataFeedSourceFile.MyPartNumberColumn].ToString();

            for (var imageIndex = 1; true; imageIndex++)
            {
                var itemImageItemImage = ParseColumnValue(
                    dataRow,
                    $"{ADDataFeedSourceFile.ItemImageFilenameColumn}_{imageIndex}"
                );
                if (itemImageItemImage.IsBlank())
                {
                    break;
                }

                string itemSmallImage = null;
                string itemMediumImage = null;
                string itemLargeImage = null;
                if (this.usePathMappingForImagesAndDocuments)
                {
                    if (this.smallImageFolder.IsNotBlank())
                    {
                        itemSmallImage = GetEnterworksSmallMediumImagePath(
                            itemImageItemImage,
                            this.imageAndDocumentSourceFolder + "/" + this.smallImageFolder
                        );
                    }

                    if (this.mediumImageFolder.IsNotBlank())
                    {
                        itemMediumImage = GetEnterworksSmallMediumImagePath(
                            itemImageItemImage,
                            this.imageAndDocumentSourceFolder + "/" + this.mediumImageFolder
                        );
                    }

                    if (this.largeImageFolder.IsNotBlank())
                    {
                        itemLargeImage = GetEnterworksImagePath(
                            itemImageItemImage,
                            this.imageAndDocumentSourceFolder + "/" + this.largeImageFolder
                        );
                    }
                }
                else
                {
                    itemSmallImage = GetEnterworksSmallMediumImagePath(
                        itemImageItemImage,
                        "ad/small"
                    );
                    itemMediumImage = GetEnterworksSmallMediumImagePath(
                        itemImageItemImage,
                        "ad/medium"
                    );
                    itemLargeImage = GetEnterworksImagePath(itemImageItemImage, "ad/large");
                }

                productImagesDataTable.Rows.Add(
                    partNumber,
                    itemSmallImage,
                    itemMediumImage,
                    itemLargeImage,
                    imageIndex
                );
            }
        }

        dataSet.Tables.Add(productImagesDataTable);

        this.LogHelperADDataFeed.LogPopulateProductImageDataTableFinish();
    }

    protected void PopulateDocumentDataTable(DataSet dataSet)
    {
        this.LogHelperADDataFeed.LogPopulateDocumentDataTableStart();

        var dataTable = dataSet.Tables[0];
        var documentDataTable = ADDataFeedDataTableProvider.CreateDocumentDataTable();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var partNumber = dataRow[ADDataFeedSourceFile.MyPartNumberColumn].ToString();

            for (var documentIndex = 1; true; documentIndex++)
            {
                var itemDocumentName = ParseColumnValue(
                    dataRow,
                    $"{ADDataFeedSourceFile.ItemDocumentNameColumn}_{documentIndex}"
                );
                if (itemDocumentName.IsBlank())
                {
                    itemDocumentName = ParseColumnValue(
                        dataRow,
                        $"{ADDataFeedSourceFile.EnterworksItemDocumentNameColumn}_{documentIndex}"
                    );

                    if (itemDocumentName.IsBlank())
                    {
                        break;
                    }
                }

                string itemDocumentPath;
                if (this.usePathMappingForImagesAndDocuments && this.documentFolder.IsNotBlank())
                {
                    itemDocumentPath = GetDocumentPath(
                        itemDocumentName,
                        this.imageAndDocumentSourceFolder + "/" + this.documentFolder
                    );
                }
                else
                {
                    itemDocumentPath = GetDocumentPath(itemDocumentName, "documents");
                }

                var itemDocumentType =
                    ParseColumnValue(
                        dataRow,
                        $"{ADDataFeedSourceFile.ItemDocumentTypeColumn}_{documentIndex}"
                    ) ?? string.Empty;
                var docCaption = ParseColumnValue(
                    dataRow,
                    $"{ADDataFeedSourceFile.DocCaptionColumn}_{documentIndex}"
                );
                if (docCaption.IsBlank())
                {
                    docCaption =
                        ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.EnterworksDocCaptionColumn}_{documentIndex}"
                        ) ?? string.Empty;
                }

                documentDataTable.Rows.Add(
                    partNumber,
                    "Product",
                    itemDocumentPath,
                    itemDocumentType,
                    docCaption
                );
            }
        }

        dataSet.Tables.Add(documentDataTable);

        this.LogHelperADDataFeed.LogPopulateDocumentDataTableFinish();
    }

    protected void PopulateCategoryDataTable(DataSet dataSet, string websiteName)
    {
        this.LogHelperADDataFeed.LogPopulateCategoryDataTableStart();

        var dataTable = dataSet.Tables[0];
        var categoryDataTable = ADDataFeedDataTableProvider.CreateCategoryDataTable();
        var websiteCategoryNames = new HashSet<string>();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var categories = GetCategoriesForDataRow(dataRow, websiteName, true);
            foreach (var category in categories)
            {
                if (!websiteCategoryNames.Contains(category.websiteCategoryName))
                {
                    categoryDataTable.Rows.Add(
                        category.websiteCategoryName,
                        category.urlSegment,
                        category.shortDescription
                    );
                    websiteCategoryNames.Add(category.websiteCategoryName);
                }
            }
        }

        dataSet.Tables.Add(categoryDataTable);

        this.LogHelperADDataFeed.LogPopulateCategoryDataTableFinish();
    }

    protected void PopulateCategoryProductDataTable(DataSet dataSet, string websiteName)
    {
        this.LogHelperADDataFeed.LogPopulateCategoryProductDataTableStart();

        var dataTable = dataSet.Tables[0];
        var categoryProductDataTable = ADDataFeedDataTableProvider.CreateCategoryProductDataTable();
        var alreadyAdded = new HashSet<(string WebsiteCategoryName, string PartNumber)>();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var partNumber = dataRow[ADDataFeedSourceFile.MyPartNumberColumn].ToString();
            var categories = GetCategoriesForDataRow(dataRow, websiteName, false);
            foreach (var category in categories)
            {
                if (alreadyAdded.Add((category.websiteCategoryName, partNumber)))
                {
                    categoryProductDataTable.Rows.Add(category.websiteCategoryName, partNumber);
                }
            }
        }

        dataSet.Tables.Add(
            categoryProductDataTable.DefaultView.ToTable(
                false, // ToTable(true) runs an incredibly slow "distinct" operation.
                "WebsiteCategoryName",
                ADDataFeedSourceFile.MyPartNumberColumn
            )
        );

        this.LogHelperADDataFeed.LogPopulateCategoryProductDataTableFinish();
    }

    protected void PopulateProductThreeSixtyImageDataTable(DataSet dataSet)
    {
        this.LogHelperADDataFeed.LogPopulateProductThreeSixtyImageDataTableStart();

        var dataTable = dataSet.Tables[0];
        var threeSixtyImageColumn = dataTable.Columns.Contains(
            ADDataFeedSourceFile.ThreeSixtyImageColumn
        )
            ? ADDataFeedSourceFile.ThreeSixtyImageColumn
            : dataTable.Columns.Contains(ADDataFeedSourceFile.ThreeSixtyImageUrlColumn)
                ? ADDataFeedSourceFile.ThreeSixtyImageUrlColumn
                : null;
        if (threeSixtyImageColumn == null)
        {
            this.LogHelperADDataFeed.LogSkippingPopulateProductThreeSixtyImageDataTable();
            return;
        }

        var productThreeSixtyImageDataTable =
            ADDataFeedDataTableProvider.CreateProductThreeSixtyImageDataTable();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var partNumber = dataRow[ADDataFeedSourceFile.MyPartNumberColumn].ToString();
            var threeSixtyImage = dataRow[threeSixtyImageColumn]?.ToString();
            if (string.IsNullOrEmpty(threeSixtyImage))
            {
                continue;
            }

            productThreeSixtyImageDataTable.Rows.Add(partNumber, threeSixtyImage);
        }

        dataSet.Tables.Add(productThreeSixtyImageDataTable);

        this.LogHelperADDataFeed.LogPopulateProductThreeSixtyImageDataTableFinish();
    }

    protected void PopulateCategoryAttributeTypeDataTable(
        DataSet dataSet,
        string websiteName,
        string[] excludedAttributes
    )
    {
        this.LogHelperADDataFeed.LogPopulateCategoryAttributeTypeDataTableStart();

        var dataTable = dataSet.Tables[0];
        var categoryAttributeTypeDataTable =
            ADDataFeedDataTableProvider.CreateCategoryAttributeTypeDataTable();
        var categoryAttributeTypes = new HashSet<Tuple<string, string>>();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var categories = GetCategoriesForDataRow(dataRow, websiteName, false);
            var attributes = GetAttributesForDataRow(dataRow, excludedAttributes);

            foreach (var category in categories)
            {
                foreach (var attribute in attributes)
                {
                    if (
                        categoryAttributeTypes.Contains(
                            new Tuple<string, string>(
                                category.websiteCategoryName,
                                attribute.attributeType
                            )
                        )
                    )
                    {
                        continue;
                    }

                    categoryAttributeTypeDataTable.Rows.Add(
                        category.websiteCategoryName,
                        attribute.attributeType
                    );
                    categoryAttributeTypes.Add(
                        new Tuple<string, string>(
                            category.websiteCategoryName,
                            attribute.attributeType
                        )
                    );
                }
            }
        }

        dataSet.Tables.Add(categoryAttributeTypeDataTable);

        this.LogHelperADDataFeed.LogPopulateCategoryAttributeTypeDataTableFinish();
    }

    private void MutateProductData(
        DataSet dataSet,
        ICollection<JobDefinitionStepFieldMap> fieldMapSteps
    )
    {
        if (dataSet is null)
        {
            throw new ArgumentNullException(nameof(dataSet));
        }

        if (fieldMapSteps is null)
        {
            throw new ArgumentNullException(nameof(fieldMapSteps));
        }

        var rowCount = 0;
        foreach (DataRow row in dataSet.Tables[0].Rows)
        {
            var unchangedData = (
                row[ADDataFeedSourceFile.SKULongDescription1Column],
                row[ADDataFeedSourceFile.SKULongDescription2Column],
                row[ADDataFeedSourceFile.EANColumn]
            );

            foreach (var step in fieldMapSteps)
            {
                try
                {
                    if (
                        step.ToProperty.EqualsIgnoreCase("ContentManager")
                        && step.FromProperty.EqualsIgnoreCase(
                            ADDataFeedSourceFile.MarketingDescriptionColumn
                        )
                        && row[ADDataFeedSourceFile.MarketingDescriptionColumn].ToString().IsBlank()
                    )
                    {
                        row[ADDataFeedSourceFile.MarketingDescriptionColumn] =
                            $"{unchangedData.Item1}, {unchangedData.Item2}";
                    }

                    if (
                        step.FromProperty.EqualsIgnoreCase(ADDataFeedSourceFile.UPCColumn)
                        && row[ADDataFeedSourceFile.UPCColumn].ToString().IsBlank()
                    )
                    {
                        row[ADDataFeedSourceFile.UPCColumn] = unchangedData.Item3;
                    }

                    if (
                        step.ToProperty.EqualsIgnoreCase("ShortDescription")
                        && step.FromProperty.EqualsIgnoreCase(
                            ADDataFeedSourceFile.SkuShortDescriptionColumn
                        )
                        && row[ADDataFeedSourceFile.SkuShortDescriptionColumn].ToString().IsBlank()
                    )
                    {
                        throw new Exception($"No Short Description provided in row {rowCount}");
                    }

                    if (
                        step.FromProperty.EqualsIgnoreCase(ADDataFeedSourceFile.Keywords)
                        || step.FromProperty.EqualsIgnoreCase(
                            $"{ADDataFeedSourceFile.Keywords},LanguageCode"
                        )
                    )
                    {
                        row[ADDataFeedSourceFile.Keywords] = AggregateFields(
                            row,
                            ADDataFeedSourceFile.MetaKeywordsFields
                        );
                    }

                    if (
                        step.FromProperty.EqualsIgnoreCase(ADDataFeedSourceFile.MetaDescription)
                        || step.FromProperty.EqualsIgnoreCase(
                            $"{ADDataFeedSourceFile.MetaDescription},LanguageCode"
                        )
                    )
                    {
                        row[ADDataFeedSourceFile.MetaDescription] = AggregateFields(
                            row,
                            ADDataFeedSourceFile.MetaDescriptionFields
                        );
                    }
                }
                catch (Exception e)
                {
                    this.LogHelperADDataFeed.LogError(
                        $"Data mutation in AD processor failed with {e.Message}"
                    );
                    continue;
                }
            }

            rowCount++;
        }
    }

    private static List<(
        string websiteCategoryName,
        string urlSegment,
        string shortDescription
    )> GetCategoriesForDataRow(DataRow dataRow, string websiteName, bool includeParentNodes)
    {
        var categories =
            new List<(string websiteCategoryName, string urlSegment, string shortDescription)>();
        var usesCategoryNumber = dataRow.Table.Columns.Contains(
            string.Format(ADDataFeedSourceFile.CategoryCodeColumnFormat, 1, 1)
        );

        if (usesCategoryNumber)
        {
            for (var categoryNumber = 1; true; categoryNumber++)
            {
                var websiteCategoryName = websiteName;
                (string websiteCategoryName, string urlSegment, string shortDescription)? category =
                    null;

                for (var categoryLevel = 1; true; categoryLevel++)
                {
                    var categoryCode = ParseColumnValue(
                        dataRow,
                        string.Format(
                            ADDataFeedSourceFile.CategoryCodeColumnFormat,
                            categoryNumber,
                            categoryLevel
                        )
                    );

                    // if first level category code is empty then all categories have been captured
                    if (categoryCode.IsBlank() && categoryLevel == 1)
                    {
                        return categories;
                    }

                    // if non-first level category code is empty then all nodes for current category have been captured
                    if (categoryCode.IsBlank())
                    {
                        if (category != null)
                        {
                            categories.Add(category.Value);
                        }

                        break;
                    }

                    if (includeParentNodes && category != null)
                    {
                        categories.Add(category.Value);
                    }

                    var categoryName = ParseColumnValue(
                        dataRow,
                        string.Format(
                            ADDataFeedSourceFile.CategoryNameColumnFormat,
                            categoryNumber,
                            categoryLevel
                        )
                    );

                    category = (
                        websiteCategoryName += $":{categoryCode}",
                        categoryCode,
                        categoryName
                    );
                }
            }
        }
        else
        {
            var websiteCategoryName = websiteName;
            (string websiteCategoryName, string urlSegment, string shortDescription)? category =
                null;

            for (var categoryLevel = 1; true; categoryLevel++)
            {
                var categoryCode = ParseColumnValue(
                    dataRow,
                    string.Format(
                        ADDataFeedSourceFile.CategoryCodeColumnFormatNoCategoryNumber,
                        categoryLevel
                    )
                );

                // if non-first level category code is empty then all nodes for current category have been captured
                if (categoryCode.IsBlank())
                {
                    if (category != null)
                    {
                        categories.Add(category.Value);
                    }

                    break;
                }

                if (includeParentNodes && category != null)
                {
                    categories.Add(category.Value);
                }

                var categoryName = ParseColumnValue(
                    dataRow,
                    string.Format(
                        ADDataFeedSourceFile.CategoryNameColumnFormatNoCategoryNumber,
                        categoryLevel
                    )
                );

                category = (websiteCategoryName += $":{categoryCode}", categoryCode, categoryName);
            }

            return categories;
        }
    }

    private static List<(
        string numericCode,
        string attributeName,
        string displaySequence
    )> GetStyleTraitForDataRow(DataRow dataRow, string[] excludedAttributes)
    {
        var styleTrait =
            new List<(string numericCode, string attributeName, string displaySequence)>();

        var numericCode = ParseColumnValue(dataRow, $"{ADDataFeedSourceFile.NumericCodeColumn}");
        var attributeName = ParseColumnValue(
            dataRow,
            $"{ADDataFeedSourceFile.CategoryAttributeNameColumn}"
        );
        var displaySequence = ParseColumnValue(
            dataRow,
            $"{ADDataFeedSourceFile.DisplaySequenceColumn}"
        );

        styleTrait.Add((numericCode, attributeName, displaySequence));
        return styleTrait;
    }

    private static List<(string numericCode, string nodeName)> GetStyleclassForDataRow(
        DataRow dataRow
    )
    {
        var styleClass = new List<(string numericCode, string nodeName)>();

        var numericCode = ParseColumnValue(dataRow, $"{ADDataFeedSourceFile.NumericCodeColumn}");
        var nodeName = ParseColumnValue(dataRow, $"{ADDataFeedSourceFile.NodeNameColumn}");

        styleClass.Add((numericCode, nodeName));
        return styleClass;
    }

    private static List<(
        string styleClass,
        string styleTrait,
        string styleTraitValue
    )> GetStyleValueForDataRow(
        DataRow dataRow,
        DataSet catDataSet,
        List<(string styleClass, string styleTrait, string styleTraitValue)> styleValues
    )
    {
        var catDataTable = catDataSet.Tables[0];

        if (catDataTable != null)
        {
            for (var styleValueProductsIndex = 1; true; styleValueProductsIndex++)
            {
                if (
                    ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.AttributeValueUomColumn}_{styleValueProductsIndex}"
                        )
                        .IsBlank()
                )
                {
                    break;
                }

                if (
                    !string.IsNullOrEmpty(
                        ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.AttributeNameColumn}_{styleValueProductsIndex}"
                        )
                    )
                    && !string.IsNullOrEmpty(
                        ParseColumnValue(dataRow, $"{ADDataFeedSourceFile.CategoryCodeColumn}")
                    )
                )
                {
                    catDataTable.CaseSensitive = false;
                    var dr = catDataTable
                        .Select(
                            $"NumericCode = '{ParseColumnValue(dataRow, $"{ADDataFeedSourceFile.CategoryCodeColumn}")}'"
                                + $" and AttributeName ='{ParseColumnValue(dataRow, $"{ADDataFeedSourceFile.AttributeNameColumn}_{styleValueProductsIndex}")}'"
                        )
                        .FirstOrDefault();
                    if (dr != null)
                    {
                        var styleClass = ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.CategoryCodeColumn}"
                        );
                        var styleTrait = ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.AttributeNameColumn}_{styleValueProductsIndex}"
                        );
                        var styleTraitValue = ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.AttributeValueUomColumn}_{styleValueProductsIndex}"
                        );
                        var myPartNumber = ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.ProductNameColumn}"
                        );
                        if (styleTrait.IsBlank())
                        {
                            break;
                        }

                        if (
                            !styleValues.Any(
                                s =>
                                    s.styleTrait.Equals(
                                        styleTrait,
                                        StringComparison.InvariantCultureIgnoreCase
                                    )
                                    && s.styleTraitValue.Equals(
                                        styleTraitValue,
                                        StringComparison.InvariantCultureIgnoreCase
                                    )
                                    && s.styleClass.Equals(
                                        styleClass,
                                        StringComparison.InvariantCultureIgnoreCase
                                    )
                            )
                        )
                        {
                            styleValues.Add((styleClass, styleTrait, styleTraitValue));
                        }
                    }
                }
            }
        }

        return styleValues;
    }

    private static List<(
        string styleClass,
        string styleTrait,
        string styleTraitValue,
        string myPartNumber
    )> GetStyleValueProductsForDataRow(
        DataRow dataRow,
        DataSet catDataSet,
        List<(
            string styleClass,
            string styleTrait,
            string styleTraitValue,
            string myPartNumber
        )> styleValueProducts
    )
    {
        var catDataTable = catDataSet.Tables[0];
        if (catDataTable != null)
        {
            for (var styleValueProductsIndex = 1; true; styleValueProductsIndex++)
            {
                if (
                    ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.AttributeValueUomColumn}_{styleValueProductsIndex}"
                        )
                        .IsBlank()
                )
                {
                    break;
                }

                if (
                    !string.IsNullOrEmpty(
                        ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.AttributeNameColumn}_{styleValueProductsIndex}"
                        )
                    )
                    && !string.IsNullOrEmpty(
                        ParseColumnValue(dataRow, $"{ADDataFeedSourceFile.CategoryCodeColumn}")
                    )
                )
                {
                    catDataTable.CaseSensitive = false;
                    var dr = catDataTable
                        .Select(
                            $"NumericCode = '{ParseColumnValue(dataRow, $"{ADDataFeedSourceFile.CategoryCodeColumn}")}'"
                                + $" and AttributeName ='{ParseColumnValue(dataRow, $"{ADDataFeedSourceFile.AttributeNameColumn}_{styleValueProductsIndex}")}'"
                        )
                        .FirstOrDefault();
                    if (dr != null)
                    {
                        var styleClass = ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.CategoryCodeColumn}"
                        );
                        var styleTrait = ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.AttributeNameColumn}_{styleValueProductsIndex}"
                        );
                        var styleTraitValue = ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.AttributeValueUomColumn}_{styleValueProductsIndex}"
                        );
                        var myPartNumber = ParseColumnValue(
                            dataRow,
                            $"{ADDataFeedSourceFile.MyPartNumberColumn}"
                        );
                        if (styleTrait.IsBlank())
                        {
                            break;
                        }

                        if (
                            !styleValueProducts.Any(
                                s =>
                                    s.styleTrait.Equals(
                                        styleTrait,
                                        StringComparison.InvariantCultureIgnoreCase
                                    )
                                    && s.styleTraitValue.Equals(
                                        styleTraitValue,
                                        StringComparison.InvariantCultureIgnoreCase
                                    )
                                    && s.styleClass.Equals(
                                        styleClass,
                                        StringComparison.InvariantCultureIgnoreCase
                                    )
                                    && s.styleClass.Equals(
                                        myPartNumber,
                                        StringComparison.InvariantCultureIgnoreCase
                                    )
                            )
                        )
                        {
                            styleValueProducts.Add(
                                (styleClass, styleTrait, styleTraitValue, myPartNumber)
                            );
                        }
                    }
                }
            }
        }

        return styleValueProducts;
    }

    private static List<(string attributeType, string attributeValue)> GetAttributesForDataRow(
        DataRow dataRow,
        string[] excludedAttributes
    )
    {
        var attributes = new List<(string attributeType, string attributeValue)>();

        for (var attributeIndex = 1; true; attributeIndex++)
        {
            var attributeType = ParseColumnValue(
                dataRow,
                $"{ADDataFeedSourceFile.AttributeNameColumn}_{attributeIndex}"
            );
            var attributeValue =
                ParseColumnValue(
                    dataRow,
                    $"{ADDataFeedSourceFile.AttributeValueColumn}_{attributeIndex}"
                )
                ?? ParseColumnValue(
                    dataRow,
                    $"{ADDataFeedSourceFile.AttributeValueUomColumn}_{attributeIndex}"
                );
            if (attributeType.IsBlank())
            {
                break;
            }

            if (
                attributeValue.IsBlank()
                || excludedAttributes.Contains(attributeType, StringComparer.OrdinalIgnoreCase)
            )
            {
                continue;
            }

            attributes.Add((attributeType, attributeValue));
        }

        var countryOfOrigin = ParseColumnValue(dataRow, ADDataFeedSourceFile.CountryOfOriginColumn);
        var manufacturerName = ParseColumnValue(
            dataRow,
            ADDataFeedSourceFile.ManufacturerNameColumn
        );

        if (!countryOfOrigin.IsBlank())
        {
            attributes.Add(("Country of Origin", countryOfOrigin));
        }

        if (!manufacturerName.IsBlank())
        {
            attributes.Add(("Manufacturer Name", manufacturerName));
        }

        return attributes;
    }

    private static string GetImagePath(string imageFileName)
    {
        if (
            imageFileName.IsBlank()
            || imageFileName.StartsWith(HttpPrefix, StringComparison.OrdinalIgnoreCase)
            || imageFileName.StartsWith(HttpsPrefix, StringComparison.OrdinalIgnoreCase)
        )
        {
            return imageFileName;
        }

        var imageSubFolderName = imageFileName.Substring(0, 1).ToLower();

        return $"/userfiles/images/products/{imageSubFolderName}/{imageFileName}";
    }

    private static string GetEnterworksSmallMediumImagePath(
        string imageFileName,
        string subDirectory
    )
    {
        if (
            imageFileName.IsBlank()
            || imageFileName.StartsWith(HttpPrefix, StringComparison.OrdinalIgnoreCase)
            || imageFileName.StartsWith(HttpsPrefix, StringComparison.OrdinalIgnoreCase)
        )
        {
            var urlExt = Path.GetExtension(imageFileName).ToLower();
            if (urlExt != JpgSuffix)
            {
                imageFileName = Path.ChangeExtension(imageFileName, JpgSuffix);
            }

            return imageFileName;
        }
        else if (!imageFileName.EndsWith(JpgSuffix, StringComparison.OrdinalIgnoreCase))
        {
            FileInfo fileInfo = new(imageFileName);
            imageFileName = fileInfo.Name.Replace(fileInfo.Extension, JpgSuffix);
            return $"/userfiles/{subDirectory}/{imageFileName}";
        }

        return $"/userfiles/{subDirectory}/{imageFileName}";
    }

    private static string GetEnterworksImagePath(string imageFileName, string subDirectory)
    {
        if (
            imageFileName.IsBlank()
            || imageFileName.StartsWith(HttpPrefix, StringComparison.OrdinalIgnoreCase)
            || imageFileName.StartsWith(HttpsPrefix, StringComparison.OrdinalIgnoreCase)
        )
        {
            return imageFileName;
        }

        return $"/userfiles/{subDirectory}/{imageFileName}";
    }

    private static string GetDocumentPath(string documentFileName, string subDirectory)
    {
        if (
            documentFileName.IsBlank()
            || documentFileName.StartsWith(HttpPrefix, StringComparison.OrdinalIgnoreCase)
            || documentFileName.StartsWith(HttpsPrefix, StringComparison.OrdinalIgnoreCase)
        )
        {
            return documentFileName;
        }

        return $"/userfiles/{subDirectory}/{documentFileName}";
    }

    private static string GetWebsiteName(IntegrationJob integrationJob)
    {
        return GetParameterValue("Website", integrationJob);
    }

    private static string GetLanguageCode(IntegrationJob integrationJob)
    {
        return GetParameterValue("LanguageCode", integrationJob);
    }

    private static string GetExcludedAttributes(IntegrationJob integrationJob)
    {
        return GetParameterValue("ExcludedAttributes", integrationJob);
    }

    private static string GetBaseLanguageFileName(IntegrationJob integrationJob)
    {
        return GetParameterValue("BaseLanguageFileName", integrationJob);
    }

    private static string GetParameterValue(string parameterName, IntegrationJob integrationJob)
    {
        var parameter = integrationJob.IntegrationJobParameters.FirstOrDefault(
            o =>
                o.JobDefinitionParameter != null
                && o.JobDefinitionParameter.Name.Equals(
                    parameterName,
                    StringComparison.OrdinalIgnoreCase
                )
        );

        return parameter != null
            ? !string.IsNullOrEmpty(parameter.Value)
                ? parameter.Value
                : parameter.JobDefinitionParameter.DefaultValue
            : string.Empty;
    }

    private static string ParseColumnValue(DataRow dataRow, string columnName)
    {
        var columnNameLowered = columnName.ToLowerInvariant();
        var columnNameWithoutUnderscores = columnName.Replace('_', ' ');
        var columnNameWithoutUnderscoresLowered = columnNameWithoutUnderscores.ToLowerInvariant();
        var columnNameWithoutUnderscoresAndSpaces = columnName.Replace("_", string.Empty);
        var columnNameWithoutUnderscoresAndSpacesLowered = columnName
            .Replace('_', ' ')
            .ToLowerInvariant();
        return dataRow.Table.Columns.Contains(columnName)
            ? dataRow[columnName].ToString()
            : dataRow.Table.Columns.Contains(columnNameWithoutUnderscores)
                ? dataRow[columnNameWithoutUnderscores].ToString()
                : dataRow.Table.Columns.Contains(columnNameLowered)
                    ? dataRow[columnNameLowered].ToString()
                    : dataRow.Table.Columns.Contains(columnNameWithoutUnderscoresLowered)
                        ? dataRow[columnNameWithoutUnderscoresLowered].ToString()
                        : dataRow.Table.Columns.Contains(
                            columnNameWithoutUnderscoresAndSpacesLowered
                        )
                            ? dataRow[columnNameWithoutUnderscoresAndSpacesLowered].ToString()
                            : dataRow.Table.Columns.Contains(columnNameWithoutUnderscoresAndSpaces)
                                ? dataRow[columnNameWithoutUnderscoresAndSpaces].ToString()
                                : null;
    }

    private static string AggregateFields(
        DataRow row,
        IEnumerable<string> aggregationKeys,
        string separator = ","
    )
    {
        var aggregatedValues = new List<string>();
        foreach (var valueKey in aggregationKeys)
        {
            var value = row[valueKey];
            if (value != null && value.ToString().IsNotBlank())
            {
                aggregatedValues.Add(value.ToString());
            }
        }

        return string.Join(separator, aggregatedValues);
    }
}
