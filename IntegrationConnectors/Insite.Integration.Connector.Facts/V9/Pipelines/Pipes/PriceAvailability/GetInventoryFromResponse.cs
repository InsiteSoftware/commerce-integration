namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Data.Entities;
using Insite.Core.Plugins.Inventory;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Core.Context;

public sealed class GetInventoryFromResponse
    : IPipe<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    public int Order => 900;

    public PriceAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        PriceAvailabilityParameter parameter,
        PriceAvailabilityResult result
    )
    {
        var getInventoryResults = new List<GetInventoryResult>();

        foreach (var pricingServiceParameter in parameter.PricingServiceParameters)
        {
            var getInventoryResult = this.GetGetInventoryResult(
                unitOfWork,
                result,
                pricingServiceParameter
            );
            if (getInventoryResult != null)
            {
                if (!result.PricingServiceResults.ContainsKey(pricingServiceParameter))
                {
                    result.PricingServiceResults[pricingServiceParameter] =
                        new PricingServiceResult();
                }

                result.PricingServiceResults[pricingServiceParameter].GetInventoryResult =
                    getInventoryResult;

                getInventoryResults.Add(getInventoryResult);
            }
        }

        if (parameter.GetInventoryParameter != null)
        {
            foreach (var productId in parameter.GetInventoryParameter.ProductIds)
            {
                var getInventoryResult = this.GetGetInventoryResult(
                    unitOfWork,
                    result,
                    productId,
                    parameter.GetInventoryParameter.WarehouseId
                );
                if (getInventoryResult != null)
                {
                    getInventoryResults.Add(getInventoryResult);
                }
            }

            foreach (var product in parameter.GetInventoryParameter.Products)
            {
                var getInventoryResult = this.GetGetInventoryResult(
                    unitOfWork,
                    result,
                    product,
                    parameter.GetInventoryParameter.WarehouseId
                );
                if (getInventoryResult != null)
                {
                    getInventoryResults.Add(getInventoryResult);
                }
            }
        }

        result.GetInventoryResult = new GetInventoryResult
        {
            Inventories = this.GetInventories(getInventoryResults),
            RequiresRealTimeInventory = false
        };

        return result;
    }

    private GetInventoryResult GetGetInventoryResult(
        IUnitOfWork unitOfWork,
        PriceAvailabilityResult result,
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

        return this.GetGetInventoryResult(result, product, pricingServiceParameter.Warehouse);
    }

    private GetInventoryResult GetGetInventoryResult(
        IUnitOfWork unitOfWork,
        PriceAvailabilityResult result,
        Guid productId,
        Guid? warehouseId
    )
    {
        var product = unitOfWork.GetRepository<Product>().Get(productId);
        if (product == null)
        {
            return null;
        }

        return this.GetGetInventoryResult(unitOfWork, result, product, warehouseId);
    }

    private GetInventoryResult GetGetInventoryResult(
        IUnitOfWork unitOfWork,
        PriceAvailabilityResult result,
        Product product,
        Guid? warehouseId
    )
    {
        var warehouse = warehouseId.HasValue
            ? unitOfWork.GetRepository<Warehouse>().Get(warehouseId.Value)?.Name ?? string.Empty
            : SiteContext.Current.WarehouseDto?.Name ?? string.Empty;

        return this.GetGetInventoryResult(result, product, warehouse);
    }

    private GetInventoryResult GetGetInventoryResult(
        PriceAvailabilityResult result,
        Product product,
        string warehouse
    )
    {
        var responseItems = result.PriceAvailabilityResponse.Response.Items
            .Where(
                o =>
                    o.ItemNumber.Equals(product.ErpNumber, StringComparison.OrdinalIgnoreCase)
                    && o.ErrorMessage.IsBlank()
            )
            .ToList();

        var responseItem = this.GetResponseItemByWarehouse(responseItems, warehouse);
        if (responseItem == null)
        {
            return null;
        }

        return this.CreateGetInventoryResult(responseItems, responseItem, product);
    }

    private ResponseItem GetResponseItemByWarehouse(
        List<ResponseItem> responseItems,
        string warehouse
    )
    {
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

    private List<WarehouseQtyOnHandDto> GetWarehouseQtyOnHandDtos(List<ResponseItem> responseItems)
    {
        return responseItems
            .Select(
                o =>
                    new WarehouseQtyOnHandDto
                    {
                        Name = o.WarehouseID,
                        Description = o.WarehouseID,
                        QtyOnHand = o.QuantityAvailable
                    }
            )
            .ToList();
    }

    private GetInventoryResult CreateGetInventoryResult(
        List<ResponseItem> responseItems,
        ResponseItem responseItem,
        Product product
    )
    {
        var warehouseQtyOnHandDtos = this.GetWarehouseQtyOnHandDtos(responseItems);

        return new GetInventoryResult
        {
            Inventories = new Dictionary<Guid, ProductInventory>
            {
                {
                    product.Id,
                    new ProductInventory
                    {
                        QtyOnHand = responseItem.QuantityAvailable,
                        WarehouseQtyOnHandDtos = warehouseQtyOnHandDtos
                    }
                }
            }
        };
    }

    private Dictionary<Guid, ProductInventory> GetInventories(
        List<GetInventoryResult> getInventoryResults
    )
    {
        var inventories = new Dictionary<Guid, ProductInventory>();

        foreach (var getInventoryResult in getInventoryResults)
        {
            foreach (var productInventory in getInventoryResult.Inventories)
            {
                if (!inventories.ContainsKey(productInventory.Key))
                {
                    inventories.Add(productInventory.Key, productInventory.Value);
                }
            }
        }

        return inventories;
    }
}
