namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetPartAvailability;

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

[TestFixture]
public class GetInventoryFromResponseTests
    : BaseForPipeTests<GetPartAvailabilityParameter, GetPartAvailabilityResult>
{
    private IList<Product> products;

    private IList<Warehouse> warehouses;

    public override Type PipeType => typeof(GetInventoryFromResponse);

    public override void SetUp()
    {
        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);

        this.warehouses = new List<Warehouse>();
        this.fakeUnitOfWork.SetupEntityList(this.warehouses);
    }

    [Test]
    public void Order_Is_700()
    {
        Assert.AreEqual(700, this.pipe.Order);
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

        var result = this.CreateGetPartAvailabilityResult(
            this.CreatePartAvailabilityResData(product.ErpNumber, string.Empty, 100)
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

        var partAvailabilityResData = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            string.Empty,
            100
        );
        var result = this.CreateGetPartAvailabilityResult(partAvailabilityResData);

        this.WhenExists(product);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.GetInventoryResult.Inventories);
        Assert.AreEqual(product.Id, result.GetInventoryResult.Inventories.First().Key);
        Assert.AreEqual(
            partAvailabilityResData.quantityAvailable,
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

        var partAvailabilityResDataWithWarehouse = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            warehouse.Name,
            100
        );
        var partAvailabilityResDataWithoutWarehouse = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            string.Empty,
            200
        );
        var result = this.CreateGetPartAvailabilityResult(
            partAvailabilityResDataWithWarehouse,
            partAvailabilityResDataWithoutWarehouse
        );

        this.WhenExists(product);
        this.WhenExists(warehouse);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.GetInventoryResult.Inventories);
        Assert.AreEqual(product.Id, result.GetInventoryResult.Inventories.First().Key);
        Assert.AreEqual(
            partAvailabilityResDataWithWarehouse.quantityAvailable,
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

        var partAvailabilityResDataOne = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            string.Empty,
            100
        );
        var partAvailabilityResDataTwo = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            "Doesnt matter",
            200
        );
        var partAvailabilityResDataThree = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            string.Empty,
            300
        );
        var result = this.CreateGetPartAvailabilityResult(
            partAvailabilityResDataOne,
            partAvailabilityResDataTwo,
            partAvailabilityResDataThree
        );

        this.WhenExists(product);

        result = this.RunExecute(parameter, result);

        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(partAvailabilityResDataOne.partsAvailableSite)
                        && o.QtyOnHand.Equals(partAvailabilityResDataOne.quantityAvailable)
                )
        );
        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(partAvailabilityResDataTwo.partsAvailableSite)
                        && o.QtyOnHand.Equals(partAvailabilityResDataTwo.quantityAvailable)
                )
        );
        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(partAvailabilityResDataThree.partsAvailableSite)
                        && o.QtyOnHand.Equals(partAvailabilityResDataThree.quantityAvailable)
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

        var partAvailabilityResData = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            string.Empty,
            100
        );
        var result = this.CreateGetPartAvailabilityResult(partAvailabilityResData);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.GetInventoryResult.Inventories);
        Assert.AreEqual(product.Id, result.GetInventoryResult.Inventories.First().Key);
        Assert.AreEqual(
            partAvailabilityResData.quantityAvailable,
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

        var partAvailabilityResDataWithWarehouse = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            warehouse.Name,
            100
        );
        var partAvailabilityResDataWithoutWarehouse = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            string.Empty,
            200
        );
        var result = this.CreateGetPartAvailabilityResult(
            partAvailabilityResDataWithWarehouse,
            partAvailabilityResDataWithoutWarehouse
        );

        this.WhenExists(warehouse);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.GetInventoryResult.Inventories);
        Assert.AreEqual(product.Id, result.GetInventoryResult.Inventories.First().Key);
        Assert.AreEqual(
            partAvailabilityResDataWithWarehouse.quantityAvailable,
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

        var partAvailabilityResDataOne = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            string.Empty,
            100
        );
        var partAvailabilityResDataTwo = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            "Doesnt matter",
            200
        );
        var partAvailabilityResDataThree = this.CreatePartAvailabilityResData(
            product.ErpNumber,
            string.Empty,
            300
        );
        var result = this.CreateGetPartAvailabilityResult(
            partAvailabilityResDataOne,
            partAvailabilityResDataTwo,
            partAvailabilityResDataThree
        );

        result = this.RunExecute(parameter, result);

        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(partAvailabilityResDataOne.partsAvailableSite)
                        && o.QtyOnHand.Equals(partAvailabilityResDataOne.quantityAvailable)
                )
        );
        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(partAvailabilityResDataTwo.partsAvailableSite)
                        && o.QtyOnHand.Equals(partAvailabilityResDataTwo.quantityAvailable)
                )
        );
        Assert.IsTrue(
            result.GetInventoryResult.Inventories
                .First()
                .Value.WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(partAvailabilityResDataThree.partsAvailableSite)
                        && o.QtyOnHand.Equals(partAvailabilityResDataThree.quantityAvailable)
                )
        );
    }

    protected partAvailabilityResData CreatePartAvailabilityResData(
        string productNo,
        string partsAvailableSite,
        int quantityAvailable
    )
    {
        return new partAvailabilityResData
        {
            productNo = productNo,
            partsAvailableSite = partsAvailableSite,
            quantityAvailable = quantityAvailable
        };
    }

    protected GetPartAvailabilityResult CreateGetPartAvailabilityResult(
        params partAvailabilityResData[] partAvailabilityResDatas
    )
    {
        return new GetPartAvailabilityResult
        {
            PartAvailabilityResponse = new partAvailabilityResponse
            {
                partsAvailabile = partAvailabilityResDatas.ToList()
            }
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
