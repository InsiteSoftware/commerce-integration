namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetPartAvailability;

using System;
using System.Collections.Generic;

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
public class AddWarehouseToRequestTests
    : BaseForPipeTests<GetPartAvailabilityParameter, GetPartAvailabilityResult>
{
    private IList<Warehouse> warehouses;

    public override Type PipeType => typeof(AddWarehouseToRequest);

    public override void SetUp()
    {
        this.warehouses = new List<Warehouse>();
        this.fakeUnitOfWork.SetupEntityList(this.warehouses);
    }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Not_Get_Warehouse_When_GetInventoryParameter_WarehouseId_Is_Null()
    {
        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter();

        var result = this.RunExecute(parameter);

        Assert.AreEqual(string.Empty, result.PartAvailabilityRequest.site);
    }

    [Test]
    public void Execute_Should_Not_Get_Warehouse_When_GetInventoryParameter_WarehouseId_Is_Not_Found_In_Repository()
    {
        var warehouse = Some.Warehouse().WithName("01").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter { WarehouseId = warehouse.Id };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(string.Empty, result.PartAvailabilityRequest.site);
    }

    [Test]
    public void Execute_Should_Get_Warehouse_From_GetInventoryParameter_Warehouse()
    {
        var warehouse = Some.Warehouse().WithName("01").Build();

        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter { WarehouseId = warehouse.Id };

        this.WhenExists(warehouse);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(warehouse.Name, result.PartAvailabilityRequest.site);
    }

    protected override GetPartAvailabilityResult GetDefaultResult()
    {
        return new GetPartAvailabilityResult
        {
            PartAvailabilityRequest = new partAvailabilityRequest()
        };
    }

    protected void WhenExists(Warehouse warehouse)
    {
        this.warehouses.Add(warehouse);
    }
}
