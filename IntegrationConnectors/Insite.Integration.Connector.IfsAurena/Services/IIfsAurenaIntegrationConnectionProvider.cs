namespace Insite.Integration.Connector.IfsAurena.Services;

using Insite.Core.Interfaces.Dependency;
using Insite.Data.Entities;

internal interface IIfsAurenaIntegrationConnectionProvider : IDependency
{
    IntegrationConnection GetIntegrationConnection(IntegrationConnection integrationConnection);
}
