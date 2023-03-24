namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.PaymentGateway.Cenpos;

public sealed class AddCreditCardToRequest : IPipe<OrderLoadParameter, OrderLoadResult>
{
    private readonly CenposSettings cenposSettings;

    public AddCreditCardToRequest(CenposSettings cenposSettings)
    {
        this.cenposSettings = cenposSettings;
    }

    public int Order => 300;

    public OrderLoadResult Execute(
        IUnitOfWork unitOfWork,
        OrderLoadParameter parameter,
        OrderLoadResult result
    )
    {
        if (!parameter.CustomerOrder.CreditCardTransactions.Any())
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(AddCreditCardToRequest)} Started.");

        var orderHeader = result.OrderLoadRequest.Request.Orders.First().OrderHeader;
        var creditCardTransaction = parameter.CustomerOrder.CreditCardTransactions.First();

        orderHeader.AuthorizationAmount = creditCardTransaction.Amount;
        orderHeader.CCRefNum = creditCardTransaction.PNRef;
        orderHeader.CCAuthNum = creditCardTransaction.AuthCode;
        orderHeader.CCMaskedNum = creditCardTransaction.CreditCardNumber;
        orderHeader.CCMerchantID = this.cenposSettings.MerchantId.ToString();
        orderHeader.CCProcessorID = "CENPOS";
        orderHeader.CCType = parameter.CustomerOrder.TermsCode;

        parameter.JobLogger?.Debug($"{nameof(AddCreditCardToRequest)} Finished.");

        return result;
    }
}
