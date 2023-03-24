namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;

using System;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class AddBillToToRequest : IPipe<CreateOrderParameter, CreateOrderResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddBillToToRequest(
        IntegrationConnectorSettings integrationConnectorSettings,
        CustomerDefaultsSettings customerDefaultsSettings
    )
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 300;

    public CreateOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateOrderParameter parameter,
        CreateOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddBillToToRequest)} Started.");

        var customerId = this.GetCustomerId(unitOfWork, parameter);

        result.CreateOrderRequest.Orders[0].OrderHeader.CustomerID = customerId;

        parameter.JobLogger?.Debug($"{nameof(AddBillToToRequest)} Finished.");

        return result;
    }

    private string GetCustomerId(IUnitOfWork unitOfWork, CreateOrderParameter parameter)
    {
        var companyNumber = this.GetCompanyNumber();
        var erpNumber = this.GetCustomerErpNumber(unitOfWork, parameter);

        return companyNumber.PadLeft(2, '0') + erpNumber.PadLeft(10, '0');
    }

    private string GetCompanyNumber()
    {
        var companyNumbers = this.integrationConnectorSettings.APlusCompany.Split(
            new[] { ',', ';', '|' },
            StringSplitOptions.RemoveEmptyEntries
        );

        return companyNumbers.Any() ? companyNumbers[0] : "1";
    }

    private string GetCustomerErpNumber(IUnitOfWork unitOfWork, CreateOrderParameter parameter)
    {
        var erpNumber = parameter.CustomerOrder.Customer.ErpNumber;
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
}
