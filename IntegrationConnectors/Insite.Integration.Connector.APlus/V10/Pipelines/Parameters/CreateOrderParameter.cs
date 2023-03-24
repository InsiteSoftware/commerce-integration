namespace Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;

using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.WebService.Interfaces;

public class CreateOrderParameter : PipeParameterBase
{
    public CustomerOrder CustomerOrder { get; set; }

    public IntegrationJob IntegrationJob { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }

    public IJobLogger JobLogger { get; set; }

    public bool IsOrderSubmit { get; set; }
}
