namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderTotal;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AddShipToToRequestTests : BaseForPipeTests<OrderTotalParameter, OrderTotalResult>
{
    private const string OneTimeAddressPrefix = "OT-";

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
    public void Execute_Should_Populate_ShipTo_Properties_From_CustomerOrder()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer())
            .WithSTAddress1("Address1")
            .WithSTAddress2("Address2")
            .WithSTAddress3("Address3")
            .WithSTAddress4("Address4")
            .WithSTCity("City")
            .WithSTPostalCode("PostalCode")
            .Build();

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.STAddress1,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToAddress1
        );
        Assert.AreEqual(
            customerOrder.STAddress2,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToAddress2
        );
        Assert.AreEqual(
            customerOrder.STAddress3,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToAddress3
        );
        Assert.AreEqual(
            customerOrder.STAddress4,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToAddress4
        );
        Assert.AreEqual(
            customerOrder.STCity,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToCity
        );
        Assert.AreEqual(
            customerOrder.STPostalCode,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToZipCode
        );
    }

    [Test]
    public void Execute_Should_Populate_State_And_Country_With_State_And_Country_Abbreviation_When_State_And_Country_Exist_In_Repository()
    {
        var state = Some.State().WithName("Wisconsin").WithAbbreviation("WI").Build();
        var country = Some.Country().WithName("United States").WithAbbreviation("US").Build();
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer())
            .WithSTState(state.Name)
            .WithSTCountry(country.Name)
            .Build();

        this.WhenGetStateByNameIs(state.Name, state);
        this.WhenGetCountryByNameIs(country.Name, country);

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            state.Abbreviation,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToState
        );
        Assert.AreEqual(
            country.Abbreviation,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToCountry
        );
    }

    [Test]
    public void Execute_Should_Populate_State_And_Country_With_CustomerOrder_STState_And_STCountry_When_State_And_Country_Do_Not_Exist_In_Repository()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer())
            .WithSTState("Wisconsin")
            .WithSTCountry("United States")
            .Build();

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.STState,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToState
        );
        Assert.AreEqual(
            customerOrder.STCountry,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToCountry
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_ShipToNumber_When_ShipTo_Is_OneTimeShipTo()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithErpSequence(OneTimeAddressPrefix + "123"))
            .Build();

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.That(
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToNumber,
            Is.Null.Or.Empty
        );
    }

    [Test]
    public void Execute_Should_Populate_ShipToNumber_With_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithErpSequence("123"))
            .Build();

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.ShipTo.ErpSequence,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToNumber
        );
    }

    [Test]
    public void Execute_Should_Populate_ShipToNumber_With_Guest_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        var guestCustomer = Some.Customer().WithErpSequence("456").Build();

        this.WhenExists(guestCustomer);
        this.WhenGuestErpCustomerIdIs(guestCustomer.Id);

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            guestCustomer.ErpSequence,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToNumber
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_ShipToNumber_When_Customer_Order_Customer_And_Guest_Customer_ErpNumbers_Are_Null_Or_Empty()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.That(
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.ShipToNumber,
            Is.Null.Or.Empty
        );
    }

    [Test]
    public void Execute_Should_Populate_Additional_ShipTo_Properties_When_FulfillmentMethod_Is_Pickup()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithShipTo(Some.Customer())
            .WithSTAddress1("Address1")
            .WithSTAddress2("Address2")
            .WithSTAddress3("Address3")
            .WithSTAddress4("Address4")
            .Build();

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.STAddress1,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.Address1
        );
        Assert.AreEqual(
            customerOrder.STAddress2,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.Address2
        );
        Assert.AreEqual(
            customerOrder.STAddress3,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.Address3
        );
        Assert.AreEqual(
            customerOrder.STAddress4,
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.Address4
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_Additional_ShipTo_Properties_When_FulfillmentMethod_Is_Not_Pickup()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.Ship.ToString())
            .WithShipTo(Some.Customer())
            .WithSTAddress1("Address1")
            .WithSTAddress2("Address2")
            .WithSTAddress3("Address3")
            .WithSTAddress4("Address4")
            .Build();

        var parameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var result = this.RunExecute(parameter);

        Assert.That(
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.Address1,
            Is.Null.Or.Empty
        );
        Assert.That(
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.Address2,
            Is.Null.Or.Empty
        );
        Assert.That(
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.Address3,
            Is.Null.Or.Empty
        );
        Assert.That(
            result.OrderTotalRequest.Request.Orders.First().OrderHeader.Address4,
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

    private void WhenGetStateByNameIs(string name, State state)
    {
        this.stateRepository.Setup(o => o.GetStateByName(name)).Returns(state);
    }

    private void WhenGetCountryByNameIs(string name, Country country)
    {
        this.countryRepository.Setup(o => o.GetCountryByName(name)).Returns(country);
    }
}
