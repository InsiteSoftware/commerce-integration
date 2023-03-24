namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.LineItemPriceAndAvailability;

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class GetInventoryFromResponseTests
    : BaseForPipeTests<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    private WarehouseDto siteContextWarehouseDto;

    private IList<Product> products;

    public override Type PipeType => typeof(GetInventoryFromResponse);

    public override void SetUp()
    {
        this.siteContextWarehouseDto = new WarehouseDto { Name = "SiteContextWarehouse" };

        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_700()
    {
        Assert.AreEqual(700, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Not_GetInventory_When_ItemAvailability_Is_Null()
    {
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty)
        {
            Product = Some.Product()
        };

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(
            pricingServiceParameter
        );
        var result = this.CreateGetLineItemPriceAndAvailabilityResult(null);

        result = this.RunExecute(parameter, result);

        Assert.AreEqual(0, result.PricingServiceResults.Count);
    }

    [Test]
    public void Execute_Should_Get_Product_From_Repository_When_Product_Is_Null()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            UnitOfMeasure = "EA",
            Warehouse = "MAIN"
        };

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(
            pricingServiceParameter
        );

        this.WhenExists(this.product);

        var result = this.RunExecute(parameter);

        var expectedWarehouseInfo = this.WarehouseInfos.FirstOrDefault(
            o =>
                o.PricingUOM.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.Warehouse.Equals(pricingServiceParameter.Warehouse)
        );

        Assert.AreEqual(
            Convert.ToDecimal(expectedWarehouseInfo.Qty),
            result.PricingServiceResults
                .First()
                .Value.GetInventoryResult.Inventories.First()
                .Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_GetInventory_By_PricingServiceParameter_UnitOfMeasure_And_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product,
            UnitOfMeasure = "EA",
            Warehouse = "MAIN"
        };

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(
            pricingServiceParameter
        );

        var result = this.RunExecute(parameter);

        var expectedWarehouseInfo = this.WarehouseInfos.FirstOrDefault(
            o =>
                o.PricingUOM.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.Warehouse.Equals(pricingServiceParameter.Warehouse)
        );

        Assert.AreEqual(
            Convert.ToDecimal(expectedWarehouseInfo.Qty),
            result.PricingServiceResults
                .First()
                .Value.GetInventoryResult.Inventories.First()
                .Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_GetInventory_By_PricingServiceParameter_UnitOfMeasure_And_SiteContext_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product,
            UnitOfMeasure = "EA"
        };

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(
            pricingServiceParameter
        );

        this.WhenSiteContextWarehouseDtoIs(this.siteContextWarehouseDto);

        var result = this.RunExecute(parameter);

        var expectedWarehouseInfo = this.WarehouseInfos.FirstOrDefault(
            o =>
                o.PricingUOM.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.Warehouse.Equals(this.siteContextWarehouseDto.Name)
        );

        Assert.AreEqual(
            Convert.ToDecimal(expectedWarehouseInfo.Qty),
            result.PricingServiceResults
                .First()
                .Value.GetInventoryResult.Inventories.First()
                .Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_GetInventory_By_PricingServiceParameter_UnitOfMeasure_And_Empty_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product,
            UnitOfMeasure = "EA"
        };

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(
            pricingServiceParameter
        );

        var result = this.RunExecute(parameter);

        var expectedWarehouseInfo = this.WarehouseInfos.FirstOrDefault(
            o =>
                o.PricingUOM.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.Warehouse.Equals(string.Empty)
        );

        Assert.AreEqual(
            Convert.ToDecimal(expectedWarehouseInfo.Qty),
            result.PricingServiceResults
                .First()
                .Value.GetInventoryResult.Inventories.First()
                .Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_GetInventory_By_Any_UnitOfMeasure_And_PricingServiceParameter_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product,
            Warehouse = "MAIN"
        };

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(
            pricingServiceParameter
        );

        var result = this.RunExecute(parameter);

        var expectedWarehouseInfo = this.WarehouseInfos.FirstOrDefault(
            o => o.Warehouse.Equals(pricingServiceParameter.Warehouse)
        );

        Assert.AreEqual(
            Convert.ToDecimal(expectedWarehouseInfo.Qty),
            result.PricingServiceResults
                .First()
                .Value.GetInventoryResult.Inventories.First()
                .Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_GetInventory_By_Any_UnitOfMeasure_And_SiteContext_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product
        };

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(
            pricingServiceParameter
        );

        this.WhenSiteContextWarehouseDtoIs(this.siteContextWarehouseDto);

        var result = this.RunExecute(parameter);

        var expectedWarehouseInfo = this.WarehouseInfos.FirstOrDefault(
            o => o.Warehouse.Equals(this.siteContextWarehouseDto.Name)
        );

        Assert.AreEqual(
            Convert.ToDecimal(expectedWarehouseInfo.Qty),
            result.PricingServiceResults
                .First()
                .Value.GetInventoryResult.Inventories.First()
                .Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_GetInventory_By_Any_UnitOfMeasure_And_Empty_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product
        };

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(
            pricingServiceParameter
        );

        var result = this.RunExecute(parameter);

        var expectedWarehouseInfo = this.WarehouseInfos.FirstOrDefault(
            o => o.Warehouse.Equals(string.Empty)
        );

        Assert.AreEqual(
            Convert.ToDecimal(expectedWarehouseInfo.Qty),
            result.PricingServiceResults
                .First()
                .Value.GetInventoryResult.Inventories.First()
                .Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_Get_WarehouseQtyOnHandDtos_By_UnitOfMeasure_And_All_Warehouses()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product,
            UnitOfMeasure = "EA",
            Warehouse = "MAIN"
        };

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(
            pricingServiceParameter
        );

        var result = this.RunExecute(parameter);

        var expectedWarehouseInfos = this.WarehouseInfos.Where(
            o => o.PricingUOM.Equals(pricingServiceParameter.UnitOfMeasure)
        );

        Assert.IsTrue(
            expectedWarehouseInfos.All(
                o =>
                    result.PricingServiceResults
                        .First()
                        .Value.GetInventoryResult.Inventories.First()
                        .Value.WarehouseQtyOnHandDtos.Any(
                            p => o.Warehouse.Equals(p.Name) && o.Qty.Equals(p.QtyOnHand.ToString())
                        )
            )
        );
    }

    protected override LineItemPriceAndAvailabilityResult GetDefaultResult()
    {
        return new LineItemPriceAndAvailabilityResult
        {
            LineItemPriceAndAvailabilityResponse = new LineItemPriceAndAvailabilityResponse
            {
                ItemAvailability = new List<ResponseItemAvailability>
                {
                    new ResponseItemAvailability
                    {
                        Item = new ResponseItem { ItemNumber = this.product.ErpNumber },
                        WarehouseInfo = this.WarehouseInfos
                    }
                }
            }
        };
    }

    protected LineItemPriceAndAvailabilityParameter CreateGetLineItemPriceAndAvailabilityParameter(
        params PricingServiceParameter[] pricingServiceParameters
    )
    {
        return new LineItemPriceAndAvailabilityParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
    }

    protected LineItemPriceAndAvailabilityResult CreateGetLineItemPriceAndAvailabilityResult(
        List<ResponseItemAvailability> responseItemAvailability
    )
    {
        return new LineItemPriceAndAvailabilityResult
        {
            LineItemPriceAndAvailabilityResponse = new LineItemPriceAndAvailabilityResponse()
            {
                ItemAvailability = responseItemAvailability
            }
        };
    }

    protected void WhenExists(Product product)
    {
        this.products.Add(product);
    }

    protected Product product = Some.Product().WithErpNumber("ABC123").Build();

    protected List<ResponseWarehouseInfo> WarehouseInfos =>
        new List<ResponseWarehouseInfo>
        {
            new ResponseWarehouseInfo
            {
                PricingUOM = "BX",
                Qty = "10",
                Warehouse = "MAIN"
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "CS",
                Qty = "15",
                Warehouse = "MAIN"
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "EA",
                Qty = "20",
                Warehouse = "MAIN"
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "BX",
                Qty = "30",
                Warehouse = this.siteContextWarehouseDto.Name
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "CS",
                Qty = "35",
                Warehouse = this.siteContextWarehouseDto.Name
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "EA",
                Qty = "40",
                Warehouse = this.siteContextWarehouseDto.Name
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "BX",
                Qty = "50",
                Warehouse = string.Empty
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "CS",
                Qty = "55",
                Warehouse = string.Empty
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "EA",
                Qty = "60",
                Warehouse = string.Empty
            }
        };
}
