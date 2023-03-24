namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;

[TestFixture]
public class AddHeaderInfoToRequestTests
    : BaseForPipeTests<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    public override Type PipeType => typeof(AddHeaderInfoToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Set_Carrier_Code_When_Customer_Order_Ship_Via_Is_Not_Null()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.ShipVia().WithErpShipCode("Truck"))
            .Build();

        var parameter = new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.ShipVia.ErpShipCode,
            result.SFOEOrderTotLoadV4Request.Request.InputHeaderDataCollection.InputHeaderDatas[
                0
            ].Carriercode
        );
    }

    [Test]
    public void Execute_Should_Set_Carrier_Code_To_Empty_String_When_Customer_Order_Ship_Via_Is_Null()
    {
        var result = this.RunExecute();

        Assert.IsEmpty(
            result.SFOEOrderTotLoadV4Request.Request.InputHeaderDataCollection.InputHeaderDatas[
                0
            ].Carriercode
        );
    }

    [TestCase(true, "O")]
    [TestCase(false, "")]
    public void Execute_Should_Set_Order_Type(bool isOrderSubmit, string orderType)
    {
        var parameter = this.GetDefaultParameter();
        parameter.IsOrderSubmit = isOrderSubmit;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            orderType,
            result.SFOEOrderTotLoadV4Request.Request.InputHeaderDataCollection.InputHeaderDatas[
                0
            ].Ordertype
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

        var parameter = new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);
        Assert.AreEqual(
            customerOrder.RequestedPickupDate.Value.ToString("yyyy/MM/dd"),
            result.SFOEOrderTotLoadV4Request.Request.InputHeaderDataCollection.InputHeaderDatas[
                0
            ].Reqshipdate
        );
    }

    [Test]
    public void Execute_Should_Set_Requested_Ship_Date_From_Requested_Delivery_Date_When_Customer_Order_Fulfillment_Method_Is_Not_Pickup()
    {
        var customerOrder = Some.CustomerOrder()
            .WithRequestedPickupDate(DateTimeOffset.Now.AddDays(2))
            .WithRequestedDeliveryDate(DateTimeOffset.Now.AddDays(4))
            .Build();

        var parameter = new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);
        Assert.AreEqual(
            customerOrder.RequestedDeliveryDate.Value.ToString("yyyy/MM/dd"),
            result.SFOEOrderTotLoadV4Request.Request.InputHeaderDataCollection.InputHeaderDatas[
                0
            ].Reqshipdate
        );
    }

    [Test]
    public void Execute_Should_Get_Warehouse_From_Customer_Order_When_Warehouse_Not_Null()
    {
        var customerOrder = Some.CustomerOrder()
            .WithDefaultWarehouse(Some.Warehouse().WithName("ABC123"))
            .Build();

        var parameter = new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.DefaultWarehouse.Name,
            result.SFOEOrderTotLoadV4Request.Request.InputHeaderDataCollection.InputHeaderDatas[
                0
            ].Warehouseid
        );
    }

    [Test]
    public void Execute_Should_Get_Warehouse_As_Empty_When_Customer_Order_Warehouse_Is_Null()
    {
        var result = this.RunExecute();

        Assert.AreEqual(
            string.Empty,
            result.SFOEOrderTotLoadV4Request.Request.InputHeaderDataCollection.InputHeaderDatas[
                0
            ].Warehouseid
        );
    }

    [TestCase(true, "LSF")]
    [TestCase(false, "TSF")]
    public void Execute_Should_Set_Web_Transaction_Type(
        bool isOrderSubmit,
        string webTransactionType
    )
    {
        var parameter = this.GetDefaultParameter();
        parameter.IsOrderSubmit = isOrderSubmit;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            webTransactionType,
            result.SFOEOrderTotLoadV4Request.Request.InputHeaderDataCollection.InputHeaderDatas[
                0
            ].Webtransactiontype
        );
    }

    protected override SFOEOrderTotLoadV4Parameter GetDefaultParameter()
    {
        return new SFOEOrderTotLoadV4Parameter { CustomerOrder = Some.CustomerOrder() };
    }

    protected override SFOEOrderTotLoadV4Result GetDefaultResult()
    {
        return new SFOEOrderTotLoadV4Result
        {
            SFOEOrderTotLoadV4Request = new SFOEOrderTotLoadV4Request { Request = new Request() }
        };
    }
}
