namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitOrderLines;

using System;
using System.Linq;
using FluentAssertions;
using Insite.Common.Providers;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderLines;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CreateRequestsTests
    : BaseForPipeTests<SubmitOrderLinesParameter, SubmitOrderLinesResult>
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

        result.CustomerOrderLineRequests.Should().NotBeEmpty();
        result.CustomerOrderLineRequests.ForEach(
            o => o.OrderNo.Should().Be(parameter.ErpOrderNumber)
        );
        result.CustomerOrderLineRequests.ForEach(
            o => o.Identity1.Should().Be(parameter.ErpOrderNumber)
        );
    }

    [Test]
    public void Execute_Should_Add_BillTo_And_ShipTo_To_Requests()
    {
        var billTo = Some.Customer().WithErpNumber("ErpNumber").Build();
        var shipTo = Some.Customer().WithErpSequence("ErpSequence").Build();
        var parameter = this.GetDefaultParameter();

        this.WhenGetBillToIs(parameter.CustomerOrder, billTo);
        this.WhenGetShipToIs(parameter.CustomerOrder, shipTo);

        var result = this.RunExecute(parameter);

        result.CustomerOrderLineRequests.Should().NotBeEmpty();
        result.CustomerOrderLineRequests.ForEach(
            o => o.DeliverToCustomerNo.Should().Be(billTo.ErpNumber)
        );
        result.CustomerOrderLineRequests.ForEach(o => o.ShipAddrNo.Should().Be(shipTo.ErpSequence));
    }

    [Test]
    public void Execute_Should_Add_CustomerOrder_Properties_To_Requests()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.RunExecute(parameter);

        result.CustomerOrderLineRequests.Should().NotBeEmpty();
        result.CustomerOrderLineRequests.ForEach(
            o => o.Contract.Should().Be(parameter.CustomerOrder.DefaultWarehouse.Name)
        );
        result.CustomerOrderLineRequests.ForEach(
            o => o.PlannedDeliveryDate.Should().Be(parameter.CustomerOrder.RequestedDeliveryDate)
        );
        result.CustomerOrderLineRequests.ForEach(
            o => o.WantedDeliveryDate.Should().Be(parameter.CustomerOrder.RequestedDeliveryDate)
        );
        result.CustomerOrderLineRequests.ForEach(
            o => o.TargetDate.Should().Be(parameter.CustomerOrder.RequestedDeliveryDate)
        );
    }

    [Test]
    public void Execute_Should_Default_Date_Properties_To_Tomorrow()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder.RequestedDeliveryDate = null;

        var dateTimeNow = DateTimeOffset.Now;
        DateTimeProvider.Current = new MockDateTimeProvider(dateTimeNow.UtcDateTime);

        var result = this.RunExecute(parameter);

        result.CustomerOrderLineRequests.Should().NotBeEmpty();
        result.CustomerOrderLineRequests.ForEach(
            o => o.PlannedDeliveryDate.Should().Be(dateTimeNow.AddDays(1))
        );
        result.CustomerOrderLineRequests.ForEach(
            o => o.WantedDeliveryDate.Should().Be(dateTimeNow.AddDays(1))
        );
        result.CustomerOrderLineRequests.ForEach(
            o => o.TargetDate.Should().Be(dateTimeNow.AddDays(1))
        );
    }

    [Test]
    public void Execute_Should_Add_OrderLine_Properties_To_Requests()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.RunExecute(parameter);

        result.CustomerOrderLineRequests.Should().NotBeEmpty();
        result.CustomerOrderLineRequests
            .First()
            .CatalogNo.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().Product.ErpNumber);
        result.CustomerOrderLineRequests
            .First()
            .PartNo.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().Product.ProductCode);
        result.CustomerOrderLineRequests
            .First()
            .SalesUnitMeas.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().UnitOfMeasure);
        result.CustomerOrderLineRequests
            .First()
            .PriceUnitMeas.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().UnitOfMeasure);
        result.CustomerOrderLineRequests
            .First()
            .BuyQtyDue.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().QtyOrdered);
        result.CustomerOrderLineRequests
            .First()
            .DesiredQty.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().QtyOrdered);
        result.CustomerOrderLineRequests
            .First()
            .BaseSaleUnitPrice.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().UnitNetPrice);
        result.CustomerOrderLineRequests
            .First()
            .BaseUnitPriceInclTax.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().UnitNetPrice);
        result.CustomerOrderLineRequests
            .First()
            .SaleUnitPrice.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().UnitNetPrice);
        result.CustomerOrderLineRequests
            .First()
            .UnitPriceInclTax.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().UnitNetPrice);
        result.CustomerOrderLineRequests
            .First()
            .Cost.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().UnitCost);
        result.CustomerOrderLineRequests
            .First()
            .NoteText.Should()
            .Be(parameter.CustomerOrder.OrderLines.First().Notes);
    }

    [Test]
    public void Execute_Should_Add_Settings_To_Requests()
    {
        var ifsAurenaSupplyCode = "SupplyCode";
        var parameter = this.GetDefaultParameter();

        this.WhenIfsAurenaSupplyCodeIs(ifsAurenaSupplyCode);

        var result = this.RunExecute(parameter);

        result.CustomerOrderLineRequests.Should().NotBeEmpty();
        result.CustomerOrderLineRequests.ForEach(
            o => o.SupplyCode.Should().Be(ifsAurenaSupplyCode)
        );
    }

    protected override SubmitOrderLinesParameter GetDefaultParameter()
    {
        var customerOrder = Some.CustomerOrder()
            .WithDefaultWarehouse(Some.Warehouse().WithName("WarehouseName"))
            .WithRequestedDeliveryDate(DateTimeOffset.Now)
            .With(
                Some.OrderLine()
                    .WithNotes("notes")
                    .With(Some.Product().WithErpNumber("ABC123").WithProductCode("ProductCode"))
            )
            .With(Some.OrderLine())
            .Build();

        return new SubmitOrderLinesParameter
        {
            ErpOrderNumber = "ERP123",
            CustomerOrder = customerOrder
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

    private void WhenIfsAurenaSupplyCodeIs(string ifsAurenaSupplyCode)
    {
        this.integrationConnectorSettings
            .Setup(o => o.IfsAurenaSupplyCode)
            .Returns(ifsAurenaSupplyCode);
    }
}
