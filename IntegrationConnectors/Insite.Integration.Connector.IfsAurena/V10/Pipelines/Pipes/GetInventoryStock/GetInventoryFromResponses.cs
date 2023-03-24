namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetInventoryStock;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.Base.Pipelines.Pipes.GetInventoryStock;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class GetInventoryFromResponses
    : GetInventoryFromResponseBase,
        IPipe<GetInventoryStockParameter, GetInventoryStockResult>
{
    private readonly IProductHelper productHelper;

    private readonly IWarehouseHelper warehouseHelper;

    public GetInventoryFromResponses(IProductHelper productHelper, IWarehouseHelper warehouseHelper)
    {
        this.productHelper = productHelper;
        this.warehouseHelper = warehouseHelper;
    }

    public int Order => 400;

    public GetInventoryStockResult Execute(
        IUnitOfWork unitOfWork,
        GetInventoryStockParameter parameter,
        GetInventoryStockResult result
    )
    {
        var products = this.productHelper.GetProducts(unitOfWork, parameter.GetInventoryParameter);
        var warehouse = this.warehouseHelper.GetWarehouse(
            unitOfWork,
            parameter.GetInventoryParameter
        );
        var getInventoryResults = new List<GetInventoryResult>();

        foreach (var product in products)
        {
            var getInventoryResult = this.GetGetInventoryResult(result, product, warehouse);
            if (getInventoryResult != null)
            {
                getInventoryResults.Add(getInventoryResult);
            }
        }

        result.GetInventoryResult = CreateGetInventoryResult(getInventoryResults);

        return result;
    }

    private GetInventoryResult GetGetInventoryResult(
        GetInventoryStockResult result,
        Product product,
        WarehouseDto warehouse
    )
    {
        var inventoryPartInStockResponses = result.InventoryPartInStockResponses
            .Where(o => o.PartNo.Equals(product.ProductCode, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var qtyOnHand = this.CalculateQtyOnHand(inventoryPartInStockResponses, warehouse);
        var warehouseQtyOnHandDtos = this.GetWarehouseQtyOnHandDtos(inventoryPartInStockResponses);

        return CreateGetInventoryResult(product, qtyOnHand, warehouseQtyOnHandDtos);
    }

    private decimal CalculateQtyOnHand(
        List<InventoryPartInStock> inventoryPartInStocks,
        WarehouseDto warehouse
    )
    {
        if (!inventoryPartInStocks.Any() || warehouse == null)
        {
            return 0;
        }

        return inventoryPartInStocks
            .Where(o => o.Contract.EqualsIgnoreCase(warehouse.Name))
            .Sum(p => p.AvailableQty.Value);
    }

    private List<WarehouseQtyOnHandDto> GetWarehouseQtyOnHandDtos(
        List<InventoryPartInStock> inventoryPartInStockResponses
    )
    {
        return inventoryPartInStockResponses
            .GroupBy(o => o.Contract)
            .Select(
                p =>
                    new WarehouseQtyOnHandDto
                    {
                        Name = p.Key,
                        Description = p.Key,
                        QtyOnHand = p.Sum(q => q.AvailableQty.Value)
                    }
            )
            .ToList();
    }
}
