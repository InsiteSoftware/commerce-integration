namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderTotal;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;
using NUnit.Framework;

[TestFixture]
public class AddHeaderInfoToRequestTests : BaseForPipeTests<OrderTotalParameter, OrderTotalResult>
{
    public override Type PipeType => typeof(AddHeaderInfoToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_WebTransactionType()
    {
        var customerOrder = Some.CustomerOrder()
            .WithDefaultWarehouse(Some.Warehouse().WithName("Main"))
            .Build();

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "TSF",
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.WebTransactionType
        );
    }

    [Test]
    public void Execute_Should_Populate_WarehouseId_With_DefaultWarehouse_Name()
    {
        var customerOrder = Some.CustomerOrder()
            .WithDefaultWarehouse(Some.Warehouse().WithName("Main"))
            .Build();

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.DefaultWarehouse.Name,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.WarehouseID
        );
    }

    [TestCase(FulfillmentMethod.PickUp, true)]
    [TestCase(FulfillmentMethod.Ship, false)]
    public void Execute_Should_Populate_IsPickupOrder(
        FulfillmentMethod fulfillmentMethod,
        bool isPickupOrder
    )
    {
        var customerOrder = Some.CustomerOrder()
            .WithDefaultWarehouse(Some.Warehouse().WithName("Main"))
            .WithFulfillmentMethod(fulfillmentMethod.ToString())
            .Build();

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            isPickupOrder,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.IsPickUpOrder
        );
    }

    [Test]
    public void Execute_Should_Set_RequestedShipDate_To_Empty_String()
    {
        var customerOrder = Some.CustomerOrder()
            .WithDefaultWarehouse(Some.Warehouse().WithName("Main"))
            .Build();

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            string.Empty,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.RequestedShipDate
        );
    }

    protected override OrderTotalResult GetDefaultResult()
    {
        return new OrderTotalResult
        {
            OrderTotalRequest = new OrderTotalRequest
            {
                Request = new Request
                {
                    Orders = new List<RequestOrder>
                    {
                        new RequestOrder { OrderHeader = new OrderHeader() }
                    }
                }
            }
        };
    }
}
