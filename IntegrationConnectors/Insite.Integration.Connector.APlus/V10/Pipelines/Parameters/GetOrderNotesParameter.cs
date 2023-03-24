namespace Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;

using Insite.Core.Plugins.Pipelines;

public class GetOrderNotesParameter : PipeParameterBase
{
    public string Notes { get; set; }

    public string LineItemType { get; set; }
}
