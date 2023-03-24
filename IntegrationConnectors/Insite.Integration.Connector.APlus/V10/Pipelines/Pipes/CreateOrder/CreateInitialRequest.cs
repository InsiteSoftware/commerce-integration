namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class CreateInitialRequest : IPipe<CreateOrderParameter, CreateOrderResult>
{
    public int Order => 100;

    public CreateOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateOrderParameter parameter,
        CreateOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Started.");

        result.CreateOrderRequest = new CreateOrderRequest { Name = "CreateOrder" };

        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Finished.");

        return result;
    }
}
