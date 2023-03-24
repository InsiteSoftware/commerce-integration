namespace Insite.Integration.Connector.IfsAurena.Services;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;

internal sealed class IfsAurenaIntegrationConnectionProvider
    : IIfsAurenaIntegrationConnectionProvider
{
    private const string IntegrationConnectionRequiredErrorMessage =
        "Ifs Aurena Integration Connector requires an Integration Connection.";

    private readonly IUnitOfWork unitOfWork;

    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public IfsAurenaIntegrationConnectionProvider(
        IUnitOfWorkFactory unitOfWorkFactory,
        IntegrationConnectorSettings integrationConnectorSettings
    )
    {
        this.unitOfWork = unitOfWorkFactory.GetUnitOfWork();
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public IntegrationConnection GetIntegrationConnection(
        IntegrationConnection integrationConnection
    )
    {
        if (integrationConnection != null)
        {
            return integrationConnection;
        }

        var acumaticaIntegrationConnectionId =
            this.integrationConnectorSettings.IfsAurenaIntegrationConnectionId;
        if (acumaticaIntegrationConnectionId.IsBlank())
        {
            throw new ArgumentException(IntegrationConnectionRequiredErrorMessage);
        }

        integrationConnection = this.unitOfWork
            .GetRepository<IntegrationConnection>()
            .Get(acumaticaIntegrationConnectionId);
        if (integrationConnection == null)
        {
            throw new ArgumentException(IntegrationConnectionRequiredErrorMessage);
        }

        return integrationConnection;
    }
}
