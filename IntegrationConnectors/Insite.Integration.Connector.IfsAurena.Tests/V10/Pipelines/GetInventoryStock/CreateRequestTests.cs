namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.GetInventoryStock;

using System;
using FluentAssertions;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetInventoryStock;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CreateRequestTests
    : BaseForPipeTests<GetInventoryStockParameter, GetInventoryStockResult>
{
    private Mock<IProductHelper> productHelper;

    private Mock<IWarehouseHelper> warehouseHelper;

    public override Type PipeType => typeof(CreateRequest);

    public override void SetUp()
    {
        this.productHelper = this.container.GetMock<IProductHelper>();
        this.warehouseHelper = this.container.GetMock<IWarehouseHelper>();
    }

    [Test]
    public void Order_Is_100()
    {
        this.pipe.Order.Should().Be(100);
    }

    [Test]
    public void Execute_Should_Create_Request_With_Single_Product_And_Single_Warehouse()
    {
        var product = Some.Product().WithProductCode("ABC123");
        var warehouse = new WarehouseDto { Name = "Warehouse1" };

        var parameter = this.GetDefaultParameter();

        this.WhenGetProductsIs(parameter.GetInventoryParameter, product);
        this.WhenGetWarehousesIs(parameter.GetInventoryParameter, warehouse);

        var result = this.RunExecute(parameter);

        result.InventoryPartInStockRequest
            .Should()
            .Be(
                "$select=PartNo,Contract,AvailableQty&$filter=(PartNo eq 'ABC123') and (Contract eq 'Warehouse1')"
            );
    }

    [Test]
    public void Execute_Should_Create_Request_With_Multiple_Products_And_Multiple_Warehouses()
    {
        var product1 = Some.Product().WithProductCode("ABC123");
        var product2 = Some.Product().WithProductCode("DEF456");
        var warehouse1 = new WarehouseDto { Name = "Warehouse1" };
        var warehouse2 = new WarehouseDto { Name = "Warehouse2" };

        var parameter = this.GetDefaultParameter();

        this.WhenGetProductsIs(parameter.GetInventoryParameter, product1, product2);
        this.WhenGetWarehousesIs(parameter.GetInventoryParameter, warehouse1, warehouse2);

        var result = this.RunExecute(parameter);

        result.InventoryPartInStockRequest
            .Should()
            .Be(
                "$select=PartNo,Contract,AvailableQty&$filter=(PartNo eq 'ABC123' or PartNo eq 'DEF456') and (Contract eq 'Warehouse1' or Contract eq 'Warehouse2')"
            );
    }

    protected override GetInventoryStockParameter GetDefaultParameter()
    {
        return new GetInventoryStockParameter
        {
            GetInventoryParameter = new GetInventoryParameter()
        };
    }

    private void WhenGetProductsIs(
        GetInventoryParameter getInventoryParameter,
        params Product[] products
    )
    {
        this.productHelper
            .Setup(o => o.GetProducts(this.fakeUnitOfWork, getInventoryParameter))
            .Returns(products);
    }

    private void WhenGetWarehousesIs(
        GetInventoryParameter getInventoryParameter,
        params WarehouseDto[] warehouses
    )
    {
        this.warehouseHelper
            .Setup(o => o.GetWarehouses(this.fakeUnitOfWork, getInventoryParameter))
            .Returns(warehouses);
    }
}
