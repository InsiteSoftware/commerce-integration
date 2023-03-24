namespace Insite.WIS.Epicor.Epicor10;

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

using Insite.WIS.Broker;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.Epicor.Epicor10SessionModService;

/// <summary>Defines the endpoint bindings that we will support with this code.</summary>
///
public class IntegrationProcessorCoreEpicor10
{
    /// <summary>Supported Binding Types</summary>
    public enum EndpointBindingType
    {
        /// <summary>WSHttpBinding</summary>
        SoapHttp,

        /// <summary>BasicHttpBinding</summary>
        BasicHttp
    }

    // Paths used in Service Endpoints
    public const string SessionServicePath = "/Ice/Lib/SessionMod.svc";

    public const string CustomerServicePath = "/Erp/BO/Customer.svc";

    public const string ShipToServicePath = "/Erp/BO/ShipTo.svc";

    public const string CustCntServicePath = "/Erp/BO/CustCnt.svc";

    public const string SalesOrderServicePath = "/Erp/BO/SalesOrder.svc";

    public const string CashGroupServicePath = "/Erp/BO/CashGrp.svc";

    public const string CashRecServicePath = "/Erp/BO/CashRec.svc";

    public const string CreditTranServicePath = "/Erp/BO/CreditTran.svc";

    public IntegrationProcessorCoreEpicor10(
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        this.SiteConnection = siteConnection;
        this.IntegrationJob = integrationJob;
    }

    public SiteConnection SiteConnection { get; private set; }

    public IntegrationJob IntegrationJob { get; private set; }

    /// <summary>No-op exclude clause used when a where clause is required</summary>
    public virtual string ExcludeClause
    {
        get
        {
            // The " 1 = 0 " exclude clause only works on Progress based systems, not on SQL Server based systems.
            // Changing it to "" effectively means we have no exclude clause.
            // return " 1 = 0 ";
            // DSZ Set to Company = ''
            return "Company = ''";
        }
    }

    /// <summary>The base to be used for all service endpoint URLs</summary>
    public string ServiceUrlBase { get; set; }

    /// <summary>The ERP Username</summary>
    public string Username { get; set; }

    /// <summary>The ERP Password</summary>
    public string Password { get; set; }

    /// <summary>The SessionId used for all service calls.</summary>
    public Guid SessionId { get; set; }

    /// <summary>The ERP CompanyId</summary>
    public string CompanyId { get; set; }

    /// <summary>Public constructor of IntegrationProcessorCoreEpicor10</summary>
    /// <param name="serviceUrlBase">The base URL that all service paths will be used for all service endpoints.</param>
    /// <param name="companyId">The company that will be used for all service endpoints.</param>
    /// <param name="username">The Epicor username to use for all service endpoints.</param>
    /// <param name="password">The Epicor password to use for all service endpoints.</param>
    public void Initialize(
        string serviceUrlBase,
        string companyId,
        string username,
        string password
    )
    {
        this.ServiceUrlBase = serviceUrlBase;
        this.CompanyId = companyId;
        this.Username = username;
        this.Password = password;
        this.SessionId = this.BeginSession();
    }

    /// <summary>Establish a session to be used for future service calls</summary>
    /// <returns>The <see cref="Guid"/> The session Id.</returns>
    public virtual Guid BeginSession()
    {
        Guid sessionId;
        using (
            var sessionModClient = this.GetClient<
                SessionModSvcContractClient,
                SessionModSvcContract
            >(SessionServicePath)
        )
        {
            sessionId = sessionModClient.Login();
            sessionModClient.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    sessionId,
                    this.Username,
                    this.SiteConnection,
                    this.IntegrationJob
                )
            );

