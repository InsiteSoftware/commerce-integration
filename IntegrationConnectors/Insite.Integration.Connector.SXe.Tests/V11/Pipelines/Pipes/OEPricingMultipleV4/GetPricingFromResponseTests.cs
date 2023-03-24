namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.OEPricingMultipleV4;

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
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;

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

        var expectedOutPrice3 = this.PriceOutV2s.FirstOrDefault(
            o =>
                o.Unit.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.Whse.Equals(pricingServiceParameter.Warehouse)
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

        var expectedOutPrice3 = this.PriceOutV2s.FirstOrDefault(
            o =>
                o.Unit.Equals(pricingServiceParameter.UnitOfMeasure)
                && o.Whse.Equals(this.siteContextWarehouseDto.Name)
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

        var expectedOutPrice3 = this.PriceOutV2s.FirstOrDefault(
            o => o.Unit.Equals(pricingServiceParameter.UnitOfMeasure) && o.Whse.Equals(string.Empty)
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

        var expectedOutPrice3 = this.PriceOutV2s.FirstOrDefault(
            o => o.Whse.Equals(pricingServiceParameter.Warehouse)
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

        var expectedOutPrice3 = this.PriceOutV2s.FirstOrDefault(
            o => o.Whse.Equals(this.siteContextWarehouseDto.Name)
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

        var expectedOutPrice3 = this.PriceOutV2s.FirstOrDefault(o => o.Whse.Equals(string.Empty));

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
        result.OEPricingMultipleV4Response.Response.PriceOutBreakCollection =
            new PriceOutBreakCollection { PriceOutBreaks = this.PriceOutBreaks };

        result = this.RunExecute(parameter, result);

        var expectedOutPriceBreak3 = this.PriceOutBreaks.FirstOrDefault(
            o => o.Whse.Equals(pricingServiceParameter.Warehouse)
        );

        Assert.AreEqual(
            expectedOutPriceBreak3.Pricebreak1,
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
        result.OEPricingMultipleV4Response.Response.PriceOutBreakCollection =
            new PriceOutBreakCollection { PriceOutBreaks = this.PriceOutBreaks };

        result = this.RunExecute(parameter, result);

        var expectedOutPriceBreak3 = this.PriceOutBreaks.FirstOrDefault(
            o => o.Whse.Equals(this.siteContextWarehouseDto.Name)
        );

        Assert.AreEqual(
            expectedOutPriceBreak3.Pricebreak1,
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
        result.OEPricingMultipleV4Response.Response.PriceOutBreakCollection =
            new PriceOutBreakCollection { PriceOutBreaks = this.PriceOutBreaks };

        result = this.RunExecute(parameter, result);

        var expectedOutPriceBreak3 = this.PriceOutBreaks.FirstOrDefault(
            o => o.Whse.Equals(string.Empty)
        );

        Assert.AreEqual(
            expectedOutPriceBreak3.Pricebreak1,
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

        var priceOutV2 = new PriceOutV2
        {
            Prod = this.product.ErpNumber,
            Whse = "MAIN",
            Unit = "EA",
            Qtyord = 1,
            Price = 10,
            Disctype = "%"
        };

        var priceOutBreak = new PriceOutBreak
        {
            Prod = this.product.ErpNumber,
            Whse = "MAIN",
            Discountpercent1 = 10,
            Quantitybreak1 = 5,
            Discountpercent2 = 15
        };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);
        var result = this.CreateGetOEPricingMultipleV4Result(
            new PriceOutV2[] { priceOutV2 },
            new PriceOutBreak[] { priceOutBreak }
        );

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

        var priceOutV2 = new PriceOutV2
        {
            Prod = this.product.ErpNumber,
            Whse = "MAIN",
            Unit = "EA",
            Qtyord = 1,
            Price = 10,
            Disctype = "$"
        };

        var priceOutBreak = new PriceOutBreak
        {
            Prod = this.product.ErpNumber,
            Whse = "MAIN",
            Discountpercent1 = 1,
            Quantitybreak1 = 5,
            Discountpercent2 = 2
        };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(pricingServiceParameter);
        var result = this.CreateGetOEPricingMultipleV4Result(
            new PriceOutV2[] { priceOutV2 },
            new PriceOutBreak[] { priceOutBreak }
        );

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
                Response = new Response
                {
                    PriceOutV2Collection = new PriceOutV2Collection
                    {
                        PriceOutV2s = this.PriceOutV2s
                    }
                }
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
        PriceOutV2[] priceOutV2s,
        PriceOutBreak[] priceOutBreaks
    )
    {
        return new OEPricingMultipleV4Result
        {
            OEPricingMultipleV4Response = new OEPricingMultipleV4Response()
            {
                Response = new Response
                {
                    PriceOutV2Collection = new PriceOutV2Collection
                    {
                        PriceOutV2s = priceOutV2s?.ToList()
                    },
                    PriceOutBreakCollection = new PriceOutBreakCollection
                    {
                        PriceOutBreaks = priceOutBreaks?.ToList()
                    }
                }
            }
        };
    }

    protected void VerifyGetProductWasCalled()
    {
        this.unitOfWork.Verify(o => o.GetRepository<Product>(), Times.Once);
    }

    protected Product product = Some.Product().WithErpNumber("ABC123").Build();

    protected List<PriceOutV2> PriceOutV2s =>
        new List<PriceOutV2>
        {
            new PriceOutV2
            {
                Prod = this.product.ErpNumber,
                Qtyord = 1,
                Unit = "BX",
                Price = 10,
                Whse = "MAIN"
            },
            new PriceOutV2
            {
                Prod = this.product.ErpNumber,
                Qtyord = 1,
                Unit = "CS",
                Price = 15,
                Whse = "MAIN"
            },
            new PriceOutV2
            {
                Prod = this.product.ErpNumber,
                Qtyord = 1,
                Unit = "EA",
                Price = 20,
                Whse = "MAIN"
            },
            new PriceOutV2
            {
                Prod = this.product.ErpNumber,
                Qtyord = 1,
                Unit = "BX",
                Price = 30,
                Whse = this.siteContextWarehouseDto.Name
            },
            new PriceOutV2
            {
                Prod = this.product.ErpNumber,
                Qtyord = 1,
                Unit = "CS",
                Price = 35,
                Whse = this.siteContextWarehouseDto.Name
            },
            new PriceOutV2
            {
                Prod = this.product.ErpNumber,
                Qtyord = 1,
                Unit = "EA",
                Price = 40,
                Whse = this.siteContextWarehouseDto.Name
            },
            new PriceOutV2
            {
                Prod = this.product.ErpNumber,
                Qtyord = 1,
                Unit = "BX",
                Price = 50,
                Whse = string.Empty
            },
            new PriceOutV2
            {
                Prod = this.product.ErpNumber,
                Qtyord = 1,
                Unit = "CS",
                Price = 55,
                Whse = string.Empty
            },
            new PriceOutV2
            {
                Prod = this.product.ErpNumber,
                Qtyord = 1,
                Unit = "EA",
                Price = 60,
                Whse = string.Empty
            }
        };

    protected List<PriceOutBreak> PriceOutBreaks =>
        new List<PriceOutBreak>
        {
            new PriceOutBreak
            {
                Prod = this.product.ErpNumber,
                Pricebreak1 = 100,
                Quantitybreak1 = 5,
                Pricebreak2 = 90,
                Whse = "MAIN"
            },
            new PriceOutBreak
            {
                Prod = this.product.ErpNumber,
                Pricebreak1 = 200,
                Quantitybreak1 = 10,
                Pricebreak2 = 190,
                Whse = this.siteContextWarehouseDto.Name
            },
            new PriceOutBreak
            {
                Prod = this.product.ErpNumber,
                Pricebreak1 = 300,
                Quantitybreak1 = 15,
                Pricebreak2 = 290,
                Whse = string.Empty
            }
        };
}
