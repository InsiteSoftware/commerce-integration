namespace Insite.Integration.Connector.SXe.Tests.V61.Pipelines.Pipes.SFCustomerSummary;

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFCustomerSummary;

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
        var arraySummary = new List<SFCustomerSummaryoutputSummary>
        {
            new SFCustomerSummaryoutputSummary
            {
                tradeBillingPeriodAmount = 0,
                tradeAgePeriod1Amount = 22,
                tradeAgePeriod2Amount = 5,
                tradeAgePeriod3Amount = 0,
                tradeAgePeriod4Amount = 1,
                tradeFutureAmount = 25
            }
        };
        var sfCustomerSummaryResponse = new SFCustomerSummaryResponse()
        {
            arraySummary = arraySummary
        };

        var result = new SFCustomerSummaryResult
        {
            SFCustomerSummaryResponse = sfCustomerSummaryResponse
        };
        result = this.RunExecute(result);

        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[0],
            arraySummary[0].tradeBillingPeriodAmount
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[1],
            arraySummary[0].tradeAgePeriod1Amount
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[2],
            arraySummary[0].tradeAgePeriod2Amount
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[3],
            arraySummary[0].tradeAgePeriod3Amount
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[4],
            arraySummary[0].tradeAgePeriod4Amount
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBucketFuture,
            arraySummary[0].tradeFutureAmount
        );
    }
}
