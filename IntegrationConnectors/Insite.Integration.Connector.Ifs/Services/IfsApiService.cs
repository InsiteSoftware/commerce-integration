namespace Insite.Integration.Connector.Ifs.Services;

using System;
using System.IO;
using System.Net;
using System.Xml.Serialization;

using Insite.Common.Logging;
using Insite.Integration.Connector.Ifs.Services;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;

internal sealed class IfsApiService : IIfsApiService
{
    private const string CustomerOrderServicesApiPath = "CustomerOrderServices";

    private const string SalesPartServicesApiPath = "SalesPartServices";

    private string Url { get; set; }

    private string UserName { get; set; }

    private string Password { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IfsApiService"/> class.
    /// </summary>
    /// <param name="url">The url of the Ifs web service.</param>
    /// <param name="userName">The username used to authenticate to the Ifs web service.</param>
    /// <param name="password">The password used to authenticate to the Ifs web service.</param>
    public IfsApiService(string url, string userName, string password)
    {
        this.Url = url;
        this.UserName = userName;
        this.Password = password;
    }

    /// <summary>
    /// Executes the createCustomerOrder with the provided <see cref="customerOrder"/>.
    /// </summary>
    /// <param name="customerOrder">The <see cref="customerOrder"/></param>
    /// <returns>The <see cref="orderResponse"/></returns>
    public orderResponse CreateCustomerOrder(customerOrder customerOrder)
    {
        LogHelper.For(this).Debug($"{nameof(customerOrder)}: {GetSerializedValue(customerOrder)}");

        orderResponse orderResponse = null;

        try
        {
            var ifsSoapService = CreateIfsSoapService(
                this.Url,
                CustomerOrderServicesApiPath,
                this.UserName,
                this.Password
            );

            orderResponse = ifsSoapService.createCustomerOrder(customerOrder);
        }
        catch (Exception exception)
        {
            orderResponse = new orderResponse { errorMessage = exception.Message };
        }

        LogHelper.For(this).Debug($"{nameof(orderResponse)}: {GetSerializedValue(orderResponse)}");

        return orderResponse;
    }

    /// <summary>
    /// Executes the getCustomerPrice with the provided <see cref="customerPriceRequest"/>.
    /// </summary>
    /// <param name="customerPriceRequest">The <see cref="customerPriceRequest"/></param>
    /// <returns>The <see cref="customerPriceResponse"/></returns>
    public customerPriceResponse GetCustomerPrice(customerPriceRequest customerPriceRequest)
    {
        LogHelper
            .For(this)
            .Debug($"{nameof(customerPriceRequest)}: {GetSerializedValue(customerPriceRequest)}");

        customerPriceResponse customerPriceResponse = null;

        try
        {
            var ifsSoapService = CreateIfsSoapService(
                this.Url,
                SalesPartServicesApiPath,
                this.UserName,
                this.Password
            );

            customerPriceResponse = ifsSoapService.getCustomerPrice(customerPriceRequest);
        }
        catch (Exception exception)
        {
            customerPriceResponse = new customerPriceResponse { errorText = exception.Message };
        }

        LogHelper
            .For(this)
            .Debug($"{nameof(customerPriceResponse)}: {GetSerializedValue(customerPriceResponse)}");

        return customerPriceResponse;
    }

    /// <summary>
    /// Executes the getPartAvailability with the provided <see cref="partAvailabilityRequest"/>.
    /// </summary>
    /// <param name="partAvailabilityRequest">The <see cref="partAvailabilityRequest"/></param>
    /// <returns>The <see cref="partAvailabilityResponse"/></returns>
    public partAvailabilityResponse GetPartAvailability(
        partAvailabilityRequest partAvailabilityRequest
    )
    {
        LogHelper
            .For(this)
            .Debug(
                $"{nameof(partAvailabilityRequest)}: {GetSerializedValue(partAvailabilityRequest)}"
            );

        partAvailabilityResponse partAvailabilityResponse = null;

        try
        {
            var ifsSoapService = CreateIfsSoapService(
                this.Url,
                SalesPartServicesApiPath,
                this.UserName,
                this.Password
            );

            partAvailabilityResponse = ifsSoapService.getPartAvailability(partAvailabilityRequest);
        }
        catch (Exception exception)
        {
            partAvailabilityResponse = new partAvailabilityResponse
            {
                errorText = exception.Message
            };
        }

        LogHelper
            .For(this)
            .Debug(
                $"{nameof(partAvailabilityResponse)}: {GetSerializedValue(partAvailabilityResponse)}"
            );

        return partAvailabilityResponse;
    }

    private static IfsSoapService CreateIfsSoapService(
        string baseUrl,
        string apiPath,
        string userName,
        string password
    )
    {
        return new IfsSoapService(
            baseUrl + (baseUrl.EndsWith("/") ? string.Empty : "/") + apiPath,
            userName,
            password
        )
        {
            Credentials = GetNetworkCredential(baseUrl, userName, password),
            PreAuthenticate = true
        };
    }

    private static NetworkCredential GetNetworkCredential(
        string url,
        string userName,
        string password
    )
    {
        var networkCredentials = new NetworkCredential(userName, password);
        var uri = new Uri(url);

        return networkCredentials.GetCredential(uri, "Basic");
    }

    private static string GetSerializedValue(object value)
    {
        var serializer = new XmlSerializer(value.GetType());
        var writer = new StringWriter();

        serializer.Serialize(writer, value);

        return writer.ToString();
    }
}
