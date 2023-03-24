namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;

using System;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Utilities;
using Insite.Data.Entities;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class GetPricingFromResponse
    : IPipe<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    private readonly ICurrencyFormatProvider currencyFormatProvider;

    public GetPricingFromResponse(ICurrencyFormatProvider currencyFormatProvider)
    {
        this.currencyFormatProvider = currencyFormatProvider;
    }

    public int Order => 800;

    public PriceAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        PriceAvailabilityParameter parameter,
        PriceAvailabilityResult result
    )
    {
        foreach (var pricingServiceParameter in parameter.PricingServiceParameters)
        {
            var pricingServiceResult = this.GetPricingServiceResult(
                unitOfWork,
                pricingServiceParameter,
                result
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
        PricingServiceParameter pricingServiceParameter,
        PriceAvailabilityResult result
    )
    {
        var product =
            pricingServiceParameter.Product
            ?? unitOfWork.GetRepository<Product>().Get(pricingServiceParameter.ProductId);
        if (product == null)
        {
            return null;
        }

        var responseItem = this.GetResponseItem(product, pricingServiceParameter.Warehouse, result);
        if (responseItem == null)
        {
            return null;
        }

        return new PricingServiceResult
        {
            UnitRegularPrice = responseItem.Price,
            UnitRegularPriceDisplay = this.currencyFormatProvider.GetString(
                responseItem.Price,
                SiteContext.Current.CurrencyDto
            )
        };
    }

    private ResponseItem GetResponseItem(
        Product product,
        string warehouse,
        PriceAvailabilityResult result
    )
    {
        var responseItems = result.PriceAvailabilityResponse.Response.Items.Where(
            o =>
                o.ItemNumber.Equals(product.ErpNumber, StringComparison.OrdinalIgnoreCase)
                && o.ErrorMessage.IsBlank()
        );

        var responseItemsByWarehouse = responseItems.Where(
            o => o.WarehouseID.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (!responseItemsByWarehouse.Any())
        {
            responseItemsByWarehouse = responseItems.Where(
                o =>
                    o.WarehouseID.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!responseItemsByWarehouse.Any())
        {
            responseItemsByWarehouse = responseItems;
        }

        return responseItemsByWarehouse.FirstOrDefault();
    }
}
