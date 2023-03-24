namespace Insite.Integration.Connector.CloudSuiteDistribution.Services;

using Insite.Core.Interfaces.Dependency;
using Insite.Data.Entities;

internal interface ICloudSuiteDistributionIntegrationConnectionProvider : IDependency
{
    IntegrationConnection GetIntegrationConnection(IntegrationConnection integrationConnection);
}
