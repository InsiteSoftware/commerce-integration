namespace Insite.Integration.Connector.IfsAurena.Services;

using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Insite.RealTimePricing.Services;
using Insite.RealTimePricing.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.IFSAurena))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.IFSAurena)]
public sealed class ExternalPricingServiceIfsAurena : ExternalPricingServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public ExternalPricingServiceIfsAurena(
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

        return pricingServiceResults.FirstOrDefault().Value;
    }

    public override IDictionary<PricingServiceParameter, PricingServiceResult> GetPrice(
        ICollection<PricingServiceParameter> productPriceParameter
    )
    {
        var getPricingParameter = new GetPricingParameter
        {
            PricingServiceParameters = productPriceParameter,
            IntegrationConnection = this.GetIntegrationConnection()
        };

        var getPricingResult = this.pipeAssemblyFactory.ExecutePipeline(
            getPricingParameter,
            new GetPricingResult()
        );

        return getPricingResult.PricingServiceResults;
    }
}
