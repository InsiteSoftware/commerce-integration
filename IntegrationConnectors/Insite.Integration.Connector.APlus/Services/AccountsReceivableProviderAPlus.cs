namespace Insite.Integration.Connector.APlus.Services;

using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[DependencyName(nameof(IntegrationConnectorType.APlus))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.APlus)]
public sealed class AccountsReceivableProviderAPlus : IAccountsReceivableProvider
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public AccountsReceivableProviderAPlus(IPipeAssemblyFactory pipeAssemblyFactory)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public GetAgingBucketsResult GetAgingBuckets(GetAgingBucketsParameter getAgingBucketsParameter)
    {
        var accountsReceivableSummaryParameter = new AccountsReceivableSummaryParameter
        {
            GetAgingBucketsParameter = getAgingBucketsParameter
        };

        var getAccountsReceivableResult = this.pipeAssemblyFactory.ExecutePipeline(
            accountsReceivableSummaryParameter,
            new AccountsReceivableSummaryResult()
        );

        if (getAccountsReceivableResult.ResultCode != ResultCode.Success)
        {
            return new GetAgingBucketsResult
            {
                ResultCode = getAccountsReceivableResult.ResultCode,
                SubCode = getAccountsReceivableResult.SubCode,
                Messages = getAccountsReceivableResult.Messages
            };
        }

        return getAccountsReceivableResult.GetAgingBucketsResult;
    }
}
