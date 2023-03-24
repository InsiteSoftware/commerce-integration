namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;

using System;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddHeaderInfoToRequest
    : IPipe<CreateCustomerOrderParameter, CreateCustomerOrderResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddHeaderInfoToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 200;

    public CreateCustomerOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateCustomerOrderParameter parameter,
        CreateCustomerOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Started.");

        result.CustomerOrder.shipViaCode =
            parameter.CustomerOrder.ShipVia?.ErpShipCode ?? string.Empty;
        result.CustomerOrder.customerPoNo = parameter.CustomerOrder.CustomerPO;
        result.CustomerOrder.custRef = parameter.CustomerOrder.OrderNumber;
        result.CustomerOrder.payTermId = parameter.CustomerOrder.TermsCode;
        result.CustomerOrder.contract =
            parameter.CustomerOrder.DefaultWarehouse?.Name ?? string.Empty;

        result.CustomerOrder.deliveryTerms = this.GetDeliveryTerms(parameter.CustomerOrder);
        result.CustomerOrder.wantedDeliveryDate = GetWantedDeliveryDate(parameter.CustomerOrder);
        result.CustomerOrder.wantedDeliveryDateSpecified = true;
        result.CustomerOrder.noteText = GetNoteText(parameter.CustomerOrder);

        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Finished.");

        return result;
    }

    private string GetDeliveryTerms(CustomerOrder customerOrder)
    {
        if (customerOrder.FulfillmentMethod.EqualsIgnoreCase(FulfillmentMethod.PickUp.ToString()))
        {
            return this.integrationConnectorSettings.IfsPickupDeliveryTerms;
        }
        else if (!string.IsNullOrEmpty(customerOrder.ShipTo?.FreightTerms))
        {
            return customerOrder.ShipTo.FreightTerms;
        }
        else
        {
            return this.integrationConnectorSettings.IfsDefaultDeliveryTerms;
        }
    }

    private static DateTime GetWantedDeliveryDate(CustomerOrder customerOrder)
    {
        var wantedDeliveryDate = customerOrder.FulfillmentMethod.EqualsIgnoreCase(
            FulfillmentMethod.PickUp.ToString()
        )
            ? customerOrder.RequestedPickupDate?.DateTime
            : customerOrder.RequestedDeliveryDate?.DateTime;

        return wantedDeliveryDate != null && wantedDeliveryDate.HasValue
            ? wantedDeliveryDate.Value
            : default(DateTime);
    }

    private static string GetNoteText(CustomerOrder customerOrder)
    {
        return customerOrder.Notes.Length <= 2000
            ? customerOrder.Notes
            : customerOrder.Notes.Substring(0, 2000);
    }
}
