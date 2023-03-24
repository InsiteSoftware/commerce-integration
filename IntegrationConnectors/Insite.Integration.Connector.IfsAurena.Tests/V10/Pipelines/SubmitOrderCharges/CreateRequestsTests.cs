namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitOrderCharges;

using System;
using System.Linq;
using FluentAssertions;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderCharges;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CreateRequestsTests
    : BaseForPipeTests<SubmitOrderChargesParameter, SubmitOrderChargesResult>
{
    private Mock<ICustomerHelper> customerHelper;

    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    public override Type PipeType => typeof(CreateRequests);

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
    public void Execute_Should_Add_ErpOrderNumber_To_Requests()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.RunExecute(parameter);

        result.CustomerOrderChargeRequests.Should().NotBeEmpty();
        result.CustomerOrderChargeRequests.ForEach(
            o => o.OrderNo.Should().Be(parameter.ErpOrderNumber)
        );
    }

    [Test]
    public void Execute_Should_Add_ShipTo_To_Requests()
    {
        var shipTo = Some.Customer().WithErpSequence("ErpSequence").Build();
        var parameter = this.GetDefaultParameter();

        this.WhenGetShipToIs(parameter.CustomerOrder, shipTo);

        var result = this.RunExecute(parameter);

        result.CustomerOrderChargeRequests.Should().NotBeEmpty();
        result.CustomerOrderChargeRequests.ForEach(
            o => o.DeliveryAddress.Should().Be(shipTo.ErpSequence)
        );
    }

    [Test]
    public void Execute_Should_Add_CustomerOrder_Properties_To_Requests()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.RunExecute(parameter);

        result.CustomerOrderChargeRequests.Should().NotBeEmpty();
        result.CustomerOrderChargeRequests.ForEach(
            o => o.Contract.Should().Be(parameter.CustomerOrder.DefaultWarehouse.Name)
        );
    }

    [Test]
    public void Execute_Should_Add_CustomerOrderPromotion_Properties_To_Requests()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.RunExecute(parameter);

        result.CustomerOrderChargeRequests.Should().NotBeEmpty();
        result.CustomerOrderChargeRequests
            .First()
            .ChargeAmount.Should()
            .Be(parameter.CustomerOrder.CustomerOrderPromotions.First().Amount);
        result.CustomerOrderChargeRequests
            .First()
            .ChargeAmountInclTax.Should()
            .Be(parameter.CustomerOrder.CustomerOrderPromotions.First().Amount);
        result.CustomerOrderChargeRequests
            .First()
            .BaseChargeAmount.Should()
            .Be(parameter.CustomerOrder.CustomerOrderPromotions.First().Amount);
        result.CustomerOrderChargeRequests
            .First()
            .BaseChargeAmtInclTax.Should()
            .Be(parameter.CustomerOrder.CustomerOrderPromotions.First().Amount);
    }

    [Test]
    public void Execute_Should_Add_Settings_To_Promotion_Requests()
    {
        var ifsAurenaPromotionChargeType = "PromotionChargeType";
        var ifsAurenaCompany = "01";
        var parameter = this.GetDefaultParameter();

        this.WhenIfsAurenaPromotionChargeTypeIs(ifsAurenaPromotionChargeType);
        this.WhenIfsAurenaCompanyIs(ifsAurenaCompany);

        var result = this.RunExecute(parameter);

        result.CustomerOrderChargeRequests.Should().NotBeEmpty();
        result.CustomerOrderChargeRequests[0].ChargeType.Should().Be(ifsAurenaPromotionChargeType);
        result.CustomerOrderChargeRequests[1].ChargeType.Should().Be(ifsAurenaPromotionChargeType);
        result.CustomerOrderChargeRequests.ForEach(o => o.Company.Should().Be(ifsAurenaCompany));
    }

    [Test]
    public void Execute_Should_Add_ShippingCharges_To_Request()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.RunExecute(parameter);

        result.CustomerOrderChargeRequests.Should().NotBeEmpty();
        result.CustomerOrderChargeRequests
            .Last()
            .ChargeAmount.Should()
            .Be(parameter.CustomerOrder.ShippingCharges);
        result.CustomerOrderChargeRequests
            .Last()
            .ChargeAmountInclTax.Should()
            .Be(parameter.CustomerOrder.ShippingCharges);
        result.CustomerOrderChargeRequests
            .Last()
            .BaseChargeAmount.Should()
            .Be(parameter.CustomerOrder.ShippingCharges);
        result.CustomerOrderChargeRequests
            .Last()
            .BaseChargeAmtInclTax.Should()
            .Be(parameter.CustomerOrder.ShippingCharges);
    }

    [Test]
    public void Execute_Should_Add_Settings_To_Freight_Requests()
    {
        var ifsAurenaFreightChargeType = "FreightChargeType";
        var ifsAurenaCompany = "01";
        var parameter = this.GetDefaultParameter();

        this.WhenIfsAurenaFreightChargeTypeIs(ifsAurenaFreightChargeType);
        this.WhenIfsAurenaCompanyIs(ifsAurenaCompany);

        var result = this.RunExecute(parameter);

        result.CustomerOrderChargeRequests.Should().NotBeEmpty();
        result.CustomerOrderChargeRequests
            .Last()
            .ChargeType.Should()
            .Be(ifsAurenaFreightChargeType);
        result.CustomerOrderChargeRequests.ForEach(o => o.Company.Should().Be(ifsAurenaCompany));
    }

    protected override SubmitOrderChargesParameter GetDefaultParameter()
    {
        var customerOrder = Some.CustomerOrder()
            .WithDefaultWarehouse(Some.Warehouse().WithName("WarehouseName"))
            .WithShippingCharges(69)
            .With(Some.CustomerOrderPromotion().WithAmount(12))
            .With(Some.CustomerOrderPromotion())
            .Build();

        return new SubmitOrderChargesParameter
        {
            ErpOrderNumber = "ERP123",
            CustomerOrder = customerOrder
        };
    }

    private void WhenGetShipToIs(CustomerOrder customerOrder, Customer customer)
    {
        this.customerHelper
            .Setup(o => o.GetShipTo(this.fakeUnitOfWork, customerOrder))
            .Returns(customer);
    }

    private void WhenIfsAurenaPromotionChargeTypeIs(string ifsAurenaPromotionChargeType)
    {
        this.integrationConnectorSettings
            .Setup(o => o.IfsAurenaPromotionChargeType)
            .Returns(ifsAurenaPromotionChargeType);
    }

    private void WhenIfsAurenaCompanyIs(string ifsAurenaCompany)
    {
        this.integrationConnectorSettings.Setup(o => o.IfsAurenaCompany).Returns(ifsAurenaCompany);
    }

    private void WhenIfsAurenaFreightChargeTypeIs(string ifsAurenaFreightChargeType)
    {
        this.integrationConnectorSettings
            .Setup(o => o.IfsAurenaFreightChargeType)
            .Returns(ifsAurenaFreightChargeType);
    }
}
