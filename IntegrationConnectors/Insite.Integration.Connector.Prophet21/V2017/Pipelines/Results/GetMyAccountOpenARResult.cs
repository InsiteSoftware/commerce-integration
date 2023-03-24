namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetMyAccountOpenAR;

public class GetMyAccountOpenARResult : PipeResultBase
{
    public GetMyAccountOpenAR GetMyAccountOpenARRequest { get; set; }

    public GetMyAccountOpenAR GetMyAccountOpenARReply { get; set; }

    public GetAgingBucketsResult GetAgingBucketsResult { get; set; }
}
