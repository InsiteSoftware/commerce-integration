namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.InventoryAllocationInquiry;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.InventoryAllocationInquiry;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.InventoryAllocationInquiry;
using NUnit.Framework;

[TestFixture]
public class GetInventoryFromResponsesTests
    : BaseForPipeTests<InventoryAllocationInquiryParameter, InventoryAllocationInquiryResult>
{
    private IList<Product> products;

    private IList<Warehouse> warehouses;

    public override Type PipeType => typeof(GetInventoryFromResponses);

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
    public void Execute_Should_Not_Get_Inventory_When_GetInventoryParameter_ProductId_Not_Found_In_Repository()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { product.Id }
        };

        var result = this.CreateInventoryAllocationInquiryResult(
            this.CreateInventoryAllocationInquiryResponse(product.ErpNumber, string.Empty, 100)
        );

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsEmpty(result.GetInventoryResult.Inventories);
    }

    [Test]
    public void Execute_Should_Get_Inventory_For_GetInventoryParameter_ProductIds()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { product.Id }
        };

        var inventoryAllocationInquiryResponse = this.CreateInventoryAllocationInquiryResponse(
            product.ErpNumber,
            string.Empty,
            100
        );
        var result = this.CreateInventoryAllocationInquiryResult(
            inventoryAllocationInquiryResponse
        );

        this.WhenExists(product);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.GetInventoryResult.Inventories);
        Assert.AreEqual(product.Id, result.GetInventoryResult.Inventories.First().Key);
        Assert.AreEqual(
            inventoryAllocationInquiryResponse.AvailableForShipping,
            result.GetInventoryResult.Inventories.First().Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_Get_Inventory_For_GetInventoryParameter_ProductIds_And_WarehouseId()
    {
        var product = Some.Product().WithErpNumber("123").Build();
        var warehouse = Some.Warehouse().WithName("01").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { product.Id },
            WarehouseId = warehouse.Id
        };

        var inventoryAllocationInquiryResponseWithWarehouse =
            this.CreateInventoryAllocationInquiryResponse(product.ErpNumber, warehouse.Name, 100);
        var inventoryAllocationInquiryResponseWithoutWarehouse =
            this.CreateInventoryAllocationInquiryResponse(product.ErpNumber, string.Empty, 200);
        var result = this.CreateInventoryAllocationInquiryResult(
            inventoryAllocationInquiryResponseWithWarehouse,
            inventoryAllocationInquiryResponseWithoutWarehouse
        );

        this.WhenExists(product);
        this.WhenExists(warehouse);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.GetInventoryResult.Inventories);
        Assert.AreEqual(product.Id, result.GetInventoryResult.Inventories.First().Key);
        Assert.AreEqual(
            inventoryAllocationInquiryResponseWithWarehouse.AvailableForShipping,
            result.GetInventoryResult.Inventories.First().Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_Get_WarehouseQtyOnHandDtos_For_GetInventoryParameter_ProductIds()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { product.Id }
        };

        var inventoryAllocationInquiryResponseOne = this.CreateInventoryAllocationInquiryResponse(
            product.ErpNumber,
            string.Empty,
            100
        );
        var inventoryAllocationInquiryResponseTwo = this.CreateInventoryAllocationInquiryResponse(
            product.ErpNumber,
            "Doesnt matter",
            200
        );
        var inventoryAllocationInquiryResponseThree = this.CreateInventoryAllocationInquiryResponse(
            product.ErpNumber,
            string.Empty,
            300
        );
        var result = this.CreateInventoryAllocationInquiryResult(
            inventoryAllocationInquiryResponseOne,
            inventoryAllocationInquiryResponseTwo,
            inventoryAllocationInquiryResponseThree
        );

        this.WhenExists(product);

        result = this.RunExecute(parameter, result);

        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(inventoryAllocationInquiryResponseOne.WarehouseID)
                        && o.QtyOnHand.Equals(
                            inventoryAllocationInquiryResponseOne.AvailableForShipping
                        )
                )
        );
        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(inventoryAllocationInquiryResponseTwo.WarehouseID)
                        && o.QtyOnHand.Equals(
                            inventoryAllocationInquiryResponseTwo.AvailableForShipping
                        )
                )
        );
        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(inventoryAllocationInquiryResponseThree.WarehouseID)
                        && o.QtyOnHand.Equals(
                            inventoryAllocationInquiryResponseThree.AvailableForShipping
                        )
                )
        );
    }

    [Test]
    public void Execute_Should_Get_Inventory_For_GetInventoryParameter_Product()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            Products = new List<Product> { product }
        };

        var inventoryAllocationInquiryResponse = this.CreateInventoryAllocationInquiryResponse(
            product.ErpNumber,
            string.Empty,
            100
        );
        var result = this.CreateInventoryAllocationInquiryResult(
            inventoryAllocationInquiryResponse
        );

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.GetInventoryResult.Inventories);
        Assert.AreEqual(product.Id, result.GetInventoryResult.Inventories.First().Key);
        Assert.AreEqual(
            inventoryAllocationInquiryResponse.AvailableForShipping,
            result.GetInventoryResult.Inventories.First().Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_Get_Inventory_For_GetInventoryParameter_Products_And_WarehouseId()
    {
        var product = Some.Product().WithErpNumber("123").Build();
        var warehouse = Some.Warehouse().WithName("01").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            Products = new List<Product> { product },
            WarehouseId = warehouse.Id
        };

        var inventoryAllocationInquiryResponseWithWarehouse =
            this.CreateInventoryAllocationInquiryResponse(product.ErpNumber, warehouse.Name, 100);
        var inventoryAllocationInquiryResponseWithoutWarehouse =
            this.CreateInventoryAllocationInquiryResponse(product.ErpNumber, string.Empty, 200);
        var result = this.CreateInventoryAllocationInquiryResult(
            inventoryAllocationInquiryResponseWithWarehouse,
            inventoryAllocationInquiryResponseWithoutWarehouse
        );

        this.WhenExists(warehouse);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.GetInventoryResult.Inventories);
        Assert.AreEqual(product.Id, result.GetInventoryResult.Inventories.First().Key);
        Assert.AreEqual(
            inventoryAllocationInquiryResponseWithWarehouse.AvailableForShipping,
            result.GetInventoryResult.Inventories.First().Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_Get_WarehouseQtyOnHandDtos_For_GetInventoryParameter_Products()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            Products = new List<Product> { product },
        };

        var inventoryAllocationInquiryResponseOne = this.CreateInventoryAllocationInquiryResponse(
            product.ErpNumber,
            string.Empty,
            100
        );
        var inventoryAllocationInquiryResponseTwo = this.CreateInventoryAllocationInquiryResponse(
            product.ErpNumber,
            "Doesnt matter",
            200
        );
        var inventoryAllocationInquiryResponseThree = this.CreateInventoryAllocationInquiryResponse(
            product.ErpNumber,
            string.Empty,
            300
        );
        var result = this.CreateInventoryAllocationInquiryResult(
            inventoryAllocationInquiryResponseOne,
            inventoryAllocationInquiryResponseTwo,
            inventoryAllocationInquiryResponseThree
        );

        result = this.RunExecute(parameter, result);

        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(inventoryAllocationInquiryResponseOne.WarehouseID)
                        && o.QtyOnHand.Equals(
                            inventoryAllocationInquiryResponseOne.AvailableForShipping
                        )
                )
        );
        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(inventoryAllocationInquiryResponseTwo.WarehouseID)
                        && o.QtyOnHand.Equals(
                            inventoryAllocationInquiryResponseTwo.AvailableForShipping
                        )
                )
        );
        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(inventoryAllocationInquiryResponseThree.WarehouseID)
                        && o.QtyOnHand.Equals(
                            inventoryAllocationInquiryResponseThree.AvailableForShipping
                        )
                )
        );
    }

    protected InventoryAllocationInquiry CreateInventoryAllocationInquiryResponse(
        string inventoryId,
        string warehouseId,
        decimal availableForShipping
    )
    {
        return new InventoryAllocationInquiry
        {
            InventoryID = inventoryId,
            WarehouseID = warehouseId,
            AvailableForShipping = availableForShipping
        };
    }

    protected InventoryAllocationInquiryResult CreateInventoryAllocationInquiryResult(
        params InventoryAllocationInquiry[] inventoryAllocationInquiryResponses
    )
    {
        return new InventoryAllocationInquiryResult
        {
            InventoryAllocationInquiryResponses = inventoryAllocationInquiryResponses.ToList()
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
}
