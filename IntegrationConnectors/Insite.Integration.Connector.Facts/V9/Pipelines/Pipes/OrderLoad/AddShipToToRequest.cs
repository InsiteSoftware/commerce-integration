namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class AddShipToToRequest : IPipe<OrderLoadParameter, OrderLoadResult>
{
    private const string OneTimeAddressPrefix = "OT-";

    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddShipToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 500;

    public OrderLoadResult Execute(
        IUnitOfWork unitOfWork,
        OrderLoadParameter parameter,
        OrderLoadResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Started.");

        var orderHeader = result.OrderLoadRequest.Request.Orders.First().OrderHeader;

        orderHeader.ShipToNumber = this.GetShipToErpSequence(unitOfWork, parameter.CustomerOrder);
        orderHeader.ShipToName = this.GetShipToName(parameter.CustomerOrder);
        orderHeader.ShipToAddress1 = parameter.CustomerOrder.STAddress1;
        orderHeader.ShipToAddress2 = parameter.CustomerOrder.STAddress2;
        orderHeader.ShipToAddress3 = parameter.CustomerOrder.STAddress3;
        orderHeader.ShipToAddress4 = parameter.CustomerOrder.STAddress4;
        orderHeader.ShipToCity = parameter.CustomerOrder.STCity;
        orderHeader.ShipToZipCode = parameter.CustomerOrder.STPostalCode;
        orderHeader.ShipToState = GetShipToState(unitOfWork, parameter.CustomerOrder);
        orderHeader.ShipToCountry = GetShipToCountry(unitOfWork, parameter.CustomerOrder);

        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Finished.");

        return result;
    }

    private string GetShipToName(CustomerOrder customerOrder)
    {
        return customerOrder.FulfillmentMethod.EqualsIgnoreCase(FulfillmentMethod.PickUp.ToString())
            ? $"Pick-Up Location:{customerOrder.DefaultWarehouse.Name}"
            : customerOrder.STCompanyName;
    }

    private string GetShipToErpSequence(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        if (customerOrder.ShipTo.ErpSequence.StartsWithIgnoreCase(OneTimeAddressPrefix))
        {
            return "TEMP";
        }

        var erpSequence = customerOrder.ShipTo?.ErpSequence;
        if (string.IsNullOrEmpty(erpSequence))
        {
            if (customerOrder.ShipToId == customerOrder.CustomerId)
            {
                erpSequence = "SAME";
            }
            else
            {
                erpSequence = this.GetGuestCustomer(unitOfWork)?.ErpSequence ?? string.Empty;
            }
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
        var shipToCountry = unitOfWork
            .GetTypedRepository<ICountryRepository>()
            .GetCountryByName(customerOrder.STCountry);
        return shipToCountry != null ? shipToCountry.Abbreviation : customerOrder.STCountry;
    }
}
