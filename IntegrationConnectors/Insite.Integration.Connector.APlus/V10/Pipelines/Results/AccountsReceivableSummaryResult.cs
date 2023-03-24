namespace Insite.Integration.Connector.APlus.V10.Pipelines.Results;

using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.AccountsReceivableSummary;

public class AccountsReceivableSummaryResult : PipeResultBase
{
    public AccountsReceivableSummaryRequest AccountsReceivableSummaryRequest { get; set; }

    public AccountsReceivableSummaryResponse AccountsReceivableSummaryResponse { get; set; }

    public GetAgingBucketsResult GetAgingBucketsResult { get; set; }
}
