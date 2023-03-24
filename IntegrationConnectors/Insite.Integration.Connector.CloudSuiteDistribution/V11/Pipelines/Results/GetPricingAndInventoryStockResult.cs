namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

using System.Collections.Generic;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4;

public class GetPricingAndInventoryStockResult : PipeResultBase
{
    public OePricingMultipleV4Request OePricingMultipleV4Request { get; set; }

    public OePricingMultipleV4Response OePricingMultipleV4Response { get; set; }

    public string SerializedOePricingMultipleV4Request { get; set; }

    public string SerializedOePricingMultipleV4Response { get; set; }

    public IDictionary<
        PricingServiceParameter,
        PricingServiceResult
    > PricingServiceResults { get; set; } =
        new Dictionary<PricingServiceParameter, PricingServiceResult>();

    public GetInventoryResult GetInventoryResult { get; set; }
}
