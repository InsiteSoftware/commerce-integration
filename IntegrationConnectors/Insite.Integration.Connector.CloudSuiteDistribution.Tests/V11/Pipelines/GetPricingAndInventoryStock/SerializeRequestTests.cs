namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetPricingAndInventoryStock;

using System;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class SerializeRequestTests
    : BaseForPipeTests<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
{
    public override Type PipeType => typeof(SerializeRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_200()
    {
        this.pipe.Order.Should().Be(200);
    }

    [Test]
    public void Execute_Should_Serialize_Requests()
    {
        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Request = new OePricingMultipleV4Request { Request = new Request() }
        };

        result = this.RunExecute(result);

        result.SerializedOePricingMultipleV4Request
            .Should()
            .BeEquivalentTo(
                CloudSuiteDistributionSerializationService.Serialize(
                    result.OePricingMultipleV4Request
                )
            );
    }
}
