namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.CustomerSummary;

using System;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.CustomerSummary;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.CustomerSummary;
using NUnit.Framework;

[TestFixture]
public class GetAgingBucketsFromResponseTests
    : BaseForPipeTests<CustomerSummaryParameter, CustomerSummaryResult>
{
    public override Type PipeType => typeof(GetAgingBucketsFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Aging_Buckets()
    {
        var result = new CustomerSummaryResult
        {
            CustomerSummaryResponse = new CustomerSummaryResponse
            {
                Response = new Response
                {
                    ARSummary = new ARSummary
                    {
                        TradeAmountDue = 180,
                        TradeAgePeriod1Amount = 30,
                        TradeAgePeriod2Amount = 40,
                        TradeAgePeriod3Amount = 50,
                        TradeAgePeriod4Amount = 60,
                        TradeFutureAmount = 100,
                    }
                }
            }
        };

        result = this.RunExecute(result);

        Assert.AreEqual(0, result.GetAgingBucketsResult.AgingBuckets[0]);
        Assert.AreEqual(30, result.GetAgingBucketsResult.AgingBuckets[1]);
        Assert.AreEqual(40, result.GetAgingBucketsResult.AgingBuckets[2]);
        Assert.AreEqual(50, result.GetAgingBucketsResult.AgingBuckets[3]);
        Assert.AreEqual(60, result.GetAgingBucketsResult.AgingBuckets[4]);
        Assert.AreEqual(100, result.GetAgingBucketsResult.AgingBucketFuture);
    }
}
