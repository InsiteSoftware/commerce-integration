namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetCartSummary;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class AddShipToToRequestTests
    : BaseForPipeTests<GetCartSummaryParameter, GetCartSummaryResult>
{
    private IList<Customer> customers;

    private Mock<CustomerDefaultsSettings> customerDefaultSettings;

    public override Type PipeType => typeof(AddShipToToRequest);

    public override void SetUp()
    {
        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);

        this.customerDefaultSettings = this.container.GetMock<CustomerDefaultsSettings>();
        this.WhenGuestErpCustomerIdIs(Guid.Empty);
    }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_GetCartSummaryRequest_Request_ShipToID_With_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithErpSequence("123"))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.ShipTo.ErpSequence,
            result.GetCartSummaryRequest.Request.ShipToID
        );
    }

    [Test]
    public void Execute_Should_Populate_GetCartSummaryRequest_Request_ShipToID_With_Guest_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        var guestCustomer = Some.Customer().WithErpSequence("456").Build();

        this.WhenExists(guestCustomer);
        this.WhenGuestErpCustomerIdIs(guestCustomer.Id);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(guestCustomer.ErpSequence, result.GetCartSummaryRequest.Request.ShipToID);
    }

    [Test]
    public void Execute_Should__Not_Populate_GetCartSummaryRequest_Request_ShipToID_When_Customer_Order_Customer_And_Guest_Customer_ErpNumbers_Are_Null_Or_Empty()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.That(result.GetCartSummaryRequest.Request.ShipToID, Is.Null.Or.Empty);
    }

    protected override GetCartSummaryResult GetDefaultResult()
    {
        return new GetCartSummaryResult
        {
            GetCartSummaryRequest = new GetCartSummary { Request = new Request() }
        };
    }

    protected void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }

    protected void WhenGuestErpCustomerIdIs(Guid guestErpCustomerId)
    {
        this.customerDefaultSettings.Setup(o => o.GuestErpCustomerId).Returns(guestErpCustomerId);
    }
}
