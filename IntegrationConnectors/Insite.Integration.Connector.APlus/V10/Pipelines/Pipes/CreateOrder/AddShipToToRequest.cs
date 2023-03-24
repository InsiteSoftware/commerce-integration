namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;

using System;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class AddShipToToRequest : IPipe<CreateOrderParameter, CreateOrderResult>
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddShipToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 400;

    public CreateOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateOrderParameter parameter,
        CreateOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Started.");

        var requestOrder = result.CreateOrderRequest.Orders[0];
        var shipToErpSequence = this.GetShipToErpSequence(unitOfWork, parameter.CustomerOrder);

        requestOrder.OrderHeader.ShipToNumber = shipToErpSequence;

        if (
            parameter.CustomerOrder.FulfillmentMethod.EqualsIgnoreCase(
                FulfillmentMethod.PickUp.ToString()
            ) || parameter.CustomerOrder.ShipTo.IsDropShip
        )
        {
            var shipToState = this.GetShipToState(unitOfWork, parameter.CustomerOrder);
            var shipToCountry = this.GetShipToCountry(unitOfWork, parameter.CustomerOrder);

            requestOrder.OrderHeader.ShipToAddr1 = parameter.CustomerOrder.STAddress1;
            requestOrder.OrderHeader.ShipToAddr2 = parameter.CustomerOrder.STAddress2;
            requestOrder.OrderHeader.ShipToAddr3 = parameter.CustomerOrder.STAddress3;
            requestOrder.OrderHeader.ShipToAddr4 = parameter.CustomerOrder.STAddress4;
            requestOrder.OrderHeader.ShipToCity = parameter.CustomerOrder.STCity;
            requestOrder.OrderHeader.ShipToState = shipToState;
            requestOrder.OrderHeader.ShipToCountry = shipToCountry;
            requestOrder.OrderHeader.ShipToZip = parameter.CustomerOrder.STPostalCode;
            requestOrder.OrderHeader.ShipToName = parameter.CustomerOrder.STCompanyName;
            requestOrder.OrderHeader.ShipToPhone = parameter.CustomerOrder.STPhone;
        }

        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Finished.");

        return result;
    }

    private string GetShipToErpSequence(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var erpSequence = customerOrder.ShipTo.ErpSequence;
        if (string.IsNullOrEmpty(erpSequence))
        {
            erpSequence = this.GetGuestCustomer(unitOfWork)?.ErpSequence ?? string.Empty;
        }

        return erpSequence;
    }

    private Customer GetGuestCustomer(IUnitOfWork unitOfWork)
    {
        return this.customerDefaultsSettings.GuestErpCustomerId != Guid.Empty
            ? unitOfWork
                .GetRepository<Customer>()
                .Get(this.customerDefaultsSettings.GuestErpCustomerId)
            : null;
    }

    private string GetShipToState(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var shipToState = unitOfWork
            .GetTypedRepository<IStateRepository>()
            .GetStateByName(customerOrder.STState);
        return shipToState != null ? shipToState.Abbreviation : customerOrder.STState;
    }

    private string GetShipToCountry(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var shipToCountry = unitOfWork
            .GetTypedRepository<ICountryRepository>()
            .GetCountryByName(customerOrder.STCountry);
        return shipToCountry != null ? shipToCountry.Abbreviation : customerOrder.STCountry;
    }
}
