namespace Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;

using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.WebService.Interfaces;

public class ARCustomerMntParameter : PipeParameterBase
{
    public Customer Customer { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }

    public IJobLogger JobLogger { get; set; }
}
