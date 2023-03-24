namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class CreateInitialRequest
    : IPipe<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    public int Order => 100;

    public PriceAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        PriceAvailabilityParameter parameter,
        PriceAvailabilityResult result
    )
    {
        result.PriceAvailabilityRequest = new PriceAvailabilityRequest
        {
            Request = new Request { RequestID = "PriceAvailability" }
        };

        return result;
    }
}
