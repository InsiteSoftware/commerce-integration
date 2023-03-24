namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.InventoryAllocationInquiry;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.InventoryAllocationInquiry;

public sealed class GetInventoryFromResponses
    : IPipe<InventoryAllocationInquiryParameter, InventoryAllocationInquiryResult>
{
    public int Order => 400;

    public InventoryAllocationInquiryResult Execute(
        IUnitOfWork unitOfWork,
        InventoryAllocationInquiryParameter parameter,
        InventoryAllocationInquiryResult result
    )
    {
        var getInventoryResults = new List<GetInventoryResult>();

        foreach (var product in parameter.GetInventoryParameter?.Products)
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

        foreach (
            var productId in parameter.GetInventoryParameter?.ProductIds.Where(
                o => !parameter.GetInventoryParameter.Products.Any(p => p.Id == o)
            )
        )
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

        result.GetInventoryResult = new GetInventoryResult
        {
            Inventories = this.GetInventories(getInventoryResults),
            RequiresRealTimeInventory = false
        };

        return result;
    }

    private GetInventoryResult GetGetInventoryResult(
        IUnitOfWork unitOfWork,
        InventoryAllocationInquiryResult result,
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
        InventoryAllocationInquiryResult result,
        Product product,
        Guid? warehouseId
    )
    {
        var warehouse =
            warehouseId != null
                ? unitOfWork.GetRepository<Warehouse>().Get(warehouseId.Value)?.Name ?? string.Empty
                : string.Empty;

        return this.GetGetInventoryResult(result, product, warehouse);
    }

    private GetInventoryResult GetGetInventoryResult(
        InventoryAllocationInquiryResult result,
        Product product,
        string warehouse
    )
    {
        var inventoryAllocationInquiryResponses =
            this.GetInventoryAllocationInquiryResponsesByErpNumber(result, product.ErpNumber);
        var inventoryAllocationInquiryResponse =
            this.GetInventoryAllocationInquiryResponseByWarehouse(result, warehouse);

        if (inventoryAllocationInquiryResponse == null)
        {
            return null;
        }

        return this.CreateGetInventoryResult(
            inventoryAllocationInquiryResponses,
            inventoryAllocationInquiryResponse,
            product
        );
    }

    private List<InventoryAllocationInquiry> GetInventoryAllocationInquiryResponsesByErpNumber(
        InventoryAllocationInquiryResult result,
        string erpNumber
    )
    {
        return result.InventoryAllocationInquiryResponses
            .Where(o => o.InventoryID.Equals(erpNumber, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private InventoryAllocationInquiry GetInventoryAllocationInquiryResponseByWarehouse(
        InventoryAllocationInquiryResult result,
        string warehouse
    )
    {
        var inventoryAllocationInquiryResponsesByWarehouse =
            result.InventoryAllocationInquiryResponses.Where(
                o => o.WarehouseID.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
            );

        if (!inventoryAllocationInquiryResponsesByWarehouse.Any())
        {
            inventoryAllocationInquiryResponsesByWarehouse =
                result.InventoryAllocationInquiryResponses.Where(
                    o =>
                        o.WarehouseID.Equals(
                            SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                            StringComparison.OrdinalIgnoreCase
                        )
                );
        }

        if (!inventoryAllocationInquiryResponsesByWarehouse.Any())
        {
            inventoryAllocationInquiryResponsesByWarehouse =
                result.InventoryAllocationInquiryResponses;
        }

        return inventoryAllocationInquiryResponsesByWarehouse.FirstOrDefault();
    }

    private GetInventoryResult CreateGetInventoryResult(
        List<InventoryAllocationInquiry> inventoryAllocationInquiryResponses,
        InventoryAllocationInquiry inventoryAllocationInquiryResponse,
        Product product
    )
    {
        var warehouseQtyOnHandDtos = this.GetWarehouseQtyOnHandDtos(
            inventoryAllocationInquiryResponses
        );

        return new GetInventoryResult
        {
            Inventories = new Dictionary<Guid, ProductInventory>
            {
                {
                    product.Id,
                    new ProductInventory
                    {
                        QtyOnHand = inventoryAllocationInquiryResponse.AvailableForShipping,
                        WarehouseQtyOnHandDtos = warehouseQtyOnHandDtos
                    }
                }
            }
        };
    }

    private List<WarehouseQtyOnHandDto> GetWarehouseQtyOnHandDtos(
        List<InventoryAllocationInquiry> inventoryAllocationInquiryResponses
    )
    {
        var warehouseQtyOnHandDtos = new List<WarehouseQtyOnHandDto>();

        foreach (var inventoryAllocationInquiryResponse in inventoryAllocationInquiryResponses)
        {
            var warehouseQtyOnHandDto = new WarehouseQtyOnHandDto
            {
                Name = inventoryAllocationInquiryResponse.WarehouseID,
                Description = inventoryAllocationInquiryResponse.WarehouseID,
                QtyOnHand = inventoryAllocationInquiryResponse.AvailableForShipping
            };

            warehouseQtyOnHandDtos.Add(warehouseQtyOnHandDto);
        }

        return warehouseQtyOnHandDtos;
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
