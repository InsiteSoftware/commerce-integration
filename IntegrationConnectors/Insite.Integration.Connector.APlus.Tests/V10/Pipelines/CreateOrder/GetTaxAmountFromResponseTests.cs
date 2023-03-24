namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.CreateOrder;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class GetTaxAmountFromResponseTests
    : BaseForPipeTests<CreateOrderParameter, CreateOrderResult>
{
    public override Type PipeType => typeof(GetTaxAmountFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_800()
    {
        Assert.AreEqual(800, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Bypass_Pipe_When_IsOrderSubmit_True()
    {
        var parameter = new CreateOrderParameter { IsOrderSubmit = true };

        var result = new CreateOrderResult
        {
            CreateOrderResponse = new CreateOrderResponse
            {
                OrderTotal = new ResponseOrderTotal { SalesTaxAmount = "1.23" }
            }
        };

        result = this.RunExecute(parameter);

        Assert.AreEqual(0, result.TaxAmount);
        Assert.AreEqual(false, result.TaxCalculated);
    }

    [TestCase("2.52", 2.52, true)]
    [TestCase("asdfas", 0, false)]
    [TestCase(null, 0, false)]
    public void Execute_Should_Get_Tax_Amount(
        string salesTax,
        decimal expectedTaxAmount,
        bool expectedTaxCalculated
    )
    {
        var result = new CreateOrderResult
        {
            CreateOrderResponse = new CreateOrderResponse
            {
                OrderTotal = new ResponseOrderTotal { SalesTaxAmount = salesTax }
            }
        };

        result = this.RunExecute(result);

        Assert.AreEqual(expectedTaxAmount, result.TaxAmount);
        Assert.AreEqual(expectedTaxCalculated, result.TaxCalculated);
    }
}
