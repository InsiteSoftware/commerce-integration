namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities.Dtos;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Facts.Services;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class AddWarehousesToRequest
    : IPipe<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    public int Order => 600;

    public PriceAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        PriceAvailabilityParameter parameter,
        PriceAvailabilityResult result
    )
    {
        var requestItems = new List<RequestItem>();
        var warehouses = this.GetWarehouses(unitOfWork, parameter);

        foreach (var requestItem in result.PriceAvailabilityRequest.Request.Items)
        {
            foreach (var warehouse in warehouses)
            {
                var requestItemClone = FactsCloneService.Clone(requestItem);
                requestItemClone.WarehouseID = warehouse.Name;

                requestItems.Add(requestItemClone);
            }
        }

        result.PriceAvailabilityRequest.Request.Items = requestItems;

        return result;
    }

    private IEnumerable<WarehouseDto> GetWarehouses(
        IUnitOfWork unitOfWork,
        PriceAvailabilityParameter parameter
    )
    {
        var warehouses = unitOfWork
            .GetTypedRepository<IWarehouseRepository>()
            .GetCachedWarehouses();
        if (parameter.GetInventoryParameter?.GetWarehouses ?? false)
        {
            return warehouses;
        }

        var warehouse =
            GetWarehouseForPricingServiceParameter(warehouses, parameter)
            ?? GetWarehouseForGetInventoryParameter(warehouses, parameter)
            ?? GetWarehouseForSiteContext(warehouses);

        return warehouse != null ? new List<WarehouseDto> { warehouse } : new List<WarehouseDto>();
    }

    private static WarehouseDto GetWarehouseForPricingServiceParameter(
        IEnumerable<WarehouseDto> warehouses,
        PriceAvailabilityParameter parameter
    )
    {
        var warehouseName = parameter.PricingServiceParameters.FirstOrDefault()?.Warehouse;

        return !string.IsNullOrEmpty(warehouseName)
            ? warehouses.FirstOrDefault(o => o.Name.EqualsIgnoreCase(warehouseName))
            : null;
    }

    private static WarehouseDto GetWarehouseForGetInventoryParameter(
        IEnumerable<WarehouseDto> warehouses,
        PriceAvailabilityParameter parameter
    )
    {
        var warehouseId = parameter.GetInventoryParameter?.WarehouseId;

        return warehouseId.HasValue
            ? warehouses.FirstOrDefault(o => o.Id == warehouseId.Value)
            : null;
    }

    private static WarehouseDto GetWarehouseForSiteContext(IEnumerable<WarehouseDto> warehouses)
    {
        var warehouseId = SiteContext.Current.WarehouseDto?.Id;

        return warehouseId.HasValue
            ? warehouses.FirstOrDefault(o => o.Id == warehouseId.Value)
            : null;
    }
}
