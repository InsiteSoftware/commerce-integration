namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;

public class SubmitCreditCardDetailsResult : PipeResultBase
{
    public CreditCardDetails CreditCardDetailsRequest { get; set; }

    public string SerializedCreditCardDetailsRequest { get; set; }

    public string ErpOrderNumber { get; set; }
}
