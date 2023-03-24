namespace Insite.Integration.Connector.SXe.Services;

using System.Collections.Generic;
using System.Linq;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.RealTimePricing.Services;
using Insite.RealTimePricing.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.SXe))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.SXe)]
public sealed class ExternalPricingServiceSXe : ExternalPricingServiceBase
{
    private readonly IDependencyLocator dependencyLocator;

    public ExternalPricingServiceSXe(
        IUnitOfWorkFactory unitOfWorkFactory,
        RealTimePricingSettings realTimePricingSettings,
        IDependencyLocator dependencyLocator
    )
        : base(unitOfWorkFactory, realTimePricingSettings)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public override PricingServiceResult GetPrice(PricingServiceParameter productPriceParameter)
    {
        return this.GetPrice(new List<PricingServiceParameter> { productPriceParameter })
            .FirstOrDefault()
            .Value;
    }

    public override IDictionary<PricingServiceParameter, PricingServiceResult> GetPrice(
        ICollection<PricingServiceParameter> productPriceParameter
    )
    {
        return this.dependencyLocator
            .GetInstance<IIntegrationConnectorServiceSXeFactory>()
            .GetIntegrationConnectorServiceSXe()
            .GetPrice(productPriceParameter, this.GetIntegrationConnection());
    }
}
