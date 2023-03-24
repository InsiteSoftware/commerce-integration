namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class CreateInitialRequest : IPipe<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    public int Order => 100;

    public GetCustomerPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetCustomerPriceParameter parameter,
        GetCustomerPriceResult result
    )
    {
        result.CustomerPriceRequest = new customerPriceRequest();

        return result;
    }
}
