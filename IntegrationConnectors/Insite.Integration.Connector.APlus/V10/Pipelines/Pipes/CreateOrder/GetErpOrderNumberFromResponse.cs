namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class GetErpOrderNumberFromResponse : IPipe<CreateOrderParameter, CreateOrderResult>
{
    public int Order => 700;

    public CreateOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateOrderParameter parameter,
        CreateOrderResult result
    )
    {
        if (!parameter.IsOrderSubmit)
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Started.");

        result.ErpOrderNumber =
            result.CreateOrderResponse?.OrderHeader != null
                ? result.CreateOrderResponse.OrderHeader.OrderNumber
                    + "-"
                    + result.CreateOrderResponse.OrderHeader.OrderGeneration.PadLeft(2, '0')
                : string.Empty;

        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Finished.");

        return result;
    }
}
