namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

using System.Collections.Generic;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;

public class SubmitOrderChargesResult : PipeResultBase
{
    public List<CustomerOrderCharge> CustomerOrderChargeRequests { get; set; } =
        new List<CustomerOrderCharge>();

    public List<string> SerializedCustomerOrderChargeRequests { get; set; } = new List<string>();

    public List<string> SerializedCustomerOrderChargeResponses { get; set; } = new List<string>();
}
