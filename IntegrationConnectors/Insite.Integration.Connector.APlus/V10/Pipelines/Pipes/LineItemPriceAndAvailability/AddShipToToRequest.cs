namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;

using System;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class AddShipToToRequest
    : IPipe<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    public int Order => 400;

    public LineItemPriceAndAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        LineItemPriceAndAvailabilityParameter parameter,
        LineItemPriceAndAvailabilityResult result
    )
    {
        var shipToId = GetShipToId(parameter);
        if (shipToId == null)
        {
            return result;
        }

        var shipTo = unitOfWork.GetRepository<Customer>().Get(shipToId.Value);
        if (shipTo == null)
        {
            throw new ArgumentException($"No customer found with id {shipToId.Value}.");
        }

        var erpSequence = shipTo.PricingCustomer?.ErpSequence ?? shipTo.ErpSequence;
        if (string.IsNullOrEmpty(erpSequence))
        {
            // no default pricing customer and not yet submitted to erp
            return result;
        }

        foreach (var item in result.LineItemPriceAndAvailabilityRequest.Items)
        {
            item.ShipToNumber = erpSequence;
        }

        return result;
    }

    private static Guid? GetShipToId(LineItemPriceAndAvailabilityParameter parameter)
    {
        return parameter.PricingServiceParameters?.FirstOrDefault()?.ShipToId
            ?? SiteContext.Current.ShipTo?.Id;
    }
}
