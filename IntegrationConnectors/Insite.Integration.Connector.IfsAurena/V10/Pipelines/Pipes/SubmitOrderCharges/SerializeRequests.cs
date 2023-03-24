namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderCharges;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Insite.Integration.Connector.IfsAurena.Services;

public sealed class SerializeRequests : IPipe<SubmitOrderChargesParameter, SubmitOrderChargesResult>
{
    public int Order => 200;

    public SubmitOrderChargesResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderChargesParameter parameter,
        SubmitOrderChargesResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(SerializeRequests)} Started.");

        result.SerializedCustomerOrderChargeRequests = result.CustomerOrderChargeRequests
            .Select(o => IfsAurenaSerializationService.Serialize(o))
            .ToList();

        parameter.JobLogger?.Debug($"{nameof(SerializeRequests)} Finished.");

        return result;
    }
}
