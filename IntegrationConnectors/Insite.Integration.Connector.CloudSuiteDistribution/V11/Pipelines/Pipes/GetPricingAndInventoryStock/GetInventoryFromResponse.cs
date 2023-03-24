namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Pipelines.Pipes.GetInventoryStock;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Data.Entities.Dtos;

public sealed class GetInventoryFromResponse
    : GetInventoryFromResponseBase,
        IPipe<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
{
    private readonly IProductHelper productHelper;

    private readonly IWarehouseHelper warehouseHelper;

    public GetInventoryFromResponse(IProductHelper productHelper, IWarehouseHelper warehouseHelper)
    {
        this.productHelper = productHelper;
        this.warehouseHelper = warehouseHelper;
    }

    public int Order => 700;

    public GetPricingAndInventoryStockResult Execute(
        IUnitOfWork unitOfWork,
        GetPricingAndInventoryStockParameter parameter,
        GetPricingAndInventoryStockResult result
    )
    {
        var getInventoryResults = new List<GetInventoryResult>();

        if (parameter.PricingServiceParameters.Any())
        {
            var productRepository = unitOfWork.GetRepository<Product>();
            var warehouse = this.warehouseHelper.GetWarehouse(
                unitOfWork,
                parameter.PricingServiceParameters.First()
            );

            foreach (var pricingServiceParameter in parameter.PricingServiceParameters)
            {
                var product =
                    pricingServiceParameter.Product
                    ?? productRepository.Get(pricingServiceParameter.ProductId);
                var getInventoryResult = GetGetInventoryResult(
                    result,
                    product,
                    pricingServiceParameter.UnitOfMeasure,
                    warehouse
                );

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
            var products = this.productHelper.GetProducts(
                unitOfWork,
                parameter.GetInventoryParameter
            );
            var warehouse = this.warehouseHelper.GetWarehouse(
                unitOfWork,
                parameter.GetInventoryParameter
            );

            foreach (var product in products)
            {
                var getInventoryResult = GetGetInventoryResult(
                    result,
                    product,
                    product.UnitOfMeasure,
                    warehouse
                );

                getInventoryResults.Add(getInventoryResult);
            }
        }

        result.GetInventoryResult = CreateGetInventoryResult(getInventoryResults);

        return result;
    }

    private static GetInventoryResult GetGetInventoryResult(
        GetPricingAndInventoryStockResult result,
        Product product,
        string unitOfMeasure,
        WarehouseDto warehouse
    )
    {
        var priceOutV2s = GetPriceOutV2sByErpNumberAndUnitOfMeasure(result, product, unitOfMeasure);
        var priceOutV2 = GetPriceOutV2ByWarehouse(priceOutV2s, warehouse);

        var qtyOnHand = GetPriceOutV2QtyOnHand(priceOutV2);
        var warehouseQtyOnHandDtos = GetWarehouseQtyOnHandDtos(priceOutV2s);

        return CreateGetInventoryResult(product, qtyOnHand, warehouseQtyOnHandDtos);
    }

    private static List<PriceOutV2> GetPriceOutV2sByErpNumberAndUnitOfMeasure(
        GetPricingAndInventoryStockResult result,
        Product product,
        string unitOfMeasure
    )
    {
        if (result.OePricingMultipleV4Response?.Response?.PriceOutV2Collection?.PriceOutV2s == null)
        {
            return new List<PriceOutV2>();
        }

        var priceOutV2sByErpNumber =
            result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s.Where(
                o => o.Prod.EqualsIgnoreCase(product.ErpNumber)
            );

        var priceOutV2sByUnitOfMeasure = priceOutV2sByErpNumber.Where(
            o => o.Unit.EqualsIgnoreCase(unitOfMeasure)
        );
        if (!priceOutV2sByUnitOfMeasure.Any())
        {
            priceOutV2sByUnitOfMeasure = priceOutV2sByErpNumber;
        }

        return priceOutV2sByUnitOfMeasure.ToList();
    }

    private static PriceOutV2 GetPriceOutV2ByWarehouse(
        List<PriceOutV2> priceOutV2s,
        WarehouseDto warehouse
    )
    {
        var priceOutV2sByWarehouse = priceOutV2s.Where(
            o => o.Whse.EqualsIgnoreCase(warehouse?.Name)
        );
        if (!priceOutV2sByWarehouse.Any())
        {
            priceOutV2sByWarehouse = priceOutV2s;
        }

        return priceOutV2sByWarehouse.OrderBy(o => o.SeqNo).FirstOrDefault();
    }

    private static List<WarehouseQtyOnHandDto> GetWarehouseQtyOnHandDtos(
        List<PriceOutV2> priceOutV2s
    )
    {
        return priceOutV2s
            .Select(
                o =>
                    new WarehouseQtyOnHandDto
                    {
                        Name = o.Whse,
                        Description = o.Whse,
                        QtyOnHand = GetPriceOutV2QtyOnHand(o)
                    }
            )
            .ToList();
    }

    private static decimal GetPriceOutV2QtyOnHand(PriceOutV2 priceOutV2)
    {
        if (priceOutV2 == null)
        {
            return 0;
        }

        var qtyOnHand = priceOutV2.NetAvail;

        if (priceOutV2.UnitConv > 0)
        {
            qtyOnHand *= priceOutV2.UnitConv;
        }

        return qtyOnHand.Value;
    }
}
