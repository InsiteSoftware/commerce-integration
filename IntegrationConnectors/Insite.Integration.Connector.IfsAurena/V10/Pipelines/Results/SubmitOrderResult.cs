namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;

public class SubmitOrderResult : PipeResultBase
{
    public CustomerOrder CustomerOrderRequest { get; set; }

    public CustomerOrder CustomerOrderResponse { get; set; }

    public string SerializedCustomerOrderRequest { get; set; }

    public string SerializedCustomerOrderResponse { get; set; }

    public string ErpOrderNumber { get; set; }
}
