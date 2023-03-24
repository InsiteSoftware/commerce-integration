namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;

using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class AddProductsToRequest
    : IPipe<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    public int Order => 300;

    public LineItemPriceAndAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        LineItemPriceAndAvailabilityParameter parameter,
        LineItemPriceAndAvailabilityResult result
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();
        var warehouseRepository = unitOfWork.GetRepository<Warehouse>();

        result.LineItemPriceAndAvailabilityRequest.Items.AddRange(
            this.GetRequestItems(parameter.PricingServiceParameters, productRepository)
        );
        result.LineItemPriceAndAvailabilityRequest.Items.AddRange(
            this.GetRequestItems(
                parameter.GetInventoryParameter,
                productRepository,
                warehouseRepository
            )
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
                pricingServiceParameter.Warehouse,
                pricingServiceParameter.QtyOrdered
            )
        );

        return requestItems;
    }

    private IList<RequestItem> GetRequestItems(
        GetInventoryParameter getInventoryParameter,
        IRepository<Product> productRepository,
        IRepository<Warehouse> warehouseRepository
    )
    {
        var requestItems = new List<RequestItem>();
        if (getInventoryParameter == null)
        {
            return requestItems;
        }

        var warehouse = GetWarehouse(getInventoryParameter, warehouseRepository);

        foreach (var product in getInventoryParameter.Products)
        {
            requestItems.Add(
                this.CreateRequestItem(product.ErpNumber, product.UnitOfMeasure, warehouse, 1)
            );
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

            requestItems.Add(
                this.CreateRequestItem(product.ErpNumber, product.UnitOfMeasure, warehouse, 1)
            );
        }

        return requestItems;
    }

    private RequestItem CreateRequestItem(
        string erpNumber,
        string unitOfMeasure,
        string warehouse,
        decimal qtyOrdered
    )
    {
        return new RequestItem
        {
            ItemNumber = erpNumber,
            UnitofMeasure = unitOfMeasure,
            OrderQuantity = qtyOrdered.ToString(),
            WarehouseID = warehouse,
            CalculatePrices = "YY"
        };
    }

    private static string GetWarehouse(
        GetInventoryParameter getInventoryParameter,
        IRepository<Warehouse> warehouseRepository
    )
    {
        if (getInventoryParameter?.GetWarehouses ?? false)
        {
            return string.Empty;
        }

        return getInventoryParameter.WarehouseId != null
            ? warehouseRepository.Get(getInventoryParameter.WarehouseId.Value)?.Name ?? string.Empty
            : string.Empty;
    }
}
