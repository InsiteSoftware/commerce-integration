namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class GetInventoryFromResponse : IPipe<GetItemPriceParameter, GetItemPriceResult>
{
    public int Order => 900;

    public GetItemPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetItemPriceParameter parameter,
        GetItemPriceResult result
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
        GetItemPriceResult result,
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

        return this.GetGetInventoryResult(
            result,
            product,
            pricingServiceParameter.UnitOfMeasure,
            pricingServiceParameter.Warehouse
        );
    }

    private GetInventoryResult GetGetInventoryResult(
        IUnitOfWork unitOfWork,
        GetItemPriceResult result,
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
        GetItemPriceResult result,
        Product product,
        Guid? warehouseId
    )
    {
        var warehouse =
            warehouseId != null
                ? unitOfWork.GetRepository<Warehouse>().Get(warehouseId.Value)?.Name ?? string.Empty
                : string.Empty;

        return this.GetGetInventoryResult(result, product, product.UnitOfMeasure, warehouse);
    }

    private GetInventoryResult GetGetInventoryResult(
        GetItemPriceResult result,
        Product product,
        string unitOfMeasure,
        string warehouse
    )
    {
        var replyItem = this.GetReplyItem(
            product.ErpNumber,
            unitOfMeasure,
            result.GetItemPriceReply.Reply
        );
        if (replyItem == null)
        {
            return null;
        }

        return this.CreateGetInventoryResult(replyItem, product);
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

    private GetInventoryResult CreateGetInventoryResult(ReplyItem replyItem, Product product)
    {
        var warehouseQtyOnHandDtos = this.GetWarehouseQtyOnHandDtos(replyItem);

        return new GetInventoryResult
        {
            Inventories = new Dictionary<Guid, ProductInventory>
            {
                {
                    product.Id,
                    new ProductInventory
                    {
                        QtyOnHand = this.ParseFreeQuantity(replyItem.FreeQuantity),
                        WarehouseQtyOnHandDtos = warehouseQtyOnHandDtos
                    }
                }
            }
        };
    }

    private List<WarehouseQtyOnHandDto> GetWarehouseQtyOnHandDtos(ReplyItem replyItem)
    {
        var warehouseQtyOnHandDtos = new List<WarehouseQtyOnHandDto>();

        if (replyItem.ListOfItemLocationQuantities == null)
        {
            return warehouseQtyOnHandDtos;
        }

        foreach (var itemLocationQuantity in replyItem.ListOfItemLocationQuantities)
        {
            var warehouseQtyOnHandDto = new WarehouseQtyOnHandDto
            {
                Name = itemLocationQuantity.LocationID,
                Description = itemLocationQuantity.LocationID,
                QtyOnHand = this.ParseFreeQuantity(itemLocationQuantity.FreeQuantity)
            };

            warehouseQtyOnHandDtos.Add(warehouseQtyOnHandDto);
        }

        return warehouseQtyOnHandDtos;
    }

    private decimal ParseFreeQuantity(string freeQuantity)
    {
        decimal.TryParse(freeQuantity, out var freeQuantityDecimal);

        return freeQuantityDecimal;
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
