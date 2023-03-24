namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetInventoryStock;

using System.Collections.Generic;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class DeserializeResponse : IPipe<GetInventoryStockParameter, GetInventoryStockResult>
{
    public int Order => 300;

    public GetInventoryStockResult Execute(
        IUnitOfWork unitOfWork,
        GetInventoryStockParameter parameter,
        GetInventoryStockResult result
    )
    {
        result.InventoryPartInStockResponses = IfsAurenaSerializationService.Deserialize<
            List<InventoryPartInStock>
        >(result.SerializedInventoryPartInStockResponse);

        return result;
    }
}
