namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetPartAvailability;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class CreateInitialRequest
    : IPipe<GetPartAvailabilityParameter, GetPartAvailabilityResult>
{
    public int Order => 100;

    public GetPartAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        GetPartAvailabilityParameter parameter,
        GetPartAvailabilityResult result
    )
    {
        result.PartAvailabilityRequest = new partAvailabilityRequest();

        return result;
    }
}
