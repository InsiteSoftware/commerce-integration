namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddCompanyNumberToRequest : IPipe<GetCartSummaryParameter, GetCartSummaryResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddCompanyNumberToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 500;

    public GetCartSummaryResult Execute(
        IUnitOfWork unitOfWork,
        GetCartSummaryParameter parameter,
        GetCartSummaryResult result
    )
    {
        result.GetCartSummaryRequest.Request.StoreName = this.GetCompanyNumber();

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
