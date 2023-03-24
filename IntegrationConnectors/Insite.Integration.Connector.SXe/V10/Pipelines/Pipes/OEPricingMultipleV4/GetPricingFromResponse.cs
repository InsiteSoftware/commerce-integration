namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.OEPricingMultipleV4;

using System;
using System.Collections.Generic;
using System.Linq;

using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Utilities;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.OEPricingMultipleV4;

public sealed class GetPricingFromResponse
    : IPipe<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    private readonly ICurrencyFormatProvider currencyFormatProvider;

    public GetPricingFromResponse(ICurrencyFormatProvider currencyFormatProvider)
    {
        this.currencyFormatProvider = currencyFormatProvider;
    }

    public int Order => 600;

    public OEPricingMultipleV4Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Parameter parameter,
        OEPricingMultipleV4Result result
    )
    {
        foreach (var pricingServiceParameter in parameter.PricingServiceParameters)
        {
            var pricingServiceResult = this.GetPricingServiceResult(
                unitOfWork,
                result,
                pricingServiceParameter
            );
            if (pricingServiceResult != null)
            {
                result.PricingServiceResults[pricingServiceParameter] = pricingServiceResult;
            }
        }

        return result;
    }

    private PricingServiceResult GetPricingServiceResult(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Result result,
        PricingServiceParameter pricingServiceParameter
    )
    {
        var product =
            pricingServiceParameter.Product
            ?? unitOfWork.GetRepository<Product>().Get(pricingServiceParameter.ProductId);
        if (product == null)
        {
            return null;
        }

        var outPrice3 = this.GetOutPrice3ByErpNumberUnitOfMeasureAndWarehouse(
            result,
            product.ErpNumber,
            pricingServiceParameter.UnitOfMeasure,
            pricingServiceParameter.Warehouse
        );
        if (outPrice3 == null)
        {
            return null;
        }

        var outPriceBreak3 = this.GetOutPriceBreak3(
            result,
            product.ErpNumber,
            pricingServiceParameter.Warehouse
        );

        return this.CreatePricingServiceResult(outPrice3, outPriceBreak3);
    }

    private Outprice3 GetOutPrice3ByErpNumberUnitOfMeasureAndWarehouse(
        OEPricingMultipleV4Result getOEPricingMultipleV4Result,
        string erpNumber,
        string unitOfMeasure,
        string warehouse
    )
    {
        if (getOEPricingMultipleV4Result.OEPricingMultipleV4Response.Outprice == null)
        {
            return null;
        }

        var outPrice3sByErpNumber =
            getOEPricingMultipleV4Result.OEPricingMultipleV4Response.Outprice.Where(
                o => o.ProductCode.Equals(erpNumber, StringComparison.OrdinalIgnoreCase)
            );

        var outPrice3sByUnitOfMeasure = outPrice3sByErpNumber.Where(
            o => o.UnitOfMeasure.Equals(unitOfMeasure, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPrice3sByUnitOfMeasure.Any())
        {
            outPrice3sByUnitOfMeasure = outPrice3sByErpNumber;
        }

        var outPrice3sByWarehouse = outPrice3sByUnitOfMeasure.Where(
            o => o.Warehouse.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPrice3sByWarehouse.Any())
        {
            outPrice3sByWarehouse = outPrice3sByUnitOfMeasure.Where(
                o =>
                    o.Warehouse.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!outPrice3sByWarehouse.Any())
        {
            outPrice3sByWarehouse = outPrice3sByUnitOfMeasure;
        }

        return outPrice3sByWarehouse.OrderBy(o => o.SequenceNumber).FirstOrDefault();
    }

    private Outpricebreak3 GetOutPriceBreak3(
        OEPricingMultipleV4Result getOEPricingMultipleV4Result,
        string erpNumber,
        string warehouse
    )
    {
        if (getOEPricingMultipleV4Result.OEPricingMultipleV4Response.Outpricebreak == null)
        {
            return null;
        }

        var outPriceBreak3sByErpNumber =
            getOEPricingMultipleV4Result.OEPricingMultipleV4Response.Outpricebreak.Where(
                o => o.ProductCode.Equals(erpNumber, StringComparison.OrdinalIgnoreCase)
            );

        var outPriceBreaks3sByWarehouse = outPriceBreak3sByErpNumber.Where(
            o => o.Warehouse.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPriceBreaks3sByWarehouse.Any())
        {
            outPriceBreaks3sByWarehouse = outPriceBreak3sByErpNumber.Where(
                o =>
                    o.Warehouse.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!outPriceBreaks3sByWarehouse.Any())
        {
            outPriceBreaks3sByWarehouse = outPriceBreak3sByErpNumber;
        }

        return outPriceBreaks3sByWarehouse.OrderBy(o => o.SequenceNumber).FirstOrDefault();
    }

    private PricingServiceResult CreatePricingServiceResult(
        Outprice3 outPrice3,
        Outpricebreak3 outPriceBreak3
    )
    {
        if (outPriceBreak3 == null)
        {
            return new PricingServiceResult
            {
                UnitRegularPrice = outPrice3.Price,
                UnitRegularPriceDisplay = this.currencyFormatProvider.GetString(
                    outPrice3.Price,
                    SiteContext.Current.CurrencyDto
                )
            };
        }

        var priceBreaks = this.GetPriceBreaks(outPrice3, outPriceBreak3);

        var priceBreak = priceBreaks
            .Where(o => o.BreakQty <= outPrice3.Quantity)
            .OrderBy(o => o.BreakQty)
            .First();

        return new PricingServiceResult
        {
            UnitRegularPrice = priceBreak.Price,
            UnitRegularPriceDisplay = this.currencyFormatProvider.GetString(
                priceBreak.Price,
                SiteContext.Current.CurrencyDto
            ),
            UnitRegularBreakPrices = priceBreaks
        };
    }

    private IList<ProductPrice> GetPriceBreaks(Outprice3 outPrice3, Outpricebreak3 outPriceBreak3)
    {
        var priceBreaks = new List<ProductPrice>();

        for (var i = 1; i < 9; i++)
        {
            var priceBreakQuantity =
                i == 1
                    ? 1
                    : this.GetPriceBreakPropertyValue(outPriceBreak3, $"QuantityBreak{i - 1}");

            var priceBreakPrice =
                i == 1
                    ? outPriceBreak3.PriceBreak1
                    : this.GetPriceBreakPropertyValue(outPriceBreak3, $"PriceBreak{i}");

            var priceBreakDiscount =
                i == 1
                    ? outPriceBreak3.DiscountPercent1
                    : this.GetPriceBreakPropertyValue(outPriceBreak3, $"DiscountPercent{i}");

            var price = priceBreakPrice > 0 ? priceBreakPrice : outPrice3.Price;

            if (priceBreakDiscount > 0)
            {
                price =
                    outPrice3.DiscountType == "%"
                        ? price * ((100 - priceBreakDiscount) / 100)
                        : price - priceBreakDiscount;
            }

            priceBreaks.Add(
                new ProductPrice
                {
                    BreakQty = priceBreakQuantity,
                    Price = price,
                    PriceDisplay = this.currencyFormatProvider.GetString(
                        price,
                        SiteContext.Current.CurrencyDto
                    )
                }
            );
        }

        priceBreaks = priceBreaks.Where(o => o.BreakQty > 0).ToList();

        return priceBreaks;
    }

    private decimal GetPriceBreakPropertyValue(Outpricebreak3 outPriceBreak3, string propertyName)
    {
        var propertyInfo = typeof(Outpricebreak3)
            .GetProperties()
            .FirstOrDefault(o => o.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        if (propertyInfo == null)
        {
            return 0;
        }

        return (decimal)propertyInfo.GetValue(outPriceBreak3);
    }
}
