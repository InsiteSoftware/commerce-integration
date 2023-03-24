namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetCartSummary;

using System;

using NUnit.Framework;

using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class AddHeaderInfoToRequestTests
    : BaseForPipeTests<GetCartSummaryParameter, GetCartSummaryResult>
{
    public override Type PipeType => typeof(AddHeaderInfoToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_GetCartSummaryRequest_Request_Properties()
    {
        var customerOrder = Some.CustomerOrder()
            .WithOrderNumber("ABC123")
            .WithCustomerPO("Test")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderNumber,
            result.GetCartSummaryRequest.Request.WebReferenceNumber
        );
        Assert.AreEqual(customerOrder.CustomerPO, result.GetCartSummaryRequest.Request.PONumber);
    }

    [Test]
    public void Execute_Should_Populate_GetCartSummaryRequest_Request_CartRequireDate_With_CustomerOrder_RequestedPickupDate()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithRequestedPickupDate(DateTimeOffset.Now.AddDays(2))
            .WithRequestedDeliveryDate(DateTimeOffset.Now.AddDays(4))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.RequestedPickupDate?.ToString("yyyy-MM-dd"),
            result.GetCartSummaryRequest.Request.CartRequiredDate
        );
    }

    [Test]
    public void Execute_Should_Populate_GetCartSummaryRequest_Request_CartRequireDate_With_CustomerOrder_RequestedDeliveryDate()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
            .WithRequestedPickupDate(DateTimeOffset.Now.AddDays(2))
            .WithRequestedDeliveryDate(DateTimeOffset.Now.AddDays(4))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.RequestedDeliveryDate?.ToString("yyyy-MM-dd"),
            result.GetCartSummaryRequest.Request.CartRequiredDate
        );
    }

    protected override GetCartSummaryResult GetDefaultResult()
    {
        return new GetCartSummaryResult
        {
            GetCartSummaryRequest = new GetCartSummary { Request = new Request() }
        };
    }
}
