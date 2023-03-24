namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;

public sealed class AddBillToToRequest
    : IPipe<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
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

    public int Order => 600;

    public SFOEOrderTotLoadV2Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV2Parameter parameter,
        SFOEOrderTotLoadV2Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddBillToToRequest)} Started.");

        result.SFOEOrderTotLoadV2Request.arrayInheader[0].customerID = this.GetCustomerId(
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
