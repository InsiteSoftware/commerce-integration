namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;

using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class AddProductsToRequest
    : IPipe<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    public int Order => 200;

    public PriceAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        PriceAvailabilityParameter parameter,
        PriceAvailabilityResult result
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();

        result.PriceAvailabilityRequest.Request.Items.AddRange(
            this.GetRequestItems(parameter.PricingServiceParameters, productRepository)
        );
        result.PriceAvailabilityRequest.Request.Items.AddRange(
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

        foreach (
            var productId in getInventoryParameter.ProductIds.Where(
                o => !getInventoryParameter.Products.Any(p => p.Id == o)
            )
        )
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
            ItemNumber = erpNumber,
            UnitOfMeasure = unitOfMeasure,
            OrderQuantity = qtyOrdered <= 0 ? 1 : qtyOrdered
        };
    }
}
