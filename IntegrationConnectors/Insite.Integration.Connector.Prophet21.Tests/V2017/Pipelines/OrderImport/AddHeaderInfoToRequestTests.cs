namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.OrderImport;

using System;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class AddHeaderInfoToRequestTests : BaseForPipeTests<OrderImportParameter, OrderImportResult>
{
    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    public override Type PipeType => typeof(AddHeaderInfoToRequest);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_OrderImportRequest_Request_Properties()
    {
        var customerOrder = Some.CustomerOrder()
            .WithOrderNumber("ABC123")
            .WithCustomerPO("Test")
            .WithNotes("Notes")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderNumber,
            result.OrderImportRequest.Request.WebReferenceNumber
        );
        Assert.AreEqual(customerOrder.CustomerPO, result.OrderImportRequest.Request.PONumber);
        Assert.AreEqual(customerOrder.Notes, result.OrderImportRequest.Request.NotepadText);
    }

    [Test]
    public void Execute_Should_Populate_OrderImportRequest_Request_RequireDate_With_CustomerOrder_RequestedPickupDate()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithRequestedPickupDate(DateTimeOffset.Now.AddDays(2))
            .WithRequestedDeliveryDate(DateTimeOffset.Now.AddDays(4))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.RequestedPickupDate?.ToString("yyyy-MM-dd"),
            result.OrderImportRequest.Request.RequireDate
        );
        Assert.AreEqual("TRUE", result.OrderImportRequest.Request.WillCall);
    }

    [Test]
    public void Execute_Should_Populate_OrderImportRequest_Request_RequireDate_With_CustomerOrder_RequestedDeliveryDate()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
            .WithRequestedPickupDate(DateTimeOffset.Now.AddDays(2))
            .WithRequestedDeliveryDate(DateTimeOffset.Now.AddDays(4))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21FreightCodeIs("FREIGHT-ECOMMERCE");

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.RequestedDeliveryDate?.ToString("yyyy-MM-dd"),
            result.OrderImportRequest.Request.RequireDate
        );
        Assert.AreEqual("FREIGHT-ECOMMERCE", result.OrderImportRequest.Request.FreightCode);
    }

    [TestCase("ship", "bill", "default", "ship")]
    [TestCase("", "bill", "default", "bill")]
    [TestCase("", "", "default", "default")]
    public void Execute_Should_Populate_OrderImportRequest_Request_FreightCode(
        string shipToFreightTerms,
        string billToFreightTerms,
        string defaultFreightCode,
        string resultFreightCode
    )
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
            .WithShipTo(Some.Customer().WithFreightTerms(shipToFreightTerms))
            .With(Some.Customer().WithFreightTerms(billToFreightTerms))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21FreightCodeIs(defaultFreightCode);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(resultFreightCode, result.OrderImportRequest.Request.FreightCode);
    }

    protected override OrderImportResult GetDefaultResult()
    {
        return new OrderImportResult
        {
            OrderImportRequest = new OrderImport { Request = new Request() }
        };
    }

    protected void WhenProphet21FreightCodeIs(string prophet21FreightCode)
    {
        this.integrationConnectorSettings
            .Setup(o => o.Prophet21FreightCode)
            .Returns(prophet21FreightCode);
    }
}
