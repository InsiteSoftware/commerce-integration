namespace Insite.Integration.Connector.Base.Helpers;

using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Inventory;
using Insite.Data.Entities;
using Insite.Data.Extensions;
using Insite.Integration.Connector.Base.Interfaces;

public class ProductHelper : IProductHelper
{
    public IEnumerable<Product> GetProducts(
        IUnitOfWork unitOfWork,
        GetInventoryParameter getInventoryParameter
    )
    {
        if (getInventoryParameter == null)
        {
            return new List<Product>();
        }

        var productIds = getInventoryParameter.ProductIds.Where(
            o => !getInventoryParameter.Products.Any(p => p.Id == o)
        );

        var products = unitOfWork
            .GetRepository<Product>()
            .GetTableAsNoTracking()
            .WhereContains(o => o.Id, productIds)
            .ToList();

        products.AddRange(getInventoryParameter.Products.Select(o => o));

        return products;
    }
}
