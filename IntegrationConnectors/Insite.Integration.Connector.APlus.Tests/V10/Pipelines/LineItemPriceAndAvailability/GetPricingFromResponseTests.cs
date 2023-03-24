namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.LineItemPriceAndAvailability;

using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Utilities;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class GetPricingFromResponseTests
    : BaseForPipeTests<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    private Mock<ICurrencyFormatProvider> currencyFormatProvider;

    private CurrencyDto siteContextCurrencyDto;

    private WarehouseDto siteContextWarehouseDto;

    private IList<Product> products;

    public override Type PipeType => typeof(GetPricingFromResponse);

    public override void SetUp()
    {
        this.currencyFormatProvider = this.container.GetMock<ICurrencyFormatProvider>();

        this.siteContextCurrencyDto = new CurrencyDto();
        this.WhenSiteContextCurrencyDtoIs(this.siteContextCurrencyDto);

        this.siteContextWarehouseDto = new WarehouseDto { Name = "SiteContextWarehouse" };

        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Not_GetPricing_When_ItemAvailability_Is_Null()
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
            Convert.ToDecimal(expectedWarehouseInfo.Price),
            result.PricingServiceResults.First().Value.UnitRegularPrice
        );
    }

    [Test]
    public void Execute_Should_GetPricing_By_PricingServiceParameter_UnitOfMeasure_And_Warehouse()
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
            Convert.ToDecimal(expectedWarehouseInfo.Price),
            result.PricingServiceResults.First().Value.UnitRegularPrice
        );
    }

    [Test]
    public void Execute_Should_GetPricing_By_PricingServiceParameter_UnitOfMeasure_And_SiteContext_Warehouse()
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
            Convert.ToDecimal(expectedWarehouseInfo.Price),
            result.PricingServiceResults.First().Value.UnitRegularPrice
        );
    }

    [Test]
    public void Execute_Should_GetPricing_By_PricingServiceParameter_UnitOfMeasure_And_Empty_Warehouse()
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
            Convert.ToDecimal(expectedWarehouseInfo.Price),
            result.PricingServiceResults.First().Value.UnitRegularPrice
        );
    }

    [Test]
    public void Execute_Should_GetPricing_By_Any_UnitOfMeasure_And_PricingServiceParameter_Warehouse()
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
            Convert.ToDecimal(expectedWarehouseInfo.Price),
            result.PricingServiceResults.First().Value.UnitRegularPrice
        );
    }

    [Test]
    public void Execute_Should_GetPricing_By_Any_UnitOfMeasure_And_SiteContext_Warehouse()
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
            Convert.ToDecimal(expectedWarehouseInfo.Price),
            result.PricingServiceResults.First().Value.UnitRegularPrice
        );
    }

    [Test]
    public void Execute_Should_GetPricing_By_Any_UnitOfMeasure_And_Empty_Warehouse()
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
            Convert.ToDecimal(expectedWarehouseInfo.Price),
            result.PricingServiceResults.First().Value.UnitRegularPrice
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
                Price = "10",
                Warehouse = "MAIN"
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "CS",
                Price = "15",
                Warehouse = "MAIN"
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "EA",
                Price = "20",
                Warehouse = "MAIN"
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "BX",
                Price = "30",
                Warehouse = this.siteContextWarehouseDto.Name
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "CS",
                Price = "35",
                Warehouse = this.siteContextWarehouseDto.Name
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "EA",
                Price = "40",
                Warehouse = this.siteContextWarehouseDto.Name
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "BX",
                Price = "50",
                Warehouse = string.Empty
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "CS",
                Price = "55",
                Warehouse = string.Empty
            },
            new ResponseWarehouseInfo
            {
                PricingUOM = "EA",
                Price = "60",
                Warehouse = string.Empty
            }
        };
}
