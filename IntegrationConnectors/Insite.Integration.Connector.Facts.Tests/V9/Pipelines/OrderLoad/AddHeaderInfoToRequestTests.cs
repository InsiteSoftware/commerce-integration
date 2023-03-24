namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderLoad;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Common.Providers;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;
using NUnit.Framework;

[TestFixture]
public class AddHeaderInfoToRequestTests : BaseForPipeTests<OrderLoadParameter, OrderLoadResult>
{
    private const string RequestedShipDateFormat = "yyyy-MM-dd";

    public override Type PipeType => typeof(AddHeaderInfoToRequest);

    public override void SetUp()
    {
        DateTimeProvider.Current = new MockDateTimeProvider(DateTime.Now);
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_OrderHeader()
    {
        var customerOrder = Some.CustomerOrder()
            .WithOrderNumber("123")
            .WithDefaultWarehouse(Some.Warehouse().WithName("MAIN"))
            .With(Some.ShipVia().WithErpShipCode("TRUCK"))
            .WithCustomerPO("PO123")
            .WithShippingCharges(12)
            .WithHandlingCharges(2)
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "LSF",
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.WebTransactionType
        );
        Assert.AreEqual("O", result.OrderLoadRequest.Request.Orders.First().OrderHeader.OrderType);
        Assert.AreEqual(
            customerOrder.OrderNumber,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.WebOrderID
        );
        Assert.AreEqual(
            customerOrder.DefaultWarehouse.Name,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.WarehouseID
        );
        Assert.AreEqual(
            customerOrder.ShipVia.ErpShipCode,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.CarrierCode
        );
        Assert.AreEqual(
            customerOrder.CustomerPO,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.PoNumber
        );
        Assert.AreEqual(
            string.Empty,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.ReviewOrderHold
        );
        Assert.AreEqual(
            true,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.IsIncludeFreight
        );
        Assert.AreEqual(
            customerOrder.ShippingCharges,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.FreightAmount
        );
        Assert.AreEqual(
            customerOrder.HandlingCharges,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.HandlingAmount
        );
    }

    [Test]
    public void Execute_Should_Populate_RequestedShipDate_With_RequestedPickupDate_When_Order_Is_Pickup()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithRequestedPickupDate(DateTimeOffset.Now.AddDays(3))
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.RequestedPickupDate.Value.ToString(RequestedShipDateFormat),
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.RequestedShipDate
        );
        Assert.AreEqual(
            true,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.IsPickUpOrder
        );
    }

    [Test]
    public void Execute_Should_Populate_RequestedShipDate_With_CurrentDate_When_Order_Is_Pickup_And_RequestedPickupDate_Is_Null()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            DateTimeProvider.Current.Now.ToString(RequestedShipDateFormat),
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.RequestedShipDate
        );
        Assert.AreEqual(
            true,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.IsPickUpOrder
        );
    }

    [Test]
    public void Execute_Should_Populate_RequestedShipDate_With_RequestedDeliveryDate_When_Order_Is_Not_Pickup()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
            .WithRequestedDeliveryDate(DateTimeOffset.Now.AddDays(3))
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.RequestedDeliveryDate.Value.ToString(RequestedShipDateFormat),
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.RequestedShipDate
        );
        Assert.AreEqual(
            false,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.IsPickUpOrder
        );
    }

    [Test]
    public void Execute_Should_Populate_RequestedShipDate_With_RequestedShipDate_When_Order_Is_Not_Pickup_And_RequestedDeliveryDate_Is_Null()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
            .WithRequestedShipDate(DateTimeOffset.Now.AddDays(3))
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.RequestedShipDate.Value.ToString(RequestedShipDateFormat),
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.RequestedShipDate
        );
        Assert.AreEqual(
            false,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.IsPickUpOrder
        );
    }

    [Test]
    public void Execute_Should_Populate_RequestedShipDate_With_CurrentDate_When_Order_Is_Not_Pickup_And_RequestedDeliveryDate_Is_Null_And_RequestedShipDate_Is_Null()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            DateTimeProvider.Current.Now.ToString(RequestedShipDateFormat),
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.RequestedShipDate
        );
        Assert.AreEqual(
            false,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.IsPickUpOrder
        );
    }

    protected override OrderLoadResult GetDefaultResult()
    {
        return new OrderLoadResult
        {
            OrderLoadRequest = new OrderLoadRequest
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
