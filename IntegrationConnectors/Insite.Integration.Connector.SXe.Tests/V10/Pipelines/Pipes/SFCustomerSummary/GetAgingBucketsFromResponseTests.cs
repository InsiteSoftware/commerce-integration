namespace Insite.Integration.Connector.SXe.Tests.V10.Pipelines.Pipes.SFCustomerSummary;

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFCustomerSummary;

[TestFixture]
public class GetAgingBucketsFromResponseTests
    : BaseForPipeTests<SFCustomerSummaryParameter, SFCustomerSummaryResult>
{
    public override Type PipeType => typeof(GetAgingBucketsFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Aging_Buckets()
    {
        var outputSummary = new List<Outsummary>
        {
            new Outsummary
            {
                TradeBillingPeriodAmount = 0,
                TradeAgePeriod1Amount = 22,
                TradeAgePeriod2Amount = 5,
                TradeAgePeriod3Amount = 0,
                TradeAgePeriod4Amount = 1,
                TradeFutureAmount = 25
            }
        };
        var sfCustomerSummaryResponse = new SFCustomerSummaryResponse()
        {
            Outsummary = outputSummary
        };

        var result = new SFCustomerSummaryResult
        {
            SFCustomerSummaryResponse = sfCustomerSummaryResponse
        };
        result = this.RunExecute(result);

        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[0],
            outputSummary[0].TradeBillingPeriodAmount
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[1],
            outputSummary[0].TradeAgePeriod1Amount
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[2],
            outputSummary[0].TradeAgePeriod2Amount
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[3],
            outputSummary[0].TradeAgePeriod3Amount
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[4],
            outputSummary[0].TradeAgePeriod4Amount
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBucketFuture,
            outputSummary[0].TradeFutureAmount
        );
    }
}
