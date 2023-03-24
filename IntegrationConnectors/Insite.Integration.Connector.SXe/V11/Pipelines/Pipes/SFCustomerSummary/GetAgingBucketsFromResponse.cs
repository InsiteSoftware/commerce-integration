namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFCustomerSummary;

using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;

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

        foreach (
            var customerSummary in result
                .SFCustomerSummaryResponse
                .Response
                .CustomerSummaryCollection
                .CustomerSummaries
        )
        {
            result.GetAgingBucketsResult.AgingBuckets[0] += customerSummary.Tradebillprdamt;
            result.GetAgingBucketsResult.AgingBuckets[1] += customerSummary.Tradeageprd1;
            result.GetAgingBucketsResult.AgingBuckets[2] += customerSummary.Tradeageprd2;
            result.GetAgingBucketsResult.AgingBuckets[3] += customerSummary.Tradeageprd3;
            result.GetAgingBucketsResult.AgingBuckets[4] += customerSummary.Tradeageprd4;

            result.GetAgingBucketsResult.AgingBucketFuture += customerSummary.Tradefutureamt;
        }

        return result;
    }
}
