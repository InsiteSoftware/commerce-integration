namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetItemPrice;

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
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class GetPricingFromResponseTests
    : BaseForPipeTests<GetItemPriceParameter, GetItemPriceResult>
{
    private Mock<ICurrencyFormatProvider> currencyFormatProvider;

    private IList<Product> products;

    public override Type PipeType => typeof(GetPricingFromResponse);

    public override void SetUp()
    {
        this.WhenSiteContextCurrencyDtoIs(new CurrencyDto { CurrencySymbol = "$" });

        this.currencyFormatProvider = this.container.GetMock<ICurrencyFormatProvider>();

        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_800()
    {
        Assert.AreEqual(800, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Product_From_Repository_When_Pricing_Service_Parameter_Product_Is_Null()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var pricingServiceParameter = new PricingServiceParameter(product.Id);

        var parameter = this.CreateGetItemPriceParameter(pricingServiceParameter);

        var replyItem = this.CreateReplyItem("123", string.Empty, "25");

        var result = this.CreateGetItemPriceResult(replyItem);

        this.WhenExists(product);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.PricingServiceResults);
        Assert.AreEqual(25M, result.PricingServiceResults.First().Value.UnitRegularPrice);
    }

    [Test]
    public void Execute_Should_Not_Create_Pricing_Service_Result_When_Pricing_Service_Parameter_Product_Is_Null_And_Product_Not_Found_In_Repository()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var pricingServiceParameter = new PricingServiceParameter(product.Id);

        var parameter = this.CreateGetItemPriceParameter(pricingServiceParameter);

        var replyItem = this.CreateReplyItem("123", string.Empty, "25");

        var result = this.CreateGetItemPriceResult(replyItem);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsEmpty(result.PricingServiceResults);
    }

    [Test]
    public void Execute_Should_Not_Create_Pricing_Service_Result_When_Get_Item_Price_Reply_Does_Not_Have_Matching_Item()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty) { Product = product };

        var parameter = this.CreateGetItemPriceParameter(pricingServiceParameter);

        var replyItem = this.CreateReplyItem("1234", string.Empty, "25");

        var result = this.CreateGetItemPriceResult(replyItem);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsEmpty(result.PricingServiceResults);
    }

    [Test]
    public void Execute_Should_Create_Pricing_Service_Results_From_Get_Item_Price_Reply()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty) { Product = product };

        var parameter = this.CreateGetItemPriceParameter(pricingServiceParameter);

        var replyItem = this.CreateReplyItem("123", string.Empty, "25");

        var result = this.CreateGetItemPriceResult(replyItem);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.PricingServiceResults);
        Assert.AreEqual(25M, result.PricingServiceResults.First().Value.UnitRegularPrice);
    }

    [Test]
    public void Execute_Should_Create_Pricing_Service_Results_From_Get_Item_Price_Reply_Where_Unit_Name_Equals_Unit_Of_Measure()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty)
        {
            Product = product,
            UnitOfMeasure = "EA"
        };

        var parameter = this.CreateGetItemPriceParameter(pricingServiceParameter);

        var replyItem1 = this.CreateReplyItem("123", "CS", "20");
        var replyItem2 = this.CreateReplyItem("123", "EA", "25");
        var replyItem3 = this.CreateReplyItem("123", "BX", "30");

        var result = this.CreateGetItemPriceResult(replyItem1, replyItem2, replyItem3);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.PricingServiceResults);
        Assert.AreEqual(25M, result.PricingServiceResults.First().Value.UnitRegularPrice);
    }

    protected GetItemPriceParameter CreateGetItemPriceParameter(
        params PricingServiceParameter[] pricingServiceParameters
    )
    {
        return new GetItemPriceParameter
        {
            PricingServiceParameters = pricingServiceParameters.ToList()
        };
    }

    protected ReplyItem CreateReplyItem(string itemId, string unitName, string netPrice)
    {
        return new ReplyItem
        {
            ItemID = itemId,
            UnitName = unitName,
            NetPrice = netPrice
        };
    }

    protected GetItemPriceResult CreateGetItemPriceResult(params ReplyItem[] replyItems)
    {
        return new GetItemPriceResult
        {
            GetItemPriceReply = new GetItemPrice
            {
                Reply = new Reply { ListOfItems = replyItems.ToList() }
            }
        };
    }

    protected void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
