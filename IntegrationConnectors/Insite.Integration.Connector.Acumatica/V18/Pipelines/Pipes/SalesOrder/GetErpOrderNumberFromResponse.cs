namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;

public sealed class GetErpOrderNumberFromResponse : IPipe<SalesOrderParameter, SalesOrderResult>
{
    public int Order => 900;

    public SalesOrderResult Execute(
        IUnitOfWork unitOfWork,
        SalesOrderParameter parameter,
        SalesOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Started.");

        result.ErpOrderNumber = result.SalesOrderResponse.OrderNbr;

        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Finished.");

        return result;
    }
}
