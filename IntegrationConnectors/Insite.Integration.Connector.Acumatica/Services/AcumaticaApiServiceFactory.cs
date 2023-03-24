namespace Insite.Integration.Connector.Acumatica.Services;

using System;
using Insite.Common.Dependencies;
using Insite.Common.Helpers;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;

internal sealed class AcumaticaApiServiceFactory : IAcumaticaApiServiceFactory, ISingletonLifetime
{
    private readonly IUnitOfWorkFactory unitOfWorkFactory;

    private readonly IDependencyLocator dependencyLocator;

    public AcumaticaApiServiceFactory(
        IUnitOfWorkFactory unitOfWorkFactory,
        IDependencyLocator dependencyLocator
    )
    {
        this.unitOfWorkFactory = unitOfWorkFactory;
        this.dependencyLocator = dependencyLocator;
    }

    public IAcumaticaApiService GetAcumaticaApiService(
        IntegrationConnection integrationConnection = null
    )
    {
        var integrationConnectorSettings =
            this.dependencyLocator.GetInstance<IntegrationConnectorSettings>();

        integrationConnection = this.GetIntegrationConnection(
            integrationConnectorSettings,
            integrationConnection
        );

#pragma warning disable CS0618 // Type or member is obsolete
        var password = EncryptionHelper.DecryptAes(integrationConnection.Password);
#pragma warning restore

        return new AcumaticaApiService(
            integrationConnection.Url,
            integrationConnection.LogOn,
            password,
            integrationConnectorSettings.AcumaticaBranchNumber,
            integrationConnectorSettings.AcumaticaCompanyNumber
        );
    }

    private IntegrationConnection GetIntegrationConnection(
        IntegrationConnectorSettings integrationConnectorSettings,
        IntegrationConnection integrationConnection
    )
    {
        if (integrationConnection != null)
        {
            return integrationConnection;
        }

        var acumaticaIntegrationConnectionId =
            integrationConnectorSettings.AcumaticaIntegrationConnectionId;
        if (acumaticaIntegrationConnectionId.IsBlank())
        {
            throw new ArgumentException(
                "Acumatica Integration Connector requires an Integration Connection."
            );
        }

        integrationConnection = this.unitOfWorkFactory
            .GetUnitOfWork()
            .GetRepository<IntegrationConnection>()
            .Get(acumaticaIntegrationConnectionId);
        if (integrationConnection == null)
        {
            throw new ArgumentException(
                "Acumatica Integration Connector requires an Integration Connection."
            );
        }

        return integrationConnection;
    }
}
