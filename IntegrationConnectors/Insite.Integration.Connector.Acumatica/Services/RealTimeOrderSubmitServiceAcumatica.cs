namespace Insite.Integration.Connector.Acumatica.Services;

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
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;

[DependencyName(nameof(IntegrationConnectorType.Acumatica))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.Acumatica)]
public sealed class RealTimeOrderSubmitServiceAcumatica
    : RealTimeOrderSubmitServiceBase,
        IRealTimeOrderSubmitService
{
    public RealTimeOrderSubmitServiceAcumatica(
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
        return this.SubmitRealTimeOrder<SalesOrderParameter, SalesOrderResult>(
            submitRealTimeOrderParameter
        );
    }
}
