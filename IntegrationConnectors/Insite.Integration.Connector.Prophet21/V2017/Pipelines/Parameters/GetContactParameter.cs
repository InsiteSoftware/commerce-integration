namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;

using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.WebService.Interfaces;

public class GetContactParameter : PipeParameterBase
{
    public CustomerOrder CustomerOrder { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }

    public IJobLogger JobLogger { get; set; }
}
