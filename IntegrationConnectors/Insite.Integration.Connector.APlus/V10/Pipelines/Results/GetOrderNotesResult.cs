namespace Insite.Integration.Connector.APlus.V10.Pipelines.Results;

using System.Collections.Generic;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;

public class GetOrderNotesResult : PipeResultBase
{
    public List<RequestLineItemInfo> LineItemInfos { get; set; }
}
