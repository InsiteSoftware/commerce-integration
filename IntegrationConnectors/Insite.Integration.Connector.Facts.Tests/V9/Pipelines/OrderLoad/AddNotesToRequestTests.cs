namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderLoad;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;
using NUnit.Framework;

[TestFixture]
public class AddNotesToRequestTests : BaseForPipeTests<OrderLoadParameter, OrderLoadResult>
{
    public override Type PipeType => typeof(AddNotesToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_700()
    {
        Assert.AreEqual(700, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_LineItemInfos_With_OrderLine_Notes_When_OrderLine_Notes_Are_Not_Empty()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.OrderLine().WithLine(1).WithNotes(string.Empty))
            .With(Some.OrderLine().WithLine(2).WithNotes("notes"))
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(1, result.OrderLoadRequest.Request.Orders.First().OrderDetail.Count);
        Assert.AreEqual(
            customerOrder.OrderLines.Last().Line,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().SequenceNumber
        );
        Assert.AreEqual(
            string.Empty,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ItemNumber
        );
        Assert.AreEqual(
            "&amp;N",
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().LineItemType
        );
        Assert.AreEqual(
            customerOrder.OrderLines.Last().Notes,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ItemDescription1
        );
        Assert.AreEqual(
            string.Empty,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ItemDescription2
        );
        Assert.AreEqual(
            false,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ShipInstructionType
        );
        Assert.AreEqual(
            0,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().OrderQty
        );
        Assert.AreEqual(
            string.Empty,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().UnitOfMeasure
        );
        Assert.AreEqual(
            string.Empty,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().WarehouseID
        );
        Assert.AreEqual(
            0,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ActualSellPrice
        );
    }

    [Test]
    public void Execute_Should_Populate_LineItemInfos_With_Order_Notes_When_Order_Notes_Are_Not_Empty()
    {
        var customerOrder = Some.CustomerOrder().WithNotes("Notes").Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(1, result.OrderLoadRequest.Request.Orders.First().OrderDetail.Count);
        Assert.AreEqual(
            1,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().SequenceNumber
        );
        Assert.AreEqual(
            string.Empty,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ItemNumber
        );
        Assert.AreEqual(
            "/N",
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().LineItemType
        );
        Assert.AreEqual(
            customerOrder.Notes,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ItemDescription1
        );
        Assert.AreEqual(
            string.Empty,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ItemDescription2
        );
        Assert.AreEqual(
            false,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ShipInstructionType
        );
        Assert.AreEqual(
            0,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().OrderQty
        );
        Assert.AreEqual(
            string.Empty,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().UnitOfMeasure
        );
        Assert.AreEqual(
            string.Empty,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().WarehouseID
        );
        Assert.AreEqual(
            0,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ActualSellPrice
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_LineItemInfos_With_Order_Notes_When_Order_Notes_Are_Empty()
    {
        var customerOrder = Some.CustomerOrder().Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(0, result.OrderLoadRequest.Request.Orders.First().OrderDetail.Count);
    }

    protected override OrderLoadResult GetDefaultResult()
    {
        return new OrderLoadResult
        {
            OrderLoadRequest = new OrderLoadRequest
            {
                Request = new Request
                {
                    Orders = new List<RequestOrder>
                    {
                        new RequestOrder { OrderHeader = new OrderHeader() }
                    }
                }
            }
        };
    }
}
