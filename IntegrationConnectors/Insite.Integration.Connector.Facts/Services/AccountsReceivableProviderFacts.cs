namespace Insite.Integration.Connector.Facts.Services;

using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

[DependencyName(nameof(IntegrationConnectorType.FACTS))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.FACTS)]
public sealed class AccountsReceivableProviderFacts : IAccountsReceivableProvider
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public AccountsReceivableProviderFacts(IPipeAssemblyFactory pipeAssemblyFactory)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public GetAgingBucketsResult GetAgingBuckets(GetAgingBucketsParameter getAgingBucketsParameter)
    {
        var customerSummaryParameter = new CustomerSummaryParameter
        {
            GetAgingBucketsParameter = getAgingBucketsParameter
        };
        var customerSummaryResult = this.pipeAssemblyFactory.ExecutePipeline(
            customerSummaryParameter,
            new CustomerSummaryResult()
        );

        if (customerSummaryResult.ResultCode != ResultCode.Success)
        {
            return new GetAgingBucketsResult
            {
                ResultCode = customerSummaryResult.ResultCode,
                SubCode = customerSummaryResult.SubCode,
                Messages = customerSummaryResult.Messages
            };
        }

        return customerSummaryResult.GetAgingBucketsResult;
    }
}
