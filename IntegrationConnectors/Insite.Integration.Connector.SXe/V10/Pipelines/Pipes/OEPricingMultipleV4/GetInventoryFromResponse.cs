namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.OEPricingMultipleV4;

using System;
using System.Collections.Generic;
using System.Linq;

using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.OEPricingMultipleV4;

public sealed class GetInventoryFromResponse
    : IPipe<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    public int Order => 700;

    public OEPricingMultipleV4Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Parameter parameter,
        OEPricingMultipleV4Result result
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
        OEPricingMultipleV4Result result,
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
        OEPricingMultipleV4Result result,
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
        OEPricingMultipleV4Result result,
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
        OEPricingMultipleV4Result result,
        Product product,
        string unitOfMeasure,
        string warehouse
    )
    {
        var outPrice3s = this.GetOutPrice3sByErpNumberAndUnitOfMeasure(
            result,
            product.ErpNumber,
            unitOfMeasure
        );
        var outPrice3 = this.GetOutPrice3ByWarehouse(outPrice3s, warehouse);

        if (outPrice3 == null)
        {
            return null;
        }

        return this.CreateGetInventoryResult(outPrice3s, outPrice3, product);
    }

    private List<Outprice3> GetOutPrice3sByErpNumberAndUnitOfMeasure(
        OEPricingMultipleV4Result getOEPricingMultipleV4Result,
        string erpNumber,
        string unitOfMeasure
    )
    {
        if (getOEPricingMultipleV4Result.OEPricingMultipleV4Response.Outprice == null)
        {
            return new List<Outprice3>();
        }

        var outPrice3sByErpNumber =
            getOEPricingMultipleV4Result.OEPricingMultipleV4Response.Outprice.Where(
                o => o.ProductCode.Equals(erpNumber, StringComparison.OrdinalIgnoreCase)
            );

        var outPrice3sByUnitOfMeasure = outPrice3sByErpNumber.Where(
            o => o.UnitOfMeasure.Equals(unitOfMeasure, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPrice3sByUnitOfMeasure.Any())
        {
            outPrice3sByUnitOfMeasure = outPrice3sByErpNumber;
        }

        return outPrice3sByUnitOfMeasure.ToList();
    }

    private Outprice3 GetOutPrice3ByWarehouse(List<Outprice3> outPrice3s, string warehouse)
    {
        var outPrice3sByWarehouse = outPrice3s.Where(
            o => o.Warehouse.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPrice3sByWarehouse.Any())
        {
            outPrice3sByWarehouse = outPrice3s.Where(
                o =>
                    o.Warehouse.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!outPrice3sByWarehouse.Any())
        {
            outPrice3sByWarehouse = outPrice3s;
        }

        return outPrice3sByWarehouse.OrderBy(o => o.SequenceNumber).FirstOrDefault();
    }

    private GetInventoryResult CreateGetInventoryResult(
        List<Outprice3> outPrice3s,
        Outprice3 outPrice3,
        Product product
    )
    {
        var warehouseQtyOnHandDtos = this.GetWarehouseQtyOnHandDtos(outPrice3s);
        var qtyOnHand = this.GetOutPrice3QtyOnHand(outPrice3);

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

    private List<WarehouseQtyOnHandDto> GetWarehouseQtyOnHandDtos(List<Outprice3> outPrice3s)
    {
        var warehouseQtyOnHandDtos = new List<WarehouseQtyOnHandDto>();

        foreach (var outPrice3 in outPrice3s)
        {
            var qtyOnHand = this.GetOutPrice3QtyOnHand(outPrice3);

            var warehouseQtyOnHandDto = new WarehouseQtyOnHandDto
            {
                Name = outPrice3.Warehouse,
                Description = outPrice3.Warehouse,
                QtyOnHand = qtyOnHand
            };

            warehouseQtyOnHandDtos.Add(warehouseQtyOnHandDto);
        }

        return warehouseQtyOnHandDtos;
    }

    private decimal GetOutPrice3QtyOnHand(Outprice3 outPrice3)
    {
        var qtyOnHand = outPrice3.NetAvailable;

        if (outPrice3.UnitConversion > 0)
        {
            qtyOnHand = qtyOnHand * outPrice3.UnitConversion;
        }

        return qtyOnHand;
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
