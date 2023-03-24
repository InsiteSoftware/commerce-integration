namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.ARCustomerMntRequest;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.ARCustomerMnt;

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

        result.ARCustomerMntRequest = new ARCustomerMntRequest();

        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Finished.");

        return result;
    }
}
