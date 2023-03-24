namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.AccountsReceivableSummary;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.AccountsReceivableSummary;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.AccountsReceivableSummary;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class GetAgingBucketsFromResponseTests
    : BaseForPipeTests<AccountsReceivableSummaryParameter, AccountsReceivableSummaryResult>
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
        var accountsReceivableSummaryResponse = new AccountsReceivableSummaryResponse
        {
            TradeBillingPeriodAmt = "2",
            TradeAgePeriod1Amt = "4",
            TradeAgePeriod2Amt = "5",
            TradeAgePeriod3Amt = "6",
            TradeAgePeriod4Amt = "7",
            TradeFutureAmt = "20"
        };

        var result = new AccountsReceivableSummaryResult
        {
            AccountsReceivableSummaryResponse = accountsReceivableSummaryResponse
        };

        result = this.RunExecute(result);

        Assert.AreEqual(2, result.GetAgingBucketsResult.AgingBuckets[0]);
        Assert.AreEqual(4, result.GetAgingBucketsResult.AgingBuckets[1]);
        Assert.AreEqual(5, result.GetAgingBucketsResult.AgingBuckets[2]);
        Assert.AreEqual(6, result.GetAgingBucketsResult.AgingBuckets[3]);
        Assert.AreEqual(7, result.GetAgingBucketsResult.AgingBuckets[4]);
        Assert.AreEqual(20, result.GetAgingBucketsResult.AgingBucketFuture);
    }

    [Test]
    public void Execute_Should_Default_Aging_Buckets_To_Zero_When_Aging_Bucket_Is_Not_Decimal()
    {
        var accountsReceivableSummaryResponse = new AccountsReceivableSummaryResponse
        {
            TradeBillingPeriodAmt = "2",
            TradeAgePeriod1Amt = "4",
            TradeAgePeriod2Amt = string.Empty,
            TradeAgePeriod3Amt = "6",
            TradeAgePeriod4Amt = "7",
            TradeFutureAmt = string.Empty
        };

        var result = new AccountsReceivableSummaryResult
        {
            AccountsReceivableSummaryResponse = accountsReceivableSummaryResponse
        };

        result = this.RunExecute(result);

        Assert.AreEqual(2, result.GetAgingBucketsResult.AgingBuckets[0]);
        Assert.AreEqual(4, result.GetAgingBucketsResult.AgingBuckets[1]);
        Assert.AreEqual(0, result.GetAgingBucketsResult.AgingBuckets[2]);
        Assert.AreEqual(6, result.GetAgingBucketsResult.AgingBuckets[3]);
        Assert.AreEqual(7, result.GetAgingBucketsResult.AgingBuckets[4]);
        Assert.AreEqual(0, result.GetAgingBucketsResult.AgingBucketFuture);
    }
}
