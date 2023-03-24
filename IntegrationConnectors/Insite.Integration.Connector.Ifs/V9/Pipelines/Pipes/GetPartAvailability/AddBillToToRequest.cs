namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetPartAvailability;

using System;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddBillToToRequest
    : IPipe<GetPartAvailabilityParameter, GetPartAvailabilityResult>
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddBillToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 300;

    public GetPartAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        GetPartAvailabilityParameter parameter,
        GetPartAvailabilityResult result
    )
    {
        var erpNumber = this.GetErpNumber(unitOfWork);
        if (string.IsNullOrEmpty(erpNumber))
        {
            // no default pricing customer and not yet submitted to erp
            return result;
        }

        result.PartAvailabilityRequest.customerNo = erpNumber;

        return result;
    }

    private string GetErpNumber(IUnitOfWork unitOfWork)
    {
        var erpNumber = SiteContext.Current.BillTo?.ErpNumber;

        if (string.IsNullOrEmpty(erpNumber))
        {
            erpNumber = this.GetGuestErpNumber(unitOfWork);
        }

        return erpNumber;
    }

    private string GetGuestErpNumber(IUnitOfWork unitOfWork)
    {
        var guestErpCustomerId = this.customerDefaultsSettings.GuestErpCustomerId;
        if (guestErpCustomerId == Guid.Empty)
        {
            return string.Empty;
        }

        return unitOfWork.GetRepository<Customer>().Get(guestErpCustomerId)?.ErpNumber
            ?? string.Empty;
    }
}
