namespace Insite.Integration.Connector.SXe.Tests.V6.Pipelines.Pipes.OEPricingMultipleV3;

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
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;

[TestFixture]
public class AddProductsToRequestTests
    : BaseForPipeTests<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
{
    private IList<Product> products;

    public override Type PipeType => typeof(AddProductsToRequest);

    public override void SetUp()
    {
        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Inproduct3_Collection_With_PricingServiceParameters_Products()
    {
        var pricingServiceParameter1 = new PricingServiceParameter(Guid.Empty)
        {
            Product = Some.Product().WithErpNumber("1"),
            UnitOfMeasure = "EA",
            QtyOrdered = 1,
            Warehouse = "W1"
        };

        var pricingServiceParameter2 = new PricingServiceParameter(Guid.Empty)
        {
            Product = Some.Product().WithErpNumber("2"),
            UnitOfMeasure = "CS",
            QtyOrdered = 2,
            Warehouse = "W2"
        };

        var parameter = new OEPricingMultipleV3Parameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>
            {
                pricingServiceParameter1,
                pricingServiceParameter2
            }
        };

        var result = this.RunExecute(parameter);

        this.VerifyOePricingMultipleV3RequestInProduct3Created(
            result.OEPricingMultipleV3Request,
            pricingServiceParameter1.Product.ErpNumber,
            pricingServiceParameter1.UnitOfMeasure,
            pricingServiceParameter1.Warehouse,
            pricingServiceParameter1.QtyOrdered
        );
        this.VerifyOePricingMultipleV3RequestInProduct3Created(
            result.OEPricingMultipleV3Request,
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

        var pricingServiceParameter1 = new PricingServiceParameter(Guid.Empty)
        {
            ProductId = product1.Id,
            UnitOfMeasure = "EA",
            QtyOrdered = 1,
            Warehouse = "W1"
        };

        var pricingServiceParameter2 = new PricingServiceParameter(Guid.Empty)
        {
            ProductId = product2.Id,
            UnitOfMeasure = "CS",
            QtyOrdered = 2,
            Warehouse = "W2"
        };

        var parameter = new OEPricingMultipleV3Parameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>
            {
                pricingServiceParameter1,
                pricingServiceParameter2
            }
        };

        var result = this.RunExecute(parameter);

        this.VerifyOePricingMultipleV3RequestInProduct3Created(
            result.OEPricingMultipleV3Request,
            product1.ErpNumber,
            pricingServiceParameter1.UnitOfMeasure,
            pricingServiceParameter1.Warehouse,
            pricingServiceParameter1.QtyOrdered
        );
        this.VerifyOePricingMultipleV3RequestInProduct3Created(
            result.OEPricingMultipleV3Request,
            product2.ErpNumber,
            pricingServiceParameter2.UnitOfMeasure,
            pricingServiceParameter2.Warehouse,
            pricingServiceParameter2.QtyOrdered
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

        this.VerifyOePricingMultipleV3RequestInProduct3Created(
            result.OEPricingMultipleV3Request,
            getInventoryParameter.Products.First().ErpNumber,
            string.Empty,
            string.Empty,
            1
        );
    }

    [Test]
    public void Execute_Should_Get_Inproduct3_Collection_With_GetInventoryParameter_ProductIdss()
    {
        var product = Some.Product().WithErpNumber("1").Build();
        var getInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { product.Id }
        };

        this.WhenExists(product);

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = getInventoryParameter;

        var result = this.RunExecute(parameter);

        this.VerifyOePricingMultipleV3RequestInProduct3Created(
            result.OEPricingMultipleV3Request,
            product.ErpNumber,
            string.Empty,
            string.Empty,
            1
        );
    }

    protected override OEPricingMultipleV3Result GetDefaultResult()
    {
        return new OEPricingMultipleV3Result
        {
            OEPricingMultipleV3Request = new OEPricingMultipleV3Request()
        };
    }

    protected void WhenExists(Product product)
    {
        this.products.Add(product);
    }

    protected void VerifyOePricingMultipleV3RequestInProduct3Created(
        OEPricingMultipleV3Request oePricingMultipleV3Request,
        string erpNumber,
        string unitOfMeasure,
        string warehouses,
        decimal qtyOrdered
    )
    {
        Assert.IsTrue(
            oePricingMultipleV3Request.arrayProduct.Any(
                o =>
                    o.productCode == erpNumber
                    && o.unitOfMeasure == unitOfMeasure
                    && o.quantity == qtyOrdered
                    && o.warehouse == warehouses
            )
        );
    }
}
