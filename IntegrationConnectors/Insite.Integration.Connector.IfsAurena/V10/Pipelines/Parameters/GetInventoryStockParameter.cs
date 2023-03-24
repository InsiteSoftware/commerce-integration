namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;

using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;

public class GetInventoryStockParameter : PipeParameterBase
{
    public GetInventoryParameter GetInventoryParameter { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }
}
