namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.CustomerPaymentMethod;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;

public sealed class GetCustomerProfileIdFromResponse
    : IPipe<CustomerPaymentMethodParameter, CustomerPaymentMethodResult>
{
    public int Order => 400;

    public CustomerPaymentMethodResult Execute(
        IUnitOfWork unitOfWork,
        CustomerPaymentMethodParameter parameter,
        CustomerPaymentMethodResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(GetCustomerProfileIdFromResponse)} Started.");

        result.CustomerProfileId = result.CustomerPaymentMethodResponse.CustomerProfileID;

        parameter.JobLogger?.Debug($"{nameof(GetCustomerProfileIdFromResponse)} Finished.");

        return result;
    }
}
