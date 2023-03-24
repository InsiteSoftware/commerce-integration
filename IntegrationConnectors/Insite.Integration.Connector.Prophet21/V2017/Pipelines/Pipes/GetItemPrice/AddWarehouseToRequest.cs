namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;

using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddWarehouseToRequest : IPipe<GetItemPriceParameter, GetItemPriceResult>
{
    public int Order => 600;

    public GetItemPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetItemPriceParameter parameter,
        GetItemPriceResult result
    )
    {
        var warehouse = this.GetWarehouse(unitOfWork, parameter);

        result.GetItemPriceRequest.Request.LocationID = warehouse;

        return result;
    }

    private string GetWarehouse(IUnitOfWork unitOfWork, GetItemPriceParameter parameter)
    {
        var warehouse = parameter.PricingServiceParameters.FirstOrDefault()?.Warehouse;

        if (string.IsNullOrEmpty(warehouse))
        {
            warehouse = this.GetWarehouse(unitOfWork, parameter.GetInventoryParameter);
        }

        if (string.IsNullOrEmpty(warehouse))
        {
            warehouse = SiteContext.Current.WarehouseDto?.Name ?? string.Empty;
        }

        return warehouse;
    }

    private string GetWarehouse(IUnitOfWork unitOfWork, GetInventoryParameter getInventoryParameter)
    {
        return getInventoryParameter?.WarehouseId != null
            ? unitOfWork
                .GetRepository<Warehouse>()
                .Get(getInventoryParameter.WarehouseId.Value)
                ?.Name ?? string.Empty
            : string.Empty;
    }
}
