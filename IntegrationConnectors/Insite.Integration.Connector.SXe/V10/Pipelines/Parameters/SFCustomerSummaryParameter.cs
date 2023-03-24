namespace Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;

using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;

public class SFCustomerSummaryParameter : PipeParameterBase
{
    public GetAgingBucketsParameter GetAgingBucketsParameter { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }
}
