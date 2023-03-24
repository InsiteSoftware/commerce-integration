namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.OEPricingMultipleV4;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;

[TestFixture]
public class AddProductsToRequestTests
    : BaseForPipeTests<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    private IList<Product> products;

    private IList<Warehouse> warehouses;

    public override Type PipeType => typeof(AddProductsToRequest);

    public override void SetUp()
    {
        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);

        this.warehouses = new List<Warehouse>();
        this.fakeUnitOfWork.SetupEntityList(this.warehouses);
    }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Inproduct3_Collection_With_PricingServiceParameters_Products()
    {
        var pricingServiceParameter1 = this.CreatePricingServiceParameter(
            Some.Product().WithErpNumber("1"),
            Guid.Empty,
            "EA",
            "W1",
            1
        );
        var pricingServiceParameter2 = this.CreatePricingServiceParameter(
            Some.Product().WithErpNumber("2"),
            Guid.Empty,
            "CS",
            "W2",
            2
        );

        var parameter = new OEPricingMultipleV4Parameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>
            {
                pricingServiceParameter1,
                pricingServiceParameter2
            }
        };

        var result = this.RunExecute(parameter);

        this.VerifyOePricingMultipleV5RequestPriceInV2Created(
            result.OEPricingMultipleV4Request,
            pricingServiceParameter1.Product.ErpNumber,
            pricingServiceParameter1.UnitOfMeasure,
            pricingServiceParameter1.Warehouse,
            pricingServiceParameter1.QtyOrdered
        );
        this.VerifyOePricingMultipleV5RequestPriceInV2Created(
            result.OEPricingMultipleV4Request,
            pricingServiceParameter2.Product.ErpNumber,
            pricingServiceParameter2.UnitOfMeasure,
            pricingServiceParameter2.Warehouse,
            pricingServiceParameter2.QtyOrdered
        );
    }

    [Test]
    public void Execute_Should_Get_Inproduct3_Collection_With_PricingServiceParameters_ProductIds()
    {
        var product1 = Some.Product().WithErpNumber("1").Build();
        var product2 = Some.Product().WithErpNumber("2").Build();

        this.WhenExists(product1);
        this.WhenExists(product2);

        var pricingServiceParameter1 = this.CreatePricingServiceParameter(
            null,
            product1.Id,
            "EA",
            "W1",
            1
        );
        var pricingServiceParameter2 = this.CreatePricingServiceParameter(
            null,
            product2.Id,
            "CS",
            "W2",
            2
        );

        var parameter = new OEPricingMultipleV4Parameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>
            {
                pricingServiceParameter1,
                pricingServiceParameter2
            }
        };

        var result = this.RunExecute(parameter);

        this.VerifyOePricingMultipleV5RequestPriceInV2Created(
            result.OEPricingMultipleV4Request,
            product1.ErpNumber,
            pricingServiceParameter1.UnitOfMeasure,
            pricingServiceParameter1.Warehouse,
            pricingServiceParameter1.QtyOrdered
        );
        this.VerifyOePricingMultipleV5RequestPriceInV2Created(
            result.OEPricingMultipleV4Request,
            product2.ErpNumber,
            pricingServiceParameter2.UnitOfMeasure,
            pricingServiceParameter2.Warehouse,
            pricingServiceParameter2.QtyOrdered
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_Warehouse_For_PricingServiceParameters_When_GetInventoryParameter_GetWarehouses_Is_True()
    {
        var pricingServiceParameter1 = this.CreatePricingServiceParameter(
            Some.Product().WithErpNumber("1"),
            Guid.Empty,
            "EA",
            "W1",
            1
        );

        var parameter = new OEPricingMultipleV4Parameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>
            {
                pricingServiceParameter1
            },
            GetInventoryParameter = new GetInventoryParameter { GetWarehouses = true }
        };

        var result = this.RunExecute(parameter);

        this.VerifyOePricingMultipleV5RequestPriceInV2Created(
            result.OEPricingMultipleV4Request,
            pricingServiceParameter1.Product.ErpNumber,
            pricingServiceParameter1.UnitOfMeasure,
            string.Empty,
            pricingServiceParameter1.QtyOrdered
        );
    }

    [Test]
    public void Execute_Should_Get_Inproduct3_Collection_With_GetInventoryParameter_Products()
    {
        var getInventoryParameter = new GetInventoryParameter
        {
            Products = new List<Product> { Some.Product().WithErpNumber("1") }
        };

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = getInventoryParameter;

        var result = this.RunExecute(parameter);

        this.VerifyOePricingMultipleV5RequestPriceInV2Created(
            result.OEPricingMultipleV4Request,
            getInventoryParameter.Products.First().ErpNumber,
            string.Empty,
            string.Empty,
            1
        );
    }

    [Test]
    public void Execute_Should_Get_Inproduct3_Collection_With_GetInventoryParameter_ProductIds()
    {
        var product = Some.Product().WithErpNumber("1").Build();
        var warehouse = Some.Warehouse().WithName("W1").Build();

        var getInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { product.Id },
            WarehouseId = warehouse.Id
        };

        this.WhenExists(product);
        this.WhenExists(warehouse);

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = getInventoryParameter;

        var result = this.RunExecute(parameter);

        this.VerifyOePricingMultipleV5RequestPriceInV2Created(
            result.OEPricingMultipleV4Request,
            product.ErpNumber,
            string.Empty,
            warehouse.Name,
            1
        );
    }

    public void Execute_Should_Not_Populate_Warehouse_For_GetInventoryParameter_When_GetInventoryParameter_GetWarehouses_Is_True()
    {
        var warehouse = Some.Warehouse().WithName("W1").Build();

        var getInventoryParameter = new GetInventoryParameter
        {
            Products = new List<Product> { Some.Product().WithErpNumber("1") },
            GetWarehouses = true,
            WarehouseId = warehouse.Id
        };

        this.WhenExists(warehouse);

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = getInventoryParameter;

        var result = this.RunExecute(parameter);

        this.VerifyOePricingMultipleV5RequestPriceInV2Created(
            result.OEPricingMultipleV4Request,
            getInventoryParameter.Products.First().ErpNumber,
            string.Empty,
            string.Empty,
            1
        );
    }

    protected override OEPricingMultipleV4Result GetDefaultResult()
    {
        return new OEPricingMultipleV4Result
        {
            OEPricingMultipleV4Request = new OEPricingMultipleV4Request { Request = new Request() }
        };
    }

    protected PricingServiceParameter CreatePricingServiceParameter(
        Product product,
        Guid productId,
        string unitOfMeasure,
        string warehouse,
        decimal qtyOrdered
    )
    {
        return new PricingServiceParameter(productId)
        {
            Product = product,
            UnitOfMeasure = unitOfMeasure,
            Warehouse = warehouse,
            QtyOrdered = qtyOrdered
        };
    }

    protected void WhenExists(Product product)
    {
        this.products.Add(product);
    }

    protected void WhenExists(Warehouse warehouse)
    {
        this.warehouses.Add(warehouse);
    }

    protected void VerifyOePricingMultipleV5RequestPriceInV2Created(
        OEPricingMultipleV4Request oePricingMultipleV5Request,
        string erpNumber,
        string unitOfMeasure,
        string warehouses,
        decimal qtyOrdered
    )
    {
        Assert.IsTrue(
            oePricingMultipleV5Request.Request.PriceInV2Collection.PriceInV2s.Any(
                o =>
                    o.Prod == erpNumber
                    && o.Unit == unitOfMeasure
                    && o.Qtyord == qtyOrdered
                    && o.Whse == warehouses
            )
        );
    }
}
