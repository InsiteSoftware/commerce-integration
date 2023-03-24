namespace Insite.Integration.Connector.Facts.V9.Pipelines.Results;

using System.Collections.Generic;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;

public class PriceAvailabilityResult : PipeResultBase
{
    public PriceAvailabilityRequest PriceAvailabilityRequest { get; set; }

    public PriceAvailabilityResponse PriceAvailabilityResponse { get; set; }

    public IDictionary<
        PricingServiceParameter,
        PricingServiceResult
    > PricingServiceResults { get; set; } =
        new Dictionary<PricingServiceParameter, PricingServiceResult>();

    public GetInventoryResult GetInventoryResult { get; set; }
}
