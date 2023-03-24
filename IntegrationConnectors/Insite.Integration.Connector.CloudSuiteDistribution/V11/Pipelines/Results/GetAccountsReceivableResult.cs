namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.SfCustomerSummary;

public class GetAccountsReceivableResult : PipeResultBase
{
    public SfCustomerSummaryRequest SfCustomerSummaryRequest { get; set; }

    public SfCustomerSummaryResponse SfCustomerSummaryResponse { get; set; }

    public string SerializedSfCustomerSummaryRequest { get; set; }

    public string SerializedSfCustomerSummaryResponse { get; set; }

    public GetAgingBucketsResult GetAgingBucketsResult { get; set; }
}
