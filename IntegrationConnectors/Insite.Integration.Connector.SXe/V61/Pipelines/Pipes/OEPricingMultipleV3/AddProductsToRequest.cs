namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;

using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;

public sealed class AddProductsToRequest
    : IPipe<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
{
    public int Order => 400;

    public OEPricingMultipleV3Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV3Parameter parameter,
        OEPricingMultipleV3Result result
    )
    {
        result.OEPricingMultipleV3Request.arrayProduct = GetInProducts(
            unitOfWork,
            parameter,
            result
        );

        return result;
    }

    private static List<OEPricingMultipleV3inputProduct> GetInProducts(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV3Parameter parameter,
        OEPricingMultipleV3Result result
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();
        var warehouseRepository = unitOfWork.GetRepository<Warehouse>();

        var inProducts = new List<OEPricingMultipleV3inputProduct>();

        inProducts.AddRange(
            GetInProducts(
                parameter.PricingServiceParameters,
                parameter.GetInventoryParameter,
                productRepository
            )
        );
        inProducts.AddRange(
            GetInProducts(parameter.GetInventoryParameter, productRepository, warehouseRepository)
        );

        return inProducts;
    }

    private static List<OEPricingMultipleV3inputProduct> GetInProducts(
        ICollection<PricingServiceParameter> pricingServiceParameters,
        GetInventoryParameter getInventoryParameter,
        IRepository<Product> productRepository
    )
    {
        var inProducts = new List<OEPricingMultipleV3inputProduct>();

        foreach (var pricingServiceParameter in pricingServiceParameters)
        {
            var inProduct = GetInProduct(
                pricingServiceParameter,
                getInventoryParameter,
                productRepository
            );
            if (inProduct != null)
            {
                inProducts.Add(inProduct);
            }
        }

        return inProducts;
    }

    private static OEPricingMultipleV3inputProduct GetInProduct(
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

        return CreateInproduct(
            product.ErpNumber,
            pricingServiceParameter.UnitOfMeasure,
            pricingServiceParameter.Warehouse,
            pricingServiceParameter.QtyOrdered
        );
    }

    private static List<OEPricingMultipleV3inputProduct> GetInProducts(
        GetInventoryParameter getInventoryParameter,
        IRepository<Product> productRepository,
        IRepository<Warehouse> warehouseRepository
    )
    {
        var inProducts = new List<OEPricingMultipleV3inputProduct>();
        if (getInventoryParameter == null)
        {
            return inProducts;
        }

        var warehouse = GetWarehouse(getInventoryParameter, warehouseRepository);

        foreach (var product in getInventoryParameter.Products)
        {
            inProducts.Add(CreateInproduct(product.ErpNumber, product.UnitOfMeasure, warehouse, 1));
        }

        foreach (var productId in getInventoryParameter.ProductIds)
        {
            var product = productRepository.Get(productId);
            if (product == null)
            {
                continue;
            }

            inProducts.Add(CreateInproduct(product.ErpNumber, product.UnitOfMeasure, warehouse, 1));
        }

        return inProducts;
    }

    private static OEPricingMultipleV3inputProduct CreateInproduct(
        string erpNumber,
        string unitOfMeasure,
        string warehouse,
        decimal qtyOrdered
    )
    {
        return new OEPricingMultipleV3inputProduct
        {
            productCode = erpNumber,
            unitOfMeasure = unitOfMeasure,
            quantity = qtyOrdered,
            warehouse = warehouse
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
