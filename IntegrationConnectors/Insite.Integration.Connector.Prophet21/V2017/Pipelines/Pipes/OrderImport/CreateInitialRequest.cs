namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class CreateInitialRequest : IPipe<OrderImportParameter, OrderImportResult>
{
    public int Order => 100;

    public OrderImportResult Execute(
        IUnitOfWork unitOfWork,
        OrderImportParameter parameter,
        OrderImportResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Started.");

        result.OrderImportRequest = new OrderImport
        {
            Request = new Request
            {
                B2BSellerVersion = new RequestB2BSellerVersion
                {
                    MajorVersion = "5",
                    MinorVersion = "11",
                    BuildNumber = "100"
                },
                Anonymous = "N",
                UseContractAddress = "False"
            }
        };

        parameter.JobLogger?.Debug($"{nameof(CreateInitialRequest)} Finished.");

        return result;
    }
}
