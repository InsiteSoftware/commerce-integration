namespace Insite.WIS.Acumatica;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Globalization;
using Insite.Common.Helpers;
using Insite.Integration.Enums;
using Insite.Integration.Attributes;
using Insite.WIS.Acumatica.Resources;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.Broker.Plugins.Constants;

[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.Acumatica)]
public class IntegrationProcessorPricingRefreshAcumatica : IntegrationProcessorPricingRefreshBase
{
    public override DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep
    )
    {
        var dataSet = ExecuteAcumaticaOData(siteConnection, integrationJob, jobStep);
        if (dataSet.Tables.Count == 0)
        {
            return dataSet;
        }

        var initialDataSet = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);
        var priceMatrixDataTable = this.BuildPriceMatrixDataTable(jobStep.Sequence);

        var priceRecords = dataSet.Tables[0]
            .AsEnumerable()
            .Select(
                o =>
                    new
                    {
                        RawDataRow = o,
                        PriceType = o[PricingRefreshAcumaticaSourceFile.PriceTypeColumn].ToString(),
                        Promotion = Convert.ToBoolean(
                            o[PricingRefreshAcumaticaSourceFile.PromotionColumn]
                        ),
                        Currency = o[PricingRefreshAcumaticaSourceFile.CurrencyColumn].ToString(),
                        Warehouse = o[PricingRefreshAcumaticaSourceFile.WarehouseColumn].ToString(),
                        UOM = o[PricingRefreshAcumaticaSourceFile.UomColumn].ToString(),
                        Customer = o[PricingRefreshAcumaticaSourceFile.CustomerColumn].ToString(),
                        CustomerPriceClass = o[
                            PricingRefreshAcumaticaSourceFile.CustomerPriceClassColumn
                        ].ToString(),
                        InventoryID = o[
                            PricingRefreshAcumaticaSourceFile.InventoryIdColumn
                        ].ToString(),
                        EffectiveDate = Convert.ToDateTime(
                            o[PricingRefreshAcumaticaSourceFile.EffectiveDateColumn]
                        )
                    }
            )
            .ToArray();

        var now = DateTime.Now;

        var maxPastDateByGroup = priceRecords
            .Where(o => o.EffectiveDate < now)
            .GroupBy(
                o =>
                    new
                    {
                        o.PriceType,
                        o.Promotion,
                        o.Currency,
                        o.Warehouse,
                        o.UOM,
                        o.Customer,
                        o.CustomerPriceClass,
                        o.InventoryID
                    }
            )
            .ToDictionary(group => group.Key, group => group.Max(o => o.EffectiveDate));

        var priceRecordGroups = priceRecords.GroupBy(
            o =>
                new
                {
                    o.PriceType,
                    o.Promotion,
                    o.Currency,
                    o.Warehouse,
                    o.UOM,
                    o.Customer,
                    o.CustomerPriceClass,
                    o.InventoryID,
                    EffectiveDate = o.EffectiveDate >= now
                        ? o.EffectiveDate
                        : maxPastDateByGroup[
#pragma warning disable SA1513
                            new
                            {
                                o.PriceType,
                                o.Promotion,
                                o.Currency,
                                o.Warehouse,
                                o.UOM,
                                o.Customer,
                                o.CustomerPriceClass,
                                o.InventoryID
                            }
#pragma warning restore SA1513
                        ]
                }
        );

        foreach (var priceRecordGroup in priceRecordGroups)
        {
            var priceMatrixDataRow = priceMatrixDataTable.NewRow();

            priceMatrixDataRow[Data.RecordTypeColumn] = GetRecordType(
                priceRecordGroup.Key.PriceType,
                priceRecordGroup.Key.Promotion
            );
            priceMatrixDataRow[Data.CurrencyCodeColumn] = priceRecordGroup.Key.Currency;
            priceMatrixDataRow[Data.WarehouseColumn] = priceRecordGroup.Key.Warehouse;
            priceMatrixDataRow[Data.UnitOfMeasureColumn] = priceRecordGroup.Key.UOM;
            priceMatrixDataRow[Data.CustomerKeyPartColumn] = this.GetCustomerKeyPart(
                priceRecordGroup.Key.Customer,
                priceRecordGroup.Key.CustomerPriceClass,
                initialDataSet
            );
            priceMatrixDataRow[Data.ProductKeyPartColumn] = this.GetProductId(
                priceRecordGroup.Key.InventoryID,
                initialDataSet
            );
            priceMatrixDataRow[Data.ActivateOnColumn] = priceRecordGroup.Key.EffectiveDate;
            priceMatrixDataRow[Data.DeactivateOnColumn] = GetDeactivateOn(
                priceRecordGroup.Select(o => o.RawDataRow)
            );

            var breakQuantityPrices = GetOrderedBreakQuantityPrices(
                priceRecordGroup.Select(o => o.RawDataRow).ToList()
            );
            for (var i = 1; i <= breakQuantityPrices.Count; i++)
            {
                var index = i.ToString("00", CultureInfo.InvariantCulture);
                var price = breakQuantityPrices.ElementAt(i - 1);

                priceMatrixDataRow[$"BreakQty{index}"] = price.Key;
                priceMatrixDataRow[$"Amount{index}"] = price.Value;
            }

            priceMatrixDataTable.Rows.Add(priceMatrixDataRow);
        }

        dataSet = new DataSet();
        dataSet.Tables.Add(priceMatrixDataTable);
        return dataSet;
    }

    private static DataSet ExecuteAcumaticaOData(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep
    )
    {
        var integrationProcessorAcumaticaOData = new IntegrationProcessorAcumaticaOData();

        return integrationProcessorAcumaticaOData.Execute(siteConnection, integrationJob, jobStep);
    }

    private static string GetRecordType(string priceType, bool promotion)
    {
        return promotion ? $"{priceType} Promotion" : priceType;
    }

    private string GetCustomerKeyPart(
        string customer,
        string customerPriceClass,
        DataSet initialDataSet
    )
    {
        return !string.IsNullOrEmpty(customer)
            ? this.GetCustomerId(customer, string.Empty, initialDataSet)
            : customerPriceClass;
    }

    private static DateTime GetDeactivateOn(IEnumerable<DataRow> dataRows)
    {
        var earliestExpirationDate = new DateTime(2059, 12, 31);

        foreach (var dataRow in dataRows)
        {
            var expirationDate = dataRow[PricingRefreshAcumaticaSourceFile.ExpirationDateColumn];
            if (expirationDate is null or DBNull)
            {
                continue;
            }

            var currentExpirationDate = Convert.ToDateTime(expirationDate);
            if (currentExpirationDate <= earliestExpirationDate)
            {
                earliestExpirationDate = currentExpirationDate;
            }
        }

        return earliestExpirationDate;
    }

    private static SortedDictionary<decimal, decimal> GetOrderedBreakQuantityPrices(
        List<DataRow> dataRows
    )
    {
        var breakQuantityPrices = new SortedDictionary<decimal, decimal>();

        foreach (var dataRow in dataRows)
        {
            breakQuantityPrices[
                Convert.ToDecimal(dataRow[PricingRefreshAcumaticaSourceFile.BreakQtyColumn])
            ] = Convert.ToDecimal(dataRow[PricingRefreshAcumaticaSourceFile.PriceColumn]);
        }

        // acumatica might deliver first price break as quantity 0 but ISC expects first price break to be quantity 1
        if (breakQuantityPrices.TryGetValue(0, out var price))
        {
            breakQuantityPrices.Remove(0);
            if (!breakQuantityPrices.ContainsKey(1))
            {
                breakQuantityPrices.Add(1, price);
            }
        }

        // ensure there is always a break price at quantity 1 for product record types (all data rows will have same price type and default price, use first data row)
        var productPriceTypes = new List<string>
        {
            PricingRefreshAcumaticaSourceFile.BasePriceType,
            PricingRefreshAcumaticaSourceFile.ProductPriceType,
            PricingRefreshAcumaticaSourceFile.ProductCustomerPriceType,
            PricingRefreshAcumaticaSourceFile.ProductCustomerClassPriceType
        };
        if (
            productPriceTypes.Any(
                o =>
                    o.Equals(
                        dataRows[0][PricingRefreshAcumaticaSourceFile.PriceTypeColumn].ToString(),
                        StringComparison.OrdinalIgnoreCase
                    )
            ) && !breakQuantityPrices.ContainsKey(1)
        )
        {
            breakQuantityPrices[1] = Convert.ToDecimal(
                dataRows[0][PricingRefreshAcumaticaSourceFile.DefaultPriceColumn]
            );
        }

        return breakQuantityPrices;
    }
}
