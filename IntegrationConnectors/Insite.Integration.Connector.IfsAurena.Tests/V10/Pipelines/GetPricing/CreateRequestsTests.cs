namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.GetPricing;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetPricing;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CreateRequestsTests : BaseForPipeTests<GetPricingParameter, GetPricingResult>
{
    private Mock<ICustomerHelper> customerHelper;

    private Mock<IWarehouseHelper> warehouseHelper;

    private IList<Product> products;

    public override Type PipeType => typeof(CreateRequests);

    public override void SetUp()
    {
        this.customerHelper = this.container.GetMock<ICustomerHelper>();
        this.warehouseHelper = this.container.GetMock<IWarehouseHelper>();

        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_100()
    {
        this.pipe.Order.Should().Be(100);
    }

    [Test]
    public void Execute_Should_Add_PricingServiceParameter_Product_To_Requests()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty) { Product = product }
        };

        var parameter = new GetPricingParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
        var result = this.RunExecute(parameter);

        result.PriceQueryRequests.First().CatalogNo.Should().Be(product.ErpNumber);
    }

    [Test]
    public void Execute_Should_Add_Repository_Product_For_PricingServiceParameter_ProductId_To_Requests()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(product.Id)
        };

        this.WhenExists(product);

        var parameter = new GetPricingParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
        var result = this.RunExecute(parameter);

        result.PriceQueryRequests.First().CatalogNo.Should().Be(product.ErpNumber);
    }

    [TestCase(-1)]
    [TestCase(0)]
    public void Execute_Should_Set_SalesQty_And_PriceQty_To_1_When_PricingServiceParameter_QtyOrdered_Is_Less_Than_Or_Equal_To_Zero(
        decimal qtyOrdered
    )
    {
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty)
            {
                Product = Some.Product().WithErpNumber("ABC123").Build(),
                QtyOrdered = qtyOrdered
            }
        };

        var parameter = new GetPricingParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
        var result = this.RunExecute(parameter);

        result.PriceQueryRequests.First().SalesQty.Should().Be(1);
        result.PriceQueryRequests.First().PriceQty.Should().Be(1);
    }

    [TestCase(0.1)]
    [TestCase(1)]
    [TestCase(10)]
    public void Execute_Should_Set_SalesQty_And_PriceQty_From_PricingServiceParameter_QtyOrdered_When_PricingServiceParameter_QtyOrdered_Is_Greater_Than_Zero(
        decimal qtyOrdered
    )
    {
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty)
            {
                Product = Some.Product().WithErpNumber("ABC123").Build(),
                QtyOrdered = qtyOrdered
            }
        };

        var parameter = new GetPricingParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
        var result = this.RunExecute(parameter);

        result.PriceQueryRequests.First().SalesQty.Should().Be(qtyOrdered);
        result.PriceQueryRequests.First().PriceQty.Should().Be(qtyOrdered);
    }

    [Test]
    public void Execute_Should_Set_CustomerNo_From_BillTo_Returned_From_BillToHelper_GetBillTo()
    {
        var customer = Some.Customer().WithErpNumber("ABC123").Build();
        var parameter = this.GetDefaultParameter();

        this.WhenGetBillToIs(parameter.PricingServiceParameters.First(), customer);

        var result = this.RunExecute(parameter);

        result.PriceQueryRequests.ForEach(o => o.CustomerNo.Should().Be(customer.ErpNumber));
    }

    [Test]
    public void Execute_Should_Set_Contract_From_Warehouse_Returned_From_WarehouseHelper_GetWarehouse()
    {
        var warehouse = new WarehouseDto { Name = "Warehouse1" };
        var parameter = this.GetDefaultParameter();

        this.WhenGetWarehouseIs(parameter.PricingServiceParameters.First(), warehouse);

        var result = this.RunExecute(parameter);

        result.PriceQueryRequests.ForEach(o => o.Contract.Should().Be(warehouse.Name));
    }

    [Test]
    public void Execute_Should_Set_CurrencyCode_From_PricingServiceParameters()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.RunExecute(parameter);

        result.PriceQueryRequests.ForEach(
            o => o.CurrencyCode.Should().Be(parameter.PricingServiceParameters.First().CurrencyCode)
        );
    }

    [Test]
    public void Execute_Should_Set_UsePriceInclTax_To_False()
    {
        var result = this.RunExecute();

        result.PriceQueryRequests.ForEach(o => o.UsePriceInclTax.Should().BeFalse());
    }

    protected override GetPricingParameter GetDefaultParameter()
    {
        return new GetPricingParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty)
                {
                    Product = Some.Product().Build(),
                    CurrencyCode = "USD"
                }
            }
        };
    }

    private void WhenGetBillToIs(PricingServiceParameter pricingServiceParameter, Customer customer)
    {
        this.customerHelper
            .Setup(o => o.GetBillTo(this.fakeUnitOfWork, pricingServiceParameter))
            .Returns(customer);
    }

    private void WhenGetWarehouseIs(
        PricingServiceParameter pricingServiceParameter,
        WarehouseDto warehouse
    )
    {
        this.warehouseHelper
            .Setup(o => o.GetWarehouse(this.fakeUnitOfWork, pricingServiceParameter))
            .Returns(warehouse);
    }

    private void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
