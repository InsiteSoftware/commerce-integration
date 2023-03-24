namespace Insite.WIS.CloudSuiteDistribution;

using System.Net.Http;
using System.Text;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Parameters.ApiRefresh;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.Providers;
using Insite.WIS.Broker.Results.ApiRefresh;

[IntegrationConnector(IntegrationConnectorType.CloudSuiteDistribution)]
public class IntegrationProcessorCSDApiRefresh
    : IntegrationProcessorApiRefreshBase,
        IIntegrationProcessor
{
    private readonly OAuthTokenProvider oAuthTokenProvider;

    public IntegrationProcessorCSDApiRefresh()
    {
        this.oAuthTokenProvider = new OAuthTokenProvider();
    }

    protected override GetRequestComponentsResult GetRequestComponents(
        GetRequestComponentsParameter parameter
    )
    {
        if (parameter.Page > 0)
        {
            return new GetRequestComponentsResult { ShouldRequestNextPage = false };
        }

        return new GetRequestComponentsResult
        {
            BaseAddress = parameter.IntegrationConnection.Url,
            RequestUri = parameter.JobDefinitionStep.FromClause,
            Method = HttpMethod.Post,
            Content = new StringContent(
                parameter.JobDefinitionStep.IntegrationQuery,
                Encoding.UTF8,
                "application/json"
            ),
            Headers = GetAuthorizationAndAcceptHeaders(
                this.oAuthTokenProvider,
                parameter.IntegrationConnection,
                "application/json"
            ),
            ShouldRequestNextPage = true
        };
    }
}
