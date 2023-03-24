namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetAccountsReceivable;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.SfCustomerSummary;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetAccountsReceivable;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class DeserializeResponseTests
    : BaseForPipeTests<GetAccountsReceivableParameter, GetAccountsReceivableResult>
{
    public override Type PipeType => typeof(DeserializeResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_400()
    {
        this.pipe.Order.Should().Be(400);
    }

    [Test]
    public void Execute_Should_Deserialize_Response()
    {
        var sfCustomerSummary = new SfCustomerSummaryResponse
        {
            Response = new Response
            {
                CustomerSummaryCollection = new CustomerSummaryCollection
                {
                    CustomerSummaries = new List<CustomerSummary>()
                }
            }
        };

        var result = new GetAccountsReceivableResult
        {
            SerializedSfCustomerSummaryResponse =
                CloudSuiteDistributionSerializationService.Serialize(sfCustomerSummary)
        };

        result = this.RunExecute(result);

        result.SfCustomerSummaryResponse.Should().BeEquivalentTo(sfCustomerSummary);
    }
}
