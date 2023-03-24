namespace Insite.Integration.Connector.APlus.V10.Pipelines.Results;

using System.Collections.Generic;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;

public class LineItemPriceAndAvailabilityResult : PipeResultBase
{
    public LineItemPriceAndAvailabilityRequest LineItemPriceAndAvailabilityRequest { get; set; }

    public LineItemPriceAndAvailabilityResponse LineItemPriceAndAvailabilityResponse { get; set; }

    public IDictionary<
        PricingServiceParameter,
        PricingServiceResult
    > PricingServiceResults { get; set; } =
        new Dictionary<PricingServiceParameter, PricingServiceResult>();

    public GetInventoryResult GetInventoryResult { get; set; }
}
