namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetPricing;

using System;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Utilities;
using Insite.Data.Entities;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class GetPricingFromResponses : IPipe<GetPricingParameter, GetPricingResult>
{
    private readonly ICurrencyFormatProvider currencyFormatProvider;

    public GetPricingFromResponses(ICurrencyFormatProvider currencyFormatProvider)
    {
        this.currencyFormatProvider = currencyFormatProvider;
    }

    public int Order => 500;

    public GetPricingResult Execute(
        IUnitOfWork unitOfWork,
        GetPricingParameter parameter,
        GetPricingResult result
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();

        foreach (var pricingServiceParameter in parameter.PricingServiceParameters)
        {
            var product =
                pricingServiceParameter.Product
                ?? productRepository.Get(pricingServiceParameter.ProductId);
            var priceQueryResponse = result.PriceQueryResponses.FirstOrDefault(
                o => o.CatalogNo.EqualsIgnoreCase(product.ErpNumber)
            );
            if (priceQueryResponse == null)
            {
                continue;
            }

            result.PricingServiceResults[pricingServiceParameter] = new PricingServiceResult
            {
                UnitRegularPrice = priceQueryResponse.NetPrice.Value,
                UnitRegularPriceDisplay = this.currencyFormatProvider.GetString(
                    priceQueryResponse.NetPrice.Value,
                    SiteContext.Current.CurrencyDto
                )
            };
        }

        return result;
    }
}
