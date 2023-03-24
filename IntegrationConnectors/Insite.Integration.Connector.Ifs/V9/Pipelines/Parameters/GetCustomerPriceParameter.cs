namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;

using System.Collections.Generic;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;

public class GetCustomerPriceParameter : PipeParameterBase
{
    public ICollection<PricingServiceParameter> PricingServiceParameters { get; set; } =
        new List<PricingServiceParameter>();

    public IntegrationConnection IntegrationConnection { get; set; }
}
