namespace Insite.Integration.Connector.SXe.Services;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Cart;
using Insite.Core.Plugins.Integration;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Attributes;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Enums;
using Insite.Integration.WebService.Interfaces;
using Insite.Integration.WebService.SystemSettings;
using Insite.Integration.Connector.Base;

[DependencyName(nameof(IntegrationConnectorType.SXe))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.SXe)]
public sealed class RealTimeOrderSubmitServiceSXe
    : RealTimeOrderSubmitServiceBase,
        IRealTimeOrderSubmitService
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public RealTimeOrderSubmitServiceSXe(
        IUnitOfWorkFactory unitOfWorkFactory,
        IIntegrationJobSchedulingService integrationJobSchedulingService,
        IWebServiceHandler webServiceHandler,
        IJobLoggerFactory jobLoggerFactory,
        IPipeAssemblyFactory pipeAssemblyFactory,
        IntegrationGeneralSettings integrationGeneralSettings,
        IntegrationConnectorSettings integrationConnectorSettings
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
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public SubmitRealTimeOrderResult SubmitRealTimeOrder(
        SubmitRealTimeOrderParameter submitRealTimeOrderParameter
    )
    {
        if (this.integrationConnectorSettings.SXeVersion == SXeVersion.SixOne)
        {
            return this.SubmitRealTimeOrder<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>(
                submitRealTimeOrderParameter
            );
        }
        else if (this.integrationConnectorSettings.SXeVersion == SXeVersion.Ten)
        {
            return this.SubmitRealTimeOrder<
                V10.Pipelines.Parameters.SFOEOrderTotLoadV4Parameter,
                V10.Pipelines.Results.SFOEOrderTotLoadV4Result
            >(submitRealTimeOrderParameter);
        }
        else
        {
            return this.SubmitRealTimeOrder<
                V11.Pipelines.Parameters.SFOEOrderTotLoadV4Parameter,
                V11.Pipelines.Results.SFOEOrderTotLoadV4Result
            >(submitRealTimeOrderParameter);
        }
    }
}
