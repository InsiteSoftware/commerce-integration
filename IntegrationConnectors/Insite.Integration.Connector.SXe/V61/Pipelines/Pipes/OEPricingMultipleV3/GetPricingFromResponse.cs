namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;

using System;
using System.Collections.Generic;
using System.Linq;

using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Utilities;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;

public sealed class GetPricingFromResponse
    : IPipe<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
{
    private readonly ICurrencyFormatProvider currencyFormatProvider;

    public GetPricingFromResponse(ICurrencyFormatProvider currencyFormatProvider)
    {
        this.currencyFormatProvider = currencyFormatProvider;
    }

    public int Order => 600;

    public OEPricingMultipleV3Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV3Parameter parameter,
        OEPricingMultipleV3Result result
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
        OEPricingMultipleV3Result result,
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

    private OEPricingMultipleV3outputPrice GetOutPrice2ByErpNumberUnitOfMeasureAndWarehouse(
        OEPricingMultipleV3Result result,
        string erpNumber,
        string unitOfMeasure,
        string warehouse
    )
    {
        if (result.OEPricingMultipleV3Response.arrayPrice == null)
        {
            return null;
        }

        var outPrice2sByErpNumber = result.OEPricingMultipleV3Response.arrayPrice.Where(
            o => o.productCode.Equals(erpNumber, StringComparison.OrdinalIgnoreCase)
        );

        var outPrice2sByUnitOfMeasure = outPrice2sByErpNumber.Where(
            o => o.unitOfMeasure.Equals(unitOfMeasure, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPrice2sByUnitOfMeasure.Any())
        {
            outPrice2sByUnitOfMeasure = outPrice2sByErpNumber;
        }

        var outPrice2sByWarehouse = outPrice2sByUnitOfMeasure.Where(
            o => o.warehouse.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPrice2sByWarehouse.Any())
        {
            outPrice2sByWarehouse = outPrice2sByUnitOfMeasure.Where(
                o =>
                    o.warehouse.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!outPrice2sByWarehouse.Any())
        {
            outPrice2sByWarehouse = outPrice2sByUnitOfMeasure;
        }

        return outPrice2sByWarehouse.OrderBy(o => o.sequenceNumber).FirstOrDefault();
    }

    private OEPricingMultipleV3outputPricebreak GetOutPriceBreak2(
        OEPricingMultipleV3Result result,
        string erpNumber,
        string warehouse
    )
    {
        if (result.OEPricingMultipleV3Response.arrayPricebreak == null)
        {
            return null;
        }

        var outPriceBreak2sByErpNumber = result.OEPricingMultipleV3Response.arrayPricebreak.Where(
            o => o.productCode.Equals(erpNumber, StringComparison.OrdinalIgnoreCase)
        );

        var outPriceBreaks2sByWarehouse = outPriceBreak2sByErpNumber.Where(
            o => o.warehouse.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPriceBreaks2sByWarehouse.Any())
        {
            outPriceBreaks2sByWarehouse = outPriceBreak2sByErpNumber.Where(
                o =>
                    o.warehouse.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!outPriceBreaks2sByWarehouse.Any())
        {
            outPriceBreaks2sByWarehouse = outPriceBreak2sByErpNumber;
        }

        return outPriceBreaks2sByWarehouse.OrderBy(o => o.sequenceNumber).FirstOrDefault();
    }

    private PricingServiceResult CreatePricingServiceResult(
        OEPricingMultipleV3outputPrice outPrice2,
        OEPricingMultipleV3outputPricebreak outPriceBreak2
    )
    {
        if (outPriceBreak2 == null)
        {
            return new PricingServiceResult
            {
                UnitRegularPrice = outPrice2.price,
                UnitRegularPriceDisplay = this.currencyFormatProvider.GetString(
                    outPrice2.price,
                    SiteContext.Current.CurrencyDto
                )
            };
        }

        var priceBreaks = this.GetPriceBreaks(outPrice2, outPriceBreak2);

        var priceBreak = priceBreaks
            .Where(o => o.BreakQty <= outPrice2.quantity)
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

    private IList<ProductPrice> GetPriceBreaks(
        OEPricingMultipleV3outputPrice outPrice2,
        OEPricingMultipleV3outputPricebreak outPriceBreak2
    )
    {
        var priceBreaks = new List<ProductPrice>();

        for (var i = 1; i < 9; i++)
        {
            var priceBreakQuantity =
                i == 1
                    ? 1
                    : this.GetPriceBreakPropertyValue(outPriceBreak2, $"quantityBreak{i - 1}");

            var priceBreakPrice =
                i == 1
                    ? outPriceBreak2.priceBreak1
                    : this.GetPriceBreakPropertyValue(outPriceBreak2, $"priceBreak{i}");

            var priceBreakDiscount =
                i == 1
                    ? outPriceBreak2.discountPercent1
                    : this.GetPriceBreakPropertyValue(outPriceBreak2, $"discountPercent{i}");

            var price = priceBreakPrice > 0 ? priceBreakPrice : outPrice2.price;

            if (priceBreakDiscount > 0)
            {
                price =
                    outPrice2.discountType == "%"
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

    private decimal GetPriceBreakPropertyValue(
        OEPricingMultipleV3outputPricebreak outPriceBreak2,
        string propertyName
    )
    {
        var propertyInfo = typeof(OEPricingMultipleV3outputPricebreak)
            .GetProperties()
            .FirstOrDefault(o => o.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        if (propertyInfo == null)
        {
            return 0;
        }

        return (decimal)propertyInfo.GetValue(outPriceBreak2);
    }
}
