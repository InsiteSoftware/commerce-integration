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
public class AddShipToToRequestTests : BaseForPipeTests<SalesOrderParameter, SalesOrderResult>
{
    private IList<Customer> customers;

    private Mock<CustomerDefaultsSettings> customerDefaultSettings;

    private Mock<IStateRepository> stateRepository;

    private Mock<ICountryRepository> countryRepository;

    public override Type PipeType => typeof(AddShipToToRequest);

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
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_LocationID_With_ShipTo_ErpSequence()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithErpSequence("123"))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customerOrder.ShipTo.ErpSequence, result.SalesOrderRequest.LocationID);
    }

    [Test]
    public void Execute_Should_Populate_LocationID_With_Guest_ShipTo_ErpSequence()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        var guestCustomer = Some.Customer().WithErpSequence("456").Build();

        this.WhenExists(guestCustomer);
        this.WhenGuestErpCustomerIdIs(guestCustomer.Id);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(guestCustomer.ErpSequence, result.SalesOrderRequest.LocationID);
    }

    [Test]
    public void Execute_Should_Not_Populate_LocationID_When_ShipTo_ErpSequence_And_Guest_Customer_ErpSequence_Are_Null_Or_Empty()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.That(result.SalesOrderRequest.LocationID, Is.Null.Or.Empty);
    }

    public void Execute_Should_Populate_ShipToAddress()
    {
        var customerOrder = Some.CustomerOrder()
            .WithSTAddress1("110 5th St N")
            .WithSTAddress2("Suite 800")
            .WithSTCity("Minneapolis")
            .WithSTState("Minnesota")
            .WithSTCountry("United States")
            .WithSTPostalCode("55402")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.STAddress1,
            result.SalesOrderRequest.ShipToAddress.AddressLine1
        );
        Assert.AreEqual(
            customerOrder.STAddress2,
            result.SalesOrderRequest.ShipToAddress.AddressLine2
        );
        Assert.AreEqual(customerOrder.STCity, result.SalesOrderRequest.ShipToAddress.City);
        Assert.AreEqual(
            customerOrder.STPostalCode,
            result.SalesOrderRequest.ShipToAddress.PostalCode
        );
    }

    [Test]
    public void Execute_Should_Populate_ShipToAddress_State_With_State_Abbreviation()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithIsDropShip(true))
            .WithSTAddress1("110 5th St N")
            .WithSTAddress2("Suite 800")
            .WithSTCity("Minneapolis")
            .WithSTState("Minnesota")
            .WithSTCountry("United States")
            .WithSTPostalCode("55402")
            .Build();

        var state = Some.State().WithAbbreviation("MN").Build();
        this.WhenGetStateByNameIs(customerOrder.STState, state);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(state.Abbreviation, result.SalesOrderRequest.ShipToAddress.State);
    }

    [Test]
    public void Execute_Should_Populate_ShipToAddress_State_With_STState()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithIsDropShip(true))
            .WithSTAddress1("110 5th St N")
            .WithSTAddress2("Suite 800")
            .WithSTCity("Minneapolis")
            .WithSTState("Minnesota")
            .WithSTCountry("United States")
            .WithSTPostalCode("55402")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customerOrder.STState, result.SalesOrderRequest.ShipToAddress.State);
    }

    [Test]
    public void Execute_Should_Populate_ShipToAddress_Country_With_Country_Abbreviation()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithIsDropShip(true))
            .WithSTAddress1("110 5th St N")
            .WithSTAddress2("Suite 800")
            .WithSTCity("Minneapolis")
            .WithSTState("Minnesota")
            .WithSTCountry("United States")
            .WithSTPostalCode("55402")
            .Build();

        var country = Some.Country().WithAbbreviation("USA").Build();
        this.WhenGetCountryByNameIs(customerOrder.STCountry, country);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(country.Abbreviation, result.SalesOrderRequest.ShipToAddress.Country);
    }

    [Test]
    public void Execute_Should_Populate_ShipToAddress_CountryShipToAddress_With_STState()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithIsDropShip(true))
            .WithSTAddress1("110 5th St N")
            .WithSTAddress2("Suite 800")
            .WithSTCity("Minneapolis")
            .WithSTState("Minnesota")
            .WithSTCountry("United States")
            .WithSTPostalCode("55402")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customerOrder.STCountry, result.SalesOrderRequest.ShipToAddress.Country);
    }

    [Test]
    public void GetShipToCountry_Should_Return_Country_Abbreviation_From_Database_When_Exists()
    {
        var country = Some.Country().WithName("Canada").WithAbbreviation("CA").Build();
        var customerOrder = Some.CustomerOrder().WithSTCountry("Canada").Build();

        this.WhenGetCountryByNameIs(country.Name, country);

        var result = (string)
            this.RunPrivateMethod("GetShipToCountry", this.unitOfWork.Object, customerOrder);

        Assert.AreEqual(country.Abbreviation, result);
    }

    [Test]
    public void GetShipToCountry_Should_Return_CustomerOrder_StCountry_When_Country_Does_Not_Exist_In_Database()
    {
        var customerOrder = Some.CustomerOrder().WithSTCountry("Canada").Build();

        var result = (string)
            this.RunPrivateMethod("GetShipToCountry", this.unitOfWork.Object, customerOrder);

        Assert.AreEqual(customerOrder.STCountry, result);
    }

    [Test]
    public void GetShipToCountry_Should_Return_Default_US_When_Country_Does_Not_Exist_In_Database_And_CustomerOrder_StCountry_IsBlank()
    {
        var customerOrder = Some.CustomerOrder().WithSTCountry(string.Empty).Build();

        var result = (string)
            this.RunPrivateMethod("GetShipToCountry", this.unitOfWork.Object, customerOrder);

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
