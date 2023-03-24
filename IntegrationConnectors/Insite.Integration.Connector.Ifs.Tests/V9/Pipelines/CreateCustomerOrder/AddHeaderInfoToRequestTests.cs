namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.CreateCustomerOrder;

using System;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;

[TestFixture]
public class AddHeaderInfoToRequestTests
    : BaseForPipeTests<CreateCustomerOrderParameter, CreateCustomerOrderResult>
{
    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    public override Type PipeType => typeof(AddHeaderInfoToRequest);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();

        this.WhenIfsDefaultDeliveryTermsIs(string.Empty);
        this.WhenIfsPickupDeliveryTermsIs(string.Empty);
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_Header_Info()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.ShipVia().WithErpShipCode("Truck"))
            .WithCustomerPO("Test PO")
            .WithOrderNumber("Web123")
            .WithTermsCode("CC")
            .WithDefaultWarehouse(Some.Warehouse().WithName("01"))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customerOrder.ShipVia.ErpShipCode, result.CustomerOrder.shipViaCode);
        Assert.AreEqual(customerOrder.CustomerPO, result.CustomerOrder.customerPoNo);
        Assert.AreEqual(customerOrder.OrderNumber, result.CustomerOrder.custRef);
        Assert.AreEqual(customerOrder.TermsCode, result.CustomerOrder.payTermId);
        Assert.AreEqual(customerOrder.DefaultWarehouse.Name, result.CustomerOrder.contract);
    }

    [Test]
    public void Execute_Should_Populate_DeliveryTerms_With_IfsPickupDeliveryTerms_When_FulfillmentMethod_Is_PickUp()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenIfsPickupDeliveryTermsIs("Pickup");

        var result = this.RunExecute(parameter);

        Assert.AreEqual("Pickup", result.CustomerOrder.deliveryTerms);
    }

    [Test]
    public void Execute_Should_Populate_DeliveryTerms_With_FreightTerms_When_Not_DropShip_And_ShipTo_Has_FreightTerms()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();
        customerOrder.ShipTo.FreightTerms = "My Terms";

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual("My Terms", result.CustomerOrder.deliveryTerms);
    }

    [Test]
    public void Execute_Should_Populate_DeliveryTerms_With_IfsDefaultDeliveryTerms()
    {
        var customerOrder = Some.CustomerOrder().Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenIfsDefaultDeliveryTermsIs("Default");

        var result = this.RunExecute(parameter);

        Assert.AreEqual("Default", result.CustomerOrder.deliveryTerms);
    }

    [Test]
    public void Execute_Should_Populate_WantedDeliveryDate_With_RequestedPickupDate_When_CustomerOrder_Is_Pickup()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithRequestedPickupDate(DateTimeOffset.Now.AddDays(1))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.RequestedPickupDate.Value.DateTime,
            result.CustomerOrder.wantedDeliveryDate
        );
    }

    [Test]
    public void Execute_Should_Populate_WantedDeliveryDate_With_RequestedDeliveryDate_When_CustomerOrder_Is_Not_Pickup()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
            .WithRequestedDeliveryDate(DateTimeOffset.Now.AddDays(1))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.RequestedDeliveryDate.Value.DateTime,
            result.CustomerOrder.wantedDeliveryDate
        );
    }

    [Test]
    public void Execute_Should_Populate_NoteText_With_Notes_To_Two_Thousand_Characters()
    {
        var customerOrder = Some.CustomerOrder().WithNotes(new string('*', 2001)).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customerOrder.Notes.Substring(0, 2000), result.CustomerOrder.noteText);
    }

    protected override CreateCustomerOrderResult GetDefaultResult()
    {
        return new CreateCustomerOrderResult { CustomerOrder = new customerOrder() };
    }

    protected void WhenIfsPickupDeliveryTermsIs(string ifsPickupDeliveryTerms)
    {
        this.integrationConnectorSettings
            .Setup(o => o.IfsPickupDeliveryTerms)
            .Returns(ifsPickupDeliveryTerms);
    }

    protected void WhenIfsDefaultDeliveryTermsIs(string ifsDefaultDeliveryTerms)
    {
        this.integrationConnectorSettings
            .Setup(o => o.IfsDefaultDeliveryTerms)
            .Returns(ifsDefaultDeliveryTerms);
    }
}
