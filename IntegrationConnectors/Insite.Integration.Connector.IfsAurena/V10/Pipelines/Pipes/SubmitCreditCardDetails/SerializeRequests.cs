namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitCreditCardDetails;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class SerializeRequests
    : IPipe<SubmitCreditCardDetailsParameter, SubmitCreditCardDetailsResult>
{
    public int Order => 200;

    public SubmitCreditCardDetailsResult Execute(
        IUnitOfWork unitOfWork,
        SubmitCreditCardDetailsParameter parameter,
        SubmitCreditCardDetailsResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(SerializeRequests)} Started.");

        result.SerializedCreditCardDetailsRequest = IfsAurenaSerializationService.Serialize(
            result.CreditCardDetailsRequest
        );

        parameter.JobLogger?.Debug($"{nameof(SerializeRequests)} Finished.");

        return result;
    }
}
