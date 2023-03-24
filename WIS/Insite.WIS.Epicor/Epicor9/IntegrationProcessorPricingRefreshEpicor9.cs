namespace Insite.WIS.Epicor.Epicor9;

using System;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

using Insite.Common.Helpers;
using Insite.Integration.Enums;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.Plugins.Constants;
using Insite.WIS.Broker.WebIntegrationService;

/// <summary>
/// Epicor 9 (Sql Server and Progress) Implementation of Pricing Refresh. Ultimately queries the PriceLst/PriceLstParts/CustomerPriceLst/CustomerGroupPriceLst tables
/// on the database and builds a dataset in the exact format of the ISC price matrix table.
/// </summary>
public class IntegrationProcessorPricingRefreshEpicor9
    : IntegrationProcessorPricingRefreshBase,
        IIntegrationProcessor
{
    /// <summary>Database schema name used by the ERP when using SQL Server.</summary>
    public virtual string ErpSqlSchemaName { get; set; }

    /// <summary>Executes the job associated with the passed in <see cref="IntegrationJob"/> and <see cref="JobDefinitionStep"/>.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <returns>The results of the passed in  <see cref="IntegrationJob"/> and <see cref="JobDefinitionStep"/></returns>
    public override DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep
    )
    {
        var priceMatrixDataTable = this.BuildPriceMatrixDataTable(jobStep.Sequence);
        var integrationProcessorPricingRefreshEpicor9Info = this.PopulatePricingRefreshParameters(
            siteConnection,
            integrationJob,
            jobStep
        );
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        var connectionString =
            jobStep.IntegrationConnectionOverride != null
                ? jobStep.IntegrationConnectionOverride.ConnectionString
                : integrationJob.JobDefinition.IntegrationConnection.ConnectionString;

        Func<string, string, DataTable> executeSql = (dataTableName, sql) =>
        {
            var ds = new DataSet();

            // build connection in either Sql or Odbc (Progress) styley
            IDbConnection conn;
            if (
                integrationProcessorPricingRefreshEpicor9Info.DbType.Equals(
                    "Sql",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                conn = new SqlConnection(connectionString);
            }
            else
            {
                conn = new OdbcConnection(connectionString);
            }

            using (conn)
            {
                conn.Open();
                siteConnection.AddLogMessage(
                    integrationJob.Id.ToString(),
                    IntegrationJobLogType.Info,
                    "Executing " + dataTableName + " Sql: " + sql
                );
                IDbDataAdapter adapter;
                if (
                    integrationProcessorPricingRefreshEpicor9Info.DbType.Equals(
                        "Sql",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
#pragma warning disable CA2100
                    adapter = new SqlDataAdapter(sql, conn as SqlConnection);
#pragma warning restore CA2100
                }
                else
                {
#pragma warning disable CA2100
                    adapter = new OdbcDataAdapter(sql, conn as OdbcConnection);
#pragma warning restore CA2100
                }

                adapter.Fill(ds);
            }

            return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
        };

        // Customer Price List
        foreach (
            DataRow customerPriceListRow in executeSql(
                "CustomerPriceList",
                this.GetCustomerPriceListSql(integrationProcessorPricingRefreshEpicor9Info)
            ).Rows
        )
        {
            // skip the row if we are unable to resolve the customer number, basically unable to create a valid customer key part
            var customerKeyPartColumn = this.GetCustomerId(
                customerPriceListRow["CustNum"].ToString().Trim(),
                customerPriceListRow["ShipToNum"].ToString().Trim(),
                initialDataset
            ); // need shipto also
            if (string.IsNullOrEmpty(customerKeyPartColumn))
            {
                continue;
            }

            var dataRow = priceMatrixDataTable.NewRow();
            dataRow[Data.RecordTypeColumn] = customerPriceListRow[Data.RecordTypeColumn].ToString();
            dataRow[Data.CurrencyCodeColumn] = string.Empty;
            dataRow[Data.WarehouseColumn] = string.Empty;
            dataRow[Data.UnitOfMeasureColumn] = string.Empty;
            dataRow[Data.CustomerKeyPartColumn] = customerKeyPartColumn;
            dataRow[Data.ProductKeyPartColumn] = customerPriceListRow[
                Data.ProductKeyPartColumn
            ].ToString();
            dataRow[Data.ActivateOnColumn] = DateTime.UtcNow;
            dataRow[Data.DeactivateOnColumn] = new DateTime(2059, 12, 31);
            priceMatrixDataTable.Rows.Add(dataRow);
        }

        // Customer Group Price List
        foreach (
            DataRow customerGroupPriceListRow in executeSql(
                "CustomerGroupPriceList",
                this.GetCustomerGroupPriceListSql(integrationProcessorPricingRefreshEpicor9Info)
            ).Rows
        )
        {
            var dataRow = priceMatrixDataTable.NewRow();
            dataRow[Data.RecordTypeColumn] = customerGroupPriceListRow[
                Data.RecordTypeColumn
            ].ToString();
            dataRow[Data.CurrencyCodeColumn] = string.Empty;
            dataRow[Data.WarehouseColumn] = string.Empty;
            dataRow[Data.UnitOfMeasureColumn] = string.Empty;
            dataRow[Data.CustomerKeyPartColumn] = customerGroupPriceListRow[
                Data.CustomerKeyPartColumn
            ].ToString();
            dataRow[Data.ProductKeyPartColumn] = customerGroupPriceListRow[
                Data.ProductKeyPartColumn
            ].ToString();
            dataRow[Data.ActivateOnColumn] = DateTime.UtcNow;
            dataRow[Data.DeactivateOnColumn] = new DateTime(2059, 12, 31);
            dataRow[Data.PriceBasis01Column] = customerGroupPriceListRow["PriceBasis"].ToString();
            priceMatrixDataTable.Rows.Add(dataRow);
        }

        // Price List Product
        // Price List Product Group
        var workingKey = string.Empty;
        var workingKeyCounter = 1;
        foreach (
            DataRow productPriceListRow in executeSql(
                "ProductPriceList",
                this.GetProductPriceListSql(integrationProcessorPricingRefreshEpicor9Info)
            ).Rows
        )
        {
            if (
                !DateTime.TryParse(
                    productPriceListRow[Data.ActivateOnColumn].ToString(),
                    out var parseActive
                )
            )
            {
                parseActive = DateTime.UtcNow;
            }

            if (
                !DateTime.TryParse(
                    productPriceListRow[Data.DeactivateOnColumn].ToString(),
                    out var parseDeactivate
                )
            )
            {
                parseDeactivate = new DateTime(2059, 12, 31);
            }

            decimal.TryParse(productPriceListRow["BreakQty"].ToString(), out var parseBreakQty);

            // skip the row if we are unable to resolve the product part number, basically unable to create a valid product key part
            var productKeyPartColumn = productPriceListRow[Data.RecordTypeColumn]
                .ToString()
                .Equals("Price List Product", StringComparison.OrdinalIgnoreCase)
                ? this.GetProductId(productPriceListRow["PartNum"].ToString(), initialDataset)
                : productPriceListRow[Data.ProductKeyPartColumn].ToString();
            if (string.IsNullOrEmpty(productKeyPartColumn))
            {
                continue;
            }

            // add/update datarow with price matrix data
            var currentKey = string.Format(
                "{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                productPriceListRow[Data.RecordTypeColumn],
                productPriceListRow[Data.CurrencyCodeColumn],
                integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse,
                productPriceListRow[Data.UnitOfMeasureColumn],
                productPriceListRow[Data.CustomerKeyPartColumn],
                productKeyPartColumn,
                parseActive
            );
            if (!workingKey.Equals(currentKey, StringComparison.OrdinalIgnoreCase))
            {
                workingKey = currentKey;
                workingKeyCounter = 1;

                // add the new row to the table
                var dataRow = priceMatrixDataTable.NewRow();
                dataRow[Data.RecordTypeColumn] = productPriceListRow[
                    Data.RecordTypeColumn
                ].ToString();
                dataRow[Data.CurrencyCodeColumn] = productPriceListRow[
                    Data.CurrencyCodeColumn
                ].ToString();
                dataRow[Data.WarehouseColumn] =
                    integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse;
                dataRow[Data.UnitOfMeasureColumn] = productPriceListRow[
                    Data.UnitOfMeasureColumn
                ].ToString();
                dataRow[Data.CustomerKeyPartColumn] = productPriceListRow[
                    Data.CustomerKeyPartColumn
                ].ToString();
                dataRow[Data.ProductKeyPartColumn] = productKeyPartColumn;
                dataRow[Data.ActivateOnColumn] = parseActive;
                dataRow[Data.DeactivateOnColumn] = parseDeactivate;
                dataRow[Data.PriceBasis01Column] = productPriceListRow["PriceBasis"].ToString();
                dataRow[Data.AdjustmentType01Column] = productPriceListRow[
                    "AdjustmentType"
                ].ToString();
                dataRow[Data.BreakQty01Column] = productPriceListRow["BreakQty"].ToString();
                dataRow[Data.Amount01Column] = productPriceListRow["Amount"].ToString();
                dataRow[Data.AltAmount01Column] = productPriceListRow["AltAmount"].ToString();
                priceMatrixDataTable.Rows.Add(dataRow);
            }
            else
            {
                // update quantity break pricing for existing row
                var dataRow = priceMatrixDataTable
                    .AsEnumerable()
                    .FirstOrDefault(
                        x =>
                            x.Field<string>("RecordType")
                                .Equals(
                                    productPriceListRow[Data.RecordTypeColumn].ToString(),
                                    StringComparison.OrdinalIgnoreCase
                                )
                            && x.Field<string>("CurrencyCode")
                                .Equals(
                                    productPriceListRow[Data.CurrencyCodeColumn].ToString(),
                                    StringComparison.OrdinalIgnoreCase
                                )
                            && x.Field<string>("Warehouse")
                                .Equals(
                                    integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            && x.Field<string>("UnitOfMeasure")
                                .Equals(
                                    productPriceListRow[Data.UnitOfMeasureColumn].ToString(),
                                    StringComparison.OrdinalIgnoreCase
                                )
                            && x.Field<string>("CustomerKeyPart")
                                .Equals(
                                    productPriceListRow[Data.CustomerKeyPartColumn].ToString(),
                                    StringComparison.OrdinalIgnoreCase
                                )
                            && x.Field<string>("ProductKeyPart")
                                .Equals(productKeyPartColumn, StringComparison.OrdinalIgnoreCase)
                            && x.Field<DateTime>("Active").Equals(parseActive)
                    );
                if (dataRow != null)
                {
                    workingKeyCounter++;
                    var formatWorkingKeyCounter = workingKeyCounter
                        .ToString(CultureInfo.InvariantCulture)
                        .PadLeft(2, '0');
                    dataRow["PriceBasis" + formatWorkingKeyCounter] = productPriceListRow[
                        "PriceBasis"
                    ].ToString();
                    dataRow["AdjustmentType" + formatWorkingKeyCounter] = productPriceListRow[
                        "AdjustmentType"
                    ].ToString();
                    dataRow["BreakQty" + formatWorkingKeyCounter] = productPriceListRow[
                        "BreakQty"
                    ].ToString();
                    dataRow["Amount" + formatWorkingKeyCounter] = productPriceListRow[
                        "Amount"
                    ].ToString();
                    dataRow["AltAmount" + formatWorkingKeyCounter] = productPriceListRow[
                        "AltAmount"
                    ].ToString();
                }
            }
        }

        // make sure all 11 instances of each field has a value.  If they are null, the fieldmap post processor will have
        // problems with them.
        foreach (DataRow dataRow in priceMatrixDataTable.Rows)
        {
            dataRow[Data.CalculationFlagsColumn] = string.Empty;
            for (var i = 1; i <= 11; i++)
            {
                var index = i.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
                if (string.IsNullOrEmpty(dataRow["Amount" + index].ToString()))
                {
                    dataRow["Amount" + index] = 0;
                }

                if (string.IsNullOrEmpty(dataRow["AltAmount" + index].ToString()))
                {
                    dataRow["AltAmount" + index] = 0;
                }

                if (string.IsNullOrEmpty(dataRow["PriceBasis" + index].ToString()))
                {
                    dataRow["PriceBasis" + index] = string.Empty;
                }

                if (string.IsNullOrEmpty(dataRow["AdjustmentType" + index].ToString()))
                {
                    dataRow["AdjustmentType" + index] = string.Empty;
                }

                if (string.IsNullOrEmpty(dataRow["BreakQty" + index].ToString()))
                {
                    dataRow["BreakQty" + index] = 0;
                }
            }
        }

        var returnDataSet = new DataSet();
        returnDataSet.Tables.Add(priceMatrixDataTable);
        return returnDataSet;
    }

    /// <summary>Retrieves the ERP_Company and ERP_Warehouse parameters defined in the jobStep.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <returns>The <see cref="IntegrationProcessorPricingRefreshEpicor9Info"/> with all of the parameters that are relevent to the pricing refresh
    /// contained within.</returns>
    protected virtual IntegrationProcessorPricingRefreshEpicor9Info PopulatePricingRefreshParameters(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep
    )
    {
        var parmCompanyNumber = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.ErpCompany, StringComparison.OrdinalIgnoreCase)
        );
        if (parmCompanyNumber == null)
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Info,
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpCompany)
            );
        }

        var parmCustomerDefaultWarehouse = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.ErpWarehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (parmCustomerDefaultWarehouse == null)
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Info,
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpWarehouse)
            );
        }

        var parmDbType = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.ErpEpicor9DbType, StringComparison.OrdinalIgnoreCase)
        );
        if (parmDbType == null)
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Info,
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpEpicor9DbType)
            );
        }

        return new IntegrationProcessorPricingRefreshEpicor9Info
        {
            CompanyNumber = (parmCompanyNumber == null) ? string.Empty : parmCompanyNumber.Value,
            CustomerDefaultWarehouse =
                (parmCustomerDefaultWarehouse == null)
                    ? string.Empty
                    : parmCustomerDefaultWarehouse.Value,
            DbType = (parmDbType == null) ? string.Empty : parmDbType.Value
        };
    }

    /// <summary>Builds the pricing sql call to query the Epicor 9 database with.</summary>
    /// <param name="integrationProcessorPricingRefreshEpicor9Info">The parameter information related to the call.</param>
    /// <returns>The pricing sql call to query the Epicor 9 database with</returns>
    protected virtual string GetCustomerPriceListSql(
        IntegrationProcessorPricingRefreshEpicor9Info integrationProcessorPricingRefreshEpicor9Info
    )
    {
        var sql =
            @"
                SELECT
                  'Customer Price List' as RecordType,
                  cpl.CustNum ,
                  cpl.ShipToNum,
                  CAST(cpl.SeqNum AS NVARCHAR(50)) as ProductKeyPart,
                  cpl.ListCode as PriceBasis
                FROM {TablePrefix}CustomerPriceLst cpl (NOLOCK)
                JOIN {TablePrefix}PriceLst pl (NOLOCK)
                  on pl.Company = cpl.Company
                  and pl.ListCode = cpl.ListCode
                WHERE
                  pl.Company = '"
            + integrationProcessorPricingRefreshEpicor9Info.CompanyNumber
            + @"' and
                  pl.StartDate <= GETDATE() and
                  (pl.EndDate Is Null OR pl.EndDate >= GETDATE()) and
                  (pl.WarehouseList = '' or pl.WarehouseList like '%"
            + integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse
            + @"%')";

        sql = integrationProcessorPricingRefreshEpicor9Info.DbType.Equals(
            "Sql",
            StringComparison.OrdinalIgnoreCase
        )
            ? sql.Replace("{TablePrefix}", this.ErpSqlSchemaName)
            : sql.Replace("{TablePrefix}", "PUB.")
                .Replace("(NOLOCK)", string.Empty)
                .Replace("ISNULL", "IFNULL")
                .Replace("LEN(", "LENGTH(")
                .Replace("NVARCHAR", "VARCHAR")
                .Replace("GETDATE()", "NOW()");

        return sql;
    }

    /// <summary>Builds the pricing sql call to query the Epicor 9 database with.</summary>
    /// <param name="integrationProcessorPricingRefreshEpicor9Info">The parameter information related to the call.</param>
    /// <returns>The pricing sql call to query the Epicor 9 database with</returns>
    protected virtual string GetCustomerGroupPriceListSql(
        IntegrationProcessorPricingRefreshEpicor9Info integrationProcessorPricingRefreshEpicor9Info
    )
    {
        var sql =
            @"
                SELECT
                  'Customer Group Price List' as RecordType,
                  cgpl.GroupCode as CustomerKeyPart,
                  CAST(cgpl.SeqNum AS NVARCHAR(50)) as ProductKeyPart,
                  cgpl.ListCode as PriceBasis
                FROM {TablePrefix}CustGrupPriceLst cgpl (NOLOCK)
                JOIN {TablePrefix}PriceLst pl (NOLOCK)
                  on pl.Company = cgpl.Company
                  and pl.ListCode = cgpl.ListCode
                WHERE
                  pl.Company = '"
            + integrationProcessorPricingRefreshEpicor9Info.CompanyNumber
            + @"' and
                  pl.StartDate <= GETDATE() and
                  (pl.EndDate Is Null OR pl.EndDate >= GETDATE()) and
                  (pl.WarehouseList = '' or pl.WarehouseList like '%"
            + integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse
            + @"%')
                ";

        sql = integrationProcessorPricingRefreshEpicor9Info.DbType.Equals(
            "Sql",
            StringComparison.OrdinalIgnoreCase
        )
            ? sql.Replace("{TablePrefix}", this.ErpSqlSchemaName)
            : sql.Replace("{TablePrefix}", "PUB.")
                .Replace("(NOLOCK)", string.Empty)
                .Replace("ISNULL", "IFNULL")
                .Replace("LEN(", "LENGTH(")
                .Replace("NVARCHAR", "VARCHAR")
                .Replace("GETDATE()", "NOW()");

        return sql;
    }

    /// <summary>Builds the pricing sql call to query the Epicor 9 database with.</summary>
    /// <param name="integrationProcessorPricingRefreshEpicor9Info">The parameter information related to the call.</param>
    /// <returns>The pricing sql call to query the Epicor 9 database with</returns>
    protected virtual string GetProductPriceListSql(
        IntegrationProcessorPricingRefreshEpicor9Info integrationProcessorPricingRefreshEpicor9Info
    )
    {
        var sql =
            @"SELECT * FROM (
                SELECT
                  'Price List Product' as RecordType,
                  plp.ListCode as CustomerKeyPart,
                  '' as ProductKeyPart,
                  plp.PartNum as PartNum,
                  pl.CurrencyCode,
                  '"
            + integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse
            + @"' as Warehouse,
                  plp.UOMCode as UnitOfMeasure,
                  pl.StartDate as Active,
                  pl.EndDate as Deactivate,
                  'O' as PriceBasis,
                  'A' as AdjustmentType,
                  CAST(0 AS DECIMAL(18,5)) as BreakQty,
                  CAST(0 AS DECIMAL(18,5)) as Amount,
                  plp.BasePrice as AltAmount
                FROM {TablePrefix}pricelst pl (NOLOCK)
                  JOIN {TablePrefix}pricelstparts plp (NOLOCK) on
                    pl.Company = plp.Company and pl.Listcode = plp.ListCode
                WHERE PL.Company = '"
            + integrationProcessorPricingRefreshEpicor9Info.CompanyNumber
            + @"' and
                  (WarehouseList = '' or WarehouseList like '%"
            + integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse
            + @"%') and
                  pl.startdate <= GETDATE() and
                  (pl.enddate is null or pl.enddate >= GETDATE())

                UNION

                SELECT
                  'Price List Product' as RecordType,
                  plb.ListCode as CustomerKeyPart,
                  '' as ProductKeyPart,
                  plp.PartNum as PartNum,
                  pl.CurrencyCode,
                  '"
            + integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse
            + @"' as Warehouse,
                  plb.UOMCode as UnitOfMeasure,
                  pl.StartDate as Active,
                  pl.EndDate as Deactivate,
                  'O' as PriceBasis,
                  CASE WHEN plb.DiscountPercent <> 0 then 'P' else 'A' end as AdjustmentType,
                  CAST(plb.Quantity AS DECIMAL(18,5)) as BreakQty,
                  CASE WHEN plb.DiscountPercent <> 0 then plb.DiscountPercent else plb.UnitPrice end as Amount,
                  CAST(0 AS DECIMAL(18,5)) as AltAmount
                FROM {TablePrefix}pricelst PL (NOLOCK)
                  JOIN {TablePrefix}pricelstparts PLP (NOLOCK) on
                    PL.Company = PLP.Company and PL.Listcode = PLP.ListCode
                  JOIN {TablePrefix}plpartbrk PLB (NOLOCK) on
                    PLB.Company = PL.Company and
                    PLB.Listcode = PL.ListCode and
                    PLB.PartNum = PLP.PartNum
                WHERE pl.Company = '"
            + integrationProcessorPricingRefreshEpicor9Info.CompanyNumber
            + @"' and
                  (pl.WarehouseList = '' or pl.WarehouseList like '%"
            + integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse
            + @"%') and
                  pl.startdate <= GETDATE() and
                  (pl.enddate is null or pl.enddate >= GETDATE())

                UNION

                SELECT
                  'Price List Product Group' as RecordType,
                  plg.ListCode as CustomerKeyPart,
                  plg.ProdCode as ProductKeyPart,
                  '' as PartNum,
                  pl.CurrencyCode,
                  '"
            + integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse
            + @"' as Warehouse,
                  plg.UOMCode as UnitOfMeasure,
                  pl.StartDate as Active,
                  pl.EndDate as Deactivate,
                  'O' as PriceBasis,
                  'A' as AdjustmentType,
                  CAST(0 AS DECIMAL(18,5)) as BreakQty,
                  CAST(0 AS DECIMAL(18,5)) as Amount,
                  plg.BasePrice as AltAmount
                FROM {TablePrefix}pricelst PL (NOLOCK)
                  JOIN {TablePrefix}pricelstgroups PLG (NOLOCK) on
                    PL.Company = plg.Company and PL.Listcode = plg.ListCode
                WHERE PL.Company = '"
            + integrationProcessorPricingRefreshEpicor9Info.CompanyNumber
            + @"' and
                  (WarehouseList = '' or WarehouseList like '%"
            + integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse
            + @"%') and
                  pl.startdate <= GETDATE() and
                  (pl.enddate is null or pl.enddate >= GETDATE())

                UNION

                SELECT
                  'Price List Product Group' as RecordType,
                  plb.ListCode as CustomerKeyPart,
                  plb.ProdCode as ProductKeyPart,
                  '' as PartNum,
                  pl.CurrencyCode,
                  '"
            + integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse
            + @"' as Warehouse,
                  plb.UOMCode as UnitOfMeasure,
                  pl.StartDate as Active,
                  pl.EndDate as Deactivate,
                  'O' as PriceBasis,
                  CASE WHEN plb.DiscountPercent <> 0 then 'P' else 'A' end as AdjustmentType,
                  CAST(plb.Quantity AS DECIMAL(18,5)) as BreakQty,
                  CASE WHEN plb.DiscountPercent <> 0 then plb.DiscountPercent else plb.UnitPrice end as Amount,
                  CAST(0 AS DECIMAL(18,5)) as AltAmount
                FROM {TablePrefix}pricelst PL (NOLOCK)
                  JOIN {TablePrefix}pricelstgroups plg (NOLOCK) on
                    PL.Company = plg.Company and PL.Listcode = plg.ListCode
                  JOIN {TablePrefix}plgrupbrk PLB (NOLOCK) on
                    PLB.Company = PL.Company and
                    PLB.Listcode = PL.ListCode and
                    PLB.ProdCode = plg.ProdCode
                WHERE pl.Company = '"
            + integrationProcessorPricingRefreshEpicor9Info.CompanyNumber
            + @"' and
                  (pl.WarehouseList = '' or pl.WarehouseList like '%"
            + integrationProcessorPricingRefreshEpicor9Info.CustomerDefaultWarehouse
            + @"%') and
                  pl.startdate <= GETDATE() and
                  (pl.enddate is null or pl.enddate >= GETDATE())
                ) UnionQuery ORDER BY RecordType, CurrencyCode, Warehouse, UnitOfMeasure, CustomerKeyPart, ProductKeyPart, Active ";

        sql = integrationProcessorPricingRefreshEpicor9Info.DbType.Equals(
            "Sql",
            StringComparison.OrdinalIgnoreCase
        )
            ? sql.Replace("{TablePrefix}", this.ErpSqlSchemaName)
            : sql.Replace("{TablePrefix}", "PUB.")
                .Replace("(NOLOCK)", string.Empty)
                .Replace("ISNULL", "IFNULL")
                .Replace("LEN(", "LENGTH(")
                .Replace("NVARCHAR", "VARCHAR")
                .Replace("GETDATE()", "NOW()");

        return sql;
    }
}
