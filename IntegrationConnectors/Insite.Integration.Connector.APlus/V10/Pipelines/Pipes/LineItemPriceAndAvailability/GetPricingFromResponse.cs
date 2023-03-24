namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;

using System;
using System.Linq;

using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Utilities;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class GetPricingFromResponse
    : IPipe<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    private readonly ICurrencyFormatProvider currencyFormatProvider;

    public GetPricingFromResponse(ICurrencyFormatProvider currencyFormatProvider)
    {
        this.currencyFormatProvider = currencyFormatProvider;
    }

    public int Order => 600;

    public LineItemPriceAndAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        LineItemPriceAndAvailabilityParameter parameter,
        LineItemPriceAndAvailabilityResult result
    )
    {
        foreach (var pricingServiceParameter in parameter.PricingServiceParameters)
        {
            var product = this.GetProduct(unitOfWork, pricingServiceParameter);

            var warehouseInfo = this.GetWarehouseInfoByItemNumberUnitOfMeasureAndWarehouse(
                pricingServiceParameter,
                result,
                product
            );
            if (warehouseInfo == null)
            {
                continue;
            }

            var pricingServiceResult = this.CreatePricingServiceResult(warehouseInfo);

            result.PricingServiceResults[pricingServiceParameter] = pricingServiceResult;
        }

        return result;
    }

    private Product GetProduct(
        IUnitOfWork unitOfWork,
        PricingServiceParameter pricingServiceParameter
    )
    {
        return pricingServiceParameter.Product
            ?? unitOfWork.GetRepository<Product>().Get(pricingServiceParameter.ProductId);
    }

    private ResponseWarehouseInfo GetWarehouseInfoByItemNumberUnitOfMeasureAndWarehouse(
        PricingServiceParameter pricingServiceParameter,
        LineItemPriceAndAvailabilityResult lineItemPriceAndAvailabilityResult,
        Product product
    )
    {
        if (
            lineItemPriceAndAvailabilityResult.LineItemPriceAndAvailabilityResponse.ItemAvailability
            == null
        )
        {
            return null;
        }

        var itemAvailability =
            lineItemPriceAndAvailabilityResult.LineItemPriceAndAvailabilityResponse.ItemAvailability.FirstOrDefault(
                o => o.Item.ItemNumber.Equals(product.ErpNumber, StringComparison.OrdinalIgnoreCase)
            );

        if (itemAvailability == null || itemAvailability.WarehouseInfo == null)
        {
            return null;
        }

        var warehouseInfosByUnitOfMeasure = itemAvailability.WarehouseInfo.Where(
            o =>
                o.PricingUOM.Equals(
                    pricingServiceParameter.UnitOfMeasure,
                    StringComparison.OrdinalIgnoreCase
                )
        );
        if (!warehouseInfosByUnitOfMeasure.Any())
        {
            warehouseInfosByUnitOfMeasure = itemAvailability.WarehouseInfo;
        }

        var warehouseInfosByWarehouse = warehouseInfosByUnitOfMeasure.Where(
            o =>
                o.Warehouse.Equals(
                    pricingServiceParameter.Warehouse,
                    StringComparison.OrdinalIgnoreCase
                )
        );
        if (!warehouseInfosByWarehouse.Any())
        {
            warehouseInfosByWarehouse = warehouseInfosByUnitOfMeasure.Where(
                o =>
                    o.Warehouse.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!warehouseInfosByWarehouse.Any())
        {
            warehouseInfosByWarehouse = warehouseInfosByUnitOfMeasure;
        }

        return warehouseInfosByWarehouse.FirstOrDefault();
    }

    private PricingServiceResult CreatePricingServiceResult(ResponseWarehouseInfo warehouseInfo)
    {
        decimal.TryParse(warehouseInfo.Price, out var price);

        return new PricingServiceResult
        {
            UnitRegularPrice = price,
            UnitRegularPriceDisplay = this.currencyFormatProvider.GetString(
                price,
                SiteContext.Current.CurrencyDto
            )
        };
    }
}
