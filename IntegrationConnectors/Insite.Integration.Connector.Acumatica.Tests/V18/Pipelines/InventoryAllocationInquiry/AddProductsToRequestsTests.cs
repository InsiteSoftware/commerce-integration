namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.InventoryAllocationInquiry;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.InventoryAllocationInquiry;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class AddProductsToRequestsTests
    : BaseForPipeTests<InventoryAllocationInquiryParameter, InventoryAllocationInquiryResult>
{
    private IList<Product> products;

    public override Type PipeType => typeof(AddProductsToRequests);

    public override void SetUp()
    {
        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
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

        CollectionAssert.IsNotEmpty(result.InventoryAllocationInquiryRequests);
        Assert.AreEqual(
            product.ErpNumber,
            result.InventoryAllocationInquiryRequests.First().InventoryID
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

        CollectionAssert.IsEmpty(result.InventoryAllocationInquiryRequests);
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

        CollectionAssert.IsNotEmpty(result.InventoryAllocationInquiryRequests);
        Assert.AreEqual(
            product.ErpNumber,
            result.InventoryAllocationInquiryRequests.First().InventoryID
        );
    }

    protected void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
