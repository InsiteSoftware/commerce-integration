namespace Insite.Integration.Connector.Facts.Services;

using System;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Insite.Common.Dependencies;
using Insite.Common.HttpUtilities;
using Insite.Common.Logging;
using Insite.Integration.Connector.Facts.V9.Api.Models.CustomerSummary;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Api.Models;

internal sealed class FactsApiService : IFactsApiService
{
    private readonly string url;

    private readonly string consumerKey;

    private readonly string password;

    private readonly HttpClient httpClient;

    public FactsApiService(string url, string consumerKey, string password)
        : this(
            url,
            consumerKey,
            password,
            DependencyLocator.Current.GetInstance<HttpClientProvider>()
        ) { }

    public FactsApiService(
        string url,
        string consumerKey,
        string password,
        HttpClientProvider httpClientProvider
    )
    {
        this.url = url;
        this.consumerKey = consumerKey;
        this.password = password;

        this.httpClient = httpClientProvider.GetHttpClient(new Uri(this.url));
    }

    public PriceAvailabilityResponse PriceAvailability(
        PriceAvailabilityRequest priceAvailabilityRequest
    )
    {
        priceAvailabilityRequest.ConsumerKey = this.consumerKey;
        priceAvailabilityRequest.Password = this.password;

        this.LogRedactingCredentials(priceAvailabilityRequest); // "log, redacting credentials"

        var xmlRequest = FactsSerializationService.Serialize(priceAvailabilityRequest);
        var xmlResponse = this.CallWebService(xmlRequest);

        LogHelper.For(this).Debug($"{nameof(PriceAvailabilityResponse)}: {xmlResponse}");

        return FactsSerializationService.Deserialize<PriceAvailabilityResponse>(xmlResponse);
    }

    public CustomerSummaryResponse CustomerSummary(CustomerSummaryRequest customerSummaryRequest)
    {
        customerSummaryRequest.ConsumerKey = this.consumerKey;
        customerSummaryRequest.Password = this.password;

        this.LogRedactingCredentials(customerSummaryRequest); // "log, redacting credentials"

        var xmlRequest = FactsSerializationService.Serialize(customerSummaryRequest);
        var xmlResponse = this.CallWebService(xmlRequest);

        LogHelper.For(this).Debug($"{nameof(CustomerSummaryResponse)}: {xmlResponse}");

        return FactsSerializationService.Deserialize<CustomerSummaryResponse>(xmlResponse);
    }

    public OrderTotalResponse OrderTotal(OrderTotalRequest orderTotalRequest)
    {
        orderTotalRequest.ConsumerKey = this.consumerKey;
        orderTotalRequest.Password = this.password;

        this.LogRedactingCredentials(orderTotalRequest); // "log, redacting credentials"

        var xmlRequest = FactsSerializationService.Serialize(orderTotalRequest);
        var xmlResponse = this.CallWebService(xmlRequest);

        LogHelper.For(this).Debug($"{nameof(OrderTotalResponse)}: {xmlResponse}");

        return FactsSerializationService.Deserialize<OrderTotalResponse>(xmlResponse);
    }

    public OrderLoadResponse OrderLoad(OrderLoadRequest orderLoadRequest)
    {
        orderLoadRequest.ConsumerKey = this.consumerKey;
        orderLoadRequest.Password = this.password;

        this.LogRedactingCredentials(orderLoadRequest); // "log, redacting credentials"

        var xmlRequest = FactsSerializationService.Serialize(orderLoadRequest);
        var xmlResponse = this.CallWebService(xmlRequest);

        LogHelper.For(this).Debug($"{nameof(OrderLoadResponse)}: {xmlResponse}");

        return FactsSerializationService.Deserialize<OrderLoadResponse>(xmlResponse);
    }

    private string CallWebService(string xmlRequest)
    {
        var httpContent = new StringContent(xmlRequest, Encoding.UTF8, MediaTypeNames.Text.Xml);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, string.Empty)
        {
            Content = httpContent,
        };
        httpRequest.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue(MediaTypeNames.Text.Xml)
        );

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        var httpResponse = this.httpClient.SendAsync(httpRequest).Result;
        if (httpResponse == null)
        {
            throw new DataException($"Error calling commerce gateway. Response is null.");
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new DataException(
                $"Error calling commerce gateway. Status Code: {httpResponse.StatusCode}. Reason Phrase: {httpResponse.ReasonPhrase}."
            );
        }

        return httpResponse.Content.ReadAsStringAsync().Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }

    /// <summary>
    /// Redact the ConsumerKey and Password fields so their real values don't end up
    /// in the debugging output, do the output, and then restore the real values
    /// </summary>
    private void LogRedactingCredentials<T>(T request)
        where T : IRequest
    {
        if (request == null)
        {
            return;
        }

        var user = request.ConsumerKey;
        var pass = request.Password;

        request.ConsumerKey = "[redacted]";
        request.Password = "[redacted]";

        var serializedRequest = FactsSerializationService.Serialize(request);

        LogHelper.For(this).Debug($"{request.GetType().Name}: {serializedRequest}");

        request.ConsumerKey = user;
        request.Password = pass;
    }
}
