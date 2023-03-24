namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.SalesOrder;

using System;
using System.Linq;
using FluentAssertions;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;
using NUnit.Framework;

[TestFixture]
public class AddProductsToRequestTests : BaseForPipeTests<SalesOrderParameter, SalesOrderResult>
{
    public override Type PipeType => typeof(AddProductsToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_700()
    {
        this.pipe.Order.Should().Be(700);
    }

    [Test]
    public void Execute_Should_Populate_CustomerOrderLines_With_OrderLines()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.OrderLine()
                    .WithLine(1)
                    .With(Some.Product().WithErpNumber("ABC123"))
                    .WithQtyOrdered(2)
                    .WithUnitOfMeasure("EA")
                    .WithTaxCode1("TAXABLE")
                    .WithUnitNetPrice(12)
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        result.SalesOrderRequest.Details
            .First()
            .rowNumber.Should()
            .Be(customerOrder.OrderLines.First().Line);
        result.SalesOrderRequest.Details
            .First()
            .InventoryID.Should()
            .Be(customerOrder.OrderLines.First().Product.ErpNumber);
        result.SalesOrderRequest.Details
            .First()
            .OrderQty.Should()
            .Be(customerOrder.OrderLines.First().QtyOrdered);
        result.SalesOrderRequest.Details
            .First()
            .UOM.Should()
            .Be(customerOrder.OrderLines.First().UnitOfMeasure);
        result.SalesOrderRequest.Details
            .First()
            .TaxCategory.Should()
            .Be(customerOrder.OrderLines.First().TaxCode1);
        result.SalesOrderRequest.Details
            .First()
            .UnitPrice.Should()
            .Be(customerOrder.OrderLines.First().UnitNetPrice);
        result.SalesOrderRequest.Details.First().ExtendedPrice.Should().Be(24);
        result.SalesOrderRequest.Details.First().OvershipThreshold.Should().Be(100);
        result.SalesOrderRequest.Details.First().UndershipThreshold.Should().Be(100);
    }

    protected override SalesOrderResult GetDefaultResult()
    {
        return new SalesOrderResult { SalesOrderRequest = new SalesOrder() };
    }
}
