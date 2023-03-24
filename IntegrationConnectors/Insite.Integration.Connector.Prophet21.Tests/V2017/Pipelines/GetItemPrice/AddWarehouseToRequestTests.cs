namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetItemPrice;

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class AddWarehouseToRequestTests
    : BaseForPipeTests<GetItemPriceParameter, GetItemPriceResult>
{
    private IList<Warehouse> warehouses;

    public override Type PipeType => typeof(AddWarehouseToRequest);

    public override void SetUp()
    {
        this.warehouses = new List<Warehouse>();
        this.fakeUnitOfWork.SetupEntityList(this.warehouses);
    }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Set_Location_Id_From_Pricing_Service_Parameter_Warehouse()
    {
        var parameter = new GetItemPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { Warehouse = "MAIN" }
            }
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual("MAIN", result.GetItemPriceRequest.Request.LocationID);
    }

    [Test]
    public void Execute_Should_Get_Warehouse_From_Repository_Using_Get_Inventory_Parameter_Warehouse_Id()
    {
        var warehouse = Some.Warehouse().WithName("MAIN").Build();

        var parameter = new GetItemPriceParameter
        {
            GetInventoryParameter = new GetInventoryParameter { WarehouseId = warehouse.Id }
        };

        this.WhenExists(warehouse);

        var result = this.RunExecute(parameter);

        Assert.AreEqual("MAIN", result.GetItemPriceRequest.Request.LocationID);
    }

    [Test]
    public void Execute_Should_Get_Warehouse_From_Site_Context_Warehouse()
    {
        this.WhenSiteContextWarehouseDtoIs(new WarehouseDto { Name = "MAIN" });

        var result = this.RunExecute();

        Assert.AreEqual("MAIN", result.GetItemPriceRequest.Request.LocationID);
    }

    protected override GetItemPriceResult GetDefaultResult()
    {
        return new GetItemPriceResult
        {
            GetItemPriceRequest = new GetItemPrice { Request = new Request() }
        };
    }

    protected void WhenExists(Warehouse warehouse)
    {
        this.warehouses.Add(warehouse);
    }
}