            // For some reason this does not appear to actually be changing the company for the session.
            sessionModClient.SetCompany(
                this.CompanyId,
                out var plantId,
                out var plantName,
                out var workstationId,
                out var workstationDescription,
                out var employeeId,
                out var countryGroupCode,
                out var countryCode
            );
        }

        return sessionId;
    }

    /// <summary>Terminate an active session</summary>
    public virtual void EndSession()
    {
        using (
            var sessionModClient = this.GetClient<
                SessionModSvcContractClient,
                SessionModSvcContract
            >(SessionServicePath)
        )
        {
            sessionModClient.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    this.SessionId,
                    this.Username,
                    this.SiteConnection,
                    this.IntegrationJob
                )
            );
            sessionModClient.Logout();
        }
    }

    /// <summary>Method to get a client for a requested service</summary>
    /// <param name="servicePath">The path to the service being instantiated.</param>
    /// <returns>The TClient for the requested service.</returns>
    public virtual TClient GetClient<TClient, TInterface>(string servicePath)
        where TClient : ClientBase<TInterface>
        where TInterface : class
    {
        Binding binding = null;

        var serviceUri = this.GetServiceUri(servicePath);
        var bindingType =
            (serviceUri.Scheme == "http")
                ? EndpointBindingType.SoapHttp
                : EndpointBindingType.BasicHttp;

        var endpointAddress = new EndpointAddress(serviceUri.AbsoluteUri);

        switch (bindingType)
        {
            case EndpointBindingType.BasicHttp:
                binding = this.GetBasicHttpBinding();
                break;
            case EndpointBindingType.SoapHttp:
                binding = this.GetWsHttpBinding();
                break;
        }

        var client = (TClient)Activator.CreateInstance(typeof(TClient), binding, endpointAddress);

        if (!string.IsNullOrEmpty(this.Username) && (client.ClientCredentials != null))
        {
            client.ClientCredentials.UserName.UserName = this.Username;
            client.ClientCredentials.UserName.Password = this.Password;
        }

        return client;
    }

    /// <summary>Method to get a complete endpoint URL for a requested service</summary>
    /// <param name="servicePath">The path to the service being requested.</param>
    /// <returns>The full url for the requested service.</returns>
    protected virtual Uri GetServiceUri(string servicePath)
    {
        return new Uri(this.ServiceUrlBase + servicePath);
    }

    /// <summary>Method to get a initialized SOAP binding</summary>
    /// <returns>The <see cref="WSHttpBinding"/>.</returns>
    protected virtual WSHttpBinding GetWsHttpBinding()
    {
        var binding = new WSHttpBinding();

        const int maxBindingSize = int.MaxValue;
        binding.MaxReceivedMessageSize = maxBindingSize;
        binding.ReaderQuotas.MaxDepth = maxBindingSize;
        binding.ReaderQuotas.MaxStringContentLength = maxBindingSize;
        binding.ReaderQuotas.MaxArrayLength = maxBindingSize;
        binding.ReaderQuotas.MaxBytesPerRead = maxBindingSize;
        binding.ReaderQuotas.MaxNameTableCharCount = maxBindingSize;

        binding.Security.Mode = SecurityMode.Message;
        binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;

        return binding;
    }

    /// <summary>Method to get a basic HTTP binding</summary>
    /// <returns>The <see cref="BasicHttpBinding"/>.</returns>
    protected virtual BasicHttpBinding GetBasicHttpBinding()
    {
        var binding = new BasicHttpBinding();

        const int maxBindingSize = int.MaxValue;
        binding.MaxReceivedMessageSize = maxBindingSize;
        binding.ReaderQuotas.MaxDepth = maxBindingSize;
        binding.ReaderQuotas.MaxStringContentLength = maxBindingSize;
        binding.ReaderQuotas.MaxArrayLength = maxBindingSize;
        binding.ReaderQuotas.MaxBytesPerRead = maxBindingSize;
        binding.ReaderQuotas.MaxNameTableCharCount = maxBindingSize;

        binding.Security.Mode = BasicHttpSecurityMode.TransportWithMessageCredential;
        binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

        return binding;
    }
}
