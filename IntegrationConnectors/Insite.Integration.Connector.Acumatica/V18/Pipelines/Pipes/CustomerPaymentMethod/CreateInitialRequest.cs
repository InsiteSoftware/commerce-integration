namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.CustomerPaymentMethod;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.CustomerPaymentMethod;

public sealed class CreateInitialRequest
    : IPipe<CustomerPaymentMethodParameter, CustomerPaymentMethodResult>
{
    public int Order => 100;

    public CustomerPaymentMethodResult Execute(
        IUnitOfWork unitOfWork,
        CustomerPaymentMethodParameter parameter,
        CustomerPaymentMethodResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Started.");

        result.CustomerPaymentMethodRequest = new CustomerPaymentMethod();

        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Finished.");

        return result;
    }
}
