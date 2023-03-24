namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

using System.Collections.Generic;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;

public class GetCustomerPriceResult : PipeResultBase
{
    public customerPriceRequest CustomerPriceRequest { get; set; }

    public customerPriceResponse CustomerPriceResponse { get; set; }

    public IDictionary<
        PricingServiceParameter,
        PricingServiceResult
    > PricingServiceResults { get; set; } =
        new Dictionary<PricingServiceParameter, PricingServiceResult>();
}
