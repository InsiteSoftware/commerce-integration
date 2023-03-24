#pragma warning disable CA1001
namespace Insite.Integration.Connector.Base.Tests.Helpers;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Common.Dependencies;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers;
using Insite.Core.TestHelpers.Builders;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Helpers;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CustomerHelperTests
{
    private FakeUnitOfWork fakeUnitOfWork;

    private Mock<ISiteContext> siteContext;

    private Mock<CustomerDefaultsSettings> customerDefaultSettings;

    private IList<Customer> customers;

    private CustomerHelper customerHelper;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.siteContext = container.GetMock<ISiteContext>();
        var dependencyLocator = container.GetMock<IDependencyLocator>();
        TestHelper.MockSiteContext(this.siteContext, dependencyLocator);

        var dataProvider = container.GetMock<IDataProvider>();
        var unitOfWork = container.GetMock<IUnitOfWork>();
        unitOfWork.Setup(o => o.DataProvider).Returns(dataProvider.Object);
        this.fakeUnitOfWork = new FakeUnitOfWork(unitOfWork.Object);

        this.customerDefaultSettings = container.GetMock<CustomerDefaultsSettings>();

        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);

        this.customerHelper = container.Resolve<CustomerHelper>();
    }

    [Test]
    public void GetBillTo_Should_Return_Customer_For_PricingServiceParameter_BillToId()
    {
        var customer = Some.Customer().Build();
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty)
        {
            BillToId = customer.Id
        };

        this.WhenExists(customer);

        var result = this.customerHelper.GetBillTo(this.fakeUnitOfWork, pricingServiceParameter);

        result.Should().Be(customer);
    }

    [Test]
    public void GetBillTo_Should_Return_Customer_From_SiteContext_BillTo_When_PricingServiceParameter_BillToId_Is_Null()
    {
        var customer = Some.Customer().Build();
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        this.WhenSiteContextBillToIs(customer);

        var result = this.customerHelper.GetBillTo(this.fakeUnitOfWork, pricingServiceParameter);

        result.Should().Be(customer);
    }

    [Test]
    public void GetBillTo_Should_Return_Customer_For_GuestErpCustomerId_When_PricingServiceParameter_BillToId_Is_Null_And_SiteContext_BillTo_Is_Null()
    {
        var customer = Some.Customer().Build();
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        this.WhenExists(customer);
        this.WhenGuestErpCustomerIdIs(customer.Id);

        var result = this.customerHelper.GetBillTo(this.fakeUnitOfWork, pricingServiceParameter);

        result.Should().Be(customer);
    }

    [Test]
    public void GetBillTo_Should_Return_Customer_PricingCustomer_If_Exists()
    {
        var customer = Some.Customer().WithPricingCustomer(Some.Customer()).Build();
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty)
        {
            BillToId = customer.Id
        };

        this.WhenExists(customer);

        var result = this.customerHelper.GetBillTo(this.fakeUnitOfWork, pricingServiceParameter);

        result.Should().Be(customer.PricingCustomer);
    }

    [Test]
    public void GetShipTo_Should_Return_Customer_For_PricingServiceParameter_ShipToId()
    {
        var customer = Some.Customer().Build();
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty)
        {
            ShipToId = customer.Id
        };

        this.WhenExists(customer);

        var result = this.customerHelper.GetShipTo(this.fakeUnitOfWork, pricingServiceParameter);

        result.Should().Be(customer);
    }

    [Test]
    public void GetShipTo_Should_Return_Customer_From_SiteContext_ShipTo_When_PricingServiceParameter_ShipToId_Is_Null()
    {
        var customer = Some.Customer().Build();
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        this.WhenSiteContextShipToIs(customer);

        var result = this.customerHelper.GetShipTo(this.fakeUnitOfWork, pricingServiceParameter);

        result.Should().Be(customer);
    }

    [Test]
    public void GetShipTo_Should_Return_Customer_For_GuestErpCustomerId_When_PricingServiceParameter_ShipToId_Is_Null_And_SiteContext_ShipTo_Is_Null()
    {
        var customer = Some.Customer().Build();
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        this.WhenExists(customer);
        this.WhenGuestErpCustomerIdIs(customer.Id);

        var result = this.customerHelper.GetShipTo(this.fakeUnitOfWork, pricingServiceParameter);

        result.Should().Be(customer);
    }

    [Test]
    public void GetShipTo_Should_Return_Customer_PricingCustomer_If_Exists()
    {
        var customer = Some.Customer().WithPricingCustomer(Some.Customer()).Build();
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty)
        {
            ShipToId = customer.Id
        };

        this.WhenExists(customer);

        var result = this.customerHelper.GetShipTo(this.fakeUnitOfWork, pricingServiceParameter);

        result.Should().Be(customer.PricingCustomer);
    }

    [Test]
    public void GetBillTo_Should_Return_CustomerOrder_Customer()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber("ABC123"))
            .Build();

        var result = this.customerHelper.GetBillTo(this.fakeUnitOfWork, customerOrder);

        result.Should().Be(customerOrder.Customer);
    }

    [Test]
    public void GetBillTo_Should_Return_Customer_For_GuestErpCustomerId_When_CustomerOrder_Customer_Is_Null()
    {
        var customer = Some.Customer().Build();
        var customerOrder = Some.CustomerOrder().Build();

        this.WhenExists(customer);
        this.WhenGuestErpCustomerIdIs(customer.Id);

        var result = this.customerHelper.GetBillTo(this.fakeUnitOfWork, customerOrder);
        result.Should().Be(customer);
    }

    [Test]
    public void GetBillTo_Should_Return_Null_When_CustomerOrder_Customer_Is_Null_And_GuestErpCustomerId_Is_Null()
    {
        var customerOrder = Some.CustomerOrder().Build();

        var result = this.customerHelper.GetBillTo(this.fakeUnitOfWork, customerOrder);

        result.Should().BeNull();
    }

    [Test]
    public void GetShipTo_Should_Return_CustomerOrder_ShipTo()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        var result = this.customerHelper.GetShipTo(this.fakeUnitOfWork, customerOrder);

        result.Should().Be(customerOrder.ShipTo);
    }

    [Test]
    public void GetShipTo_Should_Return_Customer_For_GuestErpCustomerId_When_CustomerOrder_Customer_Is_Null()
    {
        var customer = Some.Customer().Build();
        var customerOrder = Some.CustomerOrder().Build();

        this.WhenExists(customer);
        this.WhenGuestErpCustomerIdIs(customer.Id);

        var result = this.customerHelper.GetShipTo(this.fakeUnitOfWork, customerOrder);
        result.Should().Be(customer);
    }

    [Test]
    public void GetShipTo_Should_Return_Null_When_CustomerOrder_Customer_Is_Null_And_GuestErpCustomerId_Is_Null()
    {
        var customerOrder = Some.CustomerOrder().Build();

        var result = this.customerHelper.GetShipTo(this.fakeUnitOfWork, customerOrder);

        result.Should().BeNull();
    }

    private void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }

    protected void WhenSiteContextBillToIs(Customer billTo)
    {
        this.siteContext.Setup(o => o.BillTo).Returns(billTo);
    }

    protected void WhenSiteContextShipToIs(Customer shipTo)
    {
        this.siteContext.Setup(o => o.ShipTo).Returns(shipTo);
    }

    protected void WhenGuestErpCustomerIdIs(Guid guestErpCustomerId)
    {
        this.customerDefaultSettings.Setup(o => o.GuestErpCustomerId).Returns(guestErpCustomerId);
    }
}
