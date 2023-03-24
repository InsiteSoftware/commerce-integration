namespace Insite.Integration.Connector.CloudSuiteDistribution.Services;

using Insite.Core.Interfaces.Dependency;
using Insite.Core.Services;
using Insite.Data.Entities;

internal interface ICloudSuiteDistributionClient : IDependency
{
    (ResultCode resultCode, string response) GetAccountsReceivable(
        IntegrationConnection integrationConnection,
        string request
    );

    (ResultCode resultCode, string response) GetPricingAndInventoryStock(
        IntegrationConnection integrationConnection,
        string request
    );
}
