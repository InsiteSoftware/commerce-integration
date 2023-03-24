namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderTotal;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AddBillToToRequestTests : BaseForPipeTests<OrderTotalParameter, OrderTotalResult>
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
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_CustomerNumber_With_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder().With(Some.Customer().WithErpNumber("123")).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.Customer.ErpNumber,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.CustomerNumber
        );
    }

    [Test]
    public void Execute_Should_Populate_CustomerNumber_With_Guest_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder().With(Some.Customer()).Build();

        var guestCustomer = Some.Customer().WithErpNumber("456").Build();

        this.WhenExists(guestCustomer);
        this.WhenGuestErpCustomerIdIs(guestCustomer.Id);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            guestCustomer.ErpNumber,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.CustomerNumber
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_CustomerNumber_When_Customer_Order_Customer_And_Guest_Customer_ErpNumbers_Are_Null_Or_Empty()
    {
        var customerOrder = Some.CustomerOrder().With(Some.Customer()).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.That(
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.CustomerNumber,
            Is.Null.Or.Empty
        );
    }

    protected override OrderTotalResult GetDefaultResult()
    {
        return new OrderTotalResult
        {
            OrderTotalRequest = new OrderTotalRequest
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
