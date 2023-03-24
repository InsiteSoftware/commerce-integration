namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetCustomerPrice;

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

[TestFixture]
public class AddWarehouseToRequestTests
    : BaseForPipeTests<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    private IList<Warehouse> warehouses;

    public override Type PipeType => typeof(AddWarehouseToRequest);

    public override void SetUp()
    {
        this.warehouses = new List<Warehouse>();
        this.fakeUnitOfWork.SetupEntityList(this.warehouses);
    }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Set_Location_Id_From_Pricing_Service_Parameter_Warehouse()
    {
        var parameter = new GetCustomerPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { Warehouse = "MAIN" }
            }
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual("MAIN", result.CustomerPriceRequest.site);
    }

    [Test]
    public void Execute_Should_Get_Warehouse_From_Site_Context_Warehouse()
    {
        this.WhenSiteContextWarehouseDtoIs(new WarehouseDto { Name = "MAIN" });

        var result = this.RunExecute();

        Assert.AreEqual("MAIN", result.CustomerPriceRequest.site);
    }

    protected override GetCustomerPriceResult GetDefaultResult()
    {
        return new GetCustomerPriceResult { CustomerPriceRequest = new customerPriceRequest() };
    }

    protected void WhenExists(Warehouse warehouse)
    {
        this.warehouses.Add(warehouse);
    }
}
