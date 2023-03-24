namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrices;

using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddProductsToRequest : IPipe<GetItemPriceParameter, GetItemPriceResult>
{
    public int Order => 200;

    public GetItemPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetItemPriceParameter parameter,
        GetItemPriceResult result
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();

        result.GetItemPriceRequest.Request.ListOfItems.AddRange(
            this.GetRequestItems(parameter.PricingServiceParameters, productRepository)
        );
        result.GetItemPriceRequest.Request.ListOfItems.AddRange(
            this.GetRequestItems(parameter.GetInventoryParameter, productRepository)
        );

        return result;
    }

    private IList<RequestItem> GetRequestItems(
        ICollection<PricingServiceParameter> pricingServiceParameters,
        IRepository<Product> productRepository
    )
    {
        var requestItems = new List<RequestItem>();

        foreach (var pricingServiceParameter in pricingServiceParameters)
        {
            requestItems.AddRange(this.GetRequestItems(pricingServiceParameter, productRepository));
        }

        return requestItems;
    }

    private IList<RequestItem> GetRequestItems(
        PricingServiceParameter pricingServiceParameter,
        IRepository<Product> productRepository
    )
    {
        var requestItems = new List<RequestItem>();

        var product =
            pricingServiceParameter.Product
            ?? productRepository.Get(pricingServiceParameter.ProductId);
        if (product == null)
        {
            return requestItems;
        }

        requestItems.Add(
            this.CreateRequestItem(
                product.ErpNumber,
                pricingServiceParameter.UnitOfMeasure,
                pricingServiceParameter.QtyOrdered
            )
        );

        return requestItems;
    }

    private IList<RequestItem> GetRequestItems(
        GetInventoryParameter getInventoryParameter,
        IRepository<Product> productRepository
    )
    {
        var requestItems = new List<RequestItem>();
        if (getInventoryParameter == null)
        {
            return requestItems;
        }

        foreach (var product in getInventoryParameter.Products)
        {
            requestItems.Add(this.CreateRequestItem(product.ErpNumber, product.UnitOfMeasure, 1));
        }

        foreach (var productId in getInventoryParameter.ProductIds)
        {
            var product = productRepository.Get(productId);
            if (product == null)
            {
                continue;
            }

            requestItems.Add(this.CreateRequestItem(product.ErpNumber, product.UnitOfMeasure, 1));
        }

        return requestItems;
    }

    private RequestItem CreateRequestItem(
        string erpNumber,
        string unitOfMeasure,
        decimal qtyOrdered
    )
    {
        return new RequestItem
        {
            ItemID = erpNumber,
            UnitName = unitOfMeasure,
            Quantity = qtyOrdered.ToString()
        };
    }
}
