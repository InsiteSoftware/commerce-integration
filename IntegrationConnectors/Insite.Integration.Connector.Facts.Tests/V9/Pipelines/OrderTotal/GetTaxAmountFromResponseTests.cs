namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderTotal;

using System;
using System.Collections.Generic;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;
using NUnit.Framework;

[TestFixture]
public class GetTaxAmountFromResponseTests : BaseForPipeTests<OrderTotalParameter, OrderTotalResult>
{
    public override Type PipeType => typeof(GetTaxAmountFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_700()
    {
        Assert.AreEqual(700, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_TaxAmount_And_Set_TaxCalculated()
    {
        var result = new OrderTotalResult
        {
            OrderTotalResponse = new OrderTotalResponse
            {
                Response = new Response
                {
                    Orders = new List<ResponseOrder>
                    {
                        new ResponseOrder { SalesTaxAmount = 12.20M }
                    }
                }
            }
        };

        result = this.RunExecute(result);

        Assert.AreEqual(12.20M, result.TaxAmount);
        Assert.IsTrue(result.TaxCalculated);
    }

    [Test]
    public void Execute_Should_Not_Get_TaxAmount_And_Not_Set_TaxCalculated_When_OrderTotalResponse_Is_Null()
    {
        var result = this.RunExecute();

        Assert.AreEqual(default(decimal), result.TaxAmount);
        Assert.IsFalse(result.TaxCalculated);
    }

    [Test]
    public void Execute_Should_Not_Get_TaxAmount_And_Not_Set_TaxCalculated_When_OrderTotalResponse_Response_Is_Null()
    {
        var result = new OrderTotalResult { OrderTotalResponse = new OrderTotalResponse() };

        result = this.RunExecute(result);

        Assert.AreEqual(default(decimal), result.TaxAmount);
        Assert.IsFalse(result.TaxCalculated);
    }

    [Test]
    public void Execute_Should_Not_Get_TaxAmount_And_Not_Set_TaxCalculated_When_Not_Any_OrderTotalResponse_Response_Orders()
    {
        var result = new OrderTotalResult
        {
            OrderTotalResponse = new OrderTotalResponse { Response = new Response() }
        };

        result = this.RunExecute(result);

        Assert.AreEqual(default(decimal), result.TaxAmount);
        Assert.IsFalse(result.TaxCalculated);
    }
}
