namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;

public sealed class SubmitCustomerPaymentMethod : IPipe<SalesOrderParameter, SalesOrderResult>
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public SubmitCustomerPaymentMethod(IPipeAssemblyFactory pipeAssemblyFactory)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public int Order => 200;

    public SalesOrderResult Execute(
        IUnitOfWork unitOfWork,
        SalesOrderParameter parameter,
        SalesOrderResult result
    )
    {
        var creditCardTransaction = parameter.CustomerOrder.CreditCardTransactions.FirstOrDefault();
        if (creditCardTransaction == null)
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(SubmitCustomerPaymentMethod)} Started.");

        var customerPaymentMethodParameter = new CustomerPaymentMethodParameter
        {
            CreditCardTransaction = creditCardTransaction,
            IntegrationConnection = parameter.IntegrationConnection,
            JobLogger = parameter.JobLogger
        };

        var customerPaymentMethodResult = this.pipeAssemblyFactory.ExecutePipeline(
            customerPaymentMethodParameter,
            new CustomerPaymentMethodResult()
        );

        if (customerPaymentMethodResult.ResultCode != ResultCode.Success)
        {
            result.ResultCode = customerPaymentMethodResult.ResultCode;
            result.SubCode = customerPaymentMethodResult.SubCode;
            result.Messages = customerPaymentMethodResult.Messages;
        }
        else
        {
            result.SalesOrderRequest.PaymentCardIdentifier =
                customerPaymentMethodResult.CustomerProfileId;
            result.SalesOrderRequest.PaymentProfileID = creditCardTransaction.Token2;
            result.SalesOrderRequest.PreAuthorizationNbr = creditCardTransaction.PNRef;
            result.SalesOrderRequest.PreAuthorizedAmount = creditCardTransaction.Amount;
        }

        parameter.JobLogger?.Debug($"{nameof(SubmitCustomerPaymentMethod)} Finished.");

        return result;
    }
}
