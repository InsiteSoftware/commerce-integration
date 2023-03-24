namespace Insite.Integration.Connector.Facts.V9.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;

public class OrderLoadResult : PipeResultBase
{
    public OrderLoadRequest OrderLoadRequest { get; set; }

    public OrderLoadResponse OrderLoadResponse { get; set; }

    public string ErpOrderNumber { get; set; }
}
