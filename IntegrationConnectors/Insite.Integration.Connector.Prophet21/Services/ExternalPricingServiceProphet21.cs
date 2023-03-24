namespace Insite.Integration.Connector.Prophet21.Services;

using System.Collections.Generic;
using System.Linq;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.RealTimePricing.Services;
using Insite.RealTimePricing.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.Prophet21))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.Prophet21)]
public sealed class ExternalPricingServiceProphet21 : ExternalPricingServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    private readonly IProphet21IntegrationConnectionProvider prophet21IntegrationConnectionProvider;

    public ExternalPricingServiceProphet21(
        IUnitOfWorkFactory unitOfWorkFactory,
        RealTimePricingSettings realTimePricingSettings,
        IPipeAssemblyFactory pipeAssemblyFactory,
        IDependencyLocator dependencyLocator
    )
        : base(unitOfWorkFactory, realTimePricingSettings)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
        this.prophet21IntegrationConnectionProvider =
            dependencyLocator.GetInstance<IProphet21IntegrationConnectionProvider>();
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
            this.prophet21IntegrationConnectionProvider.GetIntegrationConnection(
                this.GetIntegrationConnection()
            );

        var getItemPriceParameter = new GetItemPriceParameter
        {
            PricingServiceParameters = productPriceParameter,
            IntegrationConnection = integrationConnection
        };

        var getItemPriceResult = this.pipeAssemblyFactory.ExecutePipeline(
            getItemPriceParameter,
            new GetItemPriceResult()
        );

        return getItemPriceResult.PricingServiceResults;
    }
}
