namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.CustomerPaymentMethod;

public class CustomerPaymentMethodResult : PipeResultBase
{
    public CustomerPaymentMethod CustomerPaymentMethodRequest { get; set; }

    public CustomerPaymentMethod CustomerPaymentMethodResponse { get; set; }

    public string CustomerProfileId { get; set; }
}
