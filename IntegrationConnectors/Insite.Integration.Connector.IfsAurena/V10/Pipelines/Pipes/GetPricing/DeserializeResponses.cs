namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetPricing;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class DeserializeResponses : IPipe<GetPricingParameter, GetPricingResult>
{
    public int Order => 400;

    public GetPricingResult Execute(
        IUnitOfWork unitOfWork,
        GetPricingParameter parameter,
        GetPricingResult result
    )
    {
        result.PriceQueryResponses = result.SerializedPriceQueryResponses
            .Select(o => IfsAurenaSerializationService.Deserialize<PriceQuery>(o))
            .ToList();

        return result;
    }
}
