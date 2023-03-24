namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Insite.Integration.Connector.IfsAurena.Services;

public sealed class SerializeRequest : IPipe<SubmitOrderParameter, SubmitOrderResult>
{
    public int Order => 200;

    public SubmitOrderResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderParameter parameter,
        SubmitOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(SerializeRequest)} Started.");

        result.SerializedCustomerOrderRequest = IfsAurenaSerializationService.Serialize(
            result.CustomerOrderRequest
        );

        parameter.JobLogger?.Debug($"{nameof(SerializeRequest)} Finished.");

        return result;
    }
}
