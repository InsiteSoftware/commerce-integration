namespace Insite.Integration.Connector.Base.Pipelines.Pipes.GetInventoryStock;

using System;
using System.Collections.Generic;
using Insite.Core.Plugins.Inventory;
using Insite.Data.Entities;

public class GetInventoryFromResponseBase
{
    protected static GetInventoryResult CreateGetInventoryResult(
        Product product,
        decimal qtyOnHand,
        List<WarehouseQtyOnHandDto> warehouseQtyOnHandDtos
    )
    {
        return new GetInventoryResult
        {
            Inventories = new Dictionary<Guid, ProductInventory>
            {
                {
                    product.Id,
                    new ProductInventory
                    {
                        QtyOnHand = qtyOnHand,
                        WarehouseQtyOnHandDtos = warehouseQtyOnHandDtos
                    }
                }
            }
        };
    }

    protected static GetInventoryResult CreateGetInventoryResult(
        List<GetInventoryResult> getInventoryResults
    )
    {
        return new GetInventoryResult
        {
            Inventories = GetInventories(getInventoryResults),
            RequiresRealTimeInventory = false
        };
    }

    private static Dictionary<Guid, ProductInventory> GetInventories(
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
