namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.CustomerSummary;

using System.Collections.Generic;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.CustomerSummary;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class GetAgingBucketsFromResponse
    : IPipe<CustomerSummaryParameter, CustomerSummaryResult>
{
    public int Order => 500;

    public CustomerSummaryResult Execute(
        IUnitOfWork unitOfWork,
        CustomerSummaryParameter parameter,
        CustomerSummaryResult result
    )
    {
        result.GetAgingBucketsResult = new GetAgingBucketsResult
        {
            AgingBuckets = new List<decimal> { 0, 0, 0, 0, 0 }
        };

        result.GetAgingBucketsResult.AgingBuckets[0] = GetCurrentAgingBucketAmount(
            result.CustomerSummaryResponse
        );
        result.GetAgingBucketsResult.AgingBuckets[1] = result
            .CustomerSummaryResponse
            .Response
            .ARSummary
            .TradeAgePeriod1Amount;
        result.GetAgingBucketsResult.AgingBuckets[2] = result
            .CustomerSummaryResponse
            .Response
            .ARSummary
            .TradeAgePeriod2Amount;
        result.GetAgingBucketsResult.AgingBuckets[3] = result
            .CustomerSummaryResponse
            .Response
            .ARSummary
            .TradeAgePeriod3Amount;
        result.GetAgingBucketsResult.AgingBuckets[4] = result
            .CustomerSummaryResponse
            .Response
            .ARSummary
            .TradeAgePeriod4Amount;
        result.GetAgingBucketsResult.AgingBucketFuture = result
            .CustomerSummaryResponse
            .Response
            .ARSummary
            .TradeFutureAmount;

        return result;
    }

    private static decimal GetCurrentAgingBucketAmount(
        CustomerSummaryResponse customerSummaryResponse
    )
    {
        return customerSummaryResponse.Response.ARSummary.TradeAmountDue
            - customerSummaryResponse.Response.ARSummary.TradeAgePeriod1Amount
            - customerSummaryResponse.Response.ARSummary.TradeAgePeriod2Amount
            - customerSummaryResponse.Response.ARSummary.TradeAgePeriod3Amount
            - customerSummaryResponse.Response.ARSummary.TradeAgePeriod4Amount;
    }
}
