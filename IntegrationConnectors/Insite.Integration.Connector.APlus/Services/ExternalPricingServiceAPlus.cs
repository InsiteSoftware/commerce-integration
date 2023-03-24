namespace Insite.Integration.Connector.APlus.Services;

using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.RealTimePricing.Services;
using Insite.RealTimePricing.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.APlus))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.APlus)]
public sealed class ExternalPricingServiceAPlus : ExternalPricingServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public ExternalPricingServiceAPlus(
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
        var getLineItemPriceAndAvailabilityParameter = new LineItemPriceAndAvailabilityParameter
        {
            PricingServiceParameters = productPriceParameter,
            IntegrationConnection = this.GetIntegrationConnection()
        };

        var getLineItemPriceAndAvailabilityResult = this.pipeAssemblyFactory.ExecutePipeline(
            getLineItemPriceAndAvailabilityParameter,
            new LineItemPriceAndAvailabilityResult()
        );

        return getLineItemPriceAndAvailabilityResult.PricingServiceResults;
    }
}
