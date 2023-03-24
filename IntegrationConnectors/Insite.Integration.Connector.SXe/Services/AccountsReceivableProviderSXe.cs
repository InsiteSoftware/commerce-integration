namespace Insite.Integration.Connector.SXe.Services;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Customer;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;

[DependencyName(nameof(IntegrationConnectorType.SXe))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.SXe)]
public sealed class AccountsReceivableProviderSXe : IAccountsReceivableProvider
{
    private readonly IDependencyLocator dependencyLocator;

    public AccountsReceivableProviderSXe(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public GetAgingBucketsResult GetAgingBuckets(GetAgingBucketsParameter getAgingBucketsParameter)
    {
        return this.dependencyLocator
            .GetInstance<IIntegrationConnectorServiceSXeFactory>()
            .GetIntegrationConnectorServiceSXe()
            .GetAgingBuckets(getAgingBucketsParameter);
    }
}
