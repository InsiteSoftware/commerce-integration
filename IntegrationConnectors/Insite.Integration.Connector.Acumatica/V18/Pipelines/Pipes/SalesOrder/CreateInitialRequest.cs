namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;

public sealed class CreateInitialRequest : IPipe<SalesOrderParameter, SalesOrderResult>
{
    public int Order => 100;

    public SalesOrderResult Execute(
        IUnitOfWork unitOfWork,
        SalesOrderParameter parameter,
        SalesOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Started.");

        result.SalesOrderRequest = new SalesOrder();

        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Finished.");

        return result;
    }
}
