namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.InventoryAllocationInquiry;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities.Dtos;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.InventoryAllocationInquiry;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.InventoryAllocationInquiry;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AddWarehousesToRequestsTests
    : BaseForPipeTests<InventoryAllocationInquiryParameter, InventoryAllocationInquiryResult>
{
    private Mock<IWarehouseRepository> warehouseRepository;

    private readonly List<WarehouseDto> cachedWarehouses = new List<WarehouseDto>
    {
        new WarehouseDto { Id = Guid.NewGuid(), Name = "Warehouse1" },
        new WarehouseDto { Id = Guid.NewGuid(), Name = "Warehouse2" }
    };

    public override Type PipeType => typeof(AddWarehousesToRequests);

    public override void SetUp()
    {
        this.warehouseRepository = this.container.GetMock<IWarehouseRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<IWarehouseRepository>())
            .Returns(this.warehouseRepository.Object);

        this.WhenGetCachedWarehousesIs(this.cachedWarehouses);
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Add_All_Cached_Warehouses_When_GetInventoryParameter_GetWarehouses_Is_True()
    {
        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter { GetWarehouses = true };

        var result = this.RunExecute(parameter);

        Assert.IsTrue(result.InventoryAllocationInquiryRequests.Count == 2);
        Assert.IsTrue(
            result.InventoryAllocationInquiryRequests.Any(
                o => o.WarehouseID == this.cachedWarehouses[0].Name
            )
        );
        Assert.IsTrue(
            result.InventoryAllocationInquiryRequests.Any(
                o => o.WarehouseID == this.cachedWarehouses[1].Name
            )
        );
    }

    [Test]
    public void Execute_Should_Add_GetInventoryParameter_Warehouse_When_GetInventoryParameter_GetWarehouses_Is_False()
    {
        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            GetWarehouses = false,
            WarehouseId = this.cachedWarehouses.First().Id
        };

        var result = this.RunExecute(parameter);

        Assert.IsTrue(result.InventoryAllocationInquiryRequests.Count == 1);
        Assert.IsTrue(
            result.InventoryAllocationInquiryRequests.Any(
                o => o.WarehouseID == this.cachedWarehouses[0].Name
            )
        );
    }

    [Test]
    public void Execute_Should_Add_SiteContext_Warehouse_When_GetInventoryParameter_GetWarehouses_Is_False()
    {
        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter
        {
            GetWarehouses = false,
            WarehouseId = Guid.NewGuid()
        };

        this.WhenSiteContextWarehouseDtoIs(this.cachedWarehouses[1]);

        var result = this.RunExecute(parameter);

        Assert.IsTrue(result.InventoryAllocationInquiryRequests.Count == 1);
        Assert.IsTrue(
            result.InventoryAllocationInquiryRequests.Any(
                o => o.WarehouseID == this.cachedWarehouses[1].Name
            )
        );
    }

    protected override InventoryAllocationInquiryResult GetDefaultResult()
    {
        return new InventoryAllocationInquiryResult
        {
            InventoryAllocationInquiryRequests = new List<InventoryAllocationInquiry>
            {
                new InventoryAllocationInquiry { InventoryID = "Prod1" }
            }
        };
    }

    private void WhenGetCachedWarehousesIs(IEnumerable<WarehouseDto> warehouses)
    {
        this.warehouseRepository.Setup(o => o.GetCachedWarehouses()).Returns(warehouses);
    }
}
