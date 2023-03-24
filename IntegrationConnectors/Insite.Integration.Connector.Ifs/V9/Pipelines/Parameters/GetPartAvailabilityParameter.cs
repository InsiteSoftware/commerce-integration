namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;

using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;

public class GetPartAvailabilityParameter : PipeParameterBase
{
    public GetInventoryParameter GetInventoryParameter { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }
}
