namespace Insite.Integration.Connector.SXe.V10.Pipelines.Results;

using System.Collections.Generic;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.OEPricingMultipleV4;

public class OEPricingMultipleV4Result : PipeResultBase
{
    public OEPricingMultipleV4Request OEPricingMultipleV4Request { get; set; }

    public OEPricingMultipleV4Response OEPricingMultipleV4Response { get; set; }

    public IDictionary<
        PricingServiceParameter,
        PricingServiceResult
    > PricingServiceResults { get; set; } =
        new Dictionary<PricingServiceParameter, PricingServiceResult>();

    public GetInventoryResult GetInventoryResult { get; set; }
}
