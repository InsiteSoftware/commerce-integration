namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetAccountsReceivable;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.SfCustomerSummary;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetAccountsReceivable;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class GetAgingBucketsFromResponseTests
    : BaseForPipeTests<GetAccountsReceivableParameter, GetAccountsReceivableResult>
{
    public override Type PipeType => typeof(GetAgingBucketsFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_600()
    {
        this.pipe.Order.Should().Be(600);
    }

    [Test]
    public void Execute_Should_Return_Result_If_CustomerSummaries_Are_Null()
    {
        Action runExecuteAction = () => this.RunExecute();

        runExecuteAction.Should().NotThrow();
    }

    [Test]
    public void Execute_Should_Get_Aging_Buckets()
    {
        var customerSummaries = new List<CustomerSummary>
        {
            new CustomerSummary
            {
                TradeBillPrdAmt = 0,
                TradeAgePrd1 = 22,
                TradeAgePrd2 = 5,
                TradeAgePrd3 = 0,
                TradeAgePrd4 = 1,
                TradeFutureAmt = 25
            }
        };
        var sfCustomerSummaryResponse = new SfCustomerSummaryResponse()
        {
            Response = new Response
            {
                CustomerSummaryCollection = new CustomerSummaryCollection
                {
                    CustomerSummaries = customerSummaries
                }
            }
        };

        var result = new GetAccountsReceivableResult
        {
            SfCustomerSummaryResponse = sfCustomerSummaryResponse
        };
        result = this.RunExecute(result);

        result.GetAgingBucketsResult.AgingBuckets[0]
            .Should()
            .Be(customerSummaries[0].TradeBillPrdAmt);
        result.GetAgingBucketsResult.AgingBuckets[1].Should().Be(customerSummaries[0].TradeAgePrd1);
        result.GetAgingBucketsResult.AgingBuckets[2].Should().Be(customerSummaries[0].TradeAgePrd2);
        result.GetAgingBucketsResult.AgingBuckets[3].Should().Be(customerSummaries[0].TradeAgePrd3);
        result.GetAgingBucketsResult.AgingBuckets[4].Should().Be(customerSummaries[0].TradeAgePrd4);
        result.GetAgingBucketsResult.AgingBucketFuture
            .Should()
            .Be(customerSummaries[0].TradeFutureAmt);
    }
}
