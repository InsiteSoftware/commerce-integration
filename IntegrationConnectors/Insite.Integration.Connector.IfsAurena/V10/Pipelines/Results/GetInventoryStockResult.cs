namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

using System.Collections.Generic;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;

public class GetInventoryStockResult : PipeResultBase
{
    public string InventoryPartInStockRequest { get; set; }

    public List<InventoryPartInStock> InventoryPartInStockResponses { get; set; } =
        new List<InventoryPartInStock>();

    public string SerializedInventoryPartInStockResponse { get; set; }

    public GetInventoryResult GetInventoryResult { get; set; }
}
