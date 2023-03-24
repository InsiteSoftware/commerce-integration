namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class CreateInitialRequest : IPipe<GetItemPriceParameter, GetItemPriceResult>
{
    public int Order => 100;

    public GetItemPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetItemPriceParameter parameter,
        GetItemPriceResult result
    )
    {
        result.GetItemPriceRequest = new GetItemPrice
        {
            Request = new Request
            {
                B2BSellerVersion = new RequestB2BSellerVersion
                {
                    MajorVersion = "5",
                    MinorVersion = "11",
                    BuildNumber = "100"
                },
                GetListOfItemLocationQuantities = GetGetListOfItemLocationQuantities(parameter),
                GetAvailabilityOnly = GetGetAvailabilityOnly(parameter)
            }
        };

        return result;
    }

    private static string GetGetListOfItemLocationQuantities(GetItemPriceParameter parameter)
    {
        return (parameter.GetInventoryParameter?.GetWarehouses ?? false) ? "TRUE" : "FALSE";
    }

    private static string GetGetAvailabilityOnly(GetItemPriceParameter parameter)
    {
        return parameter.PricingServiceParameters.Any() ? "FALSE" : "TRUE";
    }
}
