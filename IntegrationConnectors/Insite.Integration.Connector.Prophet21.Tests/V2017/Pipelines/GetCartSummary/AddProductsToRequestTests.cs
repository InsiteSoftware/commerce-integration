namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetCartSummary;

using System;
using System.Linq;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class AddProductsToRequestTests
    : BaseForPipeTests<GetCartSummaryParameter, GetCartSummaryResult>
{
    public override Type PipeType => typeof(AddProductsToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_GetCartSummaryRequest_Request_ListOfLineItems_With_CustomerOrder_OrderLines()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.OrderLine()
                    .With(Some.Product().WithErpNumber("1"))
                    .WithQtyOrdered(2M)
                    .WithUnitOfMeasure("EA")
                    .With(Some.Warehouse().WithName("MAIN"))
                    .WithNotes("Notes")
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderLines.First().Product.ErpNumber,
            result.GetCartSummaryRequest.Request.ListOfLineItems.First().ItemID
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().QtyOrdered.ToString(),
            result.GetCartSummaryRequest.Request.ListOfLineItems.First().OrderQuantity
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitOfMeasure,
            result.GetCartSummaryRequest.Request.ListOfLineItems.First().UnitName
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().Warehouse.Name,
            result.GetCartSummaryRequest.Request.ListOfLineItems.First().SourceLocation
        );
    }

    protected override GetCartSummaryResult GetDefaultResult()
    {
        return new GetCartSummaryResult
        {
            GetCartSummaryRequest = new GetCartSummary { Request = new Request() }
        };
    }
}
