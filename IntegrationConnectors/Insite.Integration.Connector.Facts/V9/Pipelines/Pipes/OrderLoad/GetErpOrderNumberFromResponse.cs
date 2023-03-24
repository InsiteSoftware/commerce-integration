namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class GetErpOrderNumberFromResponse : IPipe<OrderLoadParameter, OrderLoadResult>
{
    public int Order => 900;

    public OrderLoadResult Execute(
        IUnitOfWork unitOfWork,
        OrderLoadParameter parameter,
        OrderLoadResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Started.");

        result.ErpOrderNumber = result.OrderLoadResponse.Response.Orders.First().OrderNumber;

        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Finished.");

        return result;
    }
}
