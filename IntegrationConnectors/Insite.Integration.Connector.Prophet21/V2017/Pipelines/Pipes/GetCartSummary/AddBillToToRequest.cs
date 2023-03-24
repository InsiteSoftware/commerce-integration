namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddBillToToRequest : IPipe<GetCartSummaryParameter, GetCartSummaryResult>
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddBillToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 300;

    public GetCartSummaryResult Execute(
        IUnitOfWork unitOfWork,
        GetCartSummaryParameter parameter,
        GetCartSummaryResult result
    )
    {
        result.GetCartSummaryRequest.Request.CustomerCode = this.GetCustomerErpNumber(
            unitOfWork,
            parameter.CustomerOrder
        );

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
}
