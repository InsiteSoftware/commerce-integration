namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;

using System;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Utilities;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class GetPricingFromResponse : IPipe<GetItemPriceParameter, GetItemPriceResult>
{
    private readonly ICurrencyFormatProvider currencyFormatProvider;

    public GetPricingFromResponse(ICurrencyFormatProvider currencyFormatProvider)
    {
        this.currencyFormatProvider = currencyFormatProvider;
    }

    public int Order => 800;

    public GetItemPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetItemPriceParameter parameter,
        GetItemPriceResult result
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
        GetItemPriceResult result
    )
    {
        var product =
            pricingServiceParameter.Product
            ?? unitOfWork.GetRepository<Product>().Get(pricingServiceParameter.ProductId);
        if (product == null)
        {
            return null;
        }

        var replyItem = this.GetReplyItem(
            product.ErpNumber,
            pricingServiceParameter.UnitOfMeasure,
            result.GetItemPriceReply.Reply
        );
        if (replyItem == null)
        {
            return null;
        }

        return this.CreatePricingServiceResult(replyItem);
    }

    private ReplyItem GetReplyItem(string erpNumber, string unitOfMeasure, Reply reply)
    {
        var replyItemsByErpNumber = reply.ListOfItems.Where(
            o => o.ItemID.Equals(erpNumber, StringComparison.OrdinalIgnoreCase)
        );

        var replyItemsByUnitOfMeasure = replyItemsByErpNumber.Where(
            o => o.UnitName.Equals(unitOfMeasure, StringComparison.OrdinalIgnoreCase)
        );
        if (!replyItemsByUnitOfMeasure.Any())
        {
            replyItemsByUnitOfMeasure = replyItemsByErpNumber;
        }

        return replyItemsByUnitOfMeasure.FirstOrDefault();
    }

    private PricingServiceResult CreatePricingServiceResult(ReplyItem replyItem)
    {
        decimal.TryParse(replyItem.NetPrice, out var netPrice);

        return new PricingServiceResult
        {
            UnitRegularPrice = netPrice,
            UnitRegularPriceDisplay = this.currencyFormatProvider.GetString(
                netPrice,
                SiteContext.Current.CurrencyDto
            )
        };
    }
}
