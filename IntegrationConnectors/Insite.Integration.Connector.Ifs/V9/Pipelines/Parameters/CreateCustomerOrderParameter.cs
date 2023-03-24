namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;

using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.WebService.Interfaces;

public class CreateCustomerOrderParameter : PipeParameterBase
{
    public CustomerOrder CustomerOrder { get; set; }

    public IntegrationJob IntegrationJob { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }

    public IJobLogger JobLogger { get; set; }
}
