namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.CreateOrder;

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
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class AddShipToToRequestTests : BaseForPipeTests<CreateOrderParameter, CreateOrderResult>
{
    private Mock<CustomerDefaultsSettings> customerDefaultSettings;

    private IList<Customer> customers;

    private Mock<IStateRepository> stateRepository;

    private Mock<ICountryRepository> countryRepository;

    public override Type PipeType => typeof(AddShipToToRequest);

    public override void SetUp()
    {
        this.customerDefaultSettings = this.container.GetMock<CustomerDefaultsSettings>();

        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);

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
    public void Execute_Should_Set_ShipToNumber_To_CustomerOrder_ErpSequence_When_CustomerOrder_ErpSequence_Is_Not_Blank()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder.ShipTo = Some.Customer().WithErpSequence("ABC123");

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            parameter.CustomerOrder.ShipTo.ErpSequence,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToNumber
        );
    }

    [Test]
    public void Execute_Should_Set_ShipToNumber_To_Guest_Customer_ErpSequence_When_CustomerOrder_ErpSequence_Is_Blank_And_Guest_Customer_Exists()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder.ShipTo = Some.Customer().WithErpSequence(string.Empty);

        var guestCustomer = Some.Customer().WithErpSequence("ABC123").Build();

        this.WhenExists(guestCustomer);
        this.WhenGuestCustomerIdIs(guestCustomer.Id);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            guestCustomer.ErpSequence,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToNumber
        );
    }

    [Test]
    public void Execute_Should_Set_ShipToNumber_To_Empty_String_When_CustomerOrder_ErpSequence_Is_Blank_And_Guest_Customer_Does_Not_Exist()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder.ShipTo = Some.Customer().WithErpSequence(string.Empty);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(string.Empty, result.CreateOrderRequest.Orders[0].OrderHeader.ShipToNumber);
    }

    [Test]
    public void Execute_Should_Populate_ShipTo_Properties_When_CustomerOrder_FulfillmentMethod_Is_PickUp()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithShipTo(Some.Customer().WithIsDropShip(false))
            .WithSTAddress1("Address1")
            .WithSTAddress2("Address2")
            .WithSTAddress3("Address3")
            .WithSTAddress4("Address4")
            .WithSTCity("Minneapolis")
            .WithSTState("Minnesota")
            .WithSTCountry("United States")
            .WithSTPostalCode("55402")
            .WithSTCompanyName("Insite")
            .WithSTPhone("5555555555")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.STAddress1,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr1
        );
        Assert.AreEqual(
            customerOrder.STAddress2,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr2
        );
        Assert.AreEqual(
            customerOrder.STAddress3,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr3
        );
        Assert.AreEqual(
            customerOrder.STAddress4,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr4
        );
        Assert.AreEqual(
            customerOrder.STCity,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToCity
        );
        Assert.AreEqual(
            customerOrder.STState,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToState
        );
        Assert.AreEqual(
            customerOrder.STCountry,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToCountry
        );
        Assert.AreEqual(
            customerOrder.STPostalCode,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToZip
        );
        Assert.AreEqual(
            customerOrder.STCompanyName,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToName
        );
        Assert.AreEqual(
            customerOrder.STPhone,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToPhone
        );
    }

    [Test]
    public void Execute_Should_Populate_ShipTo_Properties_When_CustomerOrder_ShipTo_IsDropShip()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
            .WithShipTo(Some.Customer().WithIsDropShip(true))
            .WithSTAddress1("Address1")
            .WithSTAddress2("Address2")
            .WithSTAddress3("Address3")
            .WithSTAddress4("Address4")
            .WithSTCity("Minneapolis")
            .WithSTState("Minnesota")
            .WithSTCountry("United States")
            .WithSTPostalCode("55402")
            .WithSTCompanyName("Insite")
            .WithSTPhone("5555555555")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.STAddress1,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr1
        );
        Assert.AreEqual(
            customerOrder.STAddress2,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr2
        );
        Assert.AreEqual(
            customerOrder.STAddress3,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr3
        );
        Assert.AreEqual(
            customerOrder.STAddress4,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr4
        );
        Assert.AreEqual(
            customerOrder.STCity,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToCity
        );
        Assert.AreEqual(
            customerOrder.STState,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToState
        );
        Assert.AreEqual(
            customerOrder.STCountry,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToCountry
        );
        Assert.AreEqual(
            customerOrder.STPostalCode,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToZip
        );
        Assert.AreEqual(
            customerOrder.STCompanyName,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToName
        );
        Assert.AreEqual(
            customerOrder.STPhone,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToPhone
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_ShipTo_Properties_When_Not_FulfillmentMethod_Is_PickUp_And_Not_CustomerOrder_ShipTo_IsDropShip()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
            .WithShipTo(Some.Customer().WithIsDropShip(false))
            .WithSTAddress1("Address1")
            .WithSTAddress2("Address2")
            .WithSTAddress3("Address3")
            .WithSTAddress4("Address4")
            .WithSTCity("Minneapolis")
            .WithSTState("Minnesota")
            .WithSTCountry("United States")
            .WithSTPostalCode("55402")
            .WithSTCompanyName("Insite")
            .WithSTPhone("5555555555")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.That(result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr1, Is.Null.Or.Empty);
        Assert.That(result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr2, Is.Null.Or.Empty);
        Assert.That(result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr3, Is.Null.Or.Empty);
        Assert.That(result.CreateOrderRequest.Orders[0].OrderHeader.ShipToAddr4, Is.Null.Or.Empty);
        Assert.That(result.CreateOrderRequest.Orders[0].OrderHeader.ShipToCity, Is.Null.Or.Empty);
        Assert.That(result.CreateOrderRequest.Orders[0].OrderHeader.ShipToState, Is.Null.Or.Empty);
        Assert.That(
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToCountry,
            Is.Null.Or.Empty
        );
        Assert.That(result.CreateOrderRequest.Orders[0].OrderHeader.ShipToZip, Is.Null.Or.Empty);
        Assert.That(result.CreateOrderRequest.Orders[0].OrderHeader.ShipToName, Is.Null.Or.Empty);
        Assert.That(result.CreateOrderRequest.Orders[0].OrderHeader.ShipToPhone, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Populate_ShipToState_With_State_Abbreviation()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithShipTo(Some.Customer().WithIsDropShip(true))
            .WithSTState("Minnesota")
            .Build();

        var state = Some.State().WithAbbreviation("MN").Build();
        this.WhenGetStateByNameIs(customerOrder.STState, state);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            state.Abbreviation,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToState
        );
    }

    [Test]
    public void Execute_Should_Populate_ShipToCountry_With_Country_Abbreviation()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithShipTo(Some.Customer().WithIsDropShip(true))
            .WithSTCountry("United States")
            .Build();

        var country = Some.Country().WithAbbreviation("MN").Build();
        this.WhenGetCountryByNameIs(customerOrder.STCountry, country);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            country.Abbreviation,
            result.CreateOrderRequest.Orders[0].OrderHeader.ShipToCountry
        );
    }

    protected override CreateOrderParameter GetDefaultParameter()
    {
        return new CreateOrderParameter { CustomerOrder = Some.CustomerOrder() };
    }

    protected override CreateOrderResult GetDefaultResult()
    {
        return new CreateOrderResult
        {
            CreateOrderRequest = new CreateOrderRequest
            {
                Orders = new List<RequestOrder>
                {
                    new RequestOrder { OrderHeader = new RequestOrderHeader() }
                }
            }
        };
    }

    private void WhenGuestCustomerIdIs(Guid id)
    {
        this.customerDefaultSettings.Setup(o => o.GuestErpCustomerId).Returns(id);
    }

    protected void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
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
