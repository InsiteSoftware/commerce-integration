namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderTotal;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;
using NUnit.Framework;

[TestFixture]
public class AddProductsToRequestTests : BaseForPipeTests<OrderTotalParameter, OrderTotalResult>
{
    public override Type PipeType => typeof(AddProductsToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
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

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderLines.First().Line,
            result.OrderTotalRequest.Request.Orders.First().OrderDetail.First().SequenceNumber
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().Product.ErpNumber,
            result.OrderTotalRequest.Request.Orders.First().OrderDetail.First().ItemNumber
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().QtyOrdered,
            result.OrderTotalRequest.Request.Orders.First().OrderDetail.First().OrderQty
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitOfMeasure,
            result.OrderTotalRequest.Request.Orders.First().OrderDetail.First().UnitOfMeasure
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().Warehouse.Name,
            result.OrderTotalRequest.Request.Orders.First().OrderDetail.First().WarehouseID
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitNetPrice,
            result.OrderTotalRequest.Request.Orders.First().OrderDetail.First().ActualSellPrice
        );
    }

    protected override OrderTotalResult GetDefaultResult()
    {
        return new OrderTotalResult
        {
            OrderTotalRequest = new OrderTotalRequest
            {
                Request = new Request { Orders = new List<RequestOrder> { new RequestOrder() } }
            }
        };
    }
}
