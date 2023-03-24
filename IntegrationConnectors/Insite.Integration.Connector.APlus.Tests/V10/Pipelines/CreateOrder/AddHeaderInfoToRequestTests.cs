namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.CreateOrder;

using System;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class AddHeaderInfoToRequestTests : BaseForPipeTests<CreateOrderParameter, CreateOrderResult>
{
    public override Type PipeType => typeof(AddHeaderInfoToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_RequestOrderHeader_Properties_From_CustomerOrder_Properties()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber(string.Empty))
            .With(Some.ShipVia().WithErpShipCode("ABC"))
            .WithCustomerPO("PONumber")
            .WithRequestedDeliveryDate(DateTimeOffset.Now)
            .WithDefaultWarehouse(Some.Warehouse().WithName("01"))
            .WithOrderNumber("OrderNum1")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.ShipVia.ErpShipCode,
            result.CreateOrderRequest.Orders[0].OrderHeader.CarrierCode
        );
        Assert.AreEqual(
            customerOrder.CustomerPO,
            result.CreateOrderRequest.Orders[0].OrderHeader.PONumber
        );
        Assert.AreEqual(
            customerOrder.DefaultWarehouse.Name,
            result.CreateOrderRequest.Orders[0].OrderHeader.WarehouseId
        );
    }

    [Test]
    public void Execute_Should_Set_Requested_Ship_Date_From_Requested_Pickup_Date_When_Customer_Order_Fulfillment_Method_Is_Pickup()
    {
        var customerOrder = Some.CustomerOrder()
            .WithRequestedPickupDate(DateTimeOffset.Now.AddDays(2))
            .WithRequestedDeliveryDate(DateTimeOffset.Now.AddDays(4))
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);
        Assert.AreEqual(
            customerOrder.RequestedPickupDate.Value.ToString("yyyyMMdd"),
            result.CreateOrderRequest.Orders.First().OrderHeader.ReqShipDate
        );
    }

    [Test]
    public void Execute_Should_Set_Requested_Ship_Date_From_Requested_Delivery_Date_When_Customer_Order_Fulfillment_Method_Is_Not_Pickup()
    {
        var customerOrder = Some.CustomerOrder()
            .WithRequestedPickupDate(DateTimeOffset.Now.AddDays(2))
            .WithRequestedDeliveryDate(DateTimeOffset.Now.AddDays(4))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);
        Assert.AreEqual(
            customerOrder.RequestedDeliveryDate.Value.ToString("yyyyMMdd"),
            result.CreateOrderRequest.Orders.First().OrderHeader.ReqShipDate
        );
    }

    [Test]
    public void Execute_Should_Set_WebOrderId_When_IsOrderSubmit_True()
    {
        var customerOrder = Some.CustomerOrder()
            .WithOrderNumber("OrderNumber")
            .With(Some.Customer().WithErpNumber(string.Empty))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;
        parameter.IsOrderSubmit = true;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderNumber,
            result.CreateOrderRequest.Orders[0].OrderHeader.WebOrderID
        );
    }

    [Test]
    public void Execute_Should_Not_Set_WebOrderId_When_IsOrderSubmit_False()
    {
        var customerOrder = Some.CustomerOrder()
            .WithOrderNumber("OrderNumber")
            .With(Some.Customer().WithErpNumber(string.Empty))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;
        parameter.IsOrderSubmit = false;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(string.Empty, result.CreateOrderRequest.Orders[0].OrderHeader.WebOrderID);
    }

    [Test]
    public void Execute_Should_Set_WebTransactionType_To_LSF_When_IsOrderSubmit_True()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber(string.Empty))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;
        parameter.IsOrderSubmit = true;

        var result = this.RunExecute(parameter);

        Assert.AreEqual("LSF", result.CreateOrderRequest.Orders[0].OrderHeader.WebTransactionType);
    }

    [Test]
    public void Execute_Should_Set_WebTransactionType_To_TSF_When_IsOrderSubmit_False()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber(string.Empty))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;
        parameter.IsOrderSubmit = false;

        var result = this.RunExecute(parameter);

        Assert.AreEqual("TSF", result.CreateOrderRequest.Orders[0].OrderHeader.WebTransactionType);
    }

    [Test]
    public void Execute_Should_Change_Special_Characters_For_UserName()
    {
        var customerOrder = Some.CustomerOrder().WithPlacedByUserName("asd$fr@te^s.uf").Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "asd_fr_te_s.uf",
            result.CreateOrderRequest.Orders[0].OrderHeader.WebUserID
        );
    }

    protected override CreateOrderResult GetDefaultResult()
    {
        return new CreateOrderResult { CreateOrderRequest = new CreateOrderRequest() };
    }
}
