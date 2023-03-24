namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetPartAvailability;

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

[TestFixture]
public class AddProductsToRequestTests
    : BaseForPipeTests<GetPartAvailabilityParameter, GetPartAvailabilityResult>
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
    public void Execute_Should_Add_Product_To_Request_With_Product_From_Get_Inventory_Parameter()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            Products = new List<Product> { product }
        };

        var result = this.RunExecute(parameter);

        CollectionAssert.IsNotEmpty(result.PartAvailabilityRequest.partsAvailabile);
        Assert.AreEqual(
            product.ErpNumber,
            result.PartAvailabilityRequest.partsAvailabile.First().productNo
        );
        Assert.AreEqual(
            1000,
            result.PartAvailabilityRequest.partsAvailabile.First().wantedQuantity
        );
    }

    [Test]
    public void Execute_Should_Not_Add_Product_To_Request_When_Product_Is_Null()
    {
        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { Guid.NewGuid() }
        };

        var result = this.RunExecute(parameter);

        CollectionAssert.IsEmpty(result.PartAvailabilityRequest.partsAvailabile);
    }

    [Test]
    public void Execute_Should_Add_Product_To_Request_With_Product_From_Repository_From_Get_Inventory_Parameter_ProductId()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { product.Id }
        };

        this.WhenExists(product);

        var result = this.RunExecute(parameter);

        CollectionAssert.IsNotEmpty(result.PartAvailabilityRequest.partsAvailabile);
        Assert.AreEqual(
            product.ErpNumber,
            result.PartAvailabilityRequest.partsAvailabile.First().productNo
        );
        Assert.AreEqual(
            1000,
            result.PartAvailabilityRequest.partsAvailabile.First().wantedQuantity
        );
    }

    protected override GetPartAvailabilityResult GetDefaultResult()
    {
        return new GetPartAvailabilityResult
        {
            PartAvailabilityRequest = new partAvailabilityRequest()
        };
    }

    protected void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
