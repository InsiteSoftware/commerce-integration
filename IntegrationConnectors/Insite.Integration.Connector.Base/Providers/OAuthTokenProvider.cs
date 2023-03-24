namespace Insite.Integration.Connector.Base.Providers;

using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using Insite.Common.Helpers;
using Insite.Common.HttpUtilities;
using Insite.Core.Interfaces.Plugins.Caching;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Enums;
using Newtonsoft.Json.Linq;

public class OAuthTokenProvider : IOAuthTokenProvider
{
    private readonly ICacheManager cacheManager;

    private readonly HttpClientProvider httpClientProvider;

    public OAuthTokenProvider(ICacheManager cacheManager, HttpClientProvider httpClientProvider)
    {
        this.cacheManager = cacheManager;
        this.httpClientProvider = httpClientProvider;
    }

    public string GetAccessToken(IntegrationConnection integrationConnection)
    {
        var cacheKey = CacheKey(integrationConnection.DataSource);

        var cachedAccessToken = this.cacheManager.Get<string>(cacheKey);
        if (cachedAccessToken != null)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return EncryptionHelper.DecryptAes(this.cacheManager.Get<string>(cacheKey));
#pragma warning restore
        }

        var content = GetRequestContent(integrationConnection);
        var response = this.DoRequest(integrationConnection.DataSource, content);

        var (accessToken, expiration) = ParseAccessTokenAndExpiration(response);

#pragma warning disable CS0618 // Type or member is obsolete
        this.cacheManager.Add(cacheKey, EncryptionHelper.EncryptAes(accessToken), expiration);
#pragma warning restore

        return accessToken;
    }

    private static HttpContent GetRequestContent(IntegrationConnection integrationConnection)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return integrationConnection.TypeName
            .Trim()
            .EqualsIgnoreCase(nameof(IntegrationConnectionType.ApiRopcEndpoint))
            ? new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "client_id", integrationConnection.LogOn },
                    {
                        "client_secret",
                        EncryptionHelper.DecryptAes(integrationConnection.Password)
                    },
                    { "username", integrationConnection.GatewayHost },
                    {
                        "password",
                        EncryptionHelper.DecryptAes(integrationConnection.GatewayService)
                    },
                    { "resource", integrationConnection.Client }
                }
            )
            : integrationConnection.TypeName
                .Trim()
                .EqualsIgnoreCase(nameof(IntegrationConnectionType.SXeApi))
                ? new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        { "grant_type", "password" },
                        { "client_id", integrationConnection.MessageServerHost },
                        {
                            "client_secret",
                            EncryptionHelper.DecryptAes(integrationConnection.MessageServerService)
                        },
                        { "username", integrationConnection.GatewayHost },
                        {
                            "password",
                            EncryptionHelper.DecryptAes(integrationConnection.GatewayService)
                        }
                    }
                )
                : new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        { "grant_type", "client_credentials" },
                        { "client_id", integrationConnection.LogOn },
                        {
                            "client_secret",
                            EncryptionHelper.DecryptAes(integrationConnection.Password)
                        }
                    }
                );
#pragma warning restore
    }

    private string DoRequest(string serviceUrl, HttpContent content)
    {
        var httpClient = this.httpClientProvider.GetHttpClient(new Uri(serviceUrl));

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        using (var httpResponse = httpClient.PostAsync(string.Empty, content).Result)
        {
            var responseContent = httpResponse?.Content?.ReadAsStringAsync().Result;
            if (httpResponse == null || !httpResponse.IsSuccessStatusCode)
            {
                var errorMessage =
                    $"Error calling API. Status Code: {httpResponse?.StatusCode}. "
                    + $"Reason Phrase: {httpResponse?.ReasonPhrase}. "
                    + $"{responseContent}";

                throw new DataException(errorMessage);
            }

            return responseContent;
        }
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }

    private static (string accessToken, TimeSpan tokenExpiration) ParseAccessTokenAndExpiration(
        string response
    )
    {
        try
        {
            var responseObject = JObject.Parse(response);
            var accessToken = responseObject["access_token"].ToString();
            var tokenExpiration = TimeSpan.FromSeconds(
                responseObject["expires_in"].ToObject<int>()
            );

            return (accessToken, tokenExpiration);
        }
        catch
        {
            throw new Exception(
                $"Error parsing access_token and expires_in from response: {response}"
            );
        }
    }

    private static string CacheKey(string serviceUrl) => $"{serviceUrl}_AccessToken";
}
