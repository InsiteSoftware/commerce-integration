namespace Insite.Integration.Connector.Prophet21.Services;

using Insite.Core.Interfaces.Dependency;
using Insite.Data.Entities;

internal interface IProphet21IntegrationConnectionProvider : IDependency
{
    IntegrationConnection GetIntegrationConnection(IntegrationConnection integrationConnection);
}
