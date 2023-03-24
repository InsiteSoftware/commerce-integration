namespace Insite.WIS.AffiliatedDistributors.Providers;

using System.Data;
using Insite.WIS.AffiliatedDistributors.Constants;

public static class ADDataFeedDataTableProvider
{
    public static DataTable CreateStyleClassDataTable()
    {
        var dataTable = new DataTable("1StyleClass");

        dataTable.Columns.Add(ADDataFeedSourceFile.NumericCodeColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.NodeNameColumn);

        return dataTable;
    }

    public static DataTable CreateStyleTraitDataTable()
    {
        var dataTable = new DataTable("2StyleTrait");

        dataTable.Columns.Add(ADDataFeedSourceFile.NumericCodeColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.CategoryAttributeNameColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.DisplaySequenceColumn);

        return dataTable;
    }

    public static DataTable CreateStyleTraitValueDataTable()
    {
        var dataTable = new DataTable("3StyleTraitValue");

        dataTable.Columns.Add(ADDataFeedSourceFile.StyleClassColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.StyleTraitColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.StyleTraitValueColumn);

        return dataTable;
    }

    public static DataTable CreateStyleTraitValueProductDataTable()
    {
        var dataTable = new DataTable("5StyleTraitValueProduct");

        dataTable.Columns.Add(ADDataFeedSourceFile.StyleClassColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.StyleTraitColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.StyleTraitValueColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.MyPartNumberColumn);

        return dataTable;
    }

    public static DataTable CreateAttributeTypeDataTable()
    {
        var dataTable = new DataTable("6AttributeType");

        dataTable.Columns.Add(ADDataFeedSourceFile.AttributeNameColumn);

        return dataTable;
    }

    public static DataTable CreateAttributeValueDataTable()
    {
        var dataTable = new DataTable("7AttributeValue");

        dataTable.Columns.Add(ADDataFeedSourceFile.AttributeNameColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.AttributeValueColumn);
        dataTable.Columns.Add("SortOrder", typeof(int));

        return dataTable;
    }

    public static DataTable CreateTranslationDictionaryDataTable(string name)
    {
        var dataTable = new DataTable(name);

        dataTable.Columns.Add("Index");
        dataTable.Columns.Add("Source");
        dataTable.Columns.Add("Keyword");
        dataTable.Columns.Add("Translation");
        dataTable.Columns.Add(ADDataFeedSourceFile.LanguageCode);

        return dataTable;
    }

    public static DataTable CreateProductAttributeValueDataTable()
    {
        var dataTable = new DataTable("8ProductAttributeValue");

        dataTable.Columns.Add(ADDataFeedSourceFile.MyPartNumberColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.AttributeNameColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.AttributeValueColumn);

        return dataTable;
    }

    public static DataTable CreateSpecificationDataTable()
    {
        var dataTable = new DataTable("9Specification");

        dataTable.Columns.Add(ADDataFeedSourceFile.MyPartNumberColumn);
        dataTable.Columns.Add("Name");
        dataTable.Columns.Add("Content");
        dataTable.Columns.Add(ADDataFeedSourceFile.LanguageCode);

        return dataTable;
    }

    public static DataTable CreateProductImageDataTable()
    {
        var dataTable = new DataTable("10ProductImage");

        dataTable.Columns.Add(ADDataFeedSourceFile.MyPartNumberColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.ItemImageItemImageColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.ItemImageDetailImageColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.ItemImageEnlargeImageColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.ItemImageCaptionColumn);
        dataTable.Columns.Add("SortOrder", typeof(int));

        return dataTable;
    }

    public static DataTable CreateEnterworksProductImageDataTable()
    {
        var dataTable = new DataTable("10ProductImage");

        dataTable.Columns.Add(ADDataFeedSourceFile.MyPartNumberColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.ItemSmallImageFilenameColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.ItemMediumImageFilenameColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.ItemLargeImageFilenameColumn);
        dataTable.Columns.Add("SortOrder", typeof(int));

        return dataTable;
    }

    public static DataTable CreateDocumentDataTable()
    {
        var dataTable = new DataTable("11Document");

        dataTable.Columns.Add(ADDataFeedSourceFile.MyPartNumberColumn);
        dataTable.Columns.Add("ParentTable");
        dataTable.Columns.Add(ADDataFeedSourceFile.ItemDocumentNameColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.ItemDocumentTypeColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.DocCaptionColumn);

        return dataTable;
    }

    public static DataTable CreateCategoryDataTable()
    {
        var dataTable = new DataTable("12Category");

        dataTable.Columns.Add("WebsiteCategoryName");
        dataTable.Columns.Add("UrlSegment");
        dataTable.Columns.Add("ShortDescription");

        return dataTable;
    }

    public static DataTable CreateCategoryProductDataTable()
    {
        var dataTable = new DataTable("13CategoryProduct");

        dataTable.Columns.Add("WebsiteCategoryName");
        dataTable.Columns.Add(ADDataFeedSourceFile.MyPartNumberColumn);

        return dataTable;
    }

    public static DataTable CreateProductThreeSixtyImageDataTable()
    {
        var dataTable = new DataTable("14ProductImage");

        dataTable.Columns.Add(ADDataFeedSourceFile.MyPartNumberColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.ThreeSixtyImageColumn);

        return dataTable;
    }

    public static DataTable CreateCategoryAttributeTypeDataTable()
    {
        var dataTable = new DataTable("15CategoryAttributeType");

        dataTable.Columns.Add("WebsiteCategoryName");
        dataTable.Columns.Add(ADDataFeedSourceFile.AttributeNameColumn);

        return dataTable;
    }

    public static DataTable CreateCategoryImageDataTable()
    {
        var dataTable = new DataTable("16Category");

        dataTable.Columns.Add("WebsiteCategoryName");
        dataTable.Columns.Add(ADDataFeedSourceFile.CategoryImageName);

        return dataTable;
    }

    public static DataTable CreateBrandDataTable()
    {
        var dataTable = new DataTable("17Brand");

        dataTable.Columns.Add(ADDataFeedSourceFile.BrandNameColumn);
        dataTable.Columns.Add(ADDataFeedSourceFile.MyPartNumberColumn);

        return dataTable;
    }
}
