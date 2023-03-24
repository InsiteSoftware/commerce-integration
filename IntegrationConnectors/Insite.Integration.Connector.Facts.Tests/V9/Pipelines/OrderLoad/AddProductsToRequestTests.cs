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
public class AddProductsToRequestTests : BaseForPipeTests<OrderLoadParameter, OrderLoadResult>
{
    public override Type PipeType => typeof(AddProductsToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_LineItemInfos_With_CustomerOrder_OrderLines()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.OrderLine()
                    .WithLine(1)
                    .With(Some.Product().WithErpNumber("1"))
                    .WithQtyOrdered(2M)
                    .WithUnitOfMeasure("EA")
                    .With(Some.Warehouse().WithName("MAIN"))
                    .WithUnitNetPrice(12.20M)
            )
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderLines.First().Line,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().SequenceNumber
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().Product.ErpNumber,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ItemNumber
        );
        Assert.AreEqual(
            string.Empty,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().LineItemType
        );
        Assert.AreEqual(
            false,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ShipInstructionType
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().QtyOrdered,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().OrderQty
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitOfMeasure,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().UnitOfMeasure
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().Warehouse.Name,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().WarehouseID
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitNetPrice,
            result.OrderLoadRequest.Request.Orders.First().OrderDetail.First().ActualSellPrice
        );
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
