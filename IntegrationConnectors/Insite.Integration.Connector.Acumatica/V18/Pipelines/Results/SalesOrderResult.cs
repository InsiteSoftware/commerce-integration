namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;

public class SalesOrderResult : PipeResultBase
{
    public SalesOrder SalesOrderRequest { get; set; }

    public SalesOrder SalesOrderResponse { get; set; }

    public string ErpOrderNumber { get; set; }
}
