namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetItemPrice;

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class GetInventoryFromResponseTests
    : BaseForPipeTests<GetItemPriceParameter, GetItemPriceResult>
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
    public void Order_Is_900()
    {
        Assert.AreEqual(900, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_Get_Inventory_Results_For_Pricing_Service_Parameters_From_Get_Item_Price_Reply()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty) { Product = product };

        var parameter = this.CreateGetItemPriceParameter(null, pricingServiceParameter);

        var replyItem = this.CreateReplyItem("123", string.Empty, "25");

        var result = this.CreateGetItemPriceResult(replyItem);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.PricingServiceResults);
        Assert.AreEqual(
            25M,
            result.PricingServiceResults
                .First()
                .Value.GetInventoryResult.Inventories.First()
                .Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_Create_Get_Inventory_Result_For_Get_Inventory_Parameter_Products_From_Get_Item_Price_Reply()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var getInventoryParameter = new GetInventoryParameter
        {
            Products = new List<Product> { product }
        };

        var parameter = this.CreateGetItemPriceParameter(getInventoryParameter);

        var replyItem = this.CreateReplyItem("123", string.Empty, "25");

        var result = this.CreateGetItemPriceResult(replyItem);

        result = this.RunExecute(parameter, result);

        Assert.AreEqual(25M, result.GetInventoryResult.Inventories.First().Value.QtyOnHand);
    }

    [Test]
    public void Execute_Should_Create_Get_Inventory_Result_For_Get_Inventory_Parameter_ProductIds_From_Get_Item_Price_Reply()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var getInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { product.Id }
        };

        var parameter = this.CreateGetItemPriceParameter(getInventoryParameter);

        var replyItem = this.CreateReplyItem("123", string.Empty, "25");

        var result = this.CreateGetItemPriceResult(replyItem);

        this.WhenExists(product);

        result = this.RunExecute(parameter, result);

        Assert.AreEqual(25M, result.GetInventoryResult.Inventories.First().Value.QtyOnHand);
    }

    [Test]
    public void Execute_Should_Populate_Warehouse_Qty_On_Hand_Dtos_From_Get_Item_Price_Reply_List_Of_Item_Location_Quantities()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var getInventoryParameter = new GetInventoryParameter
        {
            Products = new List<Product> { product }
        };

        var parameter = this.CreateGetItemPriceParameter(getInventoryParameter);

        var replyItemLocationQuantityOne = this.CreateReplyItemLocationQuantity(
            "Warehouse One",
            "10"
        );
        var replyItemLocationQuantityTwo = this.CreateReplyItemLocationQuantity(
            "Warehouse Two",
            "15"
        );

        var replyItem = this.CreateReplyItem(
            "123",
            string.Empty,
            "25",
            replyItemLocationQuantityOne,
            replyItemLocationQuantityTwo
        );

        var result = this.CreateGetItemPriceResult(replyItem);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(
            result.GetInventoryResult.Inventories.Values.First().WarehouseQtyOnHandDtos
        );

        Assert.IsTrue(
            result.GetInventoryResult.Inventories.Values
                .First()
                .WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(replyItemLocationQuantityOne.LocationID)
                        && o.QtyOnHand.ToString().Equals(replyItemLocationQuantityOne.FreeQuantity)
                )
        );

        Assert.IsTrue(
            result.GetInventoryResult.Inventories.Values
                .First()
                .WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(replyItemLocationQuantityTwo.LocationID)
                        && o.QtyOnHand.ToString().Equals(replyItemLocationQuantityTwo.FreeQuantity)
                )
        );
    }

    protected GetItemPriceParameter CreateGetItemPriceParameter(
        GetInventoryParameter getInventoryParameter,
        params PricingServiceParameter[] pricingServiceParameters
    )
    {
        return new GetItemPriceParameter
        {
            GetInventoryParameter = getInventoryParameter,
            PricingServiceParameters = pricingServiceParameters.ToList()
        };
    }

    protected ReplyItem CreateReplyItem(
        string itemId,
        string unitName,
        string freeQuantity,
        params ReplyItemLocationQuantity[] replyItemLocationQuantities
    )
    {
        return new ReplyItem
        {
            ItemID = itemId,
            UnitName = unitName,
            FreeQuantity = freeQuantity,
            ListOfItemLocationQuantities = replyItemLocationQuantities.ToList()
        };
    }

    protected ReplyItemLocationQuantity CreateReplyItemLocationQuantity(
        string locationId,
        string freeQuantity
    )
    {
        return new ReplyItemLocationQuantity
        {
            LocationID = locationId,
            FreeQuantity = freeQuantity
        };
    }

    protected GetItemPriceResult CreateGetItemPriceResult(params ReplyItem[] replyItems)
    {
        return new GetItemPriceResult
        {
            GetItemPriceReply = new GetItemPrice
            {
                Reply = new Reply { ListOfItems = replyItems.ToList() }
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
