namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetCartSummary;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class GetTaxAmountFromResponseTests
    : BaseForPipeTests<GetCartSummaryParameter, GetCartSummaryResult>
{
    public override Type PipeType => typeof(GetTaxAmountFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_800()
    {
        Assert.AreEqual(800, this.pipe.Order);
    }

    [TestCase("12.01", 12.01, true)]
    [TestCase("asdfas", 0, false)]
    [TestCase(null, 0, false)]
    public void Execute_Should_Get_Tax_Amount(
        string salesTax,
        decimal expectedTaxAmount,
        bool expectedTaxCalculated
    )
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = Some.CustomerOrder();

        var result = this.GetDefaultResult();
        result.GetCartSummaryReply = new GetCartSummary
        {
            Reply = new Reply { CartSummary = new ReplyCartSummary { SalesTax = salesTax } }
        };

        result = this.RunExecute(parameter, result);

        Assert.AreEqual(expectedTaxAmount, result.TaxAmount);
        Assert.AreEqual(expectedTaxCalculated, result.TaxCalculated);
    }
}
