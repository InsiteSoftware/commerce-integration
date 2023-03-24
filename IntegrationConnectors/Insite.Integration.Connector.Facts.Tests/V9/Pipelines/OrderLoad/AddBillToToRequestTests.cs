namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderLoad;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AddBillToToRequestTests : BaseForPipeTests<OrderLoadParameter, OrderLoadResult>
{
    private IList<Customer> customers;

    private Mock<CustomerDefaultsSettings> customerDefaultSettings;

    public override Type PipeType => typeof(AddBillToToRequest);

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
    public void Execute_Should_Populate_CustomerNumber_With_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber("123"))
            .WithPlacedByUserName("admin")
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.Customer.ErpNumber,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.CustomerNumber
        );
    }

    [Test]
    public void Execute_Should_Populate_CustomerNumber_With_Guest_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer())
            .WithPlacedByUserName("admin")
            .Build();

        var guestCustomer = Some.Customer().WithErpNumber("456").Build();

        this.WhenExists(guestCustomer);
        this.WhenGuestErpCustomerIdIs(guestCustomer.Id);

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            guestCustomer.ErpNumber,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.CustomerNumber
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_CustomerNumber_When_Customer_Order_Customer_And_Guest_Customer_ErpNumbers_Are_Null_Or_Empty()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer())
            .WithPlacedByUserName("admin")
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.That(
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.CustomerNumber,
            Is.Null.Or.Empty
        );
    }

    [Test]
    public void Execute_Should_Populate_BillToContact_With_STEmail_When_CustomerOrder_IsGuestOrder_Is_True()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer())
            .WithIsGuestOrder(true)
            .WithSTEmail("GuestEmail")
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.STEmail,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.BillToContact
        );
    }

    [Test]
    public void Execute_Should_Populate_BillToContact_With_PlacedByUserName_When_CustomerOrder_IsGuestOrder_Is_False()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer())
            .WithIsGuestOrder(false)
            .WithPlacedByUserName("admin")
            .Build();

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.PlacedByUserName,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.BillToContact
        );
    }

    protected override OrderLoadResult GetDefaultResult()
    {
        return new OrderLoadResult
        {
            OrderLoadRequest = new OrderLoadRequest
            {
                Request = new Request
                {
                    Orders = new List<RequestOrder>
                    {
                        new RequestOrder { OrderHeader = new OrderHeader() }
                    }
                }
            }
        };
    }

    private void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }

    private void WhenGuestErpCustomerIdIs(Guid guestErpCustomerId)
    {
        this.customerDefaultSettings.Setup(o => o.GuestErpCustomerId).Returns(guestErpCustomerId);
    }
}
