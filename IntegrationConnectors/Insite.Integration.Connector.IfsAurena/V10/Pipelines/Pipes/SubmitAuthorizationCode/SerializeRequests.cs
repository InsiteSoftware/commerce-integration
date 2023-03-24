namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitAuthorizationCode;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class SerializeRequests
    : IPipe<SubmitAuthorizationCodeParameter, SubmitAuthorizationCodeResult>
{
    public int Order => 200;

    public SubmitAuthorizationCodeResult Execute(
        IUnitOfWork unitOfWork,
        SubmitAuthorizationCodeParameter parameter,
        SubmitAuthorizationCodeResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(SerializeRequests)} Started.");

        result.SerializedAuthorizationCodeRequest = IfsAurenaSerializationService.Serialize(
            result.AuthorizationCodeRequest
        );

        parameter.JobLogger?.Debug($"{nameof(SerializeRequests)} Finished.");

        return result;
    }
}
