namespace Insite.Integration.Connector.Base.Interfaces;

using System.Collections.Generic;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Inventory;
using Insite.Data.Entities;

public interface IProductHelper : IDependency
{
    IEnumerable<Product> GetProducts(
        IUnitOfWork unitOfWork,
        GetInventoryParameter getInventoryParameter
    );
}
