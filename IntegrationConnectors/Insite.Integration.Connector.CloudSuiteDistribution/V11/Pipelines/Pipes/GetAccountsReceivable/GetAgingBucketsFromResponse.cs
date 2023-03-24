namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetAccountsReceivable;

using System.Collections.Generic;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

public sealed class GetAgingBucketsFromResponse
    : IPipe<GetAccountsReceivableParameter, GetAccountsReceivableResult>
{
    public int Order => 600;

    public GetAccountsReceivableResult Execute(
        IUnitOfWork unitOfWork,
        GetAccountsReceivableParameter parameter,
        GetAccountsReceivableResult result
    )
    {
        if (
            result.SfCustomerSummaryResponse?.Response?.CustomerSummaryCollection?.CustomerSummaries
            == null
        )
        {
            return result;
        }

        result.GetAgingBucketsResult = new GetAgingBucketsResult
        {
            AgingBuckets = new List<decimal> { 0, 0, 0, 0, 0 }
        };

        foreach (
            var customerSummary in result
                .SfCustomerSummaryResponse
                ?.Response
                ?.CustomerSummaryCollection
                ?.CustomerSummaries
        )
        {
            result.GetAgingBucketsResult.AgingBuckets[0] += customerSummary.TradeBillPrdAmt.Value;
            result.GetAgingBucketsResult.AgingBuckets[1] += customerSummary.TradeAgePrd1.Value;
            result.GetAgingBucketsResult.AgingBuckets[2] += customerSummary.TradeAgePrd2.Value;
            result.GetAgingBucketsResult.AgingBuckets[3] += customerSummary.TradeAgePrd3.Value;
            result.GetAgingBucketsResult.AgingBuckets[4] += customerSummary.TradeAgePrd4.Value;

            result.GetAgingBucketsResult.AgingBucketFuture += customerSummary.TradeFutureAmt.Value;
        }

        return result;
    }
}
