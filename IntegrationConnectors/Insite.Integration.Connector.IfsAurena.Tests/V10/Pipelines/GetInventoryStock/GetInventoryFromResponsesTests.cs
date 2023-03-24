namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.GetInventoryStock;

using System;
using System.Linq;
using FluentAssertions;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetInventoryStock;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class GetInventoryFromResponsesTests
    : BaseForPipeTests<GetInventoryStockParameter, GetInventoryStockResult>
{
    private Mock<IProductHelper> productHelper;

    private Mock<IWarehouseHelper> warehouseHelper;

    public override Type PipeType => typeof(GetInventoryFromResponses);

    public override void SetUp()
    {
        this.productHelper = this.container.GetMock<IProductHelper>();
        this.warehouseHelper = this.container.GetMock<IWarehouseHelper>();
    }

    [Test]
    public void Order_Is_400()
    {
        this.pipe.Order.Should().Be(400);
    }

    [Test]
    public void Execute_Should_Return_QtyOnHand_Zero_When_No_Results()
    {
        var product = Some.Product().WithProductCode("123").Build();

        var parameter = this.GetDefaultParameter();

        var result = this.CreateInventoryPartInStockResult();

        this.WhenGetProductsIs(parameter.GetInventoryParameter, product);

        result = this.RunExecute(parameter, result);

        result.GetInventoryResult.Inventories.Should().NotBeEmpty();
        result.GetInventoryResult.Inventories.Should().ContainKey(product.Id);
        result.GetInventoryResult.Inventories.First().Value.QtyOnHand.Should().Be(0);
    }

    [Test]
    public void Execute_Should_Return_QtyOnHand_Zero_When_No_Results_For_Product()
    {
        var product = Some.Product().WithProductCode("123").Build();

        var parameter = this.GetDefaultParameter();

        var inventoryPartInStockResponse = this.CreateInventoryPartInStockResponse(
            "OtherProductCode",
            string.Empty,
            100
        );
        var result = this.CreateInventoryPartInStockResult(inventoryPartInStockResponse);

        this.WhenGetProductsIs(parameter.GetInventoryParameter, product);

        result = this.RunExecute(parameter, result);

        result.GetInventoryResult.Inventories.Should().NotBeEmpty();
        result.GetInventoryResult.Inventories.Should().ContainKey(product.Id);
        result.GetInventoryResult.Inventories.First().Value.QtyOnHand.Should().Be(0);
    }

    [Test]
    public void Execute_Should_GetInventory_With_QtyOnHand_Aggregated_By_Warehouse()
    {
        var product = Some.Product().WithProductCode("123").Build();
        var warehouse = new WarehouseDto { Name = "Warehouse1" };

        var parameter = this.GetDefaultParameter();

        var inventoryPartInStockResponseWithWarehouseOne = this.CreateInventoryPartInStockResponse(
            product.ProductCode,
            warehouse.Name,
            100
        );
        var inventoryPartInStockResponseWithWarehouseTwo = this.CreateInventoryPartInStockResponse(
            product.ProductCode,
            warehouse.Name,
            50
        );
        var inventoryPartInStockResponseWithoutWarehouse = this.CreateInventoryPartInStockResponse(
            product.ProductCode,
            string.Empty,
            200
        );
        var result = this.CreateInventoryPartInStockResult(
            inventoryPartInStockResponseWithWarehouseOne,
            inventoryPartInStockResponseWithWarehouseTwo,
            inventoryPartInStockResponseWithoutWarehouse
        );

        this.WhenGetProductsIs(parameter.GetInventoryParameter, product);
        this.WhenGetWarehouseIs(parameter.GetInventoryParameter, warehouse);

        result = this.RunExecute(parameter, result);

        result.GetInventoryResult.Inventories.Should().NotBeEmpty();
        result.GetInventoryResult.Inventories.Should().ContainKey(product.Id);
        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(
                inventoryPartInStockResponseWithWarehouseOne.AvailableQty
                    + inventoryPartInStockResponseWithWarehouseTwo.AvailableQty
            );
    }

    [Test]
    public void Execute_Should_Get_WarehouseQtyOnHandDtos_With_QtyOnHand_Aggregated_Per_Warehouse()
    {
        var product = Some.Product().WithProductCode("123").Build();
        var warehouse = new WarehouseDto { Name = "Warehouse1" };

        var parameter = this.GetDefaultParameter();

        var inventoryPartInStockResponseOne = this.CreateInventoryPartInStockResponse(
            product.ProductCode,
            warehouse.Name,
            100
        );
        var inventoryPartInStockResponseTwo = this.CreateInventoryPartInStockResponse(
            product.ProductCode,
            "NotWarehouseName",
            200
        );
        var inventoryPartInStockResponseThree = this.CreateInventoryPartInStockResponse(
            product.ProductCode,
            "NotWarehouseName",
            300
        );
        var result = this.CreateInventoryPartInStockResult(
            inventoryPartInStockResponseOne,
            inventoryPartInStockResponseTwo,
            inventoryPartInStockResponseThree
        );

        this.WhenGetProductsIs(parameter.GetInventoryParameter, product);
        this.WhenGetWarehouseIs(parameter.GetInventoryParameter, warehouse);

        result = this.RunExecute(parameter, result);

        result.GetInventoryResult.Inventories.First().Value.WarehouseQtyOnHandDtos[0].Name
            .Should()
            .Be(inventoryPartInStockResponseOne.Contract);
        result.GetInventoryResult.Inventories.First().Value.WarehouseQtyOnHandDtos[0].QtyOnHand
            .Should()
            .Be(inventoryPartInStockResponseOne.AvailableQty);
        result.GetInventoryResult.Inventories.First().Value.WarehouseQtyOnHandDtos[1].Name
            .Should()
            .Be(inventoryPartInStockResponseTwo.Contract);
        result.GetInventoryResult.Inventories.First().Value.WarehouseQtyOnHandDtos[1].QtyOnHand
            .Should()
            .Be(
                inventoryPartInStockResponseTwo.AvailableQty
                    + inventoryPartInStockResponseThree.AvailableQty
            );
    }

    protected override GetInventoryStockParameter GetDefaultParameter()
    {
        return new GetInventoryStockParameter
        {
            GetInventoryParameter = new GetInventoryParameter()
        };
    }

    protected InventoryPartInStock CreateInventoryPartInStockResponse(
        string partNo,
        string contract,
        decimal availableQty
    )
    {
        return new InventoryPartInStock
        {
            PartNo = partNo,
            Contract = contract,
            AvailableQty = availableQty
        };
    }

    protected GetInventoryStockResult CreateInventoryPartInStockResult(
        params InventoryPartInStock[] inventoryPartInStockResponses
    )
    {
        return new GetInventoryStockResult
        {
            InventoryPartInStockResponses = inventoryPartInStockResponses.ToList()
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

    private void WhenGetWarehouseIs(
        GetInventoryParameter getInventoryParameter,
        WarehouseDto warehouse
    )
    {
        this.warehouseHelper
            .Setup(o => o.GetWarehouse(this.fakeUnitOfWork, getInventoryParameter))
            .Returns(warehouse);
    }
}
