namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.GetPricing;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetPricing;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class SerializeRequestsTests : BaseForPipeTests<GetPricingParameter, GetPricingResult>
{
    public override Type PipeType => typeof(SerializeRequests);

    public override void SetUp() { }

    [Test]
    public void Order_Is_200()
    {
        this.pipe.Order.Should().Be(200);
    }

    [Test]
    public void Execute_Should_Serialize_Requests()
    {
        var result = new GetPricingResult
        {
            PriceQueryRequests = new List<PriceQuery>
            {
                new PriceQuery { CatalogNo = "Prod1" },
                new PriceQuery { CatalogNo = "Prod2" }
            }
        };

        result = this.RunExecute(result);

        result.SerializedPriceQueryRequests.Should().NotBeEmpty();
        result.SerializedPriceQueryRequests
            .Should()
            .ContainEquivalentOf(
                IfsAurenaSerializationService.Serialize(result.PriceQueryRequests[0])
            );
        result.SerializedPriceQueryRequests
            .Should()
            .ContainEquivalentOf(
                IfsAurenaSerializationService.Serialize(result.PriceQueryRequests[1])
            );
    }
}
