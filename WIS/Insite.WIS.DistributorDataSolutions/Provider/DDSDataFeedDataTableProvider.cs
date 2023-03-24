namespace Insite.WIS.DistributorDataSolutions.Provider;

using Insite.WIS.DistributorDataSolutions.Resources;
using System.Data;

public static class DDSDataFeedDataTableProvider
{
    public static DataTable CreateCategoryDataTable()
    {
        var dataTable = new DataTable("2Category");

        dataTable.Columns.Add("WebsiteCategoryName");
        dataTable.Columns.Add("UrlSegment");
        dataTable.Columns.Add("ShortDescription");

        dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["WebsiteCategoryName"] };

        return dataTable;
    }

    public static DataTable CreateCategoryProductDataTable()
    {
        var dataTable = new DataTable("3CategoryProduct");

        dataTable.Columns.Add("WebsiteCategoryName");
        dataTable.Columns.Add(DDSDataFeedProductSourceFile.DistributorProductIdColumn);

        return dataTable;
    }

    public static DataTable CreateAttributeTypeDataTable()
    {
        var dataTable = new DataTable("4AttributeType");

        dataTable.Columns.Add("Name");

        return dataTable;
    }

    public static DataTable CreateAttributeValueDataTable()
    {
        var dataTable = new DataTable("5AttributeValue");

        dataTable.Columns.Add(DDSDataFeedProductSourceFile.DistributorProductIdColumn);
        dataTable.Columns.Add(DDSDataFeedProductSourceFile.NameColumn);
        dataTable.Columns.Add(DDSDataFeedProductSourceFile.ValueColumn);
        dataTable.Columns.Add("SortOrder", typeof(int));

        return dataTable;
    }

    public static DataTable CreateProductImageDataTable()
    {
        var dataTable = new DataTable("6ProductImage");

        dataTable.Columns.Add(DDSDataFeedProductSourceFile.DistributorProductIdColumn);
        dataTable.Columns.Add(DDSDataFeedProductSourceFile.UriThumbColumn);
        dataTable.Columns.Add(DDSDataFeedProductSourceFile.UriSmallColumn);
        dataTable.Columns.Add(DDSDataFeedProductSourceFile.UriMediumColumn);
        dataTable.Columns.Add(DDSDataFeedProductSourceFile.UriLargeColumn);
        dataTable.Columns.Add(DDSDataFeedProductSourceFile.NameColumn);
        dataTable.Columns.Add("SortOrder", typeof(int));

        return dataTable;
    }
}
