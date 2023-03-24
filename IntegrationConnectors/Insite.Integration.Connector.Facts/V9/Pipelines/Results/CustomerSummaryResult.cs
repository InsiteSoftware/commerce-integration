namespace Insite.Integration.Connector.Facts.V9.Pipelines.Results;

using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.CustomerSummary;

public class CustomerSummaryResult : PipeResultBase
{
    public CustomerSummaryRequest CustomerSummaryRequest { get; set; }

    public CustomerSummaryResponse CustomerSummaryResponse { get; set; }

    public GetAgingBucketsResult GetAgingBucketsResult { get; set; }
}
