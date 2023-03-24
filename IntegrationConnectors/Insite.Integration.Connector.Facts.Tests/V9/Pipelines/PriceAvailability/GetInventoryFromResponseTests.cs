namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.PriceAvailability;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;
using NUnit.Framework;
using FluentAssertions;

[TestFixture]
public class GetInventoryFromResponseTests
    : BaseForPipeTests<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    private IList<Product> products;

    private IList<Warehouse> warehouses;

    public override Type PipeType => typeof(GetInventoryFromResponse);

    public override void SetUp()
    {
        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);

        this.warehouses = new List<Warehouse>();
        this.fakeUnitOfWork.SetupEntityList(this.warehouses);
    }

    [Test]
    public void Order_Is_900()
    {
        this.pipe.Order.Should().Be(900);
    }

    [Test]
    public void Execute_Should_Get_Product_From_Repository_When_Product_Is_Null()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Warehouse = this.warehouse.Name
        };

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter> { pricingServiceParameter }
        };

        this.WhenExists(this.product);

        var result = this.RunExecute(parameter);

        var expectedResponseItem = this.ResponseItems.FirstOrDefault(
            o => o.WarehouseID.Equals(pricingServiceParameter.Warehouse)
        );

        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.QtyOnHand.Should()
            .Be(expectedResponseItem.QuantityAvailable);
    }

    [Test]
    public void Execute_Should_GetInventory_By_PricingServiceParameter_Product_And_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product,
            Warehouse = this.warehouse.Name
        };

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter> { pricingServiceParameter }
        };

        var result = this.RunExecute(parameter);

        var expectedResponseItem = this.ResponseItems.FirstOrDefault(
            o => o.WarehouseID.Equals(pricingServiceParameter.Warehouse)
        );

        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.QtyOnHand.Should()
            .Be(expectedResponseItem.QuantityAvailable);
    }

    [Test]
    public void Execute_Should_GetInventory_By_PricingServiceParameter_Product_And_SiteContext_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product
        };

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter> { pricingServiceParameter }
        };

        this.WhenSiteContextWarehouseDtoIs(this.siteContextWarehouse);

        var result = this.RunExecute(parameter);

        var expectedResponseItem = this.ResponseItems.FirstOrDefault(
            o => o.WarehouseID.Equals(this.siteContextWarehouse.Name)
        );

        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.QtyOnHand.Should()
            .Be(expectedResponseItem.QuantityAvailable);
    }

    [Test]
    public void Execute_Should_GetInventory_By_PricingServiceParameter_Product_And_Empty_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product
        };

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter> { pricingServiceParameter }
        };

        var result = this.RunExecute(parameter);

        var expectedResponseItem = this.ResponseItems.FirstOrDefault(
            o => o.WarehouseID.Equals(string.Empty)
        );

        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.QtyOnHand.Should()
            .Be(expectedResponseItem.QuantityAvailable);
    }

    [Test]
    public void Execute_Should_Get_WarehouseQtyOnHandDtos_By_All_Warehouses()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product,
            Warehouse = this.warehouse.Name
        };

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter> { pricingServiceParameter }
        };

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            this.ResponseItems.All(
                o =>
                    result.PricingServiceResults
                        .First()
                        .Value.GetInventoryResult.Inventories.First()
                        .Value.WarehouseQtyOnHandDtos.Any(
                            p =>
                                o.WarehouseID.Equals(p.Name)
                                && o.QuantityAvailable.Equals(p.QtyOnHand)
                        )
            )
        );
    }

    [Test]
    public void Execute_Should_GetInventory_By_GetInventoryParameter_ProductId_And_WarehouseId()
    {
        var getInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { this.product.Id },
            WarehouseId = this.warehouse.Id
        };

        var parameter = new PriceAvailabilityParameter
        {
            GetInventoryParameter = getInventoryParameter
        };

        this.WhenExists(this.product);
        this.WhenExists(this.warehouse);

        var result = this.RunExecute(parameter);

        var expectedResponseItem = this.ResponseItems.FirstOrDefault(
            o => o.WarehouseID.Equals(this.warehouse.Name)
        );

        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(expectedResponseItem.QuantityAvailable);
    }

    [Test]
    public void Execute_Should_GetInventory_By_GetInventoryParameter_ProductId_And_SiteContext_Warehouse()
    {
        var getInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { this.product.Id }
        };

        var parameter = new PriceAvailabilityParameter
        {
            GetInventoryParameter = getInventoryParameter
        };

        this.WhenExists(this.product);
        this.WhenSiteContextWarehouseDtoIs(this.siteContextWarehouse);

        var result = this.RunExecute(parameter);

        var expectedResponseItem = this.ResponseItems.FirstOrDefault(
            o => o.WarehouseID.Equals(this.siteContextWarehouse.Name)
        );

        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(expectedResponseItem.QuantityAvailable);
    }

    [Test]
    public void Execute_Should_GetInventory_By_GetInventoryParameter_ProductId_And_Empty_Warehouse()
    {
        var getInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { this.product.Id }
        };

        var parameter = new PriceAvailabilityParameter
        {
            GetInventoryParameter = getInventoryParameter
        };

        this.WhenExists(this.product);

        var result = this.RunExecute(parameter);

        var expectedResponseItem = this.ResponseItems.FirstOrDefault(
            o => o.WarehouseID.Equals(string.Empty)
        );

        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(expectedResponseItem.QuantityAvailable);
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
                    QuantityAvailable = 15
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

        CollectionAssert.IsNotEmpty(result.PricingServiceResults);

        result.PricingServiceResults
            .First(o => o.Key.ProductId == productWithAvailability.Id)
            .Value.GetInventoryResult.Inventories.First(o => o.Key == productWithAvailability.Id)
            .Value.QtyOnHand.Should()
            .Be(15);
        result.PricingServiceResults
            .Should()
            .NotContainKey(productWithErrorPricingServiceParameter);
    }

    protected override PriceAvailabilityResult GetDefaultResult()
    {
        return new PriceAvailabilityResult
        {
            PriceAvailabilityResponse = new PriceAvailabilityResponse
            {
                Response = new Response { Items = this.ResponseItems }
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

    private void WhenExists(Warehouse warehouse)
    {
        this.warehouses.Add(warehouse);
    }

    protected Product product = Some.Product().WithErpNumber("ABC123").Build();

    protected Warehouse warehouse = Some.Warehouse().WithName("MAIN").Build();

    protected WarehouseDto siteContextWarehouse = new WarehouseDto
    {
        Name = "SiteContextWarehouse"
    };

    protected List<ResponseItem> ResponseItems =>
        new List<ResponseItem>
        {
            new ResponseItem
            {
                ItemNumber = this.product.ErpNumber,
                QuantityAvailable = 10,
                WarehouseID = "MAIN"
            },
            new ResponseItem
            {
                ItemNumber = this.product.ErpNumber,
                QuantityAvailable = 40,
                WarehouseID = this.siteContextWarehouse.Name
            },
            new ResponseItem
            {
                ItemNumber = this.product.ErpNumber,
                QuantityAvailable = 60,
                WarehouseID = string.Empty
            }
        };
}
