namespace Insite.Integration.Connector.Base.Interfaces;

using System.Collections.Generic;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Data.Entities.Dtos;

public interface IWarehouseHelper : IDependency
{
    WarehouseDto GetWarehouse(
        IUnitOfWork unitOfWork,
        PricingServiceParameter pricingServiceParameter
    );

    WarehouseDto GetWarehouse(IUnitOfWork unitOfWork, GetInventoryParameter getInventoryParameter);

    IEnumerable<WarehouseDto> GetWarehouses(
        IUnitOfWork unitOfWork,
        GetInventoryParameter getInventoryParameter
    );
}
