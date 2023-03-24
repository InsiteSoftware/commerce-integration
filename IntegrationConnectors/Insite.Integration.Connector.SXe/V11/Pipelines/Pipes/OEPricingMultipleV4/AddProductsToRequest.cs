namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.OEPricingMultipleV4;

using System.Collections.Generic;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;

public sealed class AddProductsToRequest
    : IPipe<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    public int Order => 400;

    public OEPricingMultipleV4Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Parameter parameter,
        OEPricingMultipleV4Result result
    )
    {
        result.OEPricingMultipleV4Request.Request.PriceInV2Collection = new PriceInV2Collection
        {
            PriceInV2s = GetInProducts(unitOfWork, parameter, result)
        };

        return result;
    }

    private static List<PriceInV2> GetInProducts(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Parameter parameter,
        OEPricingMultipleV4Result result
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();
        var warehouseRepository = unitOfWork.GetRepository<Warehouse>();

        var priceInV2s = new List<PriceInV2>();

        priceInV2s.AddRange(
            GetCreatePriceInV2s(
                parameter.PricingServiceParameters,
                parameter.GetInventoryParameter,
                productRepository
            )
        );
        priceInV2s.AddRange(
            GetCreatePriceInV2s(
                parameter.GetInventoryParameter,
                productRepository,
                warehouseRepository
            )
        );

        return priceInV2s;
    }

    private static PriceInV2[] GetCreatePriceInV2s(
        ICollection<PricingServiceParameter> pricingServiceParameters,
        GetInventoryParameter getInventoryParameter,
        IRepository<Product> productRepository
    )
    {
        var priceInV2s = new List<PriceInV2>();

        foreach (var pricingServiceParameter in pricingServiceParameters)
        {
            var priceInV2 = GetInProduct(
                pricingServiceParameter,
                getInventoryParameter,
                productRepository
            );
            if (priceInV2 != null)
            {
                priceInV2s.Add(priceInV2);
            }
        }

        return priceInV2s.ToArray();
    }

    private static PriceInV2 GetInProduct(
        PricingServiceParameter pricingServiceParameter,
        GetInventoryParameter getInventoryParameter,
        IRepository<Product> productRepository
    )
    {
        var product =
            pricingServiceParameter.Product
            ?? productRepository.Get(pricingServiceParameter.ProductId);
        if (product == null)
        {
            return null;
        }

        var warehouse = GetWarehouse(pricingServiceParameter, getInventoryParameter);

        return CreatePriceInV2(
            product.ErpNumber,
            pricingServiceParameter.UnitOfMeasure,
            warehouse,
            pricingServiceParameter.QtyOrdered
        );
    }

    private static PriceInV2[] GetCreatePriceInV2s(
        GetInventoryParameter getInventoryParameter,
        IRepository<Product> productRepository,
        IRepository<Warehouse> warehouseRepository
    )
    {
        var priceInV2s = new List<PriceInV2>();
        if (getInventoryParameter == null)
        {
            return priceInV2s.ToArray();
        }

        var warehouse = GetWarehouse(getInventoryParameter, warehouseRepository);

        foreach (var product in getInventoryParameter.Products)
        {
            priceInV2s.Add(CreatePriceInV2(product.ErpNumber, product.UnitOfMeasure, warehouse, 1));
        }

        foreach (var productId in getInventoryParameter.ProductIds)
        {
            var product = productRepository.Get(productId);
            if (product == null)
            {
                continue;
            }

            priceInV2s.Add(CreatePriceInV2(product.ErpNumber, product.UnitOfMeasure, warehouse, 1));
        }

        return priceInV2s.ToArray();
    }

    private static PriceInV2 CreatePriceInV2(
        string erpNumber,
        string unitOfMeasure,
        string warehouse,
        decimal qtyOrdered
    )
    {
        return new PriceInV2
        {
            Prod = erpNumber,
            Unit = unitOfMeasure,
            Qtyord = qtyOrdered,
            Whse = warehouse
        };
    }

    private static string GetWarehouse(
        GetInventoryParameter getInventoryParameter,
        IRepository<Warehouse> warehouseRepository
    )
    {
        if (getInventoryParameter?.GetWarehouses ?? false)
        {
            return string.Empty;
        }

        return getInventoryParameter.WarehouseId != null
            ? warehouseRepository.Get(getInventoryParameter.WarehouseId.Value)?.Name ?? string.Empty
            : string.Empty;
    }

    private static string GetWarehouse(
        PricingServiceParameter pricingServiceParameter,
        GetInventoryParameter getInventoryParameter
    )
    {
        if (getInventoryParameter?.GetWarehouses ?? false)
        {
            return string.Empty;
        }

        return pricingServiceParameter.Warehouse;
    }
}
