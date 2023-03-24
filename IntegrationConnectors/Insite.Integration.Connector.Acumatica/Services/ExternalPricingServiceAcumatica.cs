namespace Insite.Integration.Connector.Acumatica.Services;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Caching;
using Insite.Core.Plugins.EntityUtilities;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.RealTimePricing.Services;
using Insite.RealTimePricing.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.Acumatica))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.Acumatica)]
public sealed class ExternalPricingServiceAcumatica : PriceMatrixExternalPricingServiceBase
{
    public ExternalPricingServiceAcumatica(
        IUnitOfWorkFactory unitOfWorkFactory,
        RealTimePricingSettings realTimePricingSettings,
        Lazy<IPerRequestCacheManager> perRequestCacheManager,
        Lazy<IPriceMatrixUtilities> priceMatrixUtilities,
        PricingServiceAcumatica pricingServiceAcumatica
    )
        : base(
            unitOfWorkFactory,
            realTimePricingSettings,
            perRequestCacheManager,
            priceMatrixUtilities
        )
    {
        this.PricingService = pricingServiceAcumatica;
    }
}
