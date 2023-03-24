namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;

public sealed class AddBillToToRequest
    : IPipe<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
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

    public int Order => 700;

    public SFOEOrderTotLoadV4Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV4Parameter parameter,
        SFOEOrderTotLoadV4Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddBillToToRequest)} Started.");

        result.SFOEOrderTotLoadV4Request.Ininheader[0].CustomerID = this.GetCustomerId(
            unitOfWork,
            parameter.CustomerOrder
        );

        parameter.JobLogger?.Debug($"{nameof(AddBillToToRequest)} Finished.");

        return result;
    }

    private string GetCustomerId(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        return this.GetCompanyNumber().PadLeft(4, '0')
            + this.GetCustomerErpNumber(unitOfWork, customerOrder).PadLeft(12, '0');
    }

    private string GetCompanyNumber()
    {
        var companyNumbers = this.integrationConnectorSettings.SXeCompany.Split(
            new[] { ',', ';', '|' },
            StringSplitOptions.RemoveEmptyEntries
        );

        return companyNumbers.Any() ? companyNumbers[0] : "1";
    }

    private string GetCustomerErpNumber(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var erpNumber = customerOrder.Customer.ErpNumber;
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
