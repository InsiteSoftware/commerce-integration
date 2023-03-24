namespace Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;

using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;

public class CustomerSummaryParameter : PipeParameterBase
{
    public GetAgingBucketsParameter GetAgingBucketsParameter { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }
}
