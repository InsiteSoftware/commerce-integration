namespace Insite.Integration.Connector.SXe.Tests.V10.Pipelines.Pipes.OEPricingMultipleV4;

using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.OEPricingMultipleV4;

[TestFixture]
public class GetPricingFromResponseTests
    : BaseForPipeTests<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    private CurrencyDto siteContextCurrencyDto;

    private WarehouseDto siteContextWarehouseDto;

    public override Type PipeType => typeof(GetPricingFromResponse);

    public override void SetUp()
    {
        this.siteContextCurrencyDto = new CurrencyDto();
        this.WhenSiteContextCurrencyDtoIs(this.siteContextCurrencyDto);

        this.siteContextWarehouseDto = new WarehouseDto { Name = "SiteContextWarehouse" };
    }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Not_GetPricing_When_Outprice_Is_Null()
    {
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty)
        {
            Product = Some.Product()
        };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);
        var result = this.CreateGetOEPricingMultipleV4Result(null, null);

        result = this.RunExecute(parameter, result);

        Assert.AreEqual(0, result.PricingServiceResults.Count);
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

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);

        var result = this.RunExecute(parameter);

        var expectedOutPrice3 = this.OutPrice3s.FirstOrDefault(
            o =>
                o.UnitOfMeasure.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.Warehouse.Equals(pricingServiceParameter.Warehouse)
        );

        Assert.AreEqual(
            expectedOutPrice3.Price,
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

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);

        this.WhenSiteContextWarehouseDtoIs(this.siteContextWarehouseDto);

        var result = this.RunExecute(parameter);

        var expectedOutPrice3 = this.OutPrice3s.FirstOrDefault(
            o =>
                o.UnitOfMeasure.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.Warehouse.Equals(this.siteContextWarehouseDto.Name)
        );

        Assert.AreEqual(
            expectedOutPrice3.Price,
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

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);

        var result = this.RunExecute(parameter);

        var expectedOutPrice3 = this.OutPrice3s.FirstOrDefault(
            o =>
                o.UnitOfMeasure.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.Warehouse.Equals(string.Empty)
        );

        Assert.AreEqual(
            expectedOutPrice3.Price,
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

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);

        var result = this.RunExecute(parameter);

        var expectedOutPrice3 = this.OutPrice3s.FirstOrDefault(
            o => o.Warehouse.Equals(pricingServiceParameter.Warehouse)
        );

        Assert.AreEqual(
            expectedOutPrice3.Price,
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

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);

        this.WhenSiteContextWarehouseDtoIs(this.siteContextWarehouseDto);

        var result = this.RunExecute(parameter);

        var expectedOutPrice3 = this.OutPrice3s.FirstOrDefault(
            o => o.Warehouse.Equals(this.siteContextWarehouseDto.Name)
        );

        Assert.AreEqual(
            expectedOutPrice3.Price,
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

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);

        var result = this.RunExecute(parameter);

        var expectedOutPrice3 = this.OutPrice3s.FirstOrDefault(
            o => o.Warehouse.Equals(string.Empty)
        );

        Assert.AreEqual(
            expectedOutPrice3.Price,
            result.PricingServiceResults.First().Value.UnitRegularPrice
        );
    }

    [Test]
    public void Execute_Should_GetPriceBreaks_By_PricingServiceParameter_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product,
            Warehouse = "MAIN"
        };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);

        var result = this.GetDefaultResult();
        result.OEPricingMultipleV4Response.Outpricebreak = this.OutPriceBreaks3;

        result = this.RunExecute(parameter, result);

        var expectedOutPriceBreak3 = this.OutPriceBreaks3.FirstOrDefault(
            o => o.Warehouse.Equals(pricingServiceParameter.Warehouse)
        );

        Assert.AreEqual(
            expectedOutPriceBreak3.PriceBreak1,
            result.PricingServiceResults.First().Value.UnitRegularPrice
        );
    }

    [Test]
    public void Execute_Should_GetPriceBreaks_By_SiteContext_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product
        };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);

        this.WhenSiteContextWarehouseDtoIs(this.siteContextWarehouseDto);

        var result = this.GetDefaultResult();
        result.OEPricingMultipleV4Response.Outpricebreak = this.OutPriceBreaks3;

        result = this.RunExecute(parameter, result);

        var expectedOutPriceBreak3 = this.OutPriceBreaks3.FirstOrDefault(
            o => o.Warehouse.Equals(this.siteContextWarehouseDto.Name)
        );

        Assert.AreEqual(
            expectedOutPriceBreak3.PriceBreak1,
            result.PricingServiceResults.First().Value.UnitRegularPrice
        );
    }

    [Test]
    public void Execute_Should_GetPriceBreaks_By_Empty_Warehouse()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product
        };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);

        var result = this.GetDefaultResult();
        result.OEPricingMultipleV4Response.Outpricebreak = this.OutPriceBreaks3;

        result = this.RunExecute(parameter, result);

        var expectedOutPriceBreak3 = this.OutPriceBreaks3.FirstOrDefault(
            o => o.Warehouse.Equals(string.Empty)
        );

        Assert.AreEqual(
            expectedOutPriceBreak3.PriceBreak1,
            result.PricingServiceResults.First().Value.UnitRegularPrice
        );
    }

    [Test]
    public void Execute_Should_GetPricing_From_OutPriceBreak_DiscountPercent_PercentDiscountType_When_OutPriceBreak_Exists()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product,
            Warehouse = "MAIN",
            UnitOfMeasure = "EA"
        };

        var outPrice3 = new Outprice3
        {
            ProductCode = this.product.ErpNumber,
            Warehouse = "MAIN",
            UnitOfMeasure = "EA",
            Quantity = 1,
            Price = 10,
            DiscountType = "%"
        };

        var outPriceBreak3 = new Outpricebreak3
        {
            ProductCode = this.product.ErpNumber,
            Warehouse = "MAIN",
            DiscountPercent1 = 10,
            QuantityBreak1 = 5,
            DiscountPercent2 = 15
        };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);

        var result = this.GetDefaultResult();
        result.OEPricingMultipleV4Response.Outprice = new List<Outprice3> { outPrice3 };
        result.OEPricingMultipleV4Response.Outpricebreak = new List<Outpricebreak3>
        {
            outPriceBreak3
        };

        result = this.RunExecute(parameter, result);

        Assert.AreEqual(9, result.PricingServiceResults.First().Value.UnitRegularPrice);
        Assert.IsTrue(
            result.PricingServiceResults
                .First()
                .Value.UnitRegularBreakPrices.Any(o => o.BreakQty == 1 && o.Price == 9)
        );
        Assert.IsTrue(
            result.PricingServiceResults
                .First()
                .Value.UnitRegularBreakPrices.Any(o => o.BreakQty == 5 && o.Price == 8.50M)
        );
    }

    [Test]
    public void Execute_Should_GetPricing_From_OutPriceBreak_DiscountPercent_DollarDiscountType_When_OutPriceBreak_Exists()
    {
        var pricingServiceParameter = new PricingServiceParameter(this.product.Id)
        {
            Product = this.product,
            Warehouse = "MAIN",
            UnitOfMeasure = "EA"
        };

        var outPrice3 = new Outprice3
        {
            ProductCode = this.product.ErpNumber,
            Warehouse = "MAIN",
            UnitOfMeasure = "EA",
            Quantity = 1,
            Price = 10,
            DiscountType = "$"
        };

        var outPriceBreak3 = new Outpricebreak3
        {
            ProductCode = this.product.ErpNumber,
            Warehouse = "MAIN",
            DiscountPercent1 = 1,
            QuantityBreak1 = 5,
            DiscountPercent2 = 2
        };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);

        var result = this.GetDefaultResult();
        result.OEPricingMultipleV4Response.Outprice = new List<Outprice3> { outPrice3 };
        result.OEPricingMultipleV4Response.Outpricebreak = new List<Outpricebreak3>
        {
            outPriceBreak3
        };

        result = this.RunExecute(parameter, result);

        Assert.AreEqual(9, result.PricingServiceResults.First().Value.UnitRegularPrice);
        Assert.IsTrue(
            result.PricingServiceResults
                .First()
                .Value.UnitRegularBreakPrices.Any(o => o.BreakQty == 1 && o.Price == 9)
        );
        Assert.IsTrue(
            result.PricingServiceResults
                .First()
                .Value.UnitRegularBreakPrices.Any(o => o.BreakQty == 5 && o.Price == 8)
        );
    }

    protected override OEPricingMultipleV4Result GetDefaultResult()
    {
        return new OEPricingMultipleV4Result
        {
            OEPricingMultipleV4Response = new OEPricingMultipleV4Response
            {
                Outprice = this.OutPrice3s
            }
        };
    }

    protected OEPricingMultipleV4Parameter CreateGetOEPricingMultipleV4Parameter(
        params PricingServiceParameter[] pricingServiceParameters
    )
    {
        return new OEPricingMultipleV4Parameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
    }

    protected OEPricingMultipleV4Result CreateGetOEPricingMultipleV4Result(
        Outprice3[] outPrice3s,
        Outpricebreak3[] outPriceBreak3s
    )
    {
        return new OEPricingMultipleV4Result
        {
            OEPricingMultipleV4Response = new OEPricingMultipleV4Response()
            {
                Outprice = outPrice3s?.ToList(),
                Outpricebreak = outPriceBreak3s?.ToList()
            }
        };
    }

    protected void VerifyGetProductWasCalled()
    {
        this.unitOfWork.Verify(o => o.GetRepository<Product>(), Times.Once);
    }

    protected Product product = Some.Product().WithErpNumber("ABC123").Build();

    protected List<Outprice3> OutPrice3s =>
        new List<Outprice3>
        {
            new Outprice3
            {
                ProductCode = this.product.ErpNumber,
                Quantity = 1,
                UnitOfMeasure = "BX",
                Price = 10,
                Warehouse = "MAIN"
            },
            new Outprice3
            {
                ProductCode = this.product.ErpNumber,
                Quantity = 1,
                UnitOfMeasure = "CS",
                Price = 15,
                Warehouse = "MAIN"
            },
            new Outprice3
            {
                ProductCode = this.product.ErpNumber,
                Quantity = 1,
                UnitOfMeasure = "EA",
                Price = 20,
                Warehouse = "MAIN"
            },
            new Outprice3
            {
                ProductCode = this.product.ErpNumber,
                Quantity = 1,
                UnitOfMeasure = "BX",
                Price = 30,
                Warehouse = this.siteContextWarehouseDto.Name
            },
            new Outprice3
            {
                ProductCode = this.product.ErpNumber,
                Quantity = 1,
                UnitOfMeasure = "CS",
                Price = 35,
                Warehouse = this.siteContextWarehouseDto.Name
            },
            new Outprice3
            {
                ProductCode = this.product.ErpNumber,
                Quantity = 1,
                UnitOfMeasure = "EA",
                Price = 40,
                Warehouse = this.siteContextWarehouseDto.Name
            },
            new Outprice3
            {
                ProductCode = this.product.ErpNumber,
                Quantity = 1,
                UnitOfMeasure = "BX",
                Price = 50,
                Warehouse = string.Empty
            },
            new Outprice3
            {
                ProductCode = this.product.ErpNumber,
                Quantity = 1,
                UnitOfMeasure = "CS",
                Price = 55,
                Warehouse = string.Empty
            },
            new Outprice3
            {
                ProductCode = this.product.ErpNumber,
                Quantity = 1,
                UnitOfMeasure = "EA",
                Price = 60,
                Warehouse = string.Empty
            }
        };

    protected List<Outpricebreak3> OutPriceBreaks3 =>
        new List<Outpricebreak3>
        {
            new Outpricebreak3
            {
                ProductCode = this.product.ErpNumber,
                PriceBreak1 = 100,
                QuantityBreak1 = 5,
                PriceBreak2 = 90,
                Warehouse = "MAIN"
            },
            new Outpricebreak3
            {
                ProductCode = this.product.ErpNumber,
                PriceBreak1 = 200,
                QuantityBreak1 = 10,
                PriceBreak2 = 190,
                Warehouse = this.siteContextWarehouseDto.Name
            },
            new Outpricebreak3
            {
                ProductCode = this.product.ErpNumber,
                PriceBreak1 = 300,
                QuantityBreak1 = 15,
                PriceBreak2 = 290,
                Warehouse = string.Empty
            }
        };
}
