namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;

public class CreateCustomerOrderResult : PipeResultBase
{
    public customerOrder CustomerOrder { get; set; }

    public orderResponse OrderResponse { get; set; }

    public string ErpOrderNumber { get; set; }
}
