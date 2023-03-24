namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;

using System.Collections.Generic;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class CreateInitialRequest : IPipe<OrderLoadParameter, OrderLoadResult>
{
    public int Order => 100;

    public OrderLoadResult Execute(
        IUnitOfWork unitOfWork,
        OrderLoadParameter parameter,
        OrderLoadResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Started.");

        result.OrderLoadRequest = new OrderLoadRequest
        {
            Request = new Request
            {
                RequestID = "OrderLoad",
                Orders = new List<RequestOrder>
                {
                    new RequestOrder { OrderHeader = new OrderHeader() }
                }
            }
        };

        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Finished.");

        return result;
    }
}
