namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;

using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;

public class GetCartSummaryParameter : PipeParameterBase
{
    public CustomerOrder CustomerOrder { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }
}
