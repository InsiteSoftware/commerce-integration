namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetCustomerPrice;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Core.Plugins.Utilities;
using Insite.Data.Entities.Dtos;
using System.Linq;

[TestFixture]
public class GetPricingFromResponseTests
    : BaseForPipeTests<GetCustomerPriceParameter, GetCustomerPriceResult>
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
    public void Order_Is_700()
    {
        Assert.AreEqual(700, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Product_From_Repository_When_Pricing_Service_Parameter_Product_Is_Null()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var pricingServiceParameter = new PricingServiceParameter(product.Id);

        var parameter = this.CreateGetCustomerPriceParameter(pricingServiceParameter);

        var salesPartPriceResData = this.CreateSalesPartPriceResData("123", 50, 50);

        var result = this.CreateGetCustomerPriceResult(salesPartPriceResData);

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

        var parameter = this.CreateGetCustomerPriceParameter(pricingServiceParameter);

        var salesPartPriceResData = this.CreateSalesPartPriceResData("123", 50, 50);

        var result = this.CreateGetCustomerPriceResult(salesPartPriceResData);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsEmpty(result.PricingServiceResults);
    }

    [Test]
    public void Execute_Should_Not_Create_Pricing_Service_Result_When_Get_Item_Price_Reply_Does_Not_Have_Matching_Item()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty) { Product = product };

        var parameter = this.CreateGetCustomerPriceParameter(pricingServiceParameter);

        var salesPartPriceResData = this.CreateSalesPartPriceResData("1234", 50, 50);

        var result = this.CreateGetCustomerPriceResult(salesPartPriceResData);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsEmpty(result.PricingServiceResults);
    }

    [Test]
    public void Execute_Should_Create_Pricing_Service_Results_From_Customer_Price_Response()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty) { Product = product };

        var parameter = this.CreateGetCustomerPriceParameter(pricingServiceParameter);

        var salesPartPriceResData = this.CreateSalesPartPriceResData("123", 50, 50);

        var result = this.CreateGetCustomerPriceResult(salesPartPriceResData);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.PricingServiceResults);
        Assert.AreEqual(25M, result.PricingServiceResults.First().Value.UnitRegularPrice);
    }

    protected GetCustomerPriceParameter CreateGetCustomerPriceParameter(
        params PricingServiceParameter[] pricingServiceParameters
    )
    {
        return new GetCustomerPriceParameter
        {
            PricingServiceParameters = pricingServiceParameters.ToList()
        };
    }

    protected salesPartPriceResData CreateSalesPartPriceResData(
        string productNo,
        decimal saleUnitPrice,
        decimal discount
    )
    {
        return new salesPartPriceResData
        {
            productNo = productNo,
            saleUnitPrice = saleUnitPrice,
            discount = discount
        };
    }

    protected GetCustomerPriceResult CreateGetCustomerPriceResult(
        params salesPartPriceResData[] salesPartPriceResDatas
    )
    {
        return new GetCustomerPriceResult
        {
            CustomerPriceResponse = new customerPriceResponse
            {
                parts = salesPartPriceResDatas.ToList()
            }
        };
    }

    protected void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
