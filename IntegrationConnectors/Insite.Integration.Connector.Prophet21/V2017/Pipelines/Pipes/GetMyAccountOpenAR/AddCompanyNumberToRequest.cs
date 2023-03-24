namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetMyAccountOpenAR;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddCompanyNumberToRequest
    : IPipe<GetMyAccountOpenARParameter, GetMyAccountOpenARResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddCompanyNumberToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 300;

    public GetMyAccountOpenARResult Execute(
        IUnitOfWork unitOfWork,
        GetMyAccountOpenARParameter parameter,
        GetMyAccountOpenARResult result
    )
    {
        var companyNumber = this.GetCompanyNumber();

        result.GetMyAccountOpenARRequest.Request.StoreName = companyNumber;

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
