namespace Insite.WIS.CloudSuiteDistribution.Tests;

using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using Insite.Common.Helpers;
using Insite.WIS.Broker.Parameters.ApiRefresh;
using Insite.WIS.Broker.Results.ApiRefresh;
using Insite.WIS.Broker.WebIntegrationService;
using NUnit.Framework;

[TestFixture]
public class IntegrationProcessorCSDApiRefreshTests
{
    private TestableIntegrationProcessorCSDApiRefresh integrationProcessorCSDApiRefresh;

    [SetUp]
    public void SetUp()
    {
        this.integrationProcessorCSDApiRefresh = new TestableIntegrationProcessorCSDApiRefresh();
    }

    [Test]
    public void GetRequestComponents_Should_Set_ShouldRequestNextPage_To_False_If_Page_Is_Greater_Than_Zero()
    {
        var parameter = new GetRequestComponentsParameter { Page = 1 };

        var result = this.integrationProcessorCSDApiRefresh.CallGetRequestComponents(parameter);

        result.ShouldRequestNextPage.Should().BeFalse();
    }

    [Test]
    public async Task GetRequestComponents_Should_Populate_Request_ComponentsAsync()
    {
        var integrationConnection = new IntegrationConnection
        {
            Url = "http://webservice.com",
            DataSource = "http://tokenwebservice.com"
        };

        var jobDefinitionStep = new JobDefinitionStep
        {
            FromClause = "/webservicepath",
            IntegrationQuery = "{}"
        };

        var parameter = new GetRequestComponentsParameter
        {
            IntegrationConnection = integrationConnection,
            JobDefinitionStep = jobDefinitionStep
        };

        var accessToken = "AccessToken";

        this.WhenMemoryCacheContainsAccessToken(integrationConnection, accessToken);

        var result = this.integrationProcessorCSDApiRefresh.CallGetRequestComponents(parameter);

        result.BaseAddress.Should().Be(integrationConnection.Url);
        result.RequestUri.Should().Be(jobDefinitionStep.FromClause);
        result.Method.Should().Be(HttpMethod.Post);
        (await result.Content.ReadAsStringAsync()).Should().Be(jobDefinitionStep.IntegrationQuery);
        result.Headers["Authorization"].Should().Be($"Bearer {accessToken}");
        result.Headers["Accept"].Should().Be("application/json");
        result.ShouldRequestNextPage.Should().BeTrue();
    }

    private void WhenMemoryCacheContainsAccessToken(
        IntegrationConnection integrationConnection,
        string accessToken
    )
    {
#pragma warning disable CS0618 // Type or member is obsolete
        MemoryCache.Default.Add(
            $"{integrationConnection.DataSource}_AccessToken",
            EncryptionHelper.EncryptAes(accessToken),
            new CacheItemPolicy()
        );
#pragma warning restore
    }

    private class TestableIntegrationProcessorCSDApiRefresh : IntegrationProcessorCSDApiRefresh
    {
        public GetRequestComponentsResult CallGetRequestComponents(
            GetRequestComponentsParameter parameter
        )
        {
            return this.GetRequestComponents(parameter);
        }
    }
}
