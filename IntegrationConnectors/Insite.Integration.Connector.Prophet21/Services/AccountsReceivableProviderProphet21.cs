namespace Insite.Integration.Connector.Prophet21.Services;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[DependencyName(nameof(IntegrationConnectorType.Prophet21))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.Prophet21)]
public sealed class AccountsReceivableProviderProphet21 : IAccountsReceivableProvider
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    private readonly IProphet21IntegrationConnectionProvider prophet21IntegrationConnectionProvider;

    public AccountsReceivableProviderProphet21(
        IPipeAssemblyFactory pipeAssemblyFactory,
        IDependencyLocator dependencyLocator
    )
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
        this.prophet21IntegrationConnectionProvider =
            dependencyLocator.GetInstance<IProphet21IntegrationConnectionProvider>();
    }

    public GetAgingBucketsResult GetAgingBuckets(GetAgingBucketsParameter getAgingBucketsParameter)
    {
        var integrationConnection =
            this.prophet21IntegrationConnectionProvider.GetIntegrationConnection(null);

        var getMyAccountOpenARParameter = new GetMyAccountOpenARParameter
        {
            GetAgingBucketsParameter = getAgingBucketsParameter,
            IntegrationConnection = integrationConnection
        };

        var getMyAccountOpenARResult = this.pipeAssemblyFactory.ExecutePipeline(
            getMyAccountOpenARParameter,
            new GetMyAccountOpenARResult()
        );

        if (getMyAccountOpenARResult.ResultCode != ResultCode.Success)
        {
            return new GetAgingBucketsResult
            {
                ResultCode = getMyAccountOpenARResult.ResultCode,
                SubCode = getMyAccountOpenARResult.SubCode,
                Messages = getMyAccountOpenARResult.Messages
            };
        }

        return getMyAccountOpenARResult.GetAgingBucketsResult;
    }
}
