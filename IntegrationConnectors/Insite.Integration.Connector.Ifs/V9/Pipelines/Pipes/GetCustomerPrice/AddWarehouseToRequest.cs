namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;

using System.Linq;

using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddWarehouseToRequest : IPipe<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    public int Order => 400;

    public GetCustomerPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetCustomerPriceParameter parameter,
        GetCustomerPriceResult result
    )
    {
        var warehouse = this.GetWarehouse(parameter);

        result.CustomerPriceRequest.site = warehouse;

        return result;
    }

    private string GetWarehouse(GetCustomerPriceParameter parameter)
    {
        var warehouse = parameter.PricingServiceParameters.FirstOrDefault()?.Warehouse;

        if (string.IsNullOrEmpty(warehouse))
        {
            warehouse = SiteContext.Current.WarehouseDto?.Name ?? string.Empty;
        }

        return warehouse;
    }
}
