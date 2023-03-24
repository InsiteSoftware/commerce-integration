namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddCompanyNumberToRequest : IPipe<GetItemPriceParameter, GetItemPriceResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddCompanyNumberToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 500;

    public GetItemPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetItemPriceParameter parameter,
        GetItemPriceResult result
    )
    {
        var companyNumber = this.GetCompanyNumber();

        result.GetItemPriceRequest.Request.StoreName = companyNumber;

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
