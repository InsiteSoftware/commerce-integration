namespace Insite.Integration.Connector.SXe.V61.Pipelines.Results;

using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFCustomerSummary;

public class SFCustomerSummaryResult : PipeResultBase
{
    public SFCustomerSummaryRequest SFCustomerSummaryRequest { get; set; }

    public SFCustomerSummaryResponse SFCustomerSummaryResponse { get; set; }

    public GetAgingBucketsResult GetAgingBucketsResult { get; set; }
}
