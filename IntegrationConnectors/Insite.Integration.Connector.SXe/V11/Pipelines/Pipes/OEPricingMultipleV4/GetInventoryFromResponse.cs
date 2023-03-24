namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.OEPricingMultipleV4;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;

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
        var priceOutV2s = this.GetPriceOutV2sByErpNumberAndUnitOfMeasure(
            result,
            product.ErpNumber,
            unitOfMeasure
        );
        var priceOutV2 = this.GetPriceOutV2ByWarehouse(priceOutV2s, warehouse);

        if (priceOutV2 == null)
        {
            return null;
        }

        return this.CreateGetInventoryResult(priceOutV2s, priceOutV2, product);
    }

    private List<PriceOutV2> GetPriceOutV2sByErpNumberAndUnitOfMeasure(
        OEPricingMultipleV4Result result,
        string erpNumber,
        string unitOfMeasure
    )
    {
        if (result.OEPricingMultipleV4Response?.Response?.PriceOutV2Collection?.PriceOutV2s == null)
        {
            return new List<PriceOutV2>();
        }

        var priceOutV2sByErpNumber =
            result.OEPricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s.Where(
                o => o.Prod.Equals(erpNumber, StringComparison.OrdinalIgnoreCase)
            );

        var priceOutV2sByUnitOfMeasure = priceOutV2sByErpNumber.Where(
            o => o.Unit.Equals(unitOfMeasure, StringComparison.OrdinalIgnoreCase)
        );
        if (!priceOutV2sByUnitOfMeasure.Any())
        {
            priceOutV2sByUnitOfMeasure = priceOutV2sByErpNumber;
        }

        return priceOutV2sByUnitOfMeasure.ToList();
    }

    private PriceOutV2 GetPriceOutV2ByWarehouse(List<PriceOutV2> priceOutV2s, string warehouse)
    {
        var priceOutV2sByWarehouse = priceOutV2s.Where(
            o => o.Whse.Equals(warehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (!priceOutV2sByWarehouse.Any())
        {
            priceOutV2sByWarehouse = priceOutV2s.Where(
                o =>
                    o.Whse.Equals(
                        SiteContext.Current.WarehouseDto?.Name ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        if (!priceOutV2sByWarehouse.Any())
        {
            priceOutV2sByWarehouse = priceOutV2s;
        }

        return priceOutV2sByWarehouse.OrderBy(o => o.Seqno).FirstOrDefault();
    }

    private GetInventoryResult CreateGetInventoryResult(
        List<PriceOutV2> priceOutV2s,
        PriceOutV2 priceOutV2,
        Product product
    )
    {
        var warehouseQtyOnHandDtos = this.GetWarehouseQtyOnHandDtos(priceOutV2s);
        var qtyOnHand = this.GetPriceOutV2QtyOnHand(priceOutV2);

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

    private List<WarehouseQtyOnHandDto> GetWarehouseQtyOnHandDtos(List<PriceOutV2> priceOutV2s)
    {
        var warehouseQtyOnHandDtos = new List<WarehouseQtyOnHandDto>();

        foreach (var priceOutV2 in priceOutV2s)
        {
            var qtyOnHand = this.GetPriceOutV2QtyOnHand(priceOutV2);

            var warehouseQtyOnHandDto = new WarehouseQtyOnHandDto
            {
                Name = priceOutV2.Whse,
                Description = priceOutV2.Whse,
                QtyOnHand = qtyOnHand
            };

            warehouseQtyOnHandDtos.Add(warehouseQtyOnHandDto);
        }

        return warehouseQtyOnHandDtos;
    }

    private decimal GetPriceOutV2QtyOnHand(PriceOutV2 priceOutV2)
    {
        var qtyOnHand = priceOutV2.Netavail;

        if (priceOutV2.Unitconv > 0)
        {
            qtyOnHand = qtyOnHand * priceOutV2.Unitconv;
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
