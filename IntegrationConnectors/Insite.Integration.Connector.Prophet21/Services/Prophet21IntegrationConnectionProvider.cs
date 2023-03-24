namespace Insite.Integration.Connector.Prophet21.Services;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;

internal sealed class Prophet21IntegrationConnectionProvider
    : IProphet21IntegrationConnectionProvider
{
    private const string IntegrationConnectionRequiredErrorMessage =
        "Prophet21 Integration Connector requires an Integration Connection.";

    private readonly IUnitOfWork unitOfWork;

    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public Prophet21IntegrationConnectionProvider(
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

        var integrationConnectionId =
            this.integrationConnectorSettings.Prophet21IntegrationConnectionId;
        if (integrationConnectionId.IsBlank())
        {
            throw new ArgumentException(IntegrationConnectionRequiredErrorMessage);
        }

        integrationConnection = this.unitOfWork
            .GetRepository<IntegrationConnection>()
            .Get(integrationConnectionId);
        if (integrationConnection == null)
        {
            throw new ArgumentException(IntegrationConnectionRequiredErrorMessage);
        }

        return integrationConnection;
    }
}
