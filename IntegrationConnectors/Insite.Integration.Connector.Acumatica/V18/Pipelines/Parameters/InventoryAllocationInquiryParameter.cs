namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;

using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;

public class InventoryAllocationInquiryParameter : PipeParameterBase
{
    public GetInventoryParameter GetInventoryParameter { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }
}
