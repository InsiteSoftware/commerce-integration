namespace Insite.Integration.Connector.Ifs.Services;

using System;
using Insite.Common.Dependencies;
using Insite.Common.Helpers;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;

/// <summary>The Ifs Api Service Factory.</summary>
internal sealed class IfsApiServiceFactory : IIfsApiServiceFactory, ISingletonLifetime
{
    private readonly IUnitOfWorkFactory unitOfWorkFactory;

    private readonly IDependencyLocator dependencyLocator;

    public IfsApiServiceFactory(
        IUnitOfWorkFactory unitOfWorkFactory,
        IDependencyLocator dependencyLocator
    )
    {
        this.unitOfWorkFactory = unitOfWorkFactory;
        this.dependencyLocator = dependencyLocator;
    }

    /// <summary>Gets the IIfsApiService.</summary>
    /// <returns>The <see cref="IIfsApiService"/>.</returns>
    /// <param name="integrationConnection">The integration connection used to initialize the <see cref="IIfsApiService"/>.</param>
    public IIfsApiService GetIfsApiService(IntegrationConnection integrationConnection = null)
    {
        integrationConnection = this.GetIntegrationConnection(integrationConnection);

#pragma warning disable CS0618 // Type or member is obsolete
        var password = EncryptionHelper.DecryptAes(integrationConnection.Password);
#pragma warning restore

        return new IfsApiService(integrationConnection.Url, integrationConnection.LogOn, password);
    }

    private IntegrationConnection GetIntegrationConnection(
        IntegrationConnection integrationConnection
    )
    {
        if (integrationConnection != null)
        {
            return integrationConnection;
        }

        var ifsIntegrationConnectionId = this.dependencyLocator
            .GetInstance<IntegrationConnectorSettings>()
            .IfsIntegrationConnectionId;
        if (ifsIntegrationConnectionId.IsBlank())
        {
            throw new ArgumentException(
                "Ifs Integration Connector requires an Integration Connection."
            );
        }

        integrationConnection = this.unitOfWorkFactory
            .GetUnitOfWork()
            .GetRepository<IntegrationConnection>()
            .Get(ifsIntegrationConnectionId);
        if (integrationConnection == null)
        {
            throw new ArgumentException(
                "Ifs Integration Connector requires an Integration Connection."
            );
        }

        return integrationConnection;
    }
}
