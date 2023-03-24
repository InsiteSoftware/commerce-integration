namespace Insite.Integration.Connector.Prophet21.Services;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Insite.Common.Dependencies;
using Insite.Common.Helpers;
using Insite.Common.HttpUtilities;
using Insite.Common.Logging;
using Insite.Core.Interfaces.Plugins.Caching;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Enums;

internal sealed class Prophet21ApiService : IProphet21ApiService
{
    private readonly HttpClientProvider httpClientProvider;

    private readonly ICacheManager cacheManager;

    private readonly IProphet21IntegrationConnectionProvider prophet21IntegrationConnectionProvider;

    private const string ECommerceApiPath = "/api/ecommerce/";

    private const string TokenApiPath = "/api/security/token/";

    private const string ContactsApiPath = "/api/entity/contacts/";

    public Prophet21ApiService()
        : this(
            DependencyLocator.Current.GetInstance<HttpClientProvider>(),
            DependencyLocator.Current.GetInstance<ICacheManager>()
        ) { }

    public Prophet21ApiService(HttpClientProvider httpClientProvider, ICacheManager cacheManager)
    {
        this.httpClientProvider = httpClientProvider;
        this.cacheManager = cacheManager;
        this.prophet21IntegrationConnectionProvider =
            DependencyLocator.Current.GetInstance<IProphet21IntegrationConnectionProvider>();
    }

    public GetItemPrice GetItemPrice(
        IntegrationConnection integrationConnection,
        GetItemPrice getItemPrice
    )
    {
        var xmlRequest = Prophet21SerializationService.Serialize(getItemPrice);
        LogHelper.For(this).Debug($"GetItemPrice Request: {xmlRequest}");

        var requestContent = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");
        var xmlResponse = this.CallMiddlewareApi(
            integrationConnection,
            HttpMethod.Post,
            ECommerceApiPath,
            requestContent
        );
        LogHelper.For(this).Debug($"GetItemPrice Response: {xmlResponse}");

        return Prophet21SerializationService.Deserialize<GetItemPrice>(xmlResponse);
    }

    public GetMyAccountOpenAR GetMyAccountOpenAR(
        IntegrationConnection integrationConnection,
        GetMyAccountOpenAR getMyAccountOpenAR
    )
    {
        var xmlRequest = Prophet21SerializationService.Serialize(getMyAccountOpenAR);
        LogHelper.For(this).Debug($"GetMyAccountOpenAR Request: {xmlRequest}");

        var requestContent = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");
        var xmlResponse = this.CallMiddlewareApi(
            integrationConnection,
            HttpMethod.Post,
            ECommerceApiPath,
            requestContent
        );
        LogHelper.For(this).Debug($"GetMyAccountOpenAR Response: {xmlResponse}");

        return Prophet21SerializationService.Deserialize<GetMyAccountOpenAR>(xmlResponse);
    }

    public OrderImport OrderImport(
        IntegrationConnection integrationConnection,
        OrderImport orderImport
    )
    {
        var xmlRequest = Prophet21SerializationService.Serialize(orderImport);
        LogHelper.For(this).Debug($"OrderImport Request: {xmlRequest}");

        var requestContent = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");
        var xmlResponse = this.CallMiddlewareApi(
            integrationConnection,
            HttpMethod.Post,
            ECommerceApiPath,
            requestContent
        );
        LogHelper.For(this).Debug($"OrderImport Response: {xmlResponse}");

        return Prophet21SerializationService.Deserialize<OrderImport>(xmlResponse);
    }

    public GetCartSummary GetCartSummary(
        IntegrationConnection integrationConnection,
        GetCartSummary getCartSummary
    )
    {
        var xmlRequest = Prophet21SerializationService.Serialize(getCartSummary);
        LogHelper.For(this).Debug($"GetCartSummary Request: {xmlRequest}");

        var requestContent = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");
        var xmlResponse = this.CallMiddlewareApi(
            integrationConnection,
            HttpMethod.Post,
            ECommerceApiPath,
            requestContent
        );
        LogHelper.For(this).Debug($"GetCartSummary Response: {xmlResponse}");

        return Prophet21SerializationService.Deserialize<GetCartSummary>(xmlResponse);
    }

