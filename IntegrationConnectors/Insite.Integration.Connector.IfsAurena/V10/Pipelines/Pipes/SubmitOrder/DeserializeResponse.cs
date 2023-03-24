namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.Services;

public sealed class DeserializeResponse : IPipe<SubmitOrderParameter, SubmitOrderResult>
{
    public int Order => 400;

    public SubmitOrderResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderParameter parameter,
        SubmitOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(DeserializeResponse)} Started.");

        result.CustomerOrderResponse = IfsAurenaSerializationService.Deserialize<CustomerOrder>(
            result.SerializedCustomerOrderResponse
        );

        parameter.JobLogger?.Debug($"{nameof(DeserializeResponse)} Finished.");

        return result;
    }
}
