namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddShipToToRequest : IPipe<GetCartSummaryParameter, GetCartSummaryResult>
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddShipToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 400;

    public GetCartSummaryResult Execute(
        IUnitOfWork unitOfWork,
        GetCartSummaryParameter parameter,
        GetCartSummaryResult result
    )
    {
        result.GetCartSummaryRequest.Request.ShipToID = this.GetShipToErpSequence(
            unitOfWork,
            parameter.CustomerOrder
        );

        result.GetCartSummaryRequest.Request.CustomerShipTo = new RequestCustomerShipTo
        {
            ShipToAddress = new RequestShipToAddress
            {
                ShipToZip = parameter.CustomerOrder.STPostalCode
            }
        };

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
}
