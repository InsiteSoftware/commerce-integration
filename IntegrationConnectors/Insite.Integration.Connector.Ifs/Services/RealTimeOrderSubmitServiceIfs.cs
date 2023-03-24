namespace Insite.Integration.Connector.Ifs.Services;

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
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

[DependencyName(nameof(IntegrationConnectorType.IFS))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.IFS)]
public sealed class RealTimeOrderSubmitServiceIfs
    : RealTimeOrderSubmitServiceBase,
        IRealTimeOrderSubmitService
{
    public RealTimeOrderSubmitServiceIfs(
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
        return this.SubmitRealTimeOrder<CreateCustomerOrderParameter, CreateCustomerOrderResult>(
            submitRealTimeOrderParameter
        );
    }
}
