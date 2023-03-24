namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetMyAccountOpenAR;

using System.Collections.Generic;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class GetAgingBucketsFromResponse
    : IPipe<GetMyAccountOpenARParameter, GetMyAccountOpenARResult>
{
    public int Order => 500;

    public GetMyAccountOpenARResult Execute(
        IUnitOfWork unitOfWork,
        GetMyAccountOpenARParameter parameter,
        GetMyAccountOpenARResult result
    )
    {
        result.GetAgingBucketsResult = new GetAgingBucketsResult
        {
            AgingBuckets = new List<decimal> { 0, 0, 0, 0, 0 }
        };

        result.GetAgingBucketsResult.AgingBuckets[0] = result
            .GetMyAccountOpenARReply
            .Reply
            .AgedAR
            .CurrentAmount;
        result.GetAgingBucketsResult.AgingBuckets[1] = result
            .GetMyAccountOpenARReply
            .Reply
            .AgedAR
            .Age1Amount;
        result.GetAgingBucketsResult.AgingBuckets[2] = result
            .GetMyAccountOpenARReply
            .Reply
            .AgedAR
            .Age1ToAge2Amount;
        result.GetAgingBucketsResult.AgingBuckets[3] = result
            .GetMyAccountOpenARReply
            .Reply
            .AgedAR
            .Age2ToAge3Amount;
        result.GetAgingBucketsResult.AgingBuckets[4] = result
            .GetMyAccountOpenARReply
            .Reply
            .AgedAR
            .OverAge3Amount;

        return result;
    }
}
