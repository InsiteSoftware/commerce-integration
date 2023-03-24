namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class AddShipToToRequest : IPipe<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    private const string DefaultShipToNumber = "SAME";

    public int Order => 500;

    public PriceAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        PriceAvailabilityParameter parameter,
        PriceAvailabilityResult result
    )
    {
        var shipToId = parameter.PricingServiceParameters.FirstOrDefault()?.ShipToId;
        if (shipToId == null)
        {
            return result;
        }

        var shipTo = unitOfWork.GetRepository<Customer>().Get(shipToId.Value);
        if (shipTo == null)
        {
            throw new ArgumentException($"Customer not found. Id: {shipToId.Value}.");
        }

        var erpSequence = shipTo.PricingCustomer?.ErpSequence ?? shipTo.ErpSequence;
        if (string.IsNullOrEmpty(erpSequence))
        {
            erpSequence = DefaultShipToNumber;
        }

        result.PriceAvailabilityRequest.Request.Items.ForEach(o => o.ShipToNumber = erpSequence);

        return result;
    }
}
