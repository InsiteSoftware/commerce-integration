namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;

using System;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Utilities;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class GetPricingFromResponse
    : IPipe<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    private readonly ICurrencyFormatProvider currencyFormatProvider;

    public GetPricingFromResponse(ICurrencyFormatProvider currencyFormatProvider)
    {
        this.currencyFormatProvider = currencyFormatProvider;
    }

    public int Order => 700;

    public GetCustomerPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetCustomerPriceParameter parameter,
        GetCustomerPriceResult result
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
        GetCustomerPriceResult result
    )
    {
        var product =
            pricingServiceParameter.Product
            ?? unitOfWork.GetRepository<Product>().Get(pricingServiceParameter.ProductId);
        if (product == null)
        {
            return null;
        }

        var salesPartPriceResData = this.GetSalesPartPriceResData(product.ErpNumber, result);
        if (salesPartPriceResData == null)
        {
            return null;
        }

        return this.CreatePricingServiceResult(salesPartPriceResData);
    }

    private salesPartPriceResData GetSalesPartPriceResData(
        string erpNumber,
        GetCustomerPriceResult getCustomerPriceResult
    )
    {
        if (getCustomerPriceResult.CustomerPriceResponse?.parts == null)
        {
            return null;
        }

        return getCustomerPriceResult.CustomerPriceResponse.parts
            .Where(o => o.productNo != null)
            .Where(o => o.productNo.Equals(erpNumber, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();
    }

    private PricingServiceResult CreatePricingServiceResult(
        salesPartPriceResData salesPartPriceResData
    )
    {
        var price = Math.Round(
            salesPartPriceResData.saleUnitPrice * (1 - (salesPartPriceResData.discount / 100)),
            4
        );

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
