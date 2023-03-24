namespace Insite.Integration.Connector.CloudSuiteDistribution.Services;

using System.Collections.Generic;
using System.Linq;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using Insite.RealTimePricing.Services;
using Insite.RealTimePricing.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.CloudSuiteDistribution))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.CloudSuiteDistribution)]
public sealed class ExternalPricingServiceCloudSuiteDistribution : ExternalPricingServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    private readonly ICloudSuiteDistributionIntegrationConnectionProvider cloudSuiteDistributionIntegrationConnectionProvider;

    public ExternalPricingServiceCloudSuiteDistribution(
        IUnitOfWorkFactory unitOfWorkFactory,
        RealTimePricingSettings realTimePricingSettings,
        IPipeAssemblyFactory pipeAssemblyFactory,
        IDependencyLocator dependencyLocator
    )
        : base(unitOfWorkFactory, realTimePricingSettings)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
        this.cloudSuiteDistributionIntegrationConnectionProvider =
            dependencyLocator.GetInstance<ICloudSuiteDistributionIntegrationConnectionProvider>();
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
        var integrationConnection =
            this.cloudSuiteDistributionIntegrationConnectionProvider.GetIntegrationConnection(
                this.GetIntegrationConnection()
            );

        var getPricingAndInventoryStockParameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = productPriceParameter,
            IntegrationConnection = integrationConnection
        };

        var getPricingAndInventoryStockResult = this.pipeAssemblyFactory.ExecutePipeline(
            getPricingAndInventoryStockParameter,
            new GetPricingAndInventoryStockResult()
        );

        return getPricingAndInventoryStockResult.PricingServiceResults;
    }
}
