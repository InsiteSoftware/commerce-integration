namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.OEPricingMultipleV4;

using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.OEPricingMultipleV4;

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
        result.OEPricingMultipleV4Request.Inproduct = GetInProducts(unitOfWork, parameter, result);

        return result;
    }

    private static List<Inproduct3> GetInProducts(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Parameter parameter,
        OEPricingMultipleV4Result result
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();
        var warehouseRepository = unitOfWork.GetRepository<Warehouse>();

        var inProducts = new List<Inproduct3>();

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

    private static Inproduct3[] GetInProducts(
        ICollection<PricingServiceParameter> pricingServiceParameters,
        GetInventoryParameter getInventoryParameter,
        IRepository<Product> productRepository
    )
    {
        var inProducts = new List<Inproduct3>();

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

        return inProducts.ToArray();
    }

    private static Inproduct3 GetInProduct(
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
            warehouse,
            pricingServiceParameter.QtyOrdered
        );
    }

    private static Inproduct3[] GetInProducts(
        GetInventoryParameter getInventoryParameter,
        IRepository<Product> productRepository,
        IRepository<Warehouse> warehouseRepository
    )
    {
        var inProducts = new List<Inproduct3>();
        if (getInventoryParameter == null)
        {
            return inProducts.ToArray();
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

        return inProducts.ToArray();
    }

    private static Inproduct3 CreateInproduct(
        string erpNumber,
        string unitOfMeasure,
        string warehouse,
        decimal qtyOrdered
    )
    {
        return new Inproduct3
        {
            ProductCode = erpNumber,
            UnitOfMeasure = unitOfMeasure,
            Quantity = qtyOrdered,
            Warehouse = warehouse
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
