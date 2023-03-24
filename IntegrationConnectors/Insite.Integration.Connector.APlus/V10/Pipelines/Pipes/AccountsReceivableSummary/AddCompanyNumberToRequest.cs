namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.AccountsReceivableSummary;

using System;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class AddCompanyNumberToRequest
    : IPipe<AccountsReceivableSummaryParameter, AccountsReceivableSummaryResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddCompanyNumberToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 300;

    public AccountsReceivableSummaryResult Execute(
        IUnitOfWork unitOfWork,
        AccountsReceivableSummaryParameter parameter,
        AccountsReceivableSummaryResult result
    )
    {
        var companyNumbers = this.integrationConnectorSettings.APlusCompany.Split(
            new[] { ',', ';', '|' },
            StringSplitOptions.RemoveEmptyEntries
        );

        var companyNumber = companyNumbers.Any() ? companyNumbers[0] : "1";

        result.AccountsReceivableSummaryRequest.CompanyNumber = companyNumber;

        return result;
    }
}
