namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.CreateCustomerOrder;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;

[TestFixture]
public class AddBillToToRequestTests
    : BaseForPipeTests<CreateCustomerOrderParameter, CreateCustomerOrderResult>
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
    public void Execute_Should_Populate_CustomerNo_With_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder().With(Some.Customer().WithErpNumber("123")).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customerOrder.Customer.ErpNumber, result.CustomerOrder.customerNo);
    }

    [Test]
    public void Execute_Should_Populate_CustomerNo_With_Guest_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder().With(Some.Customer()).Build();

        var guestCustomer = Some.Customer().WithErpNumber("456").Build();

        this.WhenExists(guestCustomer);
        this.WhenGuestErpCustomerIdIs(guestCustomer.Id);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(guestCustomer.ErpNumber, result.CustomerOrder.customerNo);
    }

    [Test]
    public void Execute_Should_Not_Populate_CustomerNo_When_Customer_ErpNumber_And_Guest_Customer_ErpNumber_Are_Null_Or_Empty()
    {
        var customerOrder = Some.CustomerOrder().With(Some.Customer()).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.That(result.CustomerOrder.customerNo, Is.Null.Or.Empty);
    }

    protected override CreateCustomerOrderResult GetDefaultResult()
    {
        return new CreateCustomerOrderResult { CustomerOrder = new customerOrder() };
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
