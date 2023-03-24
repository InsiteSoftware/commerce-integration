namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetInventoryStock;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class CreateRequest : IPipe<GetInventoryStockParameter, GetInventoryStockResult>
{
    private readonly IProductHelper productHelper;

    private readonly IWarehouseHelper warehouseHelper;

    public CreateRequest(IProductHelper productHelper, IWarehouseHelper warehouseHelper)
    {
        this.productHelper = productHelper;
        this.warehouseHelper = warehouseHelper;
    }

    public int Order => 100;

    public GetInventoryStockResult Execute(
        IUnitOfWork unitOfWork,
        GetInventoryStockParameter parameter,
        GetInventoryStockResult result
    )
    {
        result.InventoryPartInStockRequest = $"$select=PartNo,Contract,AvailableQty";

        var products = this.productHelper.GetProducts(unitOfWork, parameter.GetInventoryParameter);
        result.InventoryPartInStockRequest +=
            $"&$filter=({string.Join(" or ", products.Select(o => $"PartNo eq '{o.ProductCode}'"))})";

        var warehouses = this.warehouseHelper.GetWarehouses(
            unitOfWork,
            parameter.GetInventoryParameter
        );
        result.InventoryPartInStockRequest +=
            $" and ({string.Join(" or ", warehouses.Select(o => $"Contract eq '{o.Name}'"))})";

        return result;
    }
}
