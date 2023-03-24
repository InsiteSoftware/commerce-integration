#pragma warning disable CA1001
namespace Insite.Integration.Connector.Base.Tests.Helpers;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Common.Dependencies;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers;
using Insite.Core.TestHelpers.Builders;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Helpers;
using NUnit.Framework;

[TestFixture]
public class ProductHelperTests
{
    private FakeUnitOfWork fakeUnitOfWork;

    private IList<Product> products;

    private ProductHelper productHelper;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        var siteContext = container.GetMock<ISiteContext>();
        var dependencyLocator = container.GetMock<IDependencyLocator>();
        TestHelper.MockSiteContext(siteContext, dependencyLocator);

        var dataProvider = container.GetMock<IDataProvider>();
        var unitOfWork = container.GetMock<IUnitOfWork>();
        unitOfWork.Setup(o => o.DataProvider).Returns(dataProvider.Object);
        this.fakeUnitOfWork = new FakeUnitOfWork(unitOfWork.Object);

        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);

        this.productHelper = container.Resolve<ProductHelper>();
    }

    [Test]
    public void GetProducts_Should_Return_Empty_List_If_GetInventoryParameter_Is_Null()
    {
        var result = this.productHelper.GetProducts(this.fakeUnitOfWork, null);

        result.Should().BeEmpty();
    }

    [Test]
    public void GetProducts_Should_Get_Distinct_Products_For_GetInventoryParameter_ProductIds_And_Products()
    {
        var product1 = Some.Product().Build();
        var product2 = Some.Product().Build();

        var getInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { product1.Id },
            Products = new List<Product> { product2 }
        };

        this.WhenExists(product1);

        var result = this.productHelper.GetProducts(this.fakeUnitOfWork, getInventoryParameter);

        result.Should().OnlyHaveUniqueItems();
        result.Should().Contain(product1);
        result.Should().Contain(product2);
    }

    private void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
