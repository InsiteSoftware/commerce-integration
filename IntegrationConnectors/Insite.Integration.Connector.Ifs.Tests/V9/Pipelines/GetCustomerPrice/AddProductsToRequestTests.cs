namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetCustomerPrice;

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

[TestFixture]
public class AddProductsToRequestTests
    : BaseForPipeTests<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    private IList<Product> products;

    public override Type PipeType => typeof(AddProductsToRequest);

    public override void SetUp()
    {
        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Not_Add_Product_To_Request_When_Product_Is_Null()
    {
        var parameter = this.GetDefaultParameter();
        parameter.PricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.NewGuid())
        };

        var result = this.RunExecute(parameter);

        CollectionAssert.IsEmpty(result.CustomerPriceRequest.parts);
    }

    [Test]
    public void Execute_Should_Add_Product_To_Request_With_Product_From_Pricing_Service_Parameter()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = this.GetDefaultParameter();
        parameter.PricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(product.Id) { Product = product }
        };

        var result = this.RunExecute(parameter);

        CollectionAssert.IsNotEmpty(result.CustomerPriceRequest.parts);
        Assert.AreEqual(product.ErpNumber, result.CustomerPriceRequest.parts.First().productNo);
    }

    [Test]
    public void Execute_Should_Add_Product_To_Request_With_Product_From_Repository_From_Pricing_Service_Parameter_ProductId()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = this.GetDefaultParameter();
        parameter.PricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(product.Id)
        };

        this.WhenExists(product);

        var result = this.RunExecute(parameter);

        CollectionAssert.IsNotEmpty(result.CustomerPriceRequest.parts);
        Assert.AreEqual(product.ErpNumber, result.CustomerPriceRequest.parts.First().productNo);
    }

    [Test]
    public void Execute_Should_Populate_Product_QuantityOrdered_From_Pricing_Service_Parameter()
    {
        var product = Some.Product().Build();

        var parameter = this.GetDefaultParameter();
        parameter.PricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(product.Id) { Product = product, QtyOrdered = 12 }
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(12, result.CustomerPriceRequest.parts.First().quantity);
    }

    protected override GetCustomerPriceResult GetDefaultResult()
    {
        return new GetCustomerPriceResult { CustomerPriceRequest = new customerPriceRequest() };
    }

    protected void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
