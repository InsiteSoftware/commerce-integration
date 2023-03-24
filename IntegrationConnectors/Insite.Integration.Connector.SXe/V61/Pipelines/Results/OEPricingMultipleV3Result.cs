namespace Insite.Integration.Connector.SXe.V61.Pipelines.Results;

using System.Collections.Generic;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;

public class OEPricingMultipleV3Result : PipeResultBase
{
    public OEPricingMultipleV3Request OEPricingMultipleV3Request { get; set; }

    public OEPricingMultipleV3Response OEPricingMultipleV3Response { get; set; }

    public IDictionary<
        PricingServiceParameter,
        PricingServiceResult
    > PricingServiceResults { get; set; } =
        new Dictionary<PricingServiceParameter, PricingServiceResult>();

    public GetInventoryResult GetInventoryResult { get; set; }
}
