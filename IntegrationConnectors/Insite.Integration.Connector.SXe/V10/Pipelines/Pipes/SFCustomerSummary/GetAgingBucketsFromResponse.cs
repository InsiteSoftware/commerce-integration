namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFCustomerSummary;

using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;

public sealed class GetAgingBucketsFromResponse
    : IPipe<SFCustomerSummaryParameter, SFCustomerSummaryResult>
{
    public int Order => 400;

    public SFCustomerSummaryResult Execute(
        IUnitOfWork unitOfWork,
        SFCustomerSummaryParameter parameter,
        SFCustomerSummaryResult result
    )
    {
        result.GetAgingBucketsResult = new GetAgingBucketsResult
        {
            AgingBuckets = new List<decimal> { 0, 0, 0, 0, 0 }
        };

        foreach (var outsummary in result.SFCustomerSummaryResponse.Outsummary)
        {
            result.GetAgingBucketsResult.AgingBuckets[0] += outsummary.TradeBillingPeriodAmount;
            result.GetAgingBucketsResult.AgingBuckets[1] += outsummary.TradeAgePeriod1Amount;
            result.GetAgingBucketsResult.AgingBuckets[2] += outsummary.TradeAgePeriod2Amount;
            result.GetAgingBucketsResult.AgingBuckets[3] += outsummary.TradeAgePeriod3Amount;
            result.GetAgingBucketsResult.AgingBuckets[4] += outsummary.TradeAgePeriod4Amount;

            result.GetAgingBucketsResult.AgingBucketFuture += outsummary.TradeFutureAmount;
        }

        return result;
    }
}
