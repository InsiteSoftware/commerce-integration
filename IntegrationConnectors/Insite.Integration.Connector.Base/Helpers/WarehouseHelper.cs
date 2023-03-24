namespace Insite.Integration.Connector.Base.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Data.Entities.Dtos;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Base.Interfaces;

public class WarehouseHelper : IWarehouseHelper
{
    public WarehouseDto GetWarehouse(
        IUnitOfWork unitOfWork,
        PricingServiceParameter pricingServiceParameter
    )
    {
        var warehouseRepository = unitOfWork.GetTypedRepository<IWarehouseRepository>();
        WarehouseDto warehouse = null;

        if (!string.IsNullOrWhiteSpace(pricingServiceParameter?.Warehouse))
        {
            warehouse = warehouseRepository
                .GetCachedWarehouses()
                .FirstOrDefault(o => o.Name.EqualsIgnoreCase(pricingServiceParameter.Warehouse));
        }

        return warehouse
            ?? SiteContext.Current.WarehouseDto
            ?? warehouseRepository.GetCachedDefault();
    }

    public WarehouseDto GetWarehouse(
        IUnitOfWork unitOfWork,
        GetInventoryParameter getInventoryParameter
    )
    {
        var warehouseRepository = unitOfWork.GetTypedRepository<IWarehouseRepository>();
        WarehouseDto warehouse = null;

        if (getInventoryParameter?.WarehouseId != null)
        {
            warehouse = warehouseRepository
                .GetCachedWarehouses()
                .FirstOrDefault(o => o.Id == getInventoryParameter.WarehouseId);
        }

        return warehouse
            ?? SiteContext.Current.WarehouseDto
            ?? warehouseRepository.GetCachedDefault();
    }

    public IEnumerable<WarehouseDto> GetWarehouses(
        IUnitOfWork unitOfWork,
        GetInventoryParameter getInventoryParameter
    )
    {
        if (getInventoryParameter?.GetWarehouses == true)
        {
            return unitOfWork.GetTypedRepository<IWarehouseRepository>().GetCachedWarehouses();
        }

        var warehouse = this.GetWarehouse(unitOfWork, getInventoryParameter);

        return new List<WarehouseDto> { warehouse };
    }
}
