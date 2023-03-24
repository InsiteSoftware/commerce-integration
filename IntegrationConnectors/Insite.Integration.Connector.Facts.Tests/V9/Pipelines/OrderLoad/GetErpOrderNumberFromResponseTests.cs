namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderLoad;

using System;
using System.Collections.Generic;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;
using NUnit.Framework;

[TestFixture]
public class GetErpOrderNumberFromResponseTests
    : BaseForPipeTests<OrderLoadParameter, OrderLoadResult>
{
    public override Type PipeType => typeof(GetErpOrderNumberFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_900()
    {
        Assert.AreEqual(900, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_ErpOrderNumber()
    {
        var result = new OrderLoadResult
        {
            OrderLoadResponse = new OrderLoadResponse
            {
                Response = new Response
                {
                    Orders = new List<ResponseOrder> { new ResponseOrder { OrderNumber = "123" } }
                }
            }
        };

        result = this.RunExecute(result);

        Assert.AreEqual("123", result.ErpOrderNumber);
    }
}
