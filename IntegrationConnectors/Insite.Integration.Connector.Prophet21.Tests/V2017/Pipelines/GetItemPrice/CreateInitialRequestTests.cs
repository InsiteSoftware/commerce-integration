namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetItemPrice;

using System;
using System.Collections.Generic;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class CreateInitialRequestTests : BaseForPipeTests<GetItemPriceParameter, GetItemPriceResult>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_New_GetItemPriceRequest()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.GetItemPriceRequest);
        Assert.IsNotNull(result.GetItemPriceRequest.Request);
    }

    [Test]
    public void Execute_Should_Populate_GetItemPriceRequest_Request_B2BSellerVersion()
    {
        var result = this.RunExecute();

        Assert.AreEqual("5", result.GetItemPriceRequest.Request.B2BSellerVersion.MajorVersion);
        Assert.AreEqual("11", result.GetItemPriceRequest.Request.B2BSellerVersion.MinorVersion);
        Assert.AreEqual("100", result.GetItemPriceRequest.Request.B2BSellerVersion.BuildNumber);
    }

    [Test]
    public void Execute_Should_Set_GetItemPriceRequest_Request_GetListOfItemLocationQuantities_To_False_When_GetInventoryParameter_Is_Null()
    {
        var result = this.RunExecute();

        Assert.AreEqual(
            "FALSE",
            result.GetItemPriceRequest.Request.GetListOfItemLocationQuantities
        );
    }

    [Test]
    public void Execute_Should_Set_GetItemPriceRequest_Request_GetListOfItemLocationQuantities_To_True_When_GetInventoryParameter_GetWarehouses_Is_True()
    {
        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter { GetWarehouses = true };

        var result = this.RunExecute(parameter);

        Assert.AreEqual("TRUE", result.GetItemPriceRequest.Request.GetListOfItemLocationQuantities);
    }

    [Test]
    public void Execute_Should_Set_GetItemPriceRequest_Request_GetListOfItemLocationQuantities_To_False_When_GetInventoryParameter_GetWarehouses_Is_False()
    {
        var parameter = this.GetDefaultParameter();
        parameter.GetInventoryParameter = new GetInventoryParameter { GetWarehouses = false };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "FALSE",
            result.GetItemPriceRequest.Request.GetListOfItemLocationQuantities
        );
    }

    [Test]
    public void Execute_Should_Set_GetItemPriceRequest_Request_GetAvailabilityOnly_To_False_When_PricingServiceParameters_Any()
    {
        var parameter = this.GetDefaultParameter();
        parameter.PricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty)
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual("FALSE", result.GetItemPriceRequest.Request.GetAvailabilityOnly);
    }

    [Test]
    public void Execute_Should_Set_GetItemPriceRequest_Request_GetAvailabilityOnly_To_True_When_Not_PricingServiceParameters_Any()
    {
        var parameter = this.GetDefaultParameter();

        var result = this.RunExecute(parameter);

        Assert.AreEqual("TRUE", result.GetItemPriceRequest.Request.GetAvailabilityOnly);
    }
}
