namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

using System.Collections.Generic;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;

public class SubmitOrderLinesResult : PipeResultBase
{
    public List<CustomerOrderLine> CustomerOrderLineRequests { get; set; } =
        new List<CustomerOrderLine>();

    public List<string> SerializedCustomerOrderLineRequests { get; set; } = new List<string>();

    public List<string> SerializedCustomerOrderLineResponses { get; set; } = new List<string>();
}
