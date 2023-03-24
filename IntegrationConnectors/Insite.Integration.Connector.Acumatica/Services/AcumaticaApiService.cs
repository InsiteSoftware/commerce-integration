namespace Insite.Integration.Connector.Acumatica.Services;

using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Insite.Common.Dependencies;
using Insite.Common.HttpUtilities;
using Insite.Common.Logging;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.CustomerPaymentMethod;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.InventoryAllocationInquiry;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.Login;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;
using Newtonsoft.Json;

internal sealed class AcumaticaApiService : IAcumaticaApiService
{
    private const string LoginApiPath = "entity/auth/login/";

    private const string LogoutApiPath = "entity/auth/logout/";

    private const string InventoryAllocationInquiryApiPath =
        "entity/Default/18.200.001/InventoryAllocationInquiry/";

    private const string SalesOrderApiPath = "entity/Default/18.200.001/SalesOrder/";

    private const string CustomerPaymentMethodApiPath =
        "entity/Default/18.200.001/CustomerPaymentMethod/";

    private readonly string url;

    private readonly string userName;

    private readonly string password;

    private readonly string branchNumber;

    private readonly string companyNumber;

    private readonly HttpClient httpClient;

    public AcumaticaApiService(
        string url,
        string userName,
        string password,
        string branchNumber,
        string companyNumber
    )
        : this(
            url,
            userName,
            password,
            branchNumber,
            companyNumber,
            DependencyLocator.Current.GetInstance<HttpClientProvider>()
        ) { }

    public AcumaticaApiService(
        string url,
        string userName,
        string password,
        string branchNumber,
        string companyNumber,
        HttpClientProvider httpClientProvider
    )
    {
        this.url = url;
        this.userName = userName;
        this.password = password;
        this.branchNumber = branchNumber;
        this.companyNumber = companyNumber;

        this.httpClient = httpClientProvider.GetHttpClient(new Uri(this.url));
    }

    public void Login()
    {
        LogHelper.For(this).Debug($"Login Started");

        ServicePointManager.ServerCertificateValidationCallback += (
            sender,
            cert,
            chain,
            sslPolicyErrors
        ) => true;

        var loginRequest = new LoginRequest
        {
            Name = this.userName,
            Password = this.password,
            Company = this.companyNumber,
            Branch = this.branchNumber
        };

        var jsonLoginRequest = Serialize(loginRequest);
        var httpContent = new StringContent(jsonLoginRequest, Encoding.UTF8, "application/json");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, LoginApiPath)
        {
            Content = httpContent,
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        var httpResponse = this.httpClient.SendAsync(httpRequest).Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        if (httpResponse == null)
        {
            var errorMessage = "Login failed. Response is null.";

            LogHelper.For(this).Debug(errorMessage);
            throw new DataException(errorMessage);
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorMessage = $"Login failed. {GetErrorMessageDetails(httpResponse)}";

            LogHelper.For(this).Debug(errorMessage);
            throw new DataException(errorMessage);
        }

        LogHelper.For(this).Debug($"Login Finished");
    }

    public void Logout()
    {
        LogHelper.For(this).Debug($"Logout Started");

        var httpContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, LogoutApiPath)
        {
            Content = httpContent,
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

#pragma warning disable VSTHRD002
        var httpResponse = this.httpClient.SendAsync(httpRequest).Result;
#pragma warning restore VSTHRD002
        if (httpResponse == null)
        {
            var errorMessage = "Logout failed. Response is null.";

            LogHelper.For(this).Debug(errorMessage);
            throw new DataException(errorMessage);
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorMessage = $"Logout failed. {GetErrorMessageDetails(httpResponse)}";

            LogHelper.For(this).Debug(errorMessage);
            throw new DataException(errorMessage);
        }

        LogHelper.For(this).Debug($"Logout Finished");
    }

    public InventoryAllocationInquiry InventoryAllocationInquiry(
        InventoryAllocationInquiry inventoryAllocationInquiry
    )
    {
        var jsonRequest = Serialize(inventoryAllocationInquiry);
        LogHelper.For(this).Debug($"{nameof(this.InventoryAllocationInquiry)}: {jsonRequest}");

        var jsonResponse = this.CallRestApi(InventoryAllocationInquiryApiPath, jsonRequest);
        LogHelper.For(this).Debug($"{nameof(this.InventoryAllocationInquiry)}: {jsonResponse}");

        return JsonConvert.DeserializeObject<InventoryAllocationInquiry>(jsonResponse);
    }

    public SalesOrder SalesOrder(SalesOrder salesOrder)
    {
        var jsonRequest = Serialize(salesOrder);
        LogHelper.For(this).Debug($"{nameof(this.SalesOrder)}: {jsonRequest}");

        var jsonResponse = this.CallRestApi(SalesOrderApiPath, jsonRequest);
        LogHelper.For(this).Debug($"{nameof(this.SalesOrder)}: {jsonResponse}");

        return JsonConvert.DeserializeObject<SalesOrder>(jsonResponse);
    }

    public CustomerPaymentMethod CustomerPaymentMethod(CustomerPaymentMethod customerPaymentMethod)
    {
        var jsonRequest = Serialize(customerPaymentMethod);
        LogHelper.For(this).Debug($"{nameof(this.CustomerPaymentMethod)}: {jsonRequest}");

        var jsonResponse = this.CallRestApi(CustomerPaymentMethodApiPath, jsonRequest);
        LogHelper.For(this).Debug($"{nameof(this.CustomerPaymentMethod)}: {jsonResponse}");

        return JsonConvert.DeserializeObject<CustomerPaymentMethod>(jsonResponse);
    }

    private string CallRestApi(string apiPath, string jsonContent)
    {
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var httpRequest = new HttpRequestMessage(HttpMethod.Put, apiPath)
        {
            Content = httpContent,
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

#pragma warning disable VSTHRD002
        var httpResponse = this.httpClient.SendAsync(httpRequest).Result;
#pragma warning restore VSTHRD002
        if (httpResponse == null)
        {
            var errorMessage = "Error calling Rest Api. Response is null.";

            LogHelper.For(this).Debug(errorMessage);
            throw new DataException(errorMessage);
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorMessage = $"Error calling Rest Api. {GetErrorMessageDetails(httpResponse)}";

            LogHelper.For(this).Debug(errorMessage);
            throw new DataException(errorMessage);
        }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        return httpResponse.Content.ReadAsStringAsync().Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }

    private static string Serialize<T>(T request)
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(request, settings);
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"Error serializing {nameof(T)} to Json request. Message: {exception.Message}."
            );
        }
    }

    private static string GetErrorMessageDetails(HttpResponseMessage httpResponseMessage)
    {
        var errorMessage =
            $"Status Code: {httpResponseMessage.StatusCode}. Reason Phrase: {httpResponseMessage.ReasonPhrase}.";

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        var responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        if (!string.IsNullOrEmpty(responseContent))
        {
            errorMessage += $" Content: {responseContent}";
        }

        return errorMessage;
    }
}
