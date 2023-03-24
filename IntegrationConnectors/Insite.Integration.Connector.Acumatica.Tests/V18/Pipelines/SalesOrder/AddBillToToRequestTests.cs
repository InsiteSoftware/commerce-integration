namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.SalesOrder;

using System;
using System.Collections.Generic;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AddBillToToRequestTests : BaseForPipeTests<SalesOrderParameter, SalesOrderResult>
{
    private IList<Customer> customers;

    private Mock<CustomerDefaultsSettings> customerDefaultSettings;

    private Mock<IStateRepository> stateRepository;

    private Mock<ICountryRepository> countryRepository;

    public override Type PipeType => typeof(AddBillToToRequest);

    public override void SetUp()
    {
        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);

        this.customerDefaultSettings = this.container.GetMock<CustomerDefaultsSettings>();
        this.WhenGuestErpCustomerIdIs(Guid.Empty);

        this.stateRepository = this.container.GetMock<IStateRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<IStateRepository>())
            .Returns(this.stateRepository.Object);

        this.countryRepository = this.container.GetMock<ICountryRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<ICountryRepository>())
            .Returns(this.countryRepository.Object);
    }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_CustomerID_With_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder().With(Some.Customer().WithErpNumber("123")).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customerOrder.Customer.ErpNumber, result.SalesOrderRequest.CustomerID);
    }

    [Test]
    public void Execute_Should_Populate_CustomerID_With_Guest_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder().With(Some.Customer()).Build();

        var guestCustomer = Some.Customer().WithErpNumber("456").Build();

        this.WhenExists(guestCustomer);
        this.WhenGuestErpCustomerIdIs(guestCustomer.Id);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(guestCustomer.ErpNumber, result.SalesOrderRequest.CustomerID);
    }

    [Test]
    public void Execute_Should_Not_Populate_CustomerID_When_Customer_ErpNumber_And_Guest_Customer_ErpNumber_Are_Null_Or_Empty()
    {
        var customerOrder = Some.CustomerOrder().With(Some.Customer()).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.That(result.SalesOrderRequest.CustomerID, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Populate_BillToAddress_State_With_State_Abbreviation()
    {
        var customerOrder = Some.CustomerOrder()
            .WithBTAddress1("110 5th St N")
            .WithBTAddress2("Suite 800")
            .WithBTCity("Minneapolis")
            .WithBTState("Minnesota")
            .WithBTCountry("United States")
            .WithBTPostalCode("55402")
            .Build();

        var state = Some.State().WithAbbreviation("MN").Build();
        this.WhenGetStateByNameIs(customerOrder.BTState, state);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(state.Abbreviation, result.SalesOrderRequest.BillToAddress.State);
    }

    [Test]
    public void Execute_Should_Populate_BillToAddress_State_With_STState()
    {
        var customerOrder = Some.CustomerOrder()
            .WithBTAddress1("110 5th St N")
            .WithBTAddress2("Suite 800")
            .WithBTCity("Minneapolis")
            .WithBTState("Minnesota")
            .WithBTCountry("United States")
            .WithBTPostalCode("55402")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customerOrder.BTState, result.SalesOrderRequest.BillToAddress.State);
    }

    [Test]
    public void Execute_Should_Populate_BillToAddress_CountryCode_With_Country_Abbreviation()
    {
        var customerOrder = Some.CustomerOrder()
            .WithBTAddress1("110 5th St N")
            .WithBTAddress2("Suite 800")
            .WithBTCity("Minneapolis")
            .WithBTState("Minnesota")
            .WithBTCountry("United States")
            .WithBTPostalCode("55402")
            .Build();

        var country = Some.Country().WithAbbreviation("USA").Build();
        this.WhenGetCountryByNameIs(customerOrder.BTCountry, country);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(country.Abbreviation, result.SalesOrderRequest.BillToAddress.Country);
    }

    [Test]
    public void Execute_Should_Populate_BillToAddress_CountryCode_With_BTCountry()
    {
        var customerOrder = Some.CustomerOrder()
            .WithBTAddress1("110 5th St N")
            .WithBTAddress2("Suite 800")
            .WithBTCity("Minneapolis")
            .WithBTState("Minnesota")
            .WithBTCountry("United States")
            .WithBTPostalCode("55402")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customerOrder.BTCountry, result.SalesOrderRequest.BillToAddress.Country);
    }

    [Test]
    public void GetShipToCountry_Should_Return_Country_Abbreviation_From_Database_When_Exists()
    {
        var country = Some.Country().WithName("Canada").WithAbbreviation("CA").Build();
        var customerOrder = Some.CustomerOrder().WithBTCountry("Canada").Build();

        this.WhenGetCountryByNameIs(country.Name, country);

        var result = (string)
            this.RunPrivateMethod("GetBillToCountry", this.unitOfWork.Object, customerOrder);

        Assert.AreEqual(country.Abbreviation, result);
    }

    [Test]
    public void GetShipToCountry_Should_Return_CustomerOrder_BTCountry_When_Country_Does_Not_Exist_In_Database()
    {
        var customerOrder = Some.CustomerOrder().WithBTCountry("Canada").Build();

        var result = (string)
            this.RunPrivateMethod("GetBillToCountry", this.unitOfWork.Object, customerOrder);

        Assert.AreEqual(customerOrder.BTCountry, result);
    }

    [Test]
    public void GetBillToCountry_Should_Return_Default_US_When_Country_Does_Not_Exist_In_Database_And_CustomerOrder_BTCountry_IsBlank()
    {
        var customerOrder = Some.CustomerOrder().WithBTCountry(string.Empty).Build();

        var result = (string)
            this.RunPrivateMethod("GetBillToCountry", this.unitOfWork.Object, customerOrder);

        Assert.AreEqual("US", result);
    }

    protected override SalesOrderResult GetDefaultResult()
    {
        return new SalesOrderResult { SalesOrderRequest = new SalesOrder() };
    }

    protected void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }

    protected void WhenGuestErpCustomerIdIs(Guid guestErpCustomerId)
    {
        this.customerDefaultSettings.Setup(o => o.GuestErpCustomerId).Returns(guestErpCustomerId);
    }

    protected void WhenGetStateByNameIs(string name, State state)
    {
        this.stateRepository.Setup(o => o.GetStateByName(name)).Returns(state);
    }

    protected void WhenGetCountryByNameIs(string name, Country country)
    {
        this.countryRepository.Setup(o => o.GetCountryByName(name)).Returns(country);
    }
}
