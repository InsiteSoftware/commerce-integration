namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.GetPricing;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetPricing;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class GetPricingFromResponsesTests : BaseForPipeTests<GetPricingParameter, GetPricingResult>
{
    private IList<Product> products;

    public override Type PipeType => typeof(GetPricingFromResponses);

    public override void SetUp()
    {
        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_500()
    {
        this.pipe.Order.Should().Be(500);
    }

    [Test]
    public void Execute_Should_Get_PricingServiceResult_For_PricingServiceParameter_Product()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty) { Product = product }
        };

        var parameter = new GetPricingParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };

        var result = new GetPricingResult
        {
            PriceQueryResponses = new List<PriceQuery>
            {
                new PriceQuery { CatalogNo = product.ErpNumber, NetPrice = 100 }
            }
        };

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults.Should().NotBeEmpty();
        result.PricingServiceResults.Should().ContainKey(pricingServiceParameters.First());
        result.PricingServiceResults
            .First()
            .Value.UnitRegularPrice.Should()
            .Be(result.PriceQueryResponses.First().NetPrice);
    }

    [Test]
    public void Execute_Should_Get_PricingServiceResult_For_PricingServiceParameter_ProductId()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(product.Id)
        };

        this.WhenExists(product);

        var parameter = new GetPricingParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };

        var result = new GetPricingResult
        {
            PriceQueryResponses = new List<PriceQuery>
            {
                new PriceQuery { CatalogNo = product.ErpNumber, NetPrice = 100 }
            }
        };

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults.Should().NotBeEmpty();
        result.PricingServiceResults.Should().ContainKey(pricingServiceParameters.First());
        result.PricingServiceResults
            .First()
            .Value.UnitRegularPrice.Should()
            .Be(result.PriceQueryResponses.First().NetPrice);
    }

    private void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
