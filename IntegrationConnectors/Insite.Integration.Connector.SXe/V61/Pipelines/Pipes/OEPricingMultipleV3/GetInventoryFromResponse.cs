namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;

using System;
using System.Collections.Generic;
using System.Linq;

using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;

public sealed class GetInventoryFromResponse
    : IPipe<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
{
    public int Order => 700;

    public OEPricingMultipleV3Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV3Parameter parameter,
        OEPricingMultipleV3Result result
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
        OEPricingMultipleV3Result result,
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
        OEPricingMultipleV3Result result,
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
        OEPricingMultipleV3Result result,
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
        OEPricingMultipleV3Result result,
        Product product,
        string unitOfMeasure,
        string warehouse
    )
    {
        var outPrice2s = this.GetOutPrice2sByErpNumberAndUnitOfMeasure(
            result,
            product.ErpNumber,
            unitOfMeasure
        );
        var outPrice2 = this.GetOutPrice2ByWarehouse(outPrice2s, warehouse);

        if (outPrice2 == null)
        {
            return null;
        }

        return this.CreateGetInventoryResult(outPrice2s, outPrice2, product);
    }

    private List<OEPricingMultipleV3outputPrice> GetOutPrice2sByErpNumberAndUnitOfMeasure(
        OEPricingMultipleV3Result getOEPricingMultipleV3Result,
        string erpNumber,
        string unitOfMeasure
    )
    {
        if (getOEPricingMultipleV3Result.OEPricingMultipleV3Response.arrayPrice == null)
        {
            return new List<OEPricingMultipleV3outputPrice>();
        }

        var outPrice2sByErpNumber =
            getOEPricingMultipleV3Result.OEPricingMultipleV3Response.arrayPrice.Where(
                o => o.productCode.Equals(erpNumber, StringComparison.OrdinalIgnoreCase)
            );

        var outPrice2sByUnitOfMeasure = outPrice2sByErpNumber.Where(
            o => o.unitOfMeasure.Equals(unitOfMeasure, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPrice2sByUnitOfMeasure.Any())
        {
            outPrice2sByUnitOfMeasure = outPrice2sByErpNumber;
        }

        return outPrice2sByUnitOfMeasure.ToList();
    }

    private OEPricingMultipleV3outputPrice GetOutPrice2ByWarehouse(
        List<OEPricingMultipleV3outputPrice> outPrice2s,
        string warehouse
    )
    {
        var outPrice2sByWarehouse = outPrice2s.Where(
            o => o.warehouse.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (!outPrice2sByWarehouse.Any())
        {
            outPrice2sByWarehouse = outPrice2s.Where(
                o =>
                    o.warehouse.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!outPrice2sByWarehouse.Any())
        {
            outPrice2sByWarehouse = outPrice2s;
        }

        return outPrice2sByWarehouse.OrderBy(o => o.sequenceNumber).FirstOrDefault();
    }

    private GetInventoryResult CreateGetInventoryResult(
        List<OEPricingMultipleV3outputPrice> outPrice2s,
        OEPricingMultipleV3outputPrice outPrice2,
        Product product
    )
    {
        var warehouseQtyOnHandDtos = this.GetWarehouseQtyOnHandDtos(outPrice2s);
        var qtyOnHand = this.GetOutPrice2QtyOnHand(outPrice2);

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

    private List<WarehouseQtyOnHandDto> GetWarehouseQtyOnHandDtos(
        List<OEPricingMultipleV3outputPrice> outPrice2s
    )
    {
        var warehouseQtyOnHandDtos = new List<WarehouseQtyOnHandDto>();

        foreach (var outPrice2 in outPrice2s)
        {
            var qtyOnHand = this.GetOutPrice2QtyOnHand(outPrice2);

            var warehouseQtyOnHandDto = new WarehouseQtyOnHandDto
            {
                Name = outPrice2.warehouse,
                Description = outPrice2.warehouse,
                QtyOnHand = qtyOnHand
            };

            warehouseQtyOnHandDtos.Add(warehouseQtyOnHandDto);
        }

        return warehouseQtyOnHandDtos;
    }

    private decimal GetOutPrice2QtyOnHand(OEPricingMultipleV3outputPrice outPrice2)
    {
        var qtyOnHand = outPrice2.netAvailable;

        if (outPrice2.unitConversion > 0)
        {
            qtyOnHand = qtyOnHand * outPrice2.unitConversion;
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
