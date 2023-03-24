namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.SFCustomerSummary;

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFCustomerSummary;

[TestFixture]
public class GetAgingBucketsFromResponseTests
    : BaseForPipeTests<SFCustomerSummaryParameter, SFCustomerSummaryResult>
{
    public override Type PipeType => typeof(GetAgingBucketsFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Aging_Buckets()
    {
        var customerSummaries = new List<CustomerSummary>
        {
            new CustomerSummary
            {
                Tradebillprdamt = 0,
                Tradeageprd1 = 22,
                Tradeageprd2 = 5,
                Tradeageprd3 = 0,
                Tradeageprd4 = 1,
                Tradefutureamt = 25
            }
        };
        var sfCustomerSummaryResponse = new SFCustomerSummaryResponse()
        {
            Response = new Response
            {
                CustomerSummaryCollection = new CustomerSummaryCollection
                {
                    CustomerSummaries = customerSummaries
                }
            }
        };

        var result = new SFCustomerSummaryResult
        {
            SFCustomerSummaryResponse = sfCustomerSummaryResponse
        };
        result = this.RunExecute(result);

        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[0],
            customerSummaries[0].Tradebillprdamt
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[1],
            customerSummaries[0].Tradeageprd1
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[2],
            customerSummaries[0].Tradeageprd2
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[3],
            customerSummaries[0].Tradeageprd3
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBuckets[4],
            customerSummaries[0].Tradeageprd4
        );
        Assert.AreEqual(
            result.GetAgingBucketsResult.AgingBucketFuture,
            customerSummaries[0].Tradefutureamt
        );
    }
}
