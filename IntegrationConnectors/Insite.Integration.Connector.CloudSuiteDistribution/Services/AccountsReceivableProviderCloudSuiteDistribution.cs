namespace Insite.Integration.Connector.CloudSuiteDistribution.Services;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

[DependencyName(nameof(IntegrationConnectorType.CloudSuiteDistribution))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.CloudSuiteDistribution)]
public sealed class AccountsReceivableProviderCloudSuiteDistribution : IAccountsReceivableProvider
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    private readonly ICloudSuiteDistributionIntegrationConnectionProvider cloudSuiteDistributionIntegrationConnectionProvider;

    public AccountsReceivableProviderCloudSuiteDistribution(
        IPipeAssemblyFactory pipeAssemblyFactory,
        IDependencyLocator dependencyLocator
    )
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
        this.cloudSuiteDistributionIntegrationConnectionProvider =
            dependencyLocator.GetInstance<ICloudSuiteDistributionIntegrationConnectionProvider>();
    }

    public GetAgingBucketsResult GetAgingBuckets(GetAgingBucketsParameter getAgingBucketsParameter)
    {
        var integrationConnection =
            this.cloudSuiteDistributionIntegrationConnectionProvider.GetIntegrationConnection(null);

        var getAccountsReceivableParameter = new GetAccountsReceivableParameter
        {
            GetAgingBucketsParameter = getAgingBucketsParameter,
            IntegrationConnection = integrationConnection
        };

        var getAccountsReceivableResult = this.pipeAssemblyFactory.ExecutePipeline(
            getAccountsReceivableParameter,
            new GetAccountsReceivableResult()
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
