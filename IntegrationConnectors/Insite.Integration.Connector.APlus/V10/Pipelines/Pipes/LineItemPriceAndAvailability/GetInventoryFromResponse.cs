namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;

using System;
using System.Collections.Generic;
using System.Linq;

using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class GetInventoryFromResponse
    : IPipe<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    public int Order => 700;

    public LineItemPriceAndAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        LineItemPriceAndAvailabilityParameter parameter,
        LineItemPriceAndAvailabilityResult result
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
        LineItemPriceAndAvailabilityResult result,
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
        LineItemPriceAndAvailabilityResult result,
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
        LineItemPriceAndAvailabilityResult result,
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
        LineItemPriceAndAvailabilityResult result,
        Product product,
        string unitOfMeasure,
        string warehouse
    )
    {
        var warehouseInfos = this.GetWarehouseInfosByItemNumberAndUnitOfMeasure(
            result,
            product.ErpNumber,
            unitOfMeasure
        );
        var warehouseInfo = this.GetWarehouseInfoByWarehouse(warehouseInfos, warehouse);

        if (warehouseInfo == null)
        {
            return null;
        }

        return this.CreateGetInventoryResult(warehouseInfos, warehouseInfo, product);
    }

    private List<ResponseWarehouseInfo> GetWarehouseInfosByItemNumberAndUnitOfMeasure(
        LineItemPriceAndAvailabilityResult result,
        string erpNumber,
        string unitOfMeasure
    )
    {
        if (result.LineItemPriceAndAvailabilityResponse.ItemAvailability == null)
        {
            return new List<ResponseWarehouseInfo>();
        }

        var itemAvailability =
            result.LineItemPriceAndAvailabilityResponse.ItemAvailability.FirstOrDefault(
                o => o.Item.ItemNumber.Equals(erpNumber, StringComparison.OrdinalIgnoreCase)
            );

        if (itemAvailability == null || itemAvailability.WarehouseInfo == null)
        {
            return new List<ResponseWarehouseInfo>();
        }

        var warehouseInfosByUnitOfMeasure = itemAvailability.WarehouseInfo.Where(
            o => o.PricingUOM.Equals(unitOfMeasure, StringComparison.OrdinalIgnoreCase)
        );
        if (!warehouseInfosByUnitOfMeasure.Any())
        {
            warehouseInfosByUnitOfMeasure = itemAvailability.WarehouseInfo;
        }

        return warehouseInfosByUnitOfMeasure.ToList();
    }

    private ResponseWarehouseInfo GetWarehouseInfoByWarehouse(
        List<ResponseWarehouseInfo> warehouseInfos,
        string warehouse
    )
    {
        var warehouseInfosByWarehouse = warehouseInfos.Where(
            o => o.Warehouse.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (!warehouseInfosByWarehouse.Any())
        {
            warehouseInfosByWarehouse = warehouseInfos.Where(
                o =>
                    o.Warehouse.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!warehouseInfosByWarehouse.Any())
        {
            warehouseInfosByWarehouse = warehouseInfos;
        }

        return warehouseInfosByWarehouse.FirstOrDefault();
    }

    private List<WarehouseQtyOnHandDto> GetWarehouseQtyOnHandDtos(
        List<ResponseWarehouseInfo> warehouseInfos
    )
    {
        var warehouseQtyOnHandDtos = new List<WarehouseQtyOnHandDto>();

        foreach (var warehouseInfo in warehouseInfos)
        {
            decimal.TryParse(warehouseInfo.Qty, out var quantityOnHand);

            var warehouseQtyOnHandDto = new WarehouseQtyOnHandDto
            {
                Name = warehouseInfo.Warehouse,
                Description = warehouseInfo.Warehouse,
                QtyOnHand = quantityOnHand
            };

            warehouseQtyOnHandDtos.Add(warehouseQtyOnHandDto);
        }

        return warehouseQtyOnHandDtos;
    }

    private GetInventoryResult CreateGetInventoryResult(
        List<ResponseWarehouseInfo> warehouseInfos,
        ResponseWarehouseInfo warehouseInfo,
        Product product
    )
    {
        var warehouseQtyOnHandDtos = this.GetWarehouseQtyOnHandDtos(warehouseInfos);

        decimal.TryParse(warehouseInfo.Qty, out var quantityOnHand);

        return new GetInventoryResult
        {
            Inventories = new Dictionary<Guid, ProductInventory>
            {
                {
                    product.Id,
                    new ProductInventory
                    {
                        QtyOnHand = quantityOnHand,
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
