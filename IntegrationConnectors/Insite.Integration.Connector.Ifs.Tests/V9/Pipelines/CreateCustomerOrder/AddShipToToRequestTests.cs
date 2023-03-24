namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.CreateCustomerOrder;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;

[TestFixture]
public class AddShipToToRequestTests
    : BaseForPipeTests<CreateCustomerOrderParameter, CreateCustomerOrderResult>
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
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_ShipAddrNo_With_ShipTo_ErpSequence()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithErpSequence("123"))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customerOrder.ShipTo.ErpSequence, result.CustomerOrder.shipAddrNo);
    }

    [Test]
    public void Execute_Should_Populate_ShipAddrNo_With_Guest_ShipTo_ErpSequence()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        var guestCustomer = Some.Customer().WithErpSequence("456").Build();

        this.WhenExists(guestCustomer);
        this.WhenGuestErpCustomerIdIs(guestCustomer.Id);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(guestCustomer.ErpSequence, result.CustomerOrder.shipAddrNo);
    }

    [Test]
    public void Execute_Should_Not_Populate_ShipAddrNo_When_ShipTo_ErpSequence_And_Guest_Customer_ErpSequence_Are_Null_Or_Empty()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.That(result.CustomerOrder.shipAddrNo, Is.Null.Or.Empty);
    }

    [TestCase("")]
    [TestCase("Insite Software")]
    public void Execute_Should_Populate_Delivery_Address_When_CustomerOrder_Is_PickUp(
        string companyName
    )
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithShipTo(Some.Customer().WithIsDropShip(false))
            .WithSTCompanyName(companyName)
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
            customerOrder.STCompanyName.IsNotBlank()
                ? customerOrder.STCompanyName
                : customerOrder.STAddress1,
            result.CustomerOrder.deliveryAddress.addr1
        );
        Assert.AreEqual(customerOrder.STAddress1, result.CustomerOrder.deliveryAddress.address1);
        Assert.AreEqual(customerOrder.STAddress2, result.CustomerOrder.deliveryAddress.address2);
        Assert.AreEqual(customerOrder.STCity, result.CustomerOrder.deliveryAddress.city);
        Assert.AreEqual(customerOrder.STPostalCode, result.CustomerOrder.deliveryAddress.zipCode);
    }

    [TestCase("")]
    [TestCase("Insite Software")]
    public void Execute_Should_Populate_DeliveryAddress_When_ShipTo_IsDropShip(string companyName)
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
            .WithShipTo(Some.Customer().WithIsDropShip(true))
            .WithSTCompanyName(companyName)
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
            customerOrder.STCompanyName.IsNotBlank()
                ? customerOrder.STCompanyName
                : customerOrder.STAddress1,
            result.CustomerOrder.deliveryAddress.addr1
        );
        Assert.AreEqual(customerOrder.STAddress1, result.CustomerOrder.deliveryAddress.address1);
        Assert.AreEqual(customerOrder.STAddress2, result.CustomerOrder.deliveryAddress.address2);
        Assert.AreEqual(customerOrder.STCity, result.CustomerOrder.deliveryAddress.city);
        Assert.AreEqual(customerOrder.STPostalCode, result.CustomerOrder.deliveryAddress.zipCode);
    }

    [Test]
    public void Execute_Should_Populate_DeliveryAddress_State_With_State_Abbreviation()
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

        Assert.AreEqual(state.Abbreviation, result.CustomerOrder.deliveryAddress.state);
    }

    [Test]
    public void Execute_Should_Populate_DeliveryAddress_State_With_STState()
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

        Assert.AreEqual(customerOrder.STState, result.CustomerOrder.deliveryAddress.state);
    }

    [Test]
    public void Execute_Should_Populate_DeliveryAddress_CountryCode_With_Country_Abbreviation()
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

        Assert.AreEqual(country.Abbreviation, result.CustomerOrder.deliveryAddress.countryCode);
    }

    [Test]
    public void Execute_Should_Populate_DeliveryAddress_CountryCode_With_STState()
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

        Assert.AreEqual(customerOrder.STCountry, result.CustomerOrder.deliveryAddress.countryCode);
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

    protected void WhenGetStateByNameIs(string name, State state)
    {
        this.stateRepository.Setup(o => o.GetStateByName(name)).Returns(state);
    }

    protected void WhenGetCountryByNameIs(string name, Country country)
    {
        this.countryRepository.Setup(o => o.GetCountryByName(name)).Returns(country);
    }
}
