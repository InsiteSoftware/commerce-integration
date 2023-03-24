namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

using System.Collections.Generic;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;

public class GetPricingResult : PipeResultBase
{
    public List<PriceQuery> PriceQueryRequests { get; set; } = new List<PriceQuery>();

    public List<PriceQuery> PriceQueryResponses { get; set; } = new List<PriceQuery>();

    public List<string> SerializedPriceQueryRequests { get; set; } = new List<string>();

    public List<string> SerializedPriceQueryResponses { get; set; } = new List<string>();

    public IDictionary<
        PricingServiceParameter,
        PricingServiceResult
    > PricingServiceResults { get; set; } =
        new Dictionary<PricingServiceParameter, PricingServiceResult>();
}
