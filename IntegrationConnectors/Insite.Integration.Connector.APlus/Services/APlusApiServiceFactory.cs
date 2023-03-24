namespace Insite.Integration.Connector.APlus.Services;

using System;
using Insite.Common.Dependencies;
using Insite.Common.Helpers;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;

internal sealed class APlusApiServiceFactory : IAPlusApiServiceFactory, ISingletonLifetime
{
    private readonly IUnitOfWorkFactory unitOfWorkFactory;

    private readonly IDependencyLocator dependencyLocator;

    public APlusApiServiceFactory(
        IUnitOfWorkFactory unitOfWorkFactory,
        IDependencyLocator dependencyLocator
    )
    {
        this.unitOfWorkFactory = unitOfWorkFactory;
        this.dependencyLocator = dependencyLocator;
    }

    /// <summary>Gets the IAPlusApiService.</summary>
    /// <returns>The <see cref="IAPlusApiService"/>.</returns>
    /// <param name="integrationConnection">The integration connection used to initialize the <see cref="IAPlusApiService"/>.</param>
    public IAPlusApiService GetAPlusApiService(IntegrationConnection integrationConnection = null)
    {
        integrationConnection = this.GetIntegrationConnection(integrationConnection);

#pragma warning disable CS0618 // Type or member is obsolete
        var password = EncryptionHelper.DecryptAes(integrationConnection.Password);
#pragma warning restore

        return new APlusApiService(
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

        var aPlusIntegrationConnectionId = this.dependencyLocator
            .GetInstance<IntegrationConnectorSettings>()
            .APlusIntegrationConnectionId;
        if (aPlusIntegrationConnectionId.IsBlank())
        {
            throw new ArgumentException(
                "APlus Integration Connector requires an Integration Connection."
            );
        }

        integrationConnection = this.unitOfWorkFactory
            .GetUnitOfWork()
            .GetRepository<IntegrationConnection>()
            .Get(aPlusIntegrationConnectionId);
        if (integrationConnection == null)
        {
            throw new ArgumentException(
                "APlus Integration Connector requires an Integration Connection."
            );
        }

        return integrationConnection;
    }
}
