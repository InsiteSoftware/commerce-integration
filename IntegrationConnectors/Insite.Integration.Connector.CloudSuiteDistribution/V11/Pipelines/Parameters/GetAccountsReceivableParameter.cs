namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;

using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;

public class GetAccountsReceivableParameter : PipeParameterBase
{
    public GetAgingBucketsParameter GetAgingBucketsParameter { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }
}
