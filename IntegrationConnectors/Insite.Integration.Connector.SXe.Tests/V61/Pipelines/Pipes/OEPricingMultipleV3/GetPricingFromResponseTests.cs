namespace Insite.Integration.Connector.SXe.Tests.V6.Pipelines.Pipes.OEPricingMultipleV3;

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
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;

[TestFixture]
public class GetPricingFromResponseTests
    : BaseForPipeTests<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);
        var result = this.CreateGetOEPricingMultipleV3Result(null, null);

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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);

        var result = this.RunExecute(parameter);

        var expectedOutPrice2 = this.OutPrice2s.FirstOrDefault(
            o =>
                o.unitOfMeasure.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.warehouse.Equals(pricingServiceParameter.Warehouse)
        );

        Assert.AreEqual(
            expectedOutPrice2.price,
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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);

        this.WhenSiteContextWarehouseDtoIs(this.siteContextWarehouseDto);

        var result = this.RunExecute(parameter);

        var expectedOutPrice2 = this.OutPrice2s.FirstOrDefault(
            o =>
                o.unitOfMeasure.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.warehouse.Equals(this.siteContextWarehouseDto.Name)
        );

        Assert.AreEqual(
            expectedOutPrice2.price,
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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);

        var result = this.RunExecute(parameter);

        var expectedOutPrice2 = this.OutPrice2s.FirstOrDefault(
            o =>
                o.unitOfMeasure.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.warehouse.Equals(string.Empty)
        );

        Assert.AreEqual(
            expectedOutPrice2.price,
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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);

        var result = this.RunExecute(parameter);

        var expectedOutPrice2 = this.OutPrice2s.FirstOrDefault(
            o => o.warehouse.Equals(pricingServiceParameter.Warehouse)
        );

        Assert.AreEqual(
            expectedOutPrice2.price,
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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);

        this.WhenSiteContextWarehouseDtoIs(this.siteContextWarehouseDto);

        var result = this.RunExecute(parameter);

        var expectedOutPrice2 = this.OutPrice2s.FirstOrDefault(
            o => o.warehouse.Equals(this.siteContextWarehouseDto.Name)
        );

        Assert.AreEqual(
            expectedOutPrice2.price,
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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);

        var result = this.RunExecute(parameter);

        var expectedOutPrice2 = this.OutPrice2s.FirstOrDefault(
            o => o.warehouse.Equals(string.Empty)
        );

        Assert.AreEqual(
            expectedOutPrice2.price,
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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);

        var result = this.GetDefaultResult();
        result.OEPricingMultipleV3Response.arrayPricebreak = this.OutPriceBreaks3;

        result = this.RunExecute(parameter, result);

        var expectedOutPriceBreak2 = this.OutPriceBreaks3.FirstOrDefault(
            o => o.warehouse.Equals(pricingServiceParameter.Warehouse)
        );

        Assert.AreEqual(
            expectedOutPriceBreak2.priceBreak1,
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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);

        this.WhenSiteContextWarehouseDtoIs(this.siteContextWarehouseDto);

        var result = this.GetDefaultResult();
        result.OEPricingMultipleV3Response.arrayPricebreak = this.OutPriceBreaks3;

        result = this.RunExecute(parameter, result);

        var expectedOutPriceBreak2 = this.OutPriceBreaks3.FirstOrDefault(
            o => o.warehouse.Equals(this.siteContextWarehouseDto.Name)
        );

        Assert.AreEqual(
            expectedOutPriceBreak2.priceBreak1,
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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);

        var result = this.GetDefaultResult();
        result.OEPricingMultipleV3Response.arrayPricebreak = this.OutPriceBreaks3;

        result = this.RunExecute(parameter, result);

        var expectedOutPriceBreak2 = this.OutPriceBreaks3.FirstOrDefault(
            o => o.warehouse.Equals(string.Empty)
        );

        Assert.AreEqual(
            expectedOutPriceBreak2.priceBreak1,
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

        var outPrice2 = new OEPricingMultipleV3outputPrice
        {
            productCode = this.product.ErpNumber,
            warehouse = "MAIN",
            unitOfMeasure = "EA",
            quantity = 1,
            price = 10,
            discountType = "%"
        };

        var outPriceBreak2 = new OEPricingMultipleV3outputPricebreak
        {
            productCode = this.product.ErpNumber,
            warehouse = "MAIN",
            discountPercent1 = 10,
            quantityBreak1 = 5,
            discountPercent2 = 15
        };

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);

        var result = this.GetDefaultResult();
        result.OEPricingMultipleV3Response.arrayPrice = new List<OEPricingMultipleV3outputPrice>
        {
            outPrice2
        };
        result.OEPricingMultipleV3Response.arrayPricebreak =
            new List<OEPricingMultipleV3outputPricebreak> { outPriceBreak2 };

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

        var outPrice2 = new OEPricingMultipleV3outputPrice
        {
            productCode = this.product.ErpNumber,
            warehouse = "MAIN",
            unitOfMeasure = "EA",
            quantity = 1,
            price = 10,
            discountType = "$"
        };

        var outPriceBreak2 = new OEPricingMultipleV3outputPricebreak
        {
            productCode = this.product.ErpNumber,
            warehouse = "MAIN",
            discountPercent1 = 1,
            quantityBreak1 = 5,
            discountPercent2 = 2
        };

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(pricingServiceParameter);

        var result = this.GetDefaultResult();
        result.OEPricingMultipleV3Response.arrayPrice = new List<OEPricingMultipleV3outputPrice>
        {
            outPrice2
        };
        result.OEPricingMultipleV3Response.arrayPricebreak =
            new List<OEPricingMultipleV3outputPricebreak> { outPriceBreak2 };

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

    protected override OEPricingMultipleV3Result GetDefaultResult()
    {
        return new OEPricingMultipleV3Result
        {
            OEPricingMultipleV3Response = new OEPricingMultipleV3Response
            {
                arrayPrice = this.OutPrice2s
            }
        };
    }

    protected OEPricingMultipleV3Parameter CreateGetOEPricingMultipleV3Parameter(
        params PricingServiceParameter[] pricingServiceParameters
    )
    {
        return new OEPricingMultipleV3Parameter
        {
            PricingServiceParameters = pricingServiceParameters
        };
    }

    protected OEPricingMultipleV3Result CreateGetOEPricingMultipleV3Result(
        OEPricingMultipleV3outputPrice[] outPrice2s,
        OEPricingMultipleV3outputPricebreak[] outPriceBreak2s
    )
    {
        return new OEPricingMultipleV3Result
        {
            OEPricingMultipleV3Response = new OEPricingMultipleV3Response()
            {
                arrayPrice = outPrice2s?.ToList(),
                arrayPricebreak = outPriceBreak2s?.ToList()
            }
        };
    }

    protected void VerifyGetProductWasCalled()
    {
        this.unitOfWork.Verify(o => o.GetRepository<Product>(), Times.Once);
    }

    protected Product product = Some.Product().WithErpNumber("ABC123").Build();

    protected List<OEPricingMultipleV3outputPrice> OutPrice2s =>
        new List<OEPricingMultipleV3outputPrice>
        {
            new OEPricingMultipleV3outputPrice
            {
                productCode = this.product.ErpNumber,
                quantity = 1,
                unitOfMeasure = "BX",
                price = 10,
                warehouse = "MAIN"
            },
            new OEPricingMultipleV3outputPrice
            {
                productCode = this.product.ErpNumber,
                quantity = 1,
                unitOfMeasure = "CS",
                price = 15,
                warehouse = "MAIN"
            },
            new OEPricingMultipleV3outputPrice
            {
                productCode = this.product.ErpNumber,
                quantity = 1,
                unitOfMeasure = "EA",
                price = 20,
                warehouse = "MAIN"
            },
            new OEPricingMultipleV3outputPrice
            {
                productCode = this.product.ErpNumber,
                quantity = 1,
                unitOfMeasure = "BX",
                price = 30,
                warehouse = this.siteContextWarehouseDto.Name
            },
            new OEPricingMultipleV3outputPrice
            {
                productCode = this.product.ErpNumber,
                quantity = 1,
                unitOfMeasure = "CS",
                price = 35,
                warehouse = this.siteContextWarehouseDto.Name
            },
            new OEPricingMultipleV3outputPrice
            {
                productCode = this.product.ErpNumber,
                quantity = 1,
                unitOfMeasure = "EA",
                price = 40,
                warehouse = this.siteContextWarehouseDto.Name
            },
            new OEPricingMultipleV3outputPrice
            {
                productCode = this.product.ErpNumber,
                quantity = 1,
                unitOfMeasure = "BX",
                price = 50,
                warehouse = string.Empty
            },
            new OEPricingMultipleV3outputPrice
            {
                productCode = this.product.ErpNumber,
                quantity = 1,
                unitOfMeasure = "CS",
                price = 55,
                warehouse = string.Empty
            },
            new OEPricingMultipleV3outputPrice
            {
                productCode = this.product.ErpNumber,
                quantity = 1,
                unitOfMeasure = "EA",
                price = 60,
                warehouse = string.Empty
            }
        };

    protected List<OEPricingMultipleV3outputPricebreak> OutPriceBreaks3 =>
        new List<OEPricingMultipleV3outputPricebreak>
        {
            new OEPricingMultipleV3outputPricebreak
            {
                productCode = this.product.ErpNumber,
                priceBreak1 = 100,
                quantityBreak1 = 5,
                priceBreak2 = 90,
                warehouse = "MAIN"
            },
            new OEPricingMultipleV3outputPricebreak
            {
                productCode = this.product.ErpNumber,
                priceBreak1 = 200,
                quantityBreak1 = 10,
                priceBreak2 = 190,
                warehouse = this.siteContextWarehouseDto.Name
            },
            new OEPricingMultipleV3outputPricebreak
            {
                productCode = this.product.ErpNumber,
                priceBreak1 = 300,
                quantityBreak1 = 15,
                priceBreak2 = 290,
                warehouse = string.Empty
            }
        };
}
