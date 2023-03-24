namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;

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

public sealed class AddShipToToRequest : IPipe<OrderTotalParameter, OrderTotalResult>
{
    private const string OneTimeAddressPrefix = "OT-";

    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddShipToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 400;

    public OrderTotalResult Execute(
        IUnitOfWork unitOfWork,
        OrderTotalParameter parameter,
        OrderTotalResult result
    )
    {
        var orderHeader = result.OrderTotalRequest.Request.Orders.First().OrderHeader;

        orderHeader.ShipToAddress1 = parameter.CustomerOrder.STAddress1;
        orderHeader.ShipToAddress2 = parameter.CustomerOrder.STAddress2;
        orderHeader.ShipToAddress3 = parameter.CustomerOrder.STAddress3;
        orderHeader.ShipToAddress4 = parameter.CustomerOrder.STAddress4;
        orderHeader.ShipToCity = parameter.CustomerOrder.STCity;
        orderHeader.ShipToZipCode = parameter.CustomerOrder.STPostalCode;
        orderHeader.ShipToState = this.GetShipToState(unitOfWork, parameter.CustomerOrder);
        orderHeader.ShipToCountry = this.GetShipToCountry(unitOfWork, parameter.CustomerOrder);

        if (!parameter.CustomerOrder.ShipTo.ErpSequence.StartsWithIgnoreCase(OneTimeAddressPrefix))
        {
            orderHeader.ShipToNumber = this.GetShipToErpSequence(
                unitOfWork,
                parameter.CustomerOrder
            );
        }

        if (
            parameter.CustomerOrder.FulfillmentMethod.EqualsIgnoreCase(
                FulfillmentMethod.PickUp.ToString()
            )
        )
        {
            orderHeader.Address1 = parameter.CustomerOrder.STAddress1;
            orderHeader.Address2 = parameter.CustomerOrder.STAddress2;
            orderHeader.Address3 = parameter.CustomerOrder.STAddress3;
            orderHeader.Address4 = parameter.CustomerOrder.STAddress4;
        }

        return result;
    }

    private string GetShipToErpSequence(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
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
