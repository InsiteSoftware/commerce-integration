namespace Insite.Integration.Connector.Ifs.Services;

using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.RealTimePricing.Services;
using Insite.RealTimePricing.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.IFS))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.IFS)]
public sealed class ExternalPricingServiceIfs : ExternalPricingServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public ExternalPricingServiceIfs(
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
        var getCustomerPriceParameter = new GetCustomerPriceParameter
        {
            PricingServiceParameters = productPriceParameter,
            IntegrationConnection = this.GetIntegrationConnection()
        };

        var getCustomerPriceResult = this.pipeAssemblyFactory.ExecutePipeline(
            getCustomerPriceParameter,
            new GetCustomerPriceResult()
        );

        return getCustomerPriceResult.PricingServiceResults;
    }
}
