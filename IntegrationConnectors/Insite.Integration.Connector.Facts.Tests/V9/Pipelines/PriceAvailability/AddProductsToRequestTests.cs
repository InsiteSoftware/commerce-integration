namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.PriceAvailability;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using NUnit.Framework;

[TestFixture]
public class AddProductsToRequestTests
    : BaseForPipeTests<PriceAvailabilityParameter, PriceAvailabilityResult>
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

        CollectionAssert.IsEmpty(result.PriceAvailabilityRequest.Request.Items);
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

        CollectionAssert.IsNotEmpty(result.PriceAvailabilityRequest.Request.Items);
        Assert.AreEqual(
            product.ErpNumber,
            result.PriceAvailabilityRequest.Request.Items.First().ItemNumber
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

        CollectionAssert.IsNotEmpty(result.PriceAvailabilityRequest.Request.Items);
        Assert.AreEqual(
            product.ErpNumber,
            result.PriceAvailabilityRequest.Request.Items.First().ItemNumber
        );
    }

    [Test]
    public void Execute_Should_Populate_Product_UnitOfMeasure_And_OrderQuantity_From_Pricing_Service_Parameter()
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

        Assert.AreEqual("EA", result.PriceAvailabilityRequest.Request.Items.First().UnitOfMeasure);
        Assert.AreEqual(12M, result.PriceAvailabilityRequest.Request.Items.First().OrderQuantity);
    }

    [TestCase(-1)]
    [TestCase(0)]
    public void Execute_Should_Set_QtyOrd_To_1_When_PricingServiceParameter_QtyOrdered_Is_Less_Than_Or_Equal_To_Zero(
        decimal qtyOrdered
    )
    {
        var product = Some.Product().Build();

        var parameter = this.GetDefaultParameter();
        parameter.PricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(product.Id)
            {
                Product = product,
                UnitOfMeasure = "EA",
                QtyOrdered = qtyOrdered
            }
        };

        var result = this.RunExecute(parameter);

        result.PriceAvailabilityRequest.Request.Items.First().OrderQuantity.Should().Be(1);
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

        CollectionAssert.IsNotEmpty(result.PriceAvailabilityRequest.Request.Items);
        Assert.AreEqual(
            product.ErpNumber,
            result.PriceAvailabilityRequest.Request.Items.First().ItemNumber
        );
        Assert.AreEqual(
            product.UnitOfMeasure,
            result.PriceAvailabilityRequest.Request.Items.First().UnitOfMeasure
        );
        Assert.AreEqual(1M, result.PriceAvailabilityRequest.Request.Items.First().OrderQuantity);
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

        CollectionAssert.IsNotEmpty(result.PriceAvailabilityRequest.Request.Items);
        Assert.AreEqual(
            product.ErpNumber,
            result.PriceAvailabilityRequest.Request.Items.First().ItemNumber
        );
        Assert.AreEqual(
            product.UnitOfMeasure,
            result.PriceAvailabilityRequest.Request.Items.First().UnitOfMeasure
        );
        Assert.AreEqual(1M, result.PriceAvailabilityRequest.Request.Items.First().OrderQuantity);
    }

    protected override PriceAvailabilityResult GetDefaultResult()
    {
        return new PriceAvailabilityResult
        {
            PriceAvailabilityRequest = new PriceAvailabilityRequest { Request = new Request() }
        };
    }

    private void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
