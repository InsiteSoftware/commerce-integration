namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetPartAvailability;

using System;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddShipToToRequest
    : IPipe<GetPartAvailabilityParameter, GetPartAvailabilityResult>
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddShipToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 400;

    public GetPartAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        GetPartAvailabilityParameter parameter,
        GetPartAvailabilityResult result
    )
    {
        var erpSequence = this.GetErpSequence(unitOfWork);
        if (string.IsNullOrEmpty(erpSequence))
        {
            // no default pricing customer and not yet submitted to erp
            return result;
        }

        result.PartAvailabilityRequest.addressId = erpSequence;

        return result;
    }

    private string GetErpSequence(IUnitOfWork unitOfWork)
    {
        var erpSequence = SiteContext.Current.ShipTo?.ErpSequence;

        if (string.IsNullOrEmpty(erpSequence))
        {
            erpSequence = this.GetGuestErpSequence(unitOfWork);
        }

        return erpSequence;
    }

    private string GetGuestErpSequence(IUnitOfWork unitOfWork)
    {
        var guestErpCustomerId = this.customerDefaultsSettings.GuestErpCustomerId;
        if (guestErpCustomerId == Guid.Empty)
        {
            return string.Empty;
        }

        return unitOfWork.GetRepository<Customer>().Get(guestErpCustomerId)?.ErpSequence
            ?? string.Empty;
    }
}
