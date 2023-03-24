namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.PriceAvailability;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;
using NUnit.Framework;
using FluentAssertions;

[TestFixture]
public class GetPricingFromResponseTests
    : BaseForPipeTests<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    private IList<Product> products;

    public override Type PipeType => typeof(GetPricingFromResponse);

    public override void SetUp()
    {
        this.WhenSiteContextCurrencyDtoIs(new CurrencyDto { CurrencySymbol = "$" });

        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_800()
    {
        this.pipe.Order.Should().Be(800);
    }

    [Test]
    public void Execute_Should_Not_Create_Pricing_Service_Result_When_Pricing_Service_Parameter_Product_Is_Null_And_Product_Not_Found_In_Repository()
    {
        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty)
            }
        };

        var result = this.RunExecute(parameter);

        result.PricingServiceResults.Should().BeEmpty();
    }

    [Test]
    public void Execute_Should_Not_Create_Pricing_Service_Result_When_ResponseItems_Do_Not_Match_PricingServiceParameter_Product()
    {
        var product = Some.Product().Build();

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(product.Id) { Product = product }
            }
        };

        var result = this.RunExecute(parameter);

        result.PricingServiceResults.Should().BeEmpty();
    }

    [Test]
    public void Execute_Should_Create_Pricing_Service_Result_For_Pricing_Service_Parameter_Product_And_Warehouse()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(product.Id) { Product = product, Warehouse = "MAIN" }
            }
        };

        var result = this.CreatePriceAvailabilityResult(product.ErpNumber, "MAIN", 10);

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults.Should().NotBeEmpty();
        result.PricingServiceResults.First().Value.UnitRegularPrice.Should().Be(10M);
    }

    [Test]
    public void Execute_Should_Create_Pricing_Service_Result_For_Pricing_Service_Parameter_Product_And_SiteContext_Warehouse()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(product.Id) { Product = product }
            }
        };

        var result = this.CreatePriceAvailabilityResult(product.ErpNumber, "SiteContext", 10);

        this.WhenSiteContextWarehouseDtoIs(new WarehouseDto { Name = "SiteContext" });

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults.Should().NotBeEmpty();
        result.PricingServiceResults.First().Value.UnitRegularPrice.Should().Be(10M);
    }

    [Test]
    public void Execute_Should_Create_Pricing_Service_Result_For_Pricing_Service_Parameter_Product_And_No_Matching_Warehouse()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(product.Id) { Product = product, Warehouse = "MAIN" }
            }
        };

        var result = this.CreatePriceAvailabilityResult(product.ErpNumber, "Other", 10);

        this.WhenSiteContextWarehouseDtoIs(new WarehouseDto { Name = "SiteContext" });

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults.Should().NotBeEmpty();
        result.PricingServiceResults.First().Value.UnitRegularPrice.Should().Be(10M);
    }

    [Test]
    public void Execute_Should_Create_Pricing_Service_Result_For_Pricing_Service_Parameter_ProductId()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(product.Id)
            }
        };

        var result = this.CreatePriceAvailabilityResult(product.ErpNumber, string.Empty, 10);

        this.WhenExists(product);

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults.Should().NotBeEmpty();
        result.PricingServiceResults.First().Value.UnitRegularPrice.Should().Be(10M);
    }

    [Test]
    public void Execute_Should_Only_Return_Pricing_Service_Result_For_Products_That_Do_Not_Have_Error_Message_In_Pricing_Availability_Response()
    {
        var productWithAvailability = Some.Product().WithErpNumber("123").Build();
        var productWithError = Some.Product().WithErpNumber("456").Build();
        var productWithAvailabilityPricingServiceParameter = new PricingServiceParameter(
            productWithAvailability.Id
        );
        var productWithErrorPricingServiceParameter = new PricingServiceParameter(
            productWithError.Id
        );

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                productWithAvailabilityPricingServiceParameter,
                productWithErrorPricingServiceParameter
            }
        };

        var result = this.CreatePriceAvailabilityResult(
            new List<ResponseItem>
            {
                new ResponseItem
                {
                    ItemNumber = productWithAvailability.ErpNumber,
                    WarehouseID = string.Empty,
                    Price = 19.99M
                },
                new ResponseItem
                {
                    ItemNumber = productWithError.ErpNumber,
                    ErrorMessage = "Invalid item number"
                }
            }
        );

        this.WhenExists(productWithAvailability);
        this.WhenExists(productWithError);

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults.Should().NotBeEmpty();
        result.PricingServiceResults.First().Value.UnitRegularPrice.Should().Be(19.99M);
        result.PricingServiceResults
            .Should()
            .NotContainKey(productWithErrorPricingServiceParameter);
    }

    protected override PriceAvailabilityResult GetDefaultResult()
    {
        return new PriceAvailabilityResult
        {
            PriceAvailabilityResponse = new PriceAvailabilityResponse { Response = new Response() }
        };
    }

    private PriceAvailabilityResult CreatePriceAvailabilityResult(
        string productErpNumber,
        string warehouse,
        decimal price
    )
    {
        return new PriceAvailabilityResult
        {
            PriceAvailabilityResponse = new PriceAvailabilityResponse
            {
                Response = new Response
                {
                    Items = new List<ResponseItem>
                    {
                        new ResponseItem
                        {
                            ItemNumber = productErpNumber,
                            WarehouseID = warehouse,
                            Price = price
                        }
                    }
                }
            }
        };
    }

    private PriceAvailabilityResult CreatePriceAvailabilityResult(List<ResponseItem> responseItems)
    {
        return new PriceAvailabilityResult
        {
            PriceAvailabilityResponse = new PriceAvailabilityResponse
            {
                Response = new Response { Items = responseItems }
            }
        };
    }

    private void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
