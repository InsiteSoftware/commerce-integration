namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;

public class GetPartAvailabilityResult : PipeResultBase
{
    public partAvailabilityRequest PartAvailabilityRequest { get; set; }

    public partAvailabilityResponse PartAvailabilityResponse { get; set; }

    public GetInventoryResult GetInventoryResult { get; set; }
}
