namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;

using System;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class CreateInitialRequest
    : IPipe<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    public int Order => 100;

    public LineItemPriceAndAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        LineItemPriceAndAvailabilityParameter parameter,
        LineItemPriceAndAvailabilityResult result
    )
    {
        result.LineItemPriceAndAvailabilityRequest = new LineItemPriceAndAvailabilityRequest
        {
            Name = "GetAvail",
            TransactionID = Guid.NewGuid().ToString()
        };

        return result;
    }
}
