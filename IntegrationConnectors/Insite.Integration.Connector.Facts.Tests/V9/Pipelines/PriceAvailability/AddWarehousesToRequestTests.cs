namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.PriceAvailability;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities.Dtos;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AddWarehousesToRequestTests
    : BaseForPipeTests<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    private Mock<IWarehouseRepository> warehouseRepository;

    public override Type PipeType => typeof(AddWarehousesToRequest);

    public override void SetUp()
    {
        this.warehouseRepository = this.container.GetMock<IWarehouseRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<IWarehouseRepository>())
            .Returns(this.warehouseRepository.Object);
    }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_RequestItem_WarehouseId_From_All_Cached_Warehouses_When_GetInventoryParamater_GetWarehouses_Is_True()
    {
        var warehouses = new List<WarehouseDto>
        {
            new WarehouseDto { Name = "Warehouse1" },
            new WarehouseDto { Name = "Warehouse2" }
        };

        var parameter = new PriceAvailabilityParameter
        {
            GetInventoryParameter = new GetInventoryParameter { GetWarehouses = true }
        };

        this.WhenGetCachedWarehousesIs(warehouses);

        var result = this.RunExecute(parameter);

        Assert.IsTrue(result.PriceAvailabilityRequest.Request.Items.Count == 4);
        Assert.IsTrue(
            result.PriceAvailabilityRequest.Request.Items.Count(
                o => o.WarehouseID.EqualsIgnoreCase(warehouses[0].Name)
            ) == 2
        );
        Assert.IsTrue(
            result.PriceAvailabilityRequest.Request.Items.Count(
                o => o.WarehouseID.EqualsIgnoreCase(warehouses[1].Name)
            ) == 2
        );
    }

    [Test]
    public void Execute_Should_Populate_RequestItem_WarehouseId_From_PricingServiceParameter_Warehouse()
    {
        var warehouses = new List<WarehouseDto>
        {
            new WarehouseDto { Name = "Warehouse1" },
            new WarehouseDto { Name = "Warehouse2" }
        };

        var parameter = new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { Warehouse = warehouses[0].Name }
            }
        };

        this.WhenGetCachedWarehousesIs(warehouses);

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.PriceAvailabilityRequest.Request.Items.TrueForAll(
                o => o.WarehouseID.Equals(warehouses[0].Name)
            )
        );
    }

    [Test]
    public void Execute_Should_Populate_RequestItem_WarehouseId_From_GetInventoryParameter_Warehouse()
    {
        var warehouses = new List<WarehouseDto>
        {
            new WarehouseDto { Name = "Warehouse1" },
            new WarehouseDto { Name = "Warehouse2", Id = Guid.NewGuid() }
        };

        var parameter = new PriceAvailabilityParameter
        {
            GetInventoryParameter = new GetInventoryParameter { WarehouseId = warehouses[1].Id }
        };

        this.WhenGetCachedWarehousesIs(warehouses);

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.PriceAvailabilityRequest.Request.Items.TrueForAll(
                o => o.WarehouseID.Equals(warehouses[1].Name)
            )
        );
    }

    [Test]
    public void Execute_Should_Get_Warehouse_From_SiteContext_Warehouse()
    {
        var warehouses = new List<WarehouseDto>
        {
            new WarehouseDto { Name = "Warehouse1" },
            new WarehouseDto { Name = "Warehouse2", Id = Guid.NewGuid() }
        };

        this.WhenSiteContextWarehouseDtoIs(warehouses[1]);
        this.WhenGetCachedWarehousesIs(warehouses);

        var result = this.RunExecute();

        Assert.IsTrue(
            result.PriceAvailabilityRequest.Request.Items.TrueForAll(
                o => o.WarehouseID.Equals(warehouses[1].Name)
            )
        );
    }

    protected override PriceAvailabilityResult GetDefaultResult()
    {
        return new PriceAvailabilityResult
        {
            PriceAvailabilityRequest = new PriceAvailabilityRequest
            {
                Request = new Request
                {
                    Items = new List<RequestItem> { new RequestItem(), new RequestItem() }
                }
            }
        };
    }

    private void WhenGetCachedWarehousesIs(IEnumerable<WarehouseDto> warehouses)
    {
        this.warehouseRepository.Setup(o => o.GetCachedWarehouses()).Returns(warehouses);
    }
}
