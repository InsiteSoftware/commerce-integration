namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFCustomerSummary;

using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;

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

        foreach (var outsummary in result.SFCustomerSummaryResponse.arraySummary)
        {
            result.GetAgingBucketsResult.AgingBuckets[0] += outsummary.tradeBillingPeriodAmount;
            result.GetAgingBucketsResult.AgingBuckets[1] += outsummary.tradeAgePeriod1Amount;
            result.GetAgingBucketsResult.AgingBuckets[2] += outsummary.tradeAgePeriod2Amount;
            result.GetAgingBucketsResult.AgingBuckets[3] += outsummary.tradeAgePeriod3Amount;
            result.GetAgingBucketsResult.AgingBuckets[4] += outsummary.tradeAgePeriod4Amount;

            result.GetAgingBucketsResult.AgingBucketFuture += outsummary.tradeFutureAmount;
        }

        return result;
    }
}
