namespace Insite.Integration.Connector.Base.Tests.Helpers;

using System;
using System.Linq;
using FluentAssertions;
using Insite.Common.Dependencies;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers;
using Insite.Data.Entities.Dtos;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Base.Helpers;
using Moq;
using NUnit.Framework;

[TestFixture]
public class WarehouseHelperTests
{
    private Mock<IUnitOfWork> unitOfWork;

    private Mock<ISiteContext> siteContext;

    private Mock<IWarehouseRepository> warehouseRepository;

    private WarehouseHelper warehouseHelper;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.siteContext = container.GetMock<ISiteContext>();
        var dependencyLocator = container.GetMock<IDependencyLocator>();
        TestHelper.MockSiteContext(this.siteContext, dependencyLocator);

        var dataProvider = container.GetMock<IDataProvider>();
        this.unitOfWork = container.GetMock<IUnitOfWork>();
        this.unitOfWork.Setup(o => o.DataProvider).Returns(dataProvider.Object);

        this.warehouseRepository = container.GetMock<IWarehouseRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<IWarehouseRepository>())
            .Returns(this.warehouseRepository.Object);

        this.warehouseHelper = container.Resolve<WarehouseHelper>();
    }

    [Test]
    public void GetWarehouse_Should_Return_Warehouse_For_PricingServiceParameter_Warehouse()
    {
        var warehouse = new WarehouseDto { Name = "WarehouseName" };
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty)
        {
            Warehouse = warehouse.Name
        };

        this.WhenGetCachedWarehousesIs(warehouse);

        var result = this.warehouseHelper.GetWarehouse(
            this.unitOfWork.Object,
            pricingServiceParameter
        );

        result.Should().Be(warehouse);
    }

    [Test]
    public void GetWarehouse_Should_Return_Warehouse_From_SiteContext_WarehouseDto_When_PricingServiceParameter_Warehouse_Is_Null()
    {
        var warehouse = new WarehouseDto { Name = "WarehouseName" };
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        this.WhenSiteContextWarehouseDtoIs(warehouse);

        var result = this.warehouseHelper.GetWarehouse(
            this.unitOfWork.Object,
            pricingServiceParameter
        );

        result.Should().Be(warehouse);
    }

    [Test]
    public void GetWarehouse_Should_Return_Warehouse_From_CachedDefault_When_PricingServiceParameter_Warehouse_Is_Null_And_SiteContext_WarehouseDto_Is_Null()
    {
        var warehouse = new WarehouseDto { Name = "WarehouseName" };
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        this.WhenGetCachedDefaultIs(warehouse);

        var result = this.warehouseHelper.GetWarehouse(
            this.unitOfWork.Object,
            pricingServiceParameter
        );

        result.Should().Be(warehouse);
    }

    [Test]
    public void GetWarehouse_Should_Return_Warehouse_For_GetInventoryParameter_WarehouseId()
    {
        var warehouse = new WarehouseDto { Id = Guid.NewGuid() };
        var getInventoryParameter = new GetInventoryParameter { WarehouseId = warehouse.Id };

        this.WhenGetCachedWarehousesIs(warehouse);

        var result = this.warehouseHelper.GetWarehouse(
            this.unitOfWork.Object,
            getInventoryParameter
        );

        result.Should().Be(warehouse);
    }

    [Test]
    public void GetWarehouse_Should_Return_Warehouse_From_SiteContext_WarehouseDto_When_GetInventoryParameter_WarehouseId_Is_Null()
    {
        var warehouse = new WarehouseDto { Id = Guid.NewGuid() };
        var getInventoryParameter = new GetInventoryParameter { WarehouseId = warehouse.Id };

        this.WhenSiteContextWarehouseDtoIs(warehouse);

        var result = this.warehouseHelper.GetWarehouse(
            this.unitOfWork.Object,
            getInventoryParameter
        );

        result.Should().Be(warehouse);
    }

    [Test]
    public void GetWarehouse_Should_Return_Warehouse_From_CachedDefault_When_GetInventoryParameter_WarehouseId_Is_Null_And_SiteContext_WarehouseDto_Is_Null()
    {
        var warehouse = new WarehouseDto { Id = Guid.NewGuid() };
        var getInventoryParameter = new GetInventoryParameter { WarehouseId = warehouse.Id };

        this.WhenGetCachedDefaultIs(warehouse);

        var result = this.warehouseHelper.GetWarehouse(
            this.unitOfWork.Object,
            getInventoryParameter
        );

        result.Should().Be(warehouse);
    }

    [Test]
    public void GetWarehouses_Should_Return_All_Warehouses_From_GetCachedWarehouses_When_GetInventoryParameter_GetWarehouses_Is_True()
    {
        var warehouse1 = new WarehouseDto { Id = Guid.NewGuid() };
        var warehouse2 = new WarehouseDto { Id = Guid.NewGuid() };

        var getInventoryParameter = new GetInventoryParameter
        {
            GetWarehouses = true,
            WarehouseId = warehouse1.Id
        };

        this.WhenGetCachedWarehousesIs(warehouse1, warehouse2);

        var result = this.warehouseHelper.GetWarehouses(
            this.unitOfWork.Object,
            getInventoryParameter
        );

        result.Should().Contain(warehouse1);
        result.Should().Contain(warehouse2);
    }

    [Test]
    public void GetWarehouses_Should_Return_Warehouse_For_GetInventoryParameter_WarehouseId_When_GetInventoryParameter_GetWarehouses_Is_False()
    {
        var warehouse1 = new WarehouseDto { Id = Guid.NewGuid() };
        var warehouse2 = new WarehouseDto { Id = Guid.NewGuid() };

        var getInventoryParameter = new GetInventoryParameter
        {
            GetWarehouses = false,
            WarehouseId = warehouse1.Id
        };

        this.WhenGetCachedWarehousesIs(warehouse1, warehouse2);

        var result = this.warehouseHelper.GetWarehouses(
            this.unitOfWork.Object,
            getInventoryParameter
        );

        result.Should().HaveCount(1);
        result.Should().Contain(warehouse1);
    }

    private void WhenGetCachedWarehousesIs(params WarehouseDto[] warehouses)
    {
        this.warehouseRepository.Setup(o => o.GetCachedWarehouses()).Returns(warehouses);
    }

    private void WhenGetCachedDefaultIs(WarehouseDto warehouse)
    {
        this.warehouseRepository.Setup(o => o.GetCachedDefault()).Returns(warehouse);
    }

    protected void WhenSiteContextWarehouseDtoIs(WarehouseDto warehouse)
    {
        this.siteContext.Setup(o => o.WarehouseDto).Returns(warehouse);
    }
}
