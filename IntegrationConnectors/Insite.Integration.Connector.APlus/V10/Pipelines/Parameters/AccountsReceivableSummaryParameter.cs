namespace Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;

using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;

public class AccountsReceivableSummaryParameter : PipeParameterBase
{
    public GetAgingBucketsParameter GetAgingBucketsParameter { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }
}
