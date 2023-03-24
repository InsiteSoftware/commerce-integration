namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using System.Collections.Generic;

public sealed class CreateInitialRequest : IPipe<OrderTotalParameter, OrderTotalResult>
{
    public int Order => 100;

    public OrderTotalResult Execute(
        IUnitOfWork unitOfWork,
        OrderTotalParameter parameter,
        OrderTotalResult result
    )
    {
        result.OrderTotalRequest = new OrderTotalRequest
        {
            Request = new Request
            {
                RequestID = "OrderTotal",
                Orders = new List<RequestOrder>
                {
                    new RequestOrder { OrderHeader = new OrderHeader() }
                }
            }
        };

        return result;
    }
}
