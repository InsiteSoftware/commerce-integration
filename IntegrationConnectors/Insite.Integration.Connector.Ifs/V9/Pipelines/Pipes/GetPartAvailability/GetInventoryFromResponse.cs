namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetPartAvailability;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class GetInventoryFromResponse
    : IPipe<GetPartAvailabilityParameter, GetPartAvailabilityResult>
{
    public int Order => 700;

    public GetPartAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        GetPartAvailabilityParameter parameter,
        GetPartAvailabilityResult result
    )
    {
        var getInventoryResults = new List<GetInventoryResult>();

        foreach (var productId in parameter.GetInventoryParameter?.ProductIds)
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

        result.GetInventoryResult = new GetInventoryResult
        {
            Inventories = this.GetInventories(getInventoryResults),
            RequiresRealTimeInventory = false
        };

        return result;
    }

    private GetInventoryResult GetGetInventoryResult(
        IUnitOfWork unitOfWork,
        GetPartAvailabilityResult result,
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
        GetPartAvailabilityResult result,
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
        GetPartAvailabilityResult result,
        Product product,
        string warehouse
    )
    {
        var partAvailabilityResDatas = this.GetPartAvailabilityResDatasByErpNumber(
            result,
            product.ErpNumber
        );
        var partAvailabilityResData = this.GetPartAvailabilityResDataByWarehouse(
            partAvailabilityResDatas,
            warehouse
        );

        if (partAvailabilityResData == null)
        {
            return null;
        }

        return this.CreateGetInventoryResult(
            partAvailabilityResDatas,
            partAvailabilityResData,
            product
        );
    }

    private List<partAvailabilityResData> GetPartAvailabilityResDatasByErpNumber(
        GetPartAvailabilityResult result,
        string erpNumber
    )
    {
        if (result.PartAvailabilityResponse?.partsAvailabile == null)
        {
            return new List<partAvailabilityResData>();
        }

        return result.PartAvailabilityResponse.partsAvailabile
            .Where(o => o.productNo != null)
            .Where(o => o.productNo.Equals(erpNumber, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private partAvailabilityResData GetPartAvailabilityResDataByWarehouse(
        List<partAvailabilityResData> partAvailabilityResDatas,
        string warehouse
    )
    {
        var partAvailabilityResDatasByWarehouse = partAvailabilityResDatas
            .Where(o => o.partsAvailableSite != null)
            .Where(o => o.partsAvailableSite.Equals(warehouse, StringComparison.OrdinalIgnoreCase));

        if (!partAvailabilityResDatasByWarehouse.Any())
        {
            partAvailabilityResDatasByWarehouse = partAvailabilityResDatas
                .Where(o => o.partsAvailableSite != null)
                .Where(
                    o =>
                        o.partsAvailableSite.Equals(
                            SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                            StringComparison.OrdinalIgnoreCase
                        )
                );
        }

        if (!partAvailabilityResDatasByWarehouse.Any())
        {
            partAvailabilityResDatasByWarehouse = partAvailabilityResDatas;
        }

        return partAvailabilityResDatasByWarehouse.FirstOrDefault();
    }

    private GetInventoryResult CreateGetInventoryResult(
        List<partAvailabilityResData> partAvailabilityResDatas,
        partAvailabilityResData partAvailabilityResData,
        Product product
    )
    {
        var warehouseQtyOnHandDtos = this.GetWarehouseQtyOnHandDtos(partAvailabilityResDatas);

        return new GetInventoryResult
        {
            Inventories = new Dictionary<Guid, ProductInventory>
            {
                {
                    product.Id,
                    new ProductInventory
                    {
                        QtyOnHand = partAvailabilityResData.quantityAvailable,
                        WarehouseQtyOnHandDtos = warehouseQtyOnHandDtos
                    }
                }
            }
        };
    }

    private List<WarehouseQtyOnHandDto> GetWarehouseQtyOnHandDtos(
        List<partAvailabilityResData> partAvailabilityResDatas
    )
    {
        var warehouseQtyOnHandDtos = new List<WarehouseQtyOnHandDto>();

        foreach (var partAvailabilityResData in partAvailabilityResDatas)
        {
            var warehouseQtyOnHandDto = new WarehouseQtyOnHandDto
            {
                Name = partAvailabilityResData.partsAvailableSite,
                Description = partAvailabilityResData.partsAvailableSite,
                QtyOnHand = partAvailabilityResData.quantityAvailable
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
