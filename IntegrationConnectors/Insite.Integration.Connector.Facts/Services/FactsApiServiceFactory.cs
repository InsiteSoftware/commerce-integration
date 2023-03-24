namespace Insite.Integration.Connector.Facts.Services;

using System;
using Insite.Common.Dependencies;
using Insite.Common.Helpers;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;

internal sealed class FactsApiServiceFactory : IFactsApiServiceFactory, ISingletonLifetime
{
    private readonly IUnitOfWorkFactory unitOfWorkFactory;

    private readonly IDependencyLocator dependencyLocator;

    public FactsApiServiceFactory(
        IUnitOfWorkFactory unitOfWorkFactory,
        IDependencyLocator dependencyLocator
    )
    {
        this.unitOfWorkFactory = unitOfWorkFactory;
        this.dependencyLocator = dependencyLocator;
    }

    public IFactsApiService GetFactsApiService(IntegrationConnection integrationConnection = null)
    {
        integrationConnection = this.GetIntegrationConnection(integrationConnection);

#pragma warning disable CS0618 // Type or member is obsolete
        var password = EncryptionHelper.DecryptAes(integrationConnection.Password);
#pragma warning restore

        return new FactsApiService(
            integrationConnection.Url,
            integrationConnection.LogOn,
            password
        );
    }

    private IntegrationConnection GetIntegrationConnection(
        IntegrationConnection integrationConnection
    )
    {
        if (integrationConnection != null)
        {
            return integrationConnection;
        }

        var factsIntegrationConnectionId = this.dependencyLocator
            .GetInstance<IntegrationConnectorSettings>()
            .FactsIntegrationConnectionId;
        if (factsIntegrationConnectionId.IsBlank())
        {
            throw new ArgumentException(
                "FACTS Integration Connector requires an Integration Connection."
            );
        }

        integrationConnection = this.unitOfWorkFactory
            .GetUnitOfWork()
            .GetRepository<IntegrationConnection>()
            .Get(factsIntegrationConnectionId);
        if (integrationConnection == null)
        {
            throw new ArgumentException(
                "FACTS Integration Connector requires an Integration Connection."
            );
        }

        return integrationConnection;
    }
}
