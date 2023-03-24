namespace Insite.Integration.Connector.CloudSuiteDistribution.Services;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Insite.Common.HttpUtilities;
using Insite.Core.Services;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Interfaces;

internal sealed class CloudSuiteDistributionClient : ICloudSuiteDistributionClient
{
    private readonly IOAuthTokenProvider oAuthTokenProvider;

    private readonly HttpClientProvider httpClientProvider;

    public CloudSuiteDistributionClient(
        IOAuthTokenProvider oAuthTokenProvider,
        HttpClientProvider httpClientProvider
    )
    {
        this.oAuthTokenProvider = oAuthTokenProvider;
        this.httpClientProvider = httpClientProvider;
    }

    public (ResultCode resultCode, string response) GetAccountsReceivable(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        var requestUri = "sxapisfcustomersummary";

        return this.DoRequest(integrationConnection, requestUri, request);
    }

    public (ResultCode resultCode, string response) GetPricingAndInventoryStock(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        var requestUri = "sxapioepricingmultiplev4";

        return this.DoRequest(integrationConnection, requestUri, request);
    }

    private (ResultCode resultCode, string response) DoRequest(
        IntegrationConnection integrationConnection,
        string requestUri,
        string requestContent
    )
    {
        var accessToken = this.oAuthTokenProvider.GetAccessToken(integrationConnection);
        var httpClient = this.httpClientProvider.GetHttpClient(new Uri(integrationConnection.Url));

        using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
        {
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                accessToken
            );
            httpRequestMessage.Content = new StringContent(
                requestContent,
                Encoding.UTF8,
                "application/json"
            );

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            using (var httpResponse = httpClient.SendAsync(httpRequestMessage).Result)
            {
                var responseContent = httpResponse?.Content?.ReadAsStringAsync().Result;
                if (httpResponse == null || !httpResponse.IsSuccessStatusCode)
                {
                    var errorMessage =
                        $"Error calling API. Status Code: {httpResponse?.StatusCode}. "
                        + $"Reason Phrase: {httpResponse?.ReasonPhrase}. "
                        + $"{responseContent}";

                    return (ResultCode.Error, errorMessage);
                }

                return (ResultCode.Success, responseContent);
            }
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }
    }
}
