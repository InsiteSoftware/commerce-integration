namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.ARCustomerMntRequest;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.ARCustomerMnt;

public sealed class CreateInitialRequest : IPipe<ARCustomerMntParameter, ARCustomerMntResult>
{
    public int Order => 100;

    public ARCustomerMntResult Execute(
        IUnitOfWork unitOfWork,
        ARCustomerMntParameter parameter,
        ARCustomerMntResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Started.");

        result.ARCustomerMntRequest = new ARCustomerMntRequest { Request = new Request() };

        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Finished.");

        return result;
    }
}
