namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.OrderImport;

using System;
using System.Linq;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class AddProductsToRequestTests : BaseForPipeTests<OrderImportParameter, OrderImportResult>
{
    public override Type PipeType => typeof(AddProductsToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_800()
    {
        Assert.AreEqual(800, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_OrderImportRequest_Request_ListOfLineItems_With_CustomerOrder_OrderLines()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.OrderLine()
                    .With(Some.Product().WithErpNumber("1"))
                    .WithQtyOrdered(2M)
                    .WithUnitOfMeasure("EA")
                    .WithUnitNetPrice(12)
                    .With(Some.Warehouse().WithName("MAIN"))
                    .WithNotes("Notes")
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderLines.First().Product.ErpNumber,
            result.OrderImportRequest.Request.ListOfLineItems.First().ItemID
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().QtyOrdered.ToString(),
            result.OrderImportRequest.Request.ListOfLineItems.First().OrderQuantity
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitOfMeasure,
            result.OrderImportRequest.Request.ListOfLineItems.First().UnitName
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitNetPrice.ToString(),
            result.OrderImportRequest.Request.ListOfLineItems.First().UnitPrice
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().Warehouse.Name,
            result.OrderImportRequest.Request.ListOfLineItems.First().SourceLocation
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().Notes,
            result.OrderImportRequest.Request.ListOfLineItems.First().NotepadText
        );
    }

    protected override OrderImportResult GetDefaultResult()
    {
        return new OrderImportResult
        {
            OrderImportRequest = new OrderImport { Request = new Request() }
        };
    }
}
