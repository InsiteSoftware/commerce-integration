namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitOrder;

using System;
using FluentAssertions;
using Insite.Common.Providers;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CreateRequestTests : BaseForPipeTests<SubmitOrderParameter, SubmitOrderResult>
{
    private Mock<ICustomerHelper> customerHelper;

    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    public override Type PipeType => typeof(CreateRequest);

    public override void SetUp()
    {
        this.customerHelper = this.container.GetMock<ICustomerHelper>();
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();
    }

    [Test]
    public void Order_Is_100()
    {
        this.pipe.Order.Should().Be(100);
    }

    [Test]
    public void Execute_Should_Add_BillTo_And_ShipTo_To_Requests()
    {
        var billTo = Some.Customer()
            .WithErpNumber("BillToErpNumber")
            .WithErpSequence("BillToErpSequence")
            .Build();
        var shipTo = Some.Customer().WithErpSequence("ShipToErpSequence").Build();
        var parameter = this.GetDefaultParameter();

        this.WhenGetBillToIs(parameter.CustomerOrder, billTo);
        this.WhenGetShipToIs(parameter.CustomerOrder, shipTo);

        var result = this.RunExecute(parameter);

        result.CustomerOrderRequest.CustomerNo.Should().Be(billTo.ErpNumber);
        result.CustomerOrderRequest.BillAddrNo.Should().Be(billTo.ErpSequence);
        result.CustomerOrderRequest.ShipAddrNo.Should().Be(shipTo.ErpSequence);
    }

    [Test]
    public void Execute_Should_Add_CustomerOrder_Properties_To_Requests()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.RunExecute(parameter);

        result.CustomerOrderRequest.CurrencyCode
            .Should()
            .Be(parameter.CustomerOrder.Currency.CurrencyCode);
        result.CustomerOrderRequest.ShipViaCode
            .Should()
            .Be(parameter.CustomerOrder.ShipVia.ErpShipCode);
        result.CustomerOrderRequest.WantedDeliveryDate
            .Should()
            .Be(parameter.CustomerOrder.RequestedDeliveryDate);
        result.CustomerOrderRequest.Contract
            .Should()
            .Be(parameter.CustomerOrder.DefaultWarehouse.Name);
        result.CustomerOrderRequest.CustomerPoNo.Should().Be(parameter.CustomerOrder.CustomerPO);
    }

    [Test]
    public void Execute_Should_Default_Date_Properties_To_Tomorrow()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder.RequestedDeliveryDate = null;

        var dateTimeNow = DateTimeOffset.Now;
        DateTimeProvider.Current = new MockDateTimeProvider(dateTimeNow.UtcDateTime);

        var result = this.RunExecute(parameter);

        result.CustomerOrderRequest.WantedDeliveryDate.Should().Be(dateTimeNow.AddDays(1));
    }

    [Test]
    public void Execute_Should_Add_Settings_To_Requests()
    {
        var ifsAurenaOrderCoordinator = "OrderCoordinator";
        var ifsAurenaOrderType = "OrderType";
        var parameter = this.GetDefaultParameter();

        this.WhenIfsAurenaOrderCoordinatorIs(ifsAurenaOrderCoordinator);
        this.WhenIfsAurenaOrderTypeIs(ifsAurenaOrderType);

        var result = this.RunExecute(parameter);

        result.CustomerOrderRequest.AuthorizeCode.Should().Be(ifsAurenaOrderCoordinator);
        result.CustomerOrderRequest.OrderId.Should().Be(ifsAurenaOrderType);
    }

    [Test]
    public void Execute_Should_Set_DeliveryTerms_To_IfsAurenaPickupDeliveryTerms_When_Order_Is_Pickup()
    {
        var ifsAurenaPickupDeliveryTerms = "PickupDeliveryTerms";

        var parameter = new SubmitOrderParameter
        {
            CustomerOrder = Some.CustomerOrder()
                .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
                .Build()
        };

        this.WhenIfsAurenaPickupDeliveryTermsIs(ifsAurenaPickupDeliveryTerms);

        var result = this.RunExecute(parameter);

        result.CustomerOrderRequest.DeliveryTerms.Should().Be(ifsAurenaPickupDeliveryTerms);
    }

    [Test]
    public void Execute_Should_Set_DeliveryTerms_And_PayTermId_From_ShipTo_FreightTerms_And_TermsCode_When_Order_Is_Ship()
    {
        var shipTo = Some.Customer()
            .WithTermsCode("TermsCode")
            .WithFreightTerms("FreightTerms")
            .Build();

        var parameter = new SubmitOrderParameter
        {
            CustomerOrder = Some.CustomerOrder()
                .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
                .Build()
        };

        this.WhenGetShipToIs(parameter.CustomerOrder, shipTo);

        var result = this.RunExecute(parameter);

        result.CustomerOrderRequest.DeliveryTerms.Should().Be(shipTo.FreightTerms);
        result.CustomerOrderRequest.PayTermId.Should().Be(shipTo.TermsCode);
    }

    [TestCase("")]
    [TestCase(null)]
    public void Execute_Should_Set_DeliveryTerms_To_IfsAurenaDefaultDeliveryTerms_When_Order_Is_Ship_And_ShipTo_TermsCode_Is_Null_Or_Empty(
        string termsCode
    )
    {
        var shipTo = Some.Customer().WithTermsCode(termsCode).Build();

        var ifsAurenaDefaultDeliveryTerms = "DefaultDeliveryTerms";

        var parameter = new SubmitOrderParameter
        {
            CustomerOrder = Some.CustomerOrder()
                .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
                .Build()
        };

        this.WhenGetShipToIs(parameter.CustomerOrder, shipTo);
        this.WhenIfsAurenaDefaultDeliveryTermsIs(ifsAurenaDefaultDeliveryTerms);

        var result = this.RunExecute(parameter);

        result.CustomerOrderRequest.DeliveryTerms.Should().Be(ifsAurenaDefaultDeliveryTerms);
    }

    protected override SubmitOrderParameter GetDefaultParameter()
    {
        return new SubmitOrderParameter
        {
            CustomerOrder = Some.CustomerOrder()
                .With(Some.Currency().WithCurrencyCode("USD"))
                .With(Some.ShipVia().WithErpShipCode("ErpShipCode"))
                .WithRequestedDeliveryDate(DateTimeOffset.Now)
                .WithDefaultWarehouse(Some.Warehouse().WithName("WarehouseName"))
                .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
                .WithCustomerPO("customer po")
        };
    }

    private void WhenGetBillToIs(CustomerOrder customerOrder, Customer customer)
    {
        this.customerHelper
            .Setup(o => o.GetBillTo(this.fakeUnitOfWork, customerOrder))
            .Returns(customer);
    }

    private void WhenGetShipToIs(CustomerOrder customerOrder, Customer customer)
    {
        this.customerHelper
            .Setup(o => o.GetShipTo(this.fakeUnitOfWork, customerOrder))
            .Returns(customer);
    }

    private void WhenIfsAurenaOrderCoordinatorIs(string ifsAurenaOrderCoordinator)
    {
        this.integrationConnectorSettings
            .Setup(o => o.IfsAurenaOrderCoordinator)
            .Returns(ifsAurenaOrderCoordinator);
    }

    private void WhenIfsAurenaOrderTypeIs(string ifsAurenaOrderType)
    {
        this.integrationConnectorSettings
            .Setup(o => o.IfsAurenaOrderType)
            .Returns(ifsAurenaOrderType);
    }

    private void WhenIfsAurenaPickupDeliveryTermsIs(string ifsAurenaPickupDeliveryTerms)
    {
        this.integrationConnectorSettings
            .Setup(o => o.IfsAurenaPickupDeliveryTerms)
            .Returns(ifsAurenaPickupDeliveryTerms);
    }

    private void WhenIfsAurenaDefaultDeliveryTermsIs(string ifsAurenaDefaultDeliveryTerms)
    {
        this.integrationConnectorSettings
            .Setup(o => o.IfsAurenaDefaultDeliveryTerms)
            .Returns(ifsAurenaDefaultDeliveryTerms);
    }
}
