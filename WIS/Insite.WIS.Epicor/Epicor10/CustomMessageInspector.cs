namespace Insite.WIS.Epicor.Epicor10;

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;

using Insite.WIS.Broker;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.WebIntegrationService;

using Message = System.ServiceModel.Channels.Message;

internal class CustomMessageInspector : IClientMessageInspector
{
    private readonly string epicorUserId;

    private readonly IntegrationJob integrationJob;

    private readonly Guid sessionId;

    private readonly SiteConnection siteConnection;

    public CustomMessageInspector(
        Guid sessionId,
        string epicorUserId,
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        this.sessionId = sessionId;
        this.epicorUserId = epicorUserId;
        this.siteConnection = siteConnection;
        this.integrationJob = integrationJob;
    }

    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
        var jobLogger = new IntegrationJobLogger(this.siteConnection, this.integrationJob);
        jobLogger.Debug(reply.ToString(), false);
    }

    public object BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        var jobLogger = new IntegrationJobLogger(this.siteConnection, this.integrationJob);
        jobLogger.Debug(request.ToString(), false);

        if (this.sessionId != Guid.Empty)
        {
            var sessionHeader = new SessionInfoHeader
            {
                SessionId = this.sessionId,
                EpicorUserId = this.epicorUserId
            };
            request.Headers.Add(sessionHeader);
        }

        return request;
    }
}

internal class SessionInfoHeader : MessageHeader
{
    public Guid SessionId { get; set; }

    public string EpicorUserId { get; set; }

    public override string Name
    {
        get { return "SessionInfo"; }
    }

    public override string Namespace
    {
        get { return "urn:epic:headers:SessionInfo"; }
    }

    protected override void OnWriteHeaderContents(
        XmlDictionaryWriter writer,
        MessageVersion messageVersion
    )
    {
        writer.WriteElementString(
            "SessionID",
            @"http://schemas.datacontract.org/2004/07/Epicor.Hosting",
            this.SessionId.ToString()
        );
        writer.WriteElementString(
            "UserID",
            @"http://schemas.datacontract.org/2004/07/Epicor.Hosting",
            this.EpicorUserId
        );
    }
}

internal class HookServiceBehavior : IEndpointBehavior
{
    private readonly string epicorUserId;

    private readonly IntegrationJob integrationJob;

    private readonly Guid sessionId;

    private readonly SiteConnection siteConnection;

    public HookServiceBehavior(
        Guid sessionId,
        string epicorUserId,
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        this.sessionId = sessionId;
        this.epicorUserId = epicorUserId;
        this.siteConnection = siteConnection;
        this.integrationJob = integrationJob;
    }

    public void AddBindingParameters(
        ServiceEndpoint endpoint,
        BindingParameterCollection bindingParameters
    ) { }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(
            new CustomMessageInspector(
                this.sessionId,
                this.epicorUserId,
                this.siteConnection,
                this.integrationJob
            )
        );
    }

    public void ApplyDispatchBehavior(
        ServiceEndpoint endpoint,
        EndpointDispatcher endpointDispatcher
    ) { }

    public void Validate(ServiceEndpoint endpoint) { }
}
