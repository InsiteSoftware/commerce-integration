namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.AccountsReceivableSummary;

using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class GetAgingBucketsFromResponse
    : IPipe<AccountsReceivableSummaryParameter, AccountsReceivableSummaryResult>
{
    public int Order => 500;

    public AccountsReceivableSummaryResult Execute(
        IUnitOfWork unitOfWork,
        AccountsReceivableSummaryParameter parameter,
        AccountsReceivableSummaryResult result
    )
    {
        result.GetAgingBucketsResult = new GetAgingBucketsResult
        {
            AgingBuckets = new List<decimal> { 0, 0, 0, 0, 0 }
        };

        result.GetAgingBucketsResult.AgingBuckets[0] = this.GetAgingBucketDecimal(
            result.AccountsReceivableSummaryResponse.TradeBillingPeriodAmt
        );
        result.GetAgingBucketsResult.AgingBuckets[1] = this.GetAgingBucketDecimal(
            result.AccountsReceivableSummaryResponse.TradeAgePeriod1Amt
        );
        result.GetAgingBucketsResult.AgingBuckets[2] = this.GetAgingBucketDecimal(
            result.AccountsReceivableSummaryResponse.TradeAgePeriod2Amt
        );
        result.GetAgingBucketsResult.AgingBuckets[3] = this.GetAgingBucketDecimal(
            result.AccountsReceivableSummaryResponse.TradeAgePeriod3Amt
        );
        result.GetAgingBucketsResult.AgingBuckets[4] = this.GetAgingBucketDecimal(
            result.AccountsReceivableSummaryResponse.TradeAgePeriod4Amt
        );

        result.GetAgingBucketsResult.AgingBucketFuture = this.GetAgingBucketDecimal(
            result.AccountsReceivableSummaryResponse.TradeFutureAmt
        );

        return result;
    }

    private decimal GetAgingBucketDecimal(string agingBucketString)
    {
        decimal.TryParse(agingBucketString, out var agingBucketDecimal);

        return agingBucketDecimal;
    }
}
