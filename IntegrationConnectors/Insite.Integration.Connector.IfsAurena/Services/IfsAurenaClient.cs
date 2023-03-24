namespace Insite.Integration.Connector.IfsAurena.Services;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Insite.Common.HttpUtilities;
using Insite.Core.Services;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Interfaces;

internal sealed class IfsAurenaClient : IIfsAurenaClient
{
    private readonly IIfsAurenaIntegrationConnectionProvider ifsAurenaIntegrationConnectionProvider;

    private readonly IOAuthTokenProvider oAuthTokenProvider;

    private readonly HttpClientProvider httpClientProvider;

    private static readonly HttpClientHandler HttpClientHandler = new HttpClientHandler
    {
        UseCookies = false
    };

    public IfsAurenaClient(
        IIfsAurenaIntegrationConnectionProvider ifsAurenaIntegrationConnectionProvider,
        IOAuthTokenProvider oAuthTokenProvider,
        HttpClientProvider httpClientProvider
    )
    {
        this.ifsAurenaIntegrationConnectionProvider = ifsAurenaIntegrationConnectionProvider;
        this.oAuthTokenProvider = oAuthTokenProvider;
        this.httpClientProvider = httpClientProvider;
    }

    public (ResultCode resultCode, string response) GetInventoryStock(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        var requestUri =
            $"/main/ifsapplications/projection/v1/InventoryPartInStockHandling.svc/InventoryPartInStockSet?{request}";

        return this.DoRequest(HttpMethod.Get, integrationConnection, requestUri);
    }

    public (ResultCode resultCode, string response) GetPricing(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        var requestUri = "/main/ifsapplications/projection/v1/PriceQueryHandling.svc/PriceQuerySet";

        return this.DoRequest(HttpMethod.Post, integrationConnection, requestUri, request);
    }

    public (ResultCode resultCode, string response) CleanPriceQuery(
        IntegrationConnection integrationConnection
    )
    {
        var requestUri =
            "/main/ifsapplications/projection/v1/PriceQueryHandling.svc/CleanPriceQuery";

        return this.DoRequest(HttpMethod.Post, integrationConnection, requestUri, "{}");
    }

    public (ResultCode resultCode, string response) SubmitOrder(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        var requestUri =
            "/main/ifsapplications/projection/v1/CustomerOrderHandling.svc/CustomerOrderSet";

        return this.DoRequest(HttpMethod.Post, integrationConnection, requestUri, request);
    }

    public (ResultCode resultCode, string response) SubmitOrderLine(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request
    )
    {
        var requestUri =
            $"/main/ifsapplications/projection/v1/CustomerOrderHandling.svc/CustomerOrderSet(OrderNo='{erpOrderNumber}')/OrderLinesArray";

        return this.DoRequest(HttpMethod.Post, integrationConnection, requestUri, request);
    }

    public (ResultCode resultCode, string response) SubmitOrderCharge(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request
    )
    {
        var requestUri =
            $"/main/ifsapplications/projection/v1/CustomerOrderHandling.svc/CustomerOrderSet(OrderNo='{erpOrderNumber}')/CustomerOrderChargeArray";

        return this.DoRequest(HttpMethod.Post, integrationConnection, requestUri, request);
    }

    public (ResultCode resultCode, string response) SubmitCreditCardDetails(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request
    )
    {
        var requestUri =
            $"/main/ifsapplications/projection/v1/CCustomerOrderCrecarHandling.svc/CustomerOrderSet(OrderNo='{erpOrderNumber}')/OrderCreditCardArray";

        return this.DoRequest(HttpMethod.Post, integrationConnection, requestUri, request);
    }

    public (ResultCode resultCode, string response) SubmitAuthorizationCode(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        var requestUri =
            "/main/ifsapplications/projection/v1/CCustomerOrderCrecarHandling.svc/ManualAuthorize";

        return this.DoRequest(HttpMethod.Post, integrationConnection, requestUri, request);
    }

    private (ResultCode resultCode, string response) DoRequest(
        HttpMethod httpMethod,
        IntegrationConnection integrationConnection,
        string requestUri,
        string requestContent = null
    )
    {
        integrationConnection =
            this.ifsAurenaIntegrationConnectionProvider.GetIntegrationConnection(
                integrationConnection
            );

        var accessToken = this.oAuthTokenProvider.GetAccessToken(integrationConnection);
        var httpClient = this.httpClientProvider.GetHttpClient(
            new Uri(integrationConnection.Url),
            HttpClientHandler
        );

        using (var httpRequestMessage = new HttpRequestMessage(httpMethod, requestUri))
        {
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                accessToken
            );

            if (requestContent != null)
            {
                httpRequestMessage.Content = new StringContent(
                    requestContent,
                    Encoding.UTF8,
                    "application/json"
                );
            }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            using (var httpResponse = httpClient.SendAsync(httpRequestMessage).Result)
            {
                if (httpResponse == null)
                {
                    return (ResultCode.Error, "Error calling API. Response is null.");
                }

                var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorMessage =
                        $"Error calling API. Status Code: {httpResponse.StatusCode}. "
                        + $"Reason Phrase: {httpResponse.ReasonPhrase}."
                        + $"{responseContent}";

                    return (ResultCode.Error, errorMessage);
                }

                return (ResultCode.Success, responseContent);
            }
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }
    }
}
