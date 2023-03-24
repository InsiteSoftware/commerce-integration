﻿namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;

using System.Collections.Generic;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;

public class GetPricingAndInventoryStockParameter : PipeParameterBase
{
    public ICollection<PricingServiceParameter> PricingServiceParameters { get; set; } =
        new List<PricingServiceParameter>();

    public GetInventoryParameter GetInventoryParameter { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }
}
