namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetPricingAndInventoryStock;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Insite.Common.Helpers;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CreateRequestTests
    : BaseForPipeTests<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
{
    private Mock<ICustomerHelper> customerHelper;

    private Mock<IProductHelper> productHelper;

    private Mock<IWarehouseHelper> warehouseHelper;

    private IList<Product> products;

    public override Type PipeType => typeof(CreateRequest);

    public override void SetUp()
    {
        this.customerHelper = this.container.GetMock<ICustomerHelper>();
        this.productHelper = this.container.GetMock<IProductHelper>();
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
    public void Execute_Should_Set_Credentials_From_IntegrationConnection()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var integrationConnection = Some.IntegrationConnection()
            .WithSystemNumber("1")
            .WithLogOn("web")
            .WithPassword(EncryptionHelper.EncryptAes("password"))
            .Build();
#pragma warning restore

        var parameter = new GetPricingAndInventoryStockParameter
        {
            IntegrationConnection = integrationConnection
        };

        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.CompanyNumber.Should().Be(1);
        result.OePricingMultipleV4Request.Request.OperatorInit
            .Should()
            .Be(integrationConnection.LogOn);
#pragma warning disable CS0618 // Type or member is obsolete
        result.OePricingMultipleV4Request.Request.OperatorPassword
            .Should()
            .Be(EncryptionHelper.DecryptAes(integrationConnection.Password));
#pragma warning restore
    }

    [Test]
    public void Execute_Should_Add_PricingServiceParameter_Product_To_Request()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty) { Product = product }
        };

        var parameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s
            .First()
            .Prod.Should()
            .Be(product.ErpNumber);
    }

    [Test]
    public void Execute_Should_Add_Repository_Product_For_PricingServiceParameter_ProductId_To_Request()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(product.Id)
        };

        this.WhenExists(product);

        var parameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s
            .First()
            .Prod.Should()
            .Be(product.ErpNumber);
    }

    [TestCase(-1)]
    [TestCase(0)]
    public void Execute_Should_Set_QtyOrd_To_1_When_PricingServiceParameter_QtyOrdered_Is_Less_Than_Or_Equal_To_Zero(
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

        var parameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s
            .First()
            .QtyOrd.Should()
            .Be(1);
    }

    [TestCase(0.1)]
    [TestCase(1)]
    [TestCase(10)]
    public void Execute_Should_Set_QtyOrd_From_PricingServiceParameter_QtyOrdered_When_PricingServiceParameter_QtyOrdered_Is_Greater_Than_Zero(
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

        var parameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s
            .First()
            .QtyOrd.Should()
            .Be(qtyOrdered);
    }

    [TestCase("")]
    [TestCase("EA")]
    public void Execute_Should_Set_Unit_From_PricingServiceParameter_UnitOfMeasure(
        string unitOfMeasure
    )
    {
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty)
            {
                Product = Some.Product().WithErpNumber("ABC123").Build(),
                UnitOfMeasure = unitOfMeasure
            }
        };

        var parameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s
            .First()
            .Unit.Should()
            .Be(unitOfMeasure);
    }

    [Test]
    public void Execute_Should_Set_Whse_From_Warehouse_Returned_From_WarehouseHelper_GetWarehouse()
    {
        var warehouse = new WarehouseDto { Name = "Warehouse1" };
        var parameter = this.GetDefaultParameter();

        this.WhenGetWarehouseIs(parameter.PricingServiceParameters.First(), warehouse);

        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s
            .First()
            .Whse.Should()
            .Be(warehouse.Name);
    }

    [Test]
    public void Execute_Should_Add_Products_To_Request_Returned_From_ProductHelper_GetProducts()
    {
        var products = new List<Product>
        {
            Some.Product().WithErpNumber("ABC123").Build(),
            Some.Product().WithErpNumber("DEF456").Build(),
            Some.Product().WithErpNumber("GHI789").Build()
        };

        var parameter = new GetPricingAndInventoryStockParameter
        {
            GetInventoryParameter = new GetInventoryParameter()
        };

        this.WhenGetProductsIs(parameter.GetInventoryParameter, products);

        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s[0].Prod
            .Should()
            .Be(products[0].ErpNumber);
        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s[1].Prod
            .Should()
            .Be(products[1].ErpNumber);
        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s[2].Prod
            .Should()
            .Be(products[2].ErpNumber);
    }

    [Test]
    public void Execute_Should_Set_QtyOrd_To_1_For_GetInventoryParameter_Products()
    {
        var products = new List<Product> { Some.Product().Build() };

        var parameter = new GetPricingAndInventoryStockParameter
        {
            GetInventoryParameter = new GetInventoryParameter()
        };

        this.WhenGetProductsIs(parameter.GetInventoryParameter, products);

        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s
            .First()
            .QtyOrd.Should()
            .Be(1);
    }

    [Test]
    public void Execute_Should_Set_Unit_From_Product_UnitOfMeasure()
    {
        var products = new List<Product> { Some.Product().WithUnitOfMeasure("EA").Build() };

        var parameter = new GetPricingAndInventoryStockParameter
        {
            GetInventoryParameter = new GetInventoryParameter()
        };

        this.WhenGetProductsIs(parameter.GetInventoryParameter, products);

        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s
            .First()
            .Unit.Should()
            .Be(products.First().UnitOfMeasure);
    }

    [Test]
    public void Execute_Should_Set_Whse_From_Warehouse_Returned_From_WarehouseHelper_GetWarehouse_For_GetInventoryParameter()
    {
        var warehouse = new WarehouseDto { Name = "Warehouse1" };
        var products = new List<Product> { Some.Product().Build() };

        var parameter = new GetPricingAndInventoryStockParameter
        {
            GetInventoryParameter = new GetInventoryParameter()
        };

        this.WhenGetProductsIs(parameter.GetInventoryParameter, products);
        this.WhenGetWarehouseIs(parameter.GetInventoryParameter, warehouse);

        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.PriceInV2Collection.PriceInV2s
            .First()
            .Whse.Should()
            .Be(warehouse.Name);
    }

    [Test]
    public void Execute_Should_Set_CustomerNumber_From_BillTo_Returned_From_CustomerHelper_GetBillTo()
    {
        var customer = Some.Customer().WithErpNumber("ABC123").Build();
        var parameter = this.GetDefaultParameter();

        this.WhenGetBillToIs(parameter.PricingServiceParameters.First(), customer);

        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.CustomerNumber.Should().Be(customer.ErpNumber);
    }

    [Test]
    public void Execute_Should_Set_ShipTo_From_ShipTo_Returned_From_CustomerHelper_GetShipTo()
    {
        var customer = Some.Customer().WithErpSequence("ABC123").Build();
        var parameter = this.GetDefaultParameter();

        this.WhenGetShipToIs(parameter.PricingServiceParameters.First(), customer);

        var result = this.RunExecute(parameter);

        result.OePricingMultipleV4Request.Request.ShipTo.Should().Be(customer.ErpSequence);
    }

    [Test]
    public void Execute_Should_Set_SendFullQtyOnOrder_To_True()
    {
        var result = this.RunExecute();

        result.OePricingMultipleV4Request.Request.SendFullQtyOnOrder.Should().BeTrue();
    }

    protected override GetPricingAndInventoryStockParameter GetDefaultParameter()
    {
        return new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { Product = Some.Product().Build() }
            }
        };
    }

    private void WhenGetBillToIs(PricingServiceParameter pricingServiceParameter, Customer customer)
    {
        this.customerHelper
            .Setup(o => o.GetBillTo(this.fakeUnitOfWork, pricingServiceParameter))
            .Returns(customer);
    }

    private void WhenGetShipToIs(PricingServiceParameter pricingServiceParameter, Customer customer)
    {
        this.customerHelper
            .Setup(o => o.GetShipTo(this.fakeUnitOfWork, pricingServiceParameter))
            .Returns(customer);
    }

    private void WhenGetProductsIs(
        GetInventoryParameter getInventoryParameter,
        List<Product> products
    )
    {
        this.productHelper
            .Setup(o => o.GetProducts(this.fakeUnitOfWork, getInventoryParameter))
            .Returns(products);
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

    private void WhenGetWarehouseIs(
        GetInventoryParameter getInventoryParameter,
        WarehouseDto warehouse
    )
    {
        this.warehouseHelper
            .Setup(o => o.GetWarehouse(this.fakeUnitOfWork, getInventoryParameter))
            .Returns(warehouse);
    }

    private void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
