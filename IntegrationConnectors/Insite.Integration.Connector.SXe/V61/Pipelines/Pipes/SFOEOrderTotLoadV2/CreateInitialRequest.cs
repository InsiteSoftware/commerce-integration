namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

public sealed class CreateInitialRequest
    : IPipe<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    public int Order => 300;

    public SFOEOrderTotLoadV2Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV2Parameter parameter,
        SFOEOrderTotLoadV2Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Started.");

        result.SFOEOrderTotLoadV2Request = new SFOEOrderTotLoadV2Request();

        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Finished.");

        return result;
    }
}
