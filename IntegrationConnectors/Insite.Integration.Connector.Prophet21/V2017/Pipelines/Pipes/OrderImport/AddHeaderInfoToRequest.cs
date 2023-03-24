namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;

using System;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddHeaderInfoToRequest : IPipe<OrderImportParameter, OrderImportResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddHeaderInfoToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 200;

    public OrderImportResult Execute(
        IUnitOfWork unitOfWork,
        OrderImportParameter parameter,
        OrderImportResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Started.");

        result.OrderImportRequest.Request.WebReferenceNumber = parameter.CustomerOrder.OrderNumber;
        result.OrderImportRequest.Request.PONumber = parameter.CustomerOrder.CustomerPO;
        result.OrderImportRequest.Request.NotepadText = parameter.CustomerOrder.Notes;

        if (
            parameter.CustomerOrder.FulfillmentMethod.EqualsIgnoreCase(
                FulfillmentMethod.PickUp.ToString()
            )
        )
        {
            result.OrderImportRequest.Request.RequireDate =
                parameter.CustomerOrder.RequestedPickupDate?.ToString("yyyy-MM-dd");
            result.OrderImportRequest.Request.WillCall = "TRUE";
        }
        else
        {
            result.OrderImportRequest.Request.RequireDate =
                parameter.CustomerOrder.RequestedDeliveryDate?.ToString("yyyy-MM-dd");
            result.OrderImportRequest.Request.FreightCode = this.GetFreightCode(
                parameter.CustomerOrder
            );
        }

        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Finished.");

        return result;
    }

    private string GetFreightCode(CustomerOrder customerOrder)
    {
        return !string.IsNullOrEmpty(customerOrder.ShipTo?.FreightTerms)
            ? customerOrder.ShipTo.FreightTerms
            : !string.IsNullOrEmpty(customerOrder.Customer?.FreightTerms)
                ? customerOrder.Customer.FreightTerms
                : this.integrationConnectorSettings.Prophet21FreightCode;
    }
}
