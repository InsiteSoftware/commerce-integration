namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetPricingAndInventoryStock;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class DeserializeResponseTests
    : BaseForPipeTests<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
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
        var oePricingMultipleV4Response = new OePricingMultipleV4Response
        {
            Response = new Response
            {
                PriceOutV2Collection = new PriceOutV2Collection
                {
                    PriceOutV2s = new List<PriceOutV2>()
                }
            }
        };

        var result = new GetPricingAndInventoryStockResult
        {
            SerializedOePricingMultipleV4Response =
                CloudSuiteDistributionSerializationService.Serialize(oePricingMultipleV4Response)
        };

        result = this.RunExecute(result);

        result.OePricingMultipleV4Response.Should().BeEquivalentTo(oePricingMultipleV4Response);
    }
}
