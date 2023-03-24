namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;

using System;
using Insite.Common.Providers;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;

public sealed class CreateRequest : IPipe<SubmitOrderParameter, SubmitOrderResult>
{
    private readonly ICustomerHelper customerHelper;

    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public CreateRequest(
        ICustomerHelper customerHelper,
        IntegrationConnectorSettings integrationConnectorSettings
    )
    {
        this.customerHelper = customerHelper;
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 100;

    public SubmitOrderResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderParameter parameter,
        SubmitOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CreateRequest)} Started.");

        result.CustomerOrderRequest = new CustomerOrder { IsNewCustomer = false };

        var billTo = this.customerHelper.GetBillTo(unitOfWork, parameter.CustomerOrder);
        result.CustomerOrderRequest.CustomerNo = billTo?.ErpNumber;
        result.CustomerOrderRequest.CustomerPoNo = parameter.CustomerOrder.CustomerPO;
        result.CustomerOrderRequest.BillAddrNo = billTo?.ErpSequence;

        var shipTo = this.customerHelper.GetShipTo(unitOfWork, parameter.CustomerOrder);
        result.CustomerOrderRequest.ShipAddrNo = shipTo?.ErpSequence;

        var defaultDeliveryDate = DateTimeProvider.Current.Now.AddDays(1);
        result.CustomerOrderRequest.CurrencyCode = parameter.CustomerOrder.Currency?.CurrencyCode;
        result.CustomerOrderRequest.ShipViaCode = parameter.CustomerOrder.ShipVia?.ErpShipCode;
        result.CustomerOrderRequest.WantedDeliveryDate =
            parameter.CustomerOrder.RequestedDeliveryDate ?? defaultDeliveryDate;
        result.CustomerOrderRequest.Contract = parameter.CustomerOrder.DefaultWarehouse?.Name;

        result.CustomerOrderRequest.AuthorizeCode =
            this.integrationConnectorSettings.IfsAurenaOrderCoordinator;
        result.CustomerOrderRequest.OrderId = this.integrationConnectorSettings.IfsAurenaOrderType;

        result.CustomerOrderRequest.DeliveryTerms =
            parameter.CustomerOrder.FulfillmentMethod.EqualsIgnoreCase(
                FulfillmentMethod.PickUp.ToString()
            )
                ? this.integrationConnectorSettings.IfsAurenaPickupDeliveryTerms
                : !string.IsNullOrWhiteSpace(shipTo?.FreightTerms)
                    ? shipTo.FreightTerms
                    : this.integrationConnectorSettings.IfsAurenaDefaultDeliveryTerms;
        result.CustomerOrderRequest.PayTermId = shipTo?.TermsCode;

        parameter.JobLogger?.Debug($"{nameof(CreateRequest)} Finished.");

        return result;
    }
}
