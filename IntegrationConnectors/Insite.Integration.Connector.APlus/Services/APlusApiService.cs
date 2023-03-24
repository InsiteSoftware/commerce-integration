namespace Insite.Integration.Connector.APlus.Services;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Xml.Serialization;
using Insite.Common.Dependencies;
using Insite.Common.HttpUtilities;
using Insite.Common.Logging;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.AccountsReceivableSummary;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;

internal sealed class APlusApiService : IAPlusApiService
{
    private readonly HttpClientProvider httpClientProvider;

    /// <summary>The url of the A+ web service.</summary>
    private string Url { get; set; }

    /// <summary>The subscriber id used to authenticate web service calls.</summary>
    private string SubscriberId { get; set; }

    /// <summary>The subscriber password used to authenticate web service calls.</summary>
    private string SubscriberPassword { get; set; }

    /// <summary>Initializes a new instance of the <see cref="APlusApiService"/> class./// </summary>
    /// <param name="url">The url of the A+ web service.</param>
    /// <param name="subscriberId">The subscriber id used to authenticate web service calls.</param>
    /// <param name="subscriberPassword">The subscriber password used to authenticate web service calls.</param>
    public APlusApiService(string url, string subscriberId, string subscriberPassword)
        : this(
            url,
            subscriberId,
            subscriberPassword,
            DependencyLocator.Current.GetInstance<HttpClientProvider>()
        ) { }

    /// <summary>Initializes a new instance of the <see cref="APlusApiService"/> class.</summary>
    /// <param name="url">The url of the A+ web service.</param>
    /// <param name="subscriberId">The subscriber id used to authenticate web service calls.</param>
    /// <param name="subscriberPassword">The subscriber password used to authenticate web service calls.</param>
    /// <param name="httpClientProvider">Provides <see cref="HttpClient"/> instances from a cache.</param>
    public APlusApiService(
        string url,
        string subscriberId,
        string subscriberPassword,
        HttpClientProvider httpClientProvider
    )
    {
        this.Url = url;
        this.SubscriberId = subscriberId;
        this.SubscriberPassword = subscriberPassword;
        this.httpClientProvider = httpClientProvider;
    }

    /// <summary>Executes the ARSummary web service call with the provided <see cref="AccountsReceivableSummaryRequest"/>.</summary>
    /// <returns>The <see cref="AccountsReceivableSummaryResponse"/> received from the web service.</returns>
    public AccountsReceivableSummaryResponse AccountsReceivableSummary(
        AccountsReceivableSummaryRequest accountsReceivableSummaryRequest
    )
    {
        var xmlRequest = Serialize(accountsReceivableSummaryRequest);

        var xmlResponse = this.CallCommerceGateway("ARSummary", xmlRequest);

        return Deserialize<AccountsReceivableSummaryResponse>(xmlResponse);
    }

    /// <summary>Executes the GetAvail web service call with the provided <see cref="LineItemPriceAndAvailabilityRequest"/>.</summary>
    /// <returns>The <see cref="LineItemPriceAndAvailabilityResponse"/> received from the web service.</returns>
    public LineItemPriceAndAvailabilityResponse LineItemPriceAndAvailability(
        LineItemPriceAndAvailabilityRequest lineItemPriceAndAvailabilityRequest
    )
    {
        var xmlRequest = Serialize(lineItemPriceAndAvailabilityRequest);

        var xmlResponse = this.CallCommerceGateway("GetAvail", xmlRequest);

        return Deserialize<LineItemPriceAndAvailabilityResponse>(xmlResponse);
    }

    /// <summary>Executes the CreateOrder web service call with the provided <see cref="CreateOrderRequest"/>.</summary>
    /// <returns>The <see cref="CreateOrderResponse"/> received from the web service.</returns>
    public CreateOrderResponse CreateOrder(CreateOrderRequest createOrderRequest)
    {
        var xmlRequest = Serialize(createOrderRequest);

        var xmlResponse = this.CallCommerceGateway("CreateOrder", xmlRequest);

        return Deserialize<CreateOrderResponse>(xmlResponse);
    }

    private string CallCommerceGateway(string transactionName, string xmlRequest)
    {
        LogHelper.For(this).Debug($"{transactionName} Request: {xmlRequest}");

        string response;

        var httpClient = this.httpClientProvider.GetHttpClient(new Uri(this.Url));

        var requestUri = this.GetRequestUri();
        var requestContent = this.GetRequestContent(transactionName, xmlRequest);

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        var httpResponse = httpClient.PostAsync(requestUri, requestContent).Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        if (httpResponse == null)
        {
            throw new DataException(
                $"{transactionName}: error calling commerce gateway. Response is null."
            );
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new DataException(
                $"{transactionName}: error calling commerce gateway. Status Code: {httpResponse.StatusCode}. Reason Phrase: {httpResponse.ReasonPhrase}."
            );
        }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        response = httpResponse.Content.ReadAsStringAsync().Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

        LogHelper.For(this).Debug($"{transactionName} Response: {response}");

        return response;
    }

    private string GetRequestUri()
    {
        return "?OutputType=0" + "&InputType=1";
    }

    private FormUrlEncodedContent GetRequestContent(string transactionName, string request)
    {
        return new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                { "SubscriberID", this.SubscriberId },
                { "SubscriberPassword", this.SubscriberPassword },
                { "TransactionName", transactionName },
                { "Data", request }
            }
        );
    }

    private static string Serialize<T>(T request)
    {
        try
        {
            var stringwriter = new StringWriter();
            var serializer = new XmlSerializer(typeof(T));

            serializer.Serialize(stringwriter, request);

            return stringwriter.ToString();
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"Error serializing {typeof(T).FullName} to XML request. Message: {exception.Message}."
            );
        }
    }

    private static T Deserialize<T>(string response)
    {
        try
        {
            var stringReader = new StringReader(response);
            var serializer = new XmlSerializer(typeof(T));

            return (T)serializer.Deserialize(stringReader);
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"Error deserializing XML response to {typeof(T).FullName}. XML Response: {response}. Message: {exception.Message}."
            );
        }
    }
}
