namespace Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;

using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;

public class OrderTotalParameter : PipeParameterBase
{
    public CustomerOrder CustomerOrder { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }
}
