namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class GetErpOrderNumberFromResponse : IPipe<SubmitOrderParameter, SubmitOrderResult>
{
    public int Order => 500;

    public SubmitOrderResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderParameter parameter,
        SubmitOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Started.");

        result.ErpOrderNumber = result.CustomerOrderResponse.OrderNo;

        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Finished.");

        return result;
    }
}
