namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddContactIdToRequest : IPipe<OrderImportParameter, OrderImportResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public AddContactIdToRequest(
        IntegrationConnectorSettings integrationConnectorSettings,
        IPipeAssemblyFactory pipeAssemblyFactory
    )
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public int Order => 500;

    public OrderImportResult Execute(
        IUnitOfWork unitOfWork,
        OrderImportParameter parameter,
        OrderImportResult result
    )
    {
        if (
            this.integrationConnectorSettings.Prophet21OrderSubmitContactTreatment
            == Prophet21OrderSubmitContactTreatment.DoNotSubmitContact
        )
        {
            return result;
        }

        var getContactResult = this.pipeAssemblyFactory.ExecutePipeline(
            new GetContactParameter
            {
                CustomerOrder = parameter.CustomerOrder,
                JobLogger = parameter.JobLogger
            },
            new GetContactResult()
        );

        if (getContactResult.ResultCode != ResultCode.Success)
        {
            result.ResultCode = getContactResult.ResultCode;
            result.SubCode = getContactResult.SubCode;
            result.Messages = getContactResult.Messages;
        }
        else
        {
            result.OrderImportRequest.Request.ContactID = getContactResult.ContactId;
        }

        return result;
    }
}