    public ArrayOfContact GetContacts(
        IntegrationConnection integrationConnection,
        string emailAddress
    )
    {
        LogHelper.For(this).Debug($"Get Contacts Request: {emailAddress}");

        var parameters = new Dictionary<string, string>
        {
            { "$query", $"email_address eq '{emailAddress}'" },
            { "extendedProperties", "ContactLinks" }
        };

        var xmlResponse = this.CallMiddlewareApi(
            integrationConnection,
            HttpMethod.Get,
            ContactsApiPath,
            null,
            parameters
        );
        LogHelper.For(this).Debug($"GetContacts Response: {xmlResponse}");

        return Prophet21SerializationService.Deserialize<ArrayOfContact>(xmlResponse);
    }

    public Contact CreateContact(IntegrationConnection integrationConnection, Contact contact)
    {
        var xmlRequest = Prophet21SerializationService.Serialize(contact);
        LogHelper.For(this).Debug($"Create Contact Request: {xmlRequest}");

        var requestContent = new StringContent(xmlRequest, Encoding.UTF8, "application/xml");
        var xmlResponse = this.CallMiddlewareApi(
            integrationConnection,
            HttpMethod.Post,
            ContactsApiPath,
            requestContent
        );
        LogHelper.For(this).Debug($"Create Contact Response: {xmlResponse}");

        return Prophet21SerializationService.Deserialize<Contact>(xmlResponse);
    }

    internal string GetToken(IntegrationConnection integrationConnection)
    {
        if (
            integrationConnection.TypeName
                .Trim()
                .EqualsIgnoreCase(nameof(IntegrationConnectionType.ApiClientCredentialsEndpoint))
        )
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var requestContent = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "username", integrationConnection.LogOn },
                    {
                        "client_secret",
                        EncryptionHelper.DecryptAes(integrationConnection.Password)
                    },
                    { "client-type", "application/xml" }
                }
            );
#pragma warning restore

            var xmlResponse = this.CallMiddlewareApi(
                integrationConnection,
                HttpMethod.Post,
                TokenApiPath,
                requestContent,
                null,
                false
            );

            return Prophet21SerializationService.Deserialize<Token>(xmlResponse).AccessToken;
        }
        else
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var requestContent = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "username", integrationConnection.LogOn },
                    { "password", EncryptionHelper.DecryptAes(integrationConnection.Password) }
                }
            );
#pragma warning restore

            var xmlResponse = this.CallMiddlewareApi(
                integrationConnection,
                HttpMethod.Post,
                TokenApiPath,
                requestContent,
                null,
                false
            );

            return Prophet21SerializationService.Deserialize<Token>(xmlResponse).AccessToken;
        }
    }

    private string CallMiddlewareApi(
        IntegrationConnection integrationConnection,
        HttpMethod httpMethod,
        string path,
        HttpContent content,
        IDictionary<string, string> parameters = null,
        bool shouldSetAuthorizationHeader = true
    )
    {
        integrationConnection =
            this.prophet21IntegrationConnectionProvider.GetIntegrationConnection(
                integrationConnection
            );
        var httpClient = this.httpClientProvider.GetHttpClient(
            new Uri(integrationConnection.Url + path)
        );
        var httpRequest = new HttpRequestMessage(httpMethod, GetRequestUri(parameters))
        {
            Content = content,
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

        if (shouldSetAuthorizationHeader)
        {
            var token = this.cacheManager.GetOrAdd(
                integrationConnection.Name,
                () => this.GetToken(integrationConnection),
                TimeSpan.FromHours(12)
            );
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        var httpResponse = httpClient.SendAsync(httpRequest).Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

        if (httpResponse == null)
        {
            throw new DataException($"Error calling Prophet 21 API. Response is null.");
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            if (httpResponse.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                this.cacheManager.Remove(integrationConnection.Name);
            }

            throw new DataException(
                $"Error calling Prophet 21 API. Status Code: {httpResponse.StatusCode}. Reason Phrase: {httpResponse.ReasonPhrase}."
            );
        }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        return httpResponse.Content.ReadAsStringAsync().Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }

    private static string GetRequestUri(IDictionary<string, string> parameters)
    {
        return parameters != null && parameters.Any()
            ? $"?{string.Join("&", parameters.Select(o => $"{o.Key}={o.Value}"))}"
            : string.Empty;
    }
}
