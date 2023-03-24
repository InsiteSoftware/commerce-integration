namespace Insite.Integration.Connector.IfsAurena.Services;

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
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

[DependencyName(nameof(IntegrationConnectorType.IFSAurena))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.IFSAurena)]
public sealed class RealTimeOrderSubmitServiceIfsAurena
    : RealTimeOrderSubmitServiceBase,
        IRealTimeOrderSubmitService
{
    public RealTimeOrderSubmitServiceIfsAurena(
        IUnitOfWorkFactory unitOfWorkFactory,
        IIntegrationJobSchedulingService integrationJobSchedulingService,
        IWebServiceHandler webServiceHandler,
        IJobLoggerFactory jobLoggerFactory,
        IPipeAssemblyFactory pipeAssemblyFactory,
        IntegrationGeneralSettings integrationGeneralSettings
    )
        : base(
            unitOfWorkFactory,
            integrationJobSchedulingService,
            webServiceHandler,
            jobLoggerFactory,
            pipeAssemblyFactory,
            integrationGeneralSettings
        ) { }

    public SubmitRealTimeOrderResult SubmitRealTimeOrder(
        SubmitRealTimeOrderParameter submitRealTimeOrderParameter
    )
    {
        return this.SubmitRealTimeOrder<SubmitOrderParameter, SubmitOrderResult>(
            submitRealTimeOrderParameter
        );
    }
}
