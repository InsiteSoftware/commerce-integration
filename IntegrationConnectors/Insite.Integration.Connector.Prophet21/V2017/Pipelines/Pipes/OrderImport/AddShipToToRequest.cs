namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddShipToToRequest : IPipe<OrderImportParameter, OrderImportResult>
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddShipToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 400;

    public OrderImportResult Execute(
        IUnitOfWork unitOfWork,
        OrderImportParameter parameter,
        OrderImportResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Started.");

        var shipToState = this.GetShipToState(unitOfWork, parameter.CustomerOrder);
        var shipToCountry = this.GetShipToCountry(unitOfWork, parameter.CustomerOrder);

        result.OrderImportRequest.Request.CustomerShipTo = new RequestCustomerShipTo
        {
            ShipToID = this.GetShipToErpSequence(unitOfWork, parameter.CustomerOrder),
            ShipToCarrierId = parameter.CustomerOrder.ShipVia?.ErpShipCode ?? string.Empty,
            ShipToContactFirstName = parameter.CustomerOrder.STFirstName,
            ShipToContactLastName = parameter.CustomerOrder.STLastName,
            ShipToPhone = parameter.CustomerOrder.STPhone,
            ShipToEMail = parameter.CustomerOrder.STEmail,
            ShipToAddress = new RequestShipToAddress
            {
                ShipToCompanyName = parameter.CustomerOrder.STCompanyName,
                ShipToAddress1 = parameter.CustomerOrder.STAddress1,
                ShipToAddress2 = parameter.CustomerOrder.STAddress2,
                ShipToAddress3 = parameter.CustomerOrder.STAddress3,
                ShipToCity = parameter.CustomerOrder.STCity,
                ShipToZip = parameter.CustomerOrder.STPostalCode,
                ShipToState = shipToState,
                ShipToCountry = shipToCountry
            }
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
