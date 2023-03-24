namespace Insite.Integration.Connector.SXe.Services;

using System;
using Insite.Common.Dependencies;
using Insite.Common.Helpers;
using Insite.Core.Interfaces.Data;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10;
using Insite.Integration.Connector.SXe.V11;
using Insite.Integration.Connector.SXe.V61;

internal sealed class SXeApiServiceFactory : ISXeApiServiceFactory
{
    private readonly IUnitOfWorkFactory unitOfWorkFactory;

    private readonly IDependencyLocator dependencyLocator;

    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public SXeApiServiceFactory(
        IUnitOfWorkFactory unitOfWorkFactory,
        IDependencyLocator dependencyLocator,
        IntegrationConnectorSettings integrationConnectorSettings
    )
    {
        this.unitOfWorkFactory = unitOfWorkFactory;
        this.dependencyLocator = dependencyLocator;
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public ISXeApiServiceV61 GetSXeApiServiceV61(IntegrationConnection integrationConnection = null)
    {
        integrationConnection = this.GetIntegrationConnection(integrationConnection);

        var companyNumber = GetCompanyNumber(integrationConnection);
        var operatorPassword = GetOperatorPassword(integrationConnection);

        return new SXeApiServiceV61(
            integrationConnection.Url,
            integrationConnection.AppServerHost,
            integrationConnection.LogOn,
            operatorPassword,
            companyNumber
        );
    }

    public ISXeApiServiceV10 GetSXeApiServiceV10(IntegrationConnection integrationConnection = null)
    {
        integrationConnection = this.GetIntegrationConnection(integrationConnection);

        var companyNumber = GetCompanyNumber(integrationConnection);
        var operatorPassword = GetOperatorPassword(integrationConnection);

        return new SXeApiServiceV10(
            integrationConnection.Url,
            integrationConnection.AppServerHost,
            integrationConnection.LogOn,
            operatorPassword,
            companyNumber
        );
    }

    public ISXeApiServiceV11 GetSXeApiServiceV11(IntegrationConnection integrationConnection = null)
    {
        integrationConnection = this.GetIntegrationConnection(integrationConnection);

        var companyNumber = GetCompanyNumber(integrationConnection);
        var operatorPassword = GetOperatorPassword(integrationConnection);
        var clientSecret = GetClientSecret(integrationConnection);
        var password = GetPassword(integrationConnection);

        return new SXeApiServiceV11(
            integrationConnection.Url,
            this.integrationConnectorSettings.SXeAccessTokenUrl,
            integrationConnection.LogOn,
            operatorPassword,
            companyNumber,
            integrationConnection.MessageServerHost,
            clientSecret,
            integrationConnection.GatewayHost,
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

        var sXeIntegrationConnectionId = this.dependencyLocator
            .GetInstance<IntegrationConnectorSettings>()
            .SXeIntegrationConnectionId;
        if (sXeIntegrationConnectionId.IsBlank())
        {
            throw new ArgumentException(
                "SXe Integration Connector requires an Integration Connection."
            );
        }

        integrationConnection = this.unitOfWorkFactory
            .GetUnitOfWork()
            .GetRepository<IntegrationConnection>()
            .Get(sXeIntegrationConnectionId);
        if (integrationConnection == null)
        {
            throw new ArgumentException(
                "SXe Integration Connector requires an Integration Connection."
            );
        }

        return integrationConnection;
    }

    private static int GetCompanyNumber(IntegrationConnection integrationConnection)
    {
        if (!int.TryParse(integrationConnection.SystemNumber, out var companyNumber))
        {
            throw new ArgumentException(
                "SXe Integration Connection company number must be in integer format."
            );
        }

        return companyNumber;
    }

    private static string GetOperatorPassword(IntegrationConnection integrationConnection)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return EncryptionHelper.DecryptAes(integrationConnection.Password);
#pragma warning restore
    }

    private static string GetClientSecret(IntegrationConnection integrationConnection)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return EncryptionHelper.DecryptAes(integrationConnection.MessageServerService);
#pragma warning restore
    }

    private static string GetPassword(IntegrationConnection integrationConnection)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return EncryptionHelper.DecryptAes(integrationConnection.GatewayService);
#pragma warning restore
    }
}
