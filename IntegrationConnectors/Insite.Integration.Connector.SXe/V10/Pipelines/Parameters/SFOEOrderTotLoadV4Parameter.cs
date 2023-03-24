namespace Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;

using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.WebService.Interfaces;

public class SFOEOrderTotLoadV4Parameter : PipeParameterBase
{
    public CustomerOrder CustomerOrder { get; set; }

    public IntegrationJob IntegrationJob { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }

    public IJobLogger JobLogger { get; set; }

    public bool IsOrderSubmit { get; set; }
}
