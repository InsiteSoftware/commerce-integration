namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetPartAvailability;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddWarehouseToRequest
    : IPipe<GetPartAvailabilityParameter, GetPartAvailabilityResult>
{
    public int Order => 500;

    public GetPartAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        GetPartAvailabilityParameter parameter,
        GetPartAvailabilityResult result
    )
    {
        var warehouse = this.GetWarehouse(unitOfWork, parameter);

        result.PartAvailabilityRequest.site = warehouse;

        return result;
    }

    private string GetWarehouse(IUnitOfWork unitOfWork, GetPartAvailabilityParameter parameter)
    {
        if (parameter.GetInventoryParameter.WarehouseId == null)
        {
            return string.Empty;
        }

        return unitOfWork
                .GetRepository<Warehouse>()
                .Get(parameter.GetInventoryParameter.WarehouseId.Value)
                ?.Name ?? string.Empty;
    }
}
