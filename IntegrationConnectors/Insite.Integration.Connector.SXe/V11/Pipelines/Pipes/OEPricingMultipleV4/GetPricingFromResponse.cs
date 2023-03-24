namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.OEPricingMultipleV4;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Utilities;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;

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

        var outPrice2 = this.GetOutPrice2ByErpNumberUnitOfMeasureAndWarehouse(
            result,
            product.ErpNumber,
            pricingServiceParameter.UnitOfMeasure,
            pricingServiceParameter.Warehouse
        );
        if (outPrice2 == null)
        {
            return null;
        }

        var outPriceBreak2 = this.GetOutPriceBreak2(
            result,
            product.ErpNumber,
            pricingServiceParameter.Warehouse
        );

        return this.CreatePricingServiceResult(outPrice2, outPriceBreak2);
    }

    private PriceOutV2 GetOutPrice2ByErpNumberUnitOfMeasureAndWarehouse(
        OEPricingMultipleV4Result result,
        string erpNumber,
        string unitOfMeasure,
        string warehouse
    )
    {
        if (result.OEPricingMultipleV4Response?.Response?.PriceOutV2Collection?.PriceOutV2s == null)
        {
            return null;
        }

        var outPrice2sByErpNumber =
            result.OEPricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s.Where(
                o => o.Prod.Equals(erpNumber, StringComparison.OrdinalIgnoreCase)
            );

        var outPrice2sByUnitOfMeasure = outPrice2sByErpNumber.Where(
            o => o.Unit.Equals(unitOfMeasure, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPrice2sByUnitOfMeasure.Any())
        {
            outPrice2sByUnitOfMeasure = outPrice2sByErpNumber;
        }

        var outPrice2sByWarehouse = outPrice2sByUnitOfMeasure.Where(
            o => o.Whse.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPrice2sByWarehouse.Any())
        {
            outPrice2sByWarehouse = outPrice2sByUnitOfMeasure.Where(
                o =>
                    o.Whse.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!outPrice2sByWarehouse.Any())
        {
            outPrice2sByWarehouse = outPrice2sByUnitOfMeasure;
        }

        return outPrice2sByWarehouse.OrderBy(o => o.Seqno).FirstOrDefault();
    }

    private PriceOutBreak GetOutPriceBreak2(
        OEPricingMultipleV4Result result,
        string erpNumber,
        string warehouse
    )
    {
        if (
            result.OEPricingMultipleV4Response?.Response?.PriceOutBreakCollection?.PriceOutBreaks
            == null
        )
        {
            return null;
        }

        var outPriceBreak2sByErpNumber =
            result.OEPricingMultipleV4Response.Response.PriceOutBreakCollection.PriceOutBreaks.Where(
                o => o.Prod.Equals(erpNumber, StringComparison.OrdinalIgnoreCase)
            );

        var outPriceBreaks2sByWarehouse = outPriceBreak2sByErpNumber.Where(
            o => o.Whse.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPriceBreaks2sByWarehouse.Any())
        {
            outPriceBreaks2sByWarehouse = outPriceBreak2sByErpNumber.Where(
                o =>
                    o.Whse.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!outPriceBreaks2sByWarehouse.Any())
        {
            outPriceBreaks2sByWarehouse = outPriceBreak2sByErpNumber;
        }

        return outPriceBreaks2sByWarehouse.OrderBy(o => o.Seqno).FirstOrDefault();
    }

    private PricingServiceResult CreatePricingServiceResult(
        PriceOutV2 priceOutV2,
        PriceOutBreak priceOutBreak
    )
    {
        if (priceOutBreak == null)
        {
            return new PricingServiceResult
            {
                UnitRegularPrice = priceOutV2.Price,
                UnitRegularPriceDisplay = this.currencyFormatProvider.GetString(
                    priceOutV2.Price,
                    SiteContext.Current.CurrencyDto
                )
            };
        }

        var priceBreaks = this.GetPriceBreaks(priceOutV2, priceOutBreak);

        var priceBreak = priceBreaks
            .Where(o => o.BreakQty <= priceOutV2.Qtyord)
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

    private IList<ProductPrice> GetPriceBreaks(PriceOutV2 priceOutV2, PriceOutBreak priceOutBreak)
    {
        var priceBreaks = new List<ProductPrice>();

        for (var i = 1; i < 9; i++)
        {
            var priceBreakQuantity =
                i == 1
                    ? 1
                    : this.GetPriceBreakPropertyValue(priceOutBreak, $"Quantitybreak{i - 1}");

            var priceBreakPrice =
                i == 1
                    ? priceOutBreak.Pricebreak1
                    : this.GetPriceBreakPropertyValue(priceOutBreak, $"Pricebreak{i}");

            var priceBreakDiscount =
                i == 1
                    ? priceOutBreak.Discountpercent1
                    : this.GetPriceBreakPropertyValue(priceOutBreak, $"Discountpercent{i}");

            var price = priceBreakPrice > 0 ? priceBreakPrice : priceOutV2.Price;

            if (priceBreakDiscount > 0)
            {
                price =
                    priceOutV2.Disctype == "%"
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

    private decimal GetPriceBreakPropertyValue(PriceOutBreak priceOutBreak, string propertyName)
    {
        var propertyInfo = typeof(PriceOutBreak)
            .GetProperties()
            .FirstOrDefault(o => o.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        if (propertyInfo == null)
        {
            return 0;
        }

        return (decimal)propertyInfo.GetValue(priceOutBreak);
    }
}
