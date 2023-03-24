namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class GetTaxAmountFromResponse : IPipe<CreateOrderParameter, CreateOrderResult>
{
    public int Order => 800;

    public CreateOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateOrderParameter parameter,
        CreateOrderResult result
    )
    {
        if (parameter.IsOrderSubmit)
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(GetTaxAmountFromResponse)} Started.");

        if (
            decimal.TryParse(
                result.CreateOrderResponse?.OrderTotal?.SalesTaxAmount,
                out var salesTaxAmount
            )
        )
        {
            result.TaxCalculated = true;
        }

        result.TaxAmount = salesTaxAmount;

        parameter.JobLogger?.Debug($"{nameof(GetTaxAmountFromResponse)} Finished.");

        return result;
    }
}
