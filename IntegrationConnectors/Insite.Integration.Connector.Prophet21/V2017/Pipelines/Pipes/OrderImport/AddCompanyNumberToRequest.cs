namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddCompanyNumberToRequest : IPipe<OrderImportParameter, OrderImportResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddCompanyNumberToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 600;

    public OrderImportResult Execute(
        IUnitOfWork unitOfWork,
        OrderImportParameter parameter,
        OrderImportResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddCompanyNumberToRequest)} Started.");

        result.OrderImportRequest.Request.StoreName = this.GetCompanyNumber();

        parameter.JobLogger?.Debug($"{nameof(AddCompanyNumberToRequest)} Finished.");

        return result;
    }

    private string GetCompanyNumber()
    {
        var companyNumbers = this.integrationConnectorSettings.Prophet21Company.Split(
            new[] { ',', ';', '|' },
            StringSplitOptions.RemoveEmptyEntries
        );

        return companyNumbers.Any() ? companyNumbers[0] : "01";
    }
}
