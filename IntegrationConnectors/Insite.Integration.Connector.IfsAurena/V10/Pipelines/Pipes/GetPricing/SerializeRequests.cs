namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetPricing;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class SerializeRequests : IPipe<GetPricingParameter, GetPricingResult>
{
    public int Order => 200;

    public GetPricingResult Execute(
        IUnitOfWork unitOfWork,
        GetPricingParameter parameter,
        GetPricingResult result
    )
    {
        result.SerializedPriceQueryRequests = result.PriceQueryRequests
            .Select(o => IfsAurenaSerializationService.Serialize(o))
            .ToList();

        return result;
    }
}
