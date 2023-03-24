namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.CustomerPaymentMethod;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.CustomerPaymentMethod;

public sealed class AddPaymentInfoToRequest
    : IPipe<CustomerPaymentMethodParameter, CustomerPaymentMethodResult>
{
    public int Order => 200;

    public CustomerPaymentMethodResult Execute(
        IUnitOfWork unitOfWork,
        CustomerPaymentMethodParameter parameter,
        CustomerPaymentMethodResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddPaymentInfoToRequest)} Started.");

        result.CustomerPaymentMethodRequest.PaymentMethod = GetPaymentMethod(
            unitOfWork,
            parameter.CreditCardTransaction
        );
        result.CustomerPaymentMethodRequest.CustomerID = parameter
            .CreditCardTransaction
            .CustomerNumber;

        result.CustomerPaymentMethodRequest.Details.Add(
            new Detail { Name = parameter.CreditCardTransaction.Token2 }
        );

        parameter.JobLogger?.Debug($"{nameof(AddPaymentInfoToRequest)} Finished.");

        return result;
    }

    private static string GetPaymentMethod(
        IUnitOfWork unitOfWork,
        CreditCardTransaction creditCardTransaction
    )
    {
        return unitOfWork
            .GetTypedRepository<ISystemListRepository>()
            .GetActiveSystemListValues(SystemListValueTypes.CreditCardTypeMapping)
            .FirstOrDefault(o => o.Description.Equals(creditCardTransaction.CardType))
            ?.Name;
    }
}
