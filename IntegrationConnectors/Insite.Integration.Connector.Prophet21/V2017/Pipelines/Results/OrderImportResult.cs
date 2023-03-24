namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;

public class OrderImportResult : PipeResultBase
{
    public OrderImport OrderImportRequest { get; set; }

    public OrderImport OrderImportReply { get; set; }

    public string ErpOrderNumber { get; set; }
}
