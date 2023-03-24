namespace Insite.Integration.Connector.Facts.Services;

using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.RealTimePricing.Services;
using Insite.RealTimePricing.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.FACTS))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.FACTS)]
public sealed class ExternalPricingServiceFacts : ExternalPricingServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public ExternalPricingServiceFacts(
        IUnitOfWorkFactory unitOfWorkFactory,
        RealTimePricingSettings realTimePricingSettings,
        IPipeAssemblyFactory pipeAssemblyFactory
    )
        : base(unitOfWorkFactory, realTimePricingSettings)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public override PricingServiceResult GetPrice(PricingServiceParameter productPriceParameter)
    {
        var pricingServiceResults = this.GetPrice(
            new List<PricingServiceParameter> { productPriceParameter }
        );

        return pricingServiceResults.First().Value;
    }

    public override IDictionary<PricingServiceParameter, PricingServiceResult> GetPrice(
        ICollection<PricingServiceParameter> productPriceParameter
    )
    {
        var priceAvailabilityParameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = productPriceParameter,
            IntegrationConnection = this.GetIntegrationConnection()
        };

        var priceAvailabilityResult = this.pipeAssemblyFactory.ExecutePipeline(
            priceAvailabilityParameter,
            new PriceAvailabilityResult()
        );

        return priceAvailabilityResult.PricingServiceResults;
    }
}
