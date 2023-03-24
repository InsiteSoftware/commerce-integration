namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;

using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddCurrencyCodeToRequest
    : IPipe<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    public int Order => 500;

    public GetCustomerPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetCustomerPriceParameter parameter,
        GetCustomerPriceResult result
    )
    {
        result.CustomerPriceRequest.currencyCode = parameter.PricingServiceParameters
            .FirstOrDefault()
            ?.CurrencyCode;

        return result;
    }
}
