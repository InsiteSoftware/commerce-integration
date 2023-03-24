namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.GetPricing;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetPricing;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class DeserializeResponsesTests : BaseForPipeTests<GetPricingParameter, GetPricingResult>
{
    public override Type PipeType => typeof(DeserializeResponses);

    public override void SetUp() { }

    [Test]
    public void Order_Is_400()
    {
        this.pipe.Order.Should().Be(400);
    }

    [Test]
    public void Execute_Should_Deserialize_Responses()
    {
        var priceQueryResponses = new List<PriceQuery>
        {
            new PriceQuery { CatalogNo = "1" },
            new PriceQuery { CatalogNo = "2" }
        };

        var result = new GetPricingResult
        {
            SerializedPriceQueryResponses = priceQueryResponses
                .Select(o => IfsAurenaSerializationService.Serialize(o))
                .ToList()
        };

        result = this.RunExecute(result);

        result.PriceQueryResponses.Should().NotBeEmpty();
        result.PriceQueryResponses.Should().BeEquivalentTo(priceQueryResponses);
    }
}
