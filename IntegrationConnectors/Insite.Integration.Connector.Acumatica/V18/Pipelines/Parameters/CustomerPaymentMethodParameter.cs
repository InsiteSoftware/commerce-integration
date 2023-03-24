namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;

using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.WebService.Interfaces;

public class CustomerPaymentMethodParameter : PipeParameterBase
{
    public CreditCardTransaction CreditCardTransaction { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }

    public IJobLogger JobLogger { get; set; }
}
