namespace Insite.Integration.Connector.SXe.Services;

using System.Collections.Generic;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Inventory;
using Insite.Data.Entities;

internal interface IIntegrationConnectorServiceSXe : IMultiInstanceDependency
{
    void CalculateTax(CustomerOrder customerOrder);

    GetAgingBucketsResult GetAgingBuckets(GetAgingBucketsParameter getAgingBucketsParameter);

    GetInventoryResult GetInventory(
        GetInventoryParameter getInventoryParameter,
        IntegrationConnection integrationConnection
    );

    IDictionary<PricingServiceParameter, PricingServiceResult> GetPrice(
        ICollection<PricingServiceParameter> pricingServiceParameters,
        IntegrationConnection integrationConnection
    );
}
