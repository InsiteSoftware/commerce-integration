namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;

using System.Collections.Generic;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.InventoryAllocationInquiry;

public class InventoryAllocationInquiryResult : PipeResultBase
{
    public List<InventoryAllocationInquiry> InventoryAllocationInquiryRequests { get; set; } =
        new List<InventoryAllocationInquiry>();

    public List<InventoryAllocationInquiry> InventoryAllocationInquiryResponses { get; set; } =
        new List<InventoryAllocationInquiry>();

    public GetInventoryResult GetInventoryResult { get; set; }
}
