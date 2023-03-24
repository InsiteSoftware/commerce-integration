namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class CreateInitialRequest
    : IPipe<CreateCustomerOrderParameter, CreateCustomerOrderResult>
{
    public int Order => 100;

    public CreateCustomerOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateCustomerOrderParameter parameter,
        CreateCustomerOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Started.");

        result.CustomerOrder = new customerOrder();

        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Finished.");

        return result;
    }
}
