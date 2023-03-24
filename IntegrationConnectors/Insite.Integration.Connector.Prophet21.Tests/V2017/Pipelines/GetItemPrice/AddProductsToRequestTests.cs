namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetItemPrice;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrices;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class AddProductsToRequestTests : BaseForPipeTests<GetItemPriceParameter, GetItemPriceResult>
{
    private IList<Product> products;

    public override Type PipeType => typeof(AddProductsToRequest);

    public override void SetUp()
    {
        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Not_Add_Product_To_Request_When_Product_Is_Null()
    {
        var parameter = this.GetDefaultParameter();
        parameter.PricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.NewGuid())
        };

        var result = this.RunExecute(parameter);

        CollectionAssert.IsEmpty(result.GetItemPriceRequest.Request.ListOfItems);
    }

    [Test]
    public void Execute_Should_Add_Product_To_Request_With_Product_From_Pricing_Service_Parameter()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = this.GetDefaultParameter();
        parameter.PricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(product.Id) { Product = product }
        };

        var result = this.RunExecute(parameter);

        CollectionAssert.IsNotEmpty(result.GetItemPriceRequest.Request.ListOfItems);
        Assert.AreEqual(
            product.ErpNumber,
            result.GetItemPriceRequest.Request.ListOfItems.First().ItemID
        );
    }

    [Test]
    public void Execute_Should_Add_Product_To_Request_With_Product_From_Repository_From_Pricing_Service_Parameter_ProductId()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = this.GetDefaultParameter();
        parameter.PricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(product.Id)
        };

        this.WhenExists(product);

        var result = this.RunExecute(parameter);

        CollectionAssert.IsNotEmpty(result.GetItemPriceRequest.Request.ListOfItems);
        Assert.AreEqual(
            product.ErpNumber,
            result.GetItemPriceRequest.Request.ListOfItems.First().ItemID
        );
    }

    [Test]
    public void Execute_Should_Populate_Product_UnitOfMeasure_And_QuantityOrdered_From_Pricing_Service_Parameter()
    {
        var product = Some.Product().Build();

        var parameter = this.GetDefaultParameter();
        parameter.PricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(product.Id)
            {
                Product = product,
                UnitOfMeasure = "EA",
                QtyOrdered = 12
            }
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual("EA", result.GetItemPriceRequest.Request.ListOfItems.First().UnitName);
        Assert.AreEqual("12", result.GetItemPriceRequest.Request.ListOfItems.First().Quantity);
    }

    [Test]
    public void Execute_Should_Add_Product_To_Request_With_Product_From_Get_Inventory_Parameter()
    {
        var product = Some.Product().WithErpNumber("123").WithUnitOfMeasure("EA").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            Products = new List<Product> { product }
        };

        var result = this.RunExecute(parameter);

        CollectionAssert.IsNotEmpty(result.GetItemPriceRequest.Request.ListOfItems);
        Assert.AreEqual(
            product.ErpNumber,
            result.GetItemPriceRequest.Request.ListOfItems.First().ItemID
        );
        Assert.AreEqual(
            product.UnitOfMeasure,
            result.GetItemPriceRequest.Request.ListOfItems.First().UnitName
        );
        Assert.AreEqual("1", result.GetItemPriceRequest.Request.ListOfItems.First().Quantity);
    }

    [Test]
    public void Execute_Should_Add_Product_To_Request_With_Product_From_Repository_From_Get_Inventory_Parameter_ProductId()
    {
        var product = Some.Product().WithErpNumber("123").WithUnitOfMeasure("EA").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { product.Id }
        };

        this.WhenExists(product);

        var result = this.RunExecute(parameter);

        CollectionAssert.IsNotEmpty(result.GetItemPriceRequest.Request.ListOfItems);
        Assert.AreEqual(
            product.ErpNumber,
            result.GetItemPriceRequest.Request.ListOfItems.First().ItemID
        );
        Assert.AreEqual(
            product.UnitOfMeasure,
            result.GetItemPriceRequest.Request.ListOfItems.First().UnitName
        );
        Assert.AreEqual("1", result.GetItemPriceRequest.Request.ListOfItems.First().Quantity);
    }

    protected override GetItemPriceResult GetDefaultResult()
    {
        return new GetItemPriceResult
        {
            GetItemPriceRequest = new GetItemPrice { Request = new Request() }
        };
    }

    protected void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
