namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddShipToToRequest
    : IPipe<CreateCustomerOrderParameter, CreateCustomerOrderResult>
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddShipToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 400;

    public CreateCustomerOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateCustomerOrderParameter parameter,
        CreateCustomerOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Started.");

        if (
            parameter.CustomerOrder.FulfillmentMethod.EqualsIgnoreCase(
                FulfillmentMethod.PickUp.ToString()
            ) || (parameter.CustomerOrder.ShipTo?.IsDropShip ?? false)
        )
        {
            result.CustomerOrder.addrFlagDb = "Y";

            result.CustomerOrder.deliveryAddress = new address
            {
                addr1 = parameter.CustomerOrder.STCompanyName.IsNotBlank()
                    ? parameter.CustomerOrder.STCompanyName
                    : parameter.CustomerOrder.STAddress1,
                address1 = parameter.CustomerOrder.STAddress1,
                address2 = parameter.CustomerOrder.STAddress2,
                city = parameter.CustomerOrder.STCity,
                state = GetShipToState(unitOfWork, parameter.CustomerOrder),
                countryCode = GetShipToCountry(unitOfWork, parameter.CustomerOrder),
                zipCode = parameter.CustomerOrder.STPostalCode
            };
        }
        else
        {
            result.CustomerOrder.shipAddrNo = this.GetShipToErpSequence(
                unitOfWork,
                parameter.CustomerOrder
            );
        }

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
