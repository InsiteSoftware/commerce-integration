namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.CustomerSummary;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class AddCompanyNumberToRequest
    : IPipe<CustomerSummaryParameter, CustomerSummaryResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddCompanyNumberToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 300;

    public CustomerSummaryResult Execute(
        IUnitOfWork unitOfWork,
        CustomerSummaryParameter parameter,
        CustomerSummaryResult result
    )
    {
        result.CustomerSummaryRequest.Request.Company = this.GetCompanyNumber();

        return result;
    }

    private string GetCompanyNumber()
    {
        var companyNumbers = this.integrationConnectorSettings.FactsCompanyNumber.Split(
            new[] { ',', ';', '|' },
            StringSplitOptions.RemoveEmptyEntries
        );

        return companyNumbers.Any() ? companyNumbers[0] : "01";
    }
}
