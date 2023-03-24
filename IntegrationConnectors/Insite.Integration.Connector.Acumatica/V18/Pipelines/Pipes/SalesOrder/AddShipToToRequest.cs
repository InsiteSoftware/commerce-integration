namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;

public sealed class AddShipToToRequest : IPipe<SalesOrderParameter, SalesOrderResult>
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddShipToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 500;

    public SalesOrderResult Execute(
        IUnitOfWork unitOfWork,
        SalesOrderParameter parameter,
        SalesOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Started.");

        result.SalesOrderRequest.ShipToAddressOverride =
            parameter.CustomerOrder.ShipTo?.IsDropShip ?? false;
        result.SalesOrderRequest.LocationID = this.GetShipToErpSequence(
            unitOfWork,
            parameter.CustomerOrder
        );

        result.SalesOrderRequest.ShipToAddress = new Shiptoaddress
        {
            AddressLine1 = parameter.CustomerOrder.STAddress1,
            AddressLine2 = parameter.CustomerOrder.STAddress2,
            City = parameter.CustomerOrder.STCity,
            Country = GetShipToCountry(unitOfWork, parameter.CustomerOrder),
            PostalCode = parameter.CustomerOrder.STPostalCode,
            State = GetShipToState(unitOfWork, parameter.CustomerOrder)
        };

        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Finished.");

        return result;
    }

    private string GetShipToErpSequence(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var erpSequence = customerOrder.ShipTo?.ErpSequence;
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

    private static string GetShipToState(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var shipToState = unitOfWork
            .GetTypedRepository<IStateRepository>()
            .GetStateByName(customerOrder.STState);
        return shipToState != null ? shipToState.Abbreviation : customerOrder.STState;
    }

    private static string GetShipToCountry(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var shipToCountry = string.Empty;

        if (!customerOrder.STCountry.IsBlank())
        {
            shipToCountry = unitOfWork
                .GetTypedRepository<ICountryRepository>()
                .GetCountryByName(customerOrder.STCountry)
                ?.Abbreviation;
        }

        if (shipToCountry.IsBlank())
        {
            shipToCountry = customerOrder.STCountry;
        }

        if (shipToCountry.IsBlank())
        {
            shipToCountry = "US";
        }

        return shipToCountry;
    }
}
