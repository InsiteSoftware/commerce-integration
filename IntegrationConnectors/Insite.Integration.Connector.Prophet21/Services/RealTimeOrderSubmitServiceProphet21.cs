namespace Insite.Integration.Connector.Prophet21.Services;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Cart;
using Insite.Core.Plugins.Integration;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.WebService.Interfaces;
using Insite.Integration.WebService.SystemSettings;
using Insite.Integration.Connector.Base;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Data.Entities;

[DependencyName(nameof(IntegrationConnectorType.Prophet21))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.Prophet21)]
public sealed class RealTimeOrderSubmitServiceProphet21
    : RealTimeOrderSubmitServiceBase,
        IRealTimeOrderSubmitService
{
    private readonly IProphet21IntegrationConnectionProvider prophet21IntegrationConnectionProvider;

    public RealTimeOrderSubmitServiceProphet21(
        IUnitOfWorkFactory unitOfWorkFactory,
        IIntegrationJobSchedulingService integrationJobSchedulingService,
        IWebServiceHandler webServiceHandler,
        IJobLoggerFactory jobLoggerFactory,
        IPipeAssemblyFactory pipeAssemblyFactory,
        IntegrationGeneralSettings integrationGeneralSettings,
        IDependencyLocator dependencyLocator
    )
        : base(
            unitOfWorkFactory,
            integrationJobSchedulingService,
            webServiceHandler,
            jobLoggerFactory,
            pipeAssemblyFactory,
            integrationGeneralSettings
        )
    {
        this.prophet21IntegrationConnectionProvider =
            dependencyLocator.GetInstance<IProphet21IntegrationConnectionProvider>();
    }

    public SubmitRealTimeOrderResult SubmitRealTimeOrder(
        SubmitRealTimeOrderParameter submitRealTimeOrderParameter
    )
    {
        return this.SubmitRealTimeOrder<OrderImportParameter, OrderImportResult>(
            submitRealTimeOrderParameter
        );
    }

    protected override IntegrationConnection GetIntegrationConnection()
    {
        return this.prophet21IntegrationConnectionProvider.GetIntegrationConnection(null);
    }
}
