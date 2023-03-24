namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetAccountsReceivable;

using Insite.Common.Helpers;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.SfCustomerSummary;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

public sealed class CreateRequest
    : IPipe<GetAccountsReceivableParameter, GetAccountsReceivableResult>
{
    public int Order => 100;

    public GetAccountsReceivableResult Execute(
        IUnitOfWork unitOfWork,
        GetAccountsReceivableParameter parameter,
        GetAccountsReceivableResult result
    )
    {
        int.TryParse(parameter.IntegrationConnection?.SystemNumber, out var companyNumber);

#pragma warning disable CS0618 // Type or member is obsolete
        result.SfCustomerSummaryRequest = new SfCustomerSummaryRequest
        {
            Request = new Request
            {
                CompanyNumber = companyNumber,
                OperatorInit = parameter.IntegrationConnection?.LogOn,
                OperatorPassword = EncryptionHelper.DecryptAes(
                    parameter.IntegrationConnection?.Password
                ),
                CustomerNumber = parameter.GetAgingBucketsParameter?.Customer?.ErpNumber
            }
        };
#pragma warning restore

        return result;
    }
}
