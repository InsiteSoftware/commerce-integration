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

public sealed class AddBillToToRequest : IPipe<SalesOrderParameter, SalesOrderResult>
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddBillToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 400;

    public SalesOrderResult Execute(
        IUnitOfWork unitOfWork,
        SalesOrderParameter parameter,
        SalesOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddBillToToRequest)} Started.");

        result.SalesOrderRequest.CustomerID = this.GetCustomerErpNumber(
            unitOfWork,
            parameter.CustomerOrder
        );

        result.SalesOrderRequest.BillToAddress = new Billtoaddress
        {
            AddressLine1 = parameter.CustomerOrder.BTAddress1,
            AddressLine2 = parameter.CustomerOrder.BTAddress2,
            City = parameter.CustomerOrder.BTCity,
            Country = GetBillToCountry(unitOfWork, parameter.CustomerOrder),
            PostalCode = parameter.CustomerOrder.BTPostalCode,
            State = GetBillToState(unitOfWork, parameter.CustomerOrder)
        };

        parameter.JobLogger?.Debug($"{nameof(AddBillToToRequest)} Finished.");

        return result;
    }

    private string GetCustomerErpNumber(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var erpNumber = customerOrder.Customer?.ErpNumber;
        if (string.IsNullOrEmpty(erpNumber))
        {
            erpNumber = this.GetGuestCustomer(unitOfWork)?.ErpNumber ?? string.Empty;
        }

        return erpNumber;
    }

    private Customer GetGuestCustomer(IUnitOfWork unitOfWork)
    {
        return this.customerDefaultsSettings.GuestErpCustomerId != Guid.Empty
            ? unitOfWork
                .GetRepository<Customer>()
                .Get(this.customerDefaultsSettings.GuestErpCustomerId)
            : null;
    }

    private static string GetBillToState(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var billToState = unitOfWork
            .GetTypedRepository<IStateRepository>()
            .GetStateByName(customerOrder.BTState);
        return billToState != null ? billToState.Abbreviation : customerOrder.BTState;
    }

    private static string GetBillToCountry(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var billToCountry = string.Empty;

        if (!customerOrder.BTCountry.IsBlank())
        {
            billToCountry = unitOfWork
                .GetTypedRepository<ICountryRepository>()
                .GetCountryByName(customerOrder.BTCountry)
                ?.Abbreviation;
        }

        if (billToCountry.IsBlank())
        {
            billToCountry = customerOrder.BTCountry;
        }

        if (billToCountry.IsBlank())
        {
            billToCountry = "US";
        }

        return billToCountry;
    }
}
