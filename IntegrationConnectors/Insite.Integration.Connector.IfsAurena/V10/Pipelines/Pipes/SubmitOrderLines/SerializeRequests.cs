namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderLines;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Insite.Integration.Connector.IfsAurena.Services;

public sealed class SerializeRequests : IPipe<SubmitOrderLinesParameter, SubmitOrderLinesResult>
{
    public int Order => 200;

    public SubmitOrderLinesResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderLinesParameter parameter,
        SubmitOrderLinesResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(SerializeRequests)} Started.");

        result.SerializedCustomerOrderLineRequests = result.CustomerOrderLineRequests
            .Select(o => IfsAurenaSerializationService.Serialize(o))
            .ToList();

        parameter.JobLogger?.Debug($"{nameof(SerializeRequests)} Finished.");

        return result;
    }
}
