namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.OrderImport;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class AddShipToToRequestTests : BaseForPipeTests<OrderImportParameter, OrderImportResult>
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
    public void Execute_Should_Populate_OrderImportRequest_Request_CustomerShipTo_ShipToID_With_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithErpSequence("123"))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.ShipTo.ErpSequence,
            result.OrderImportRequest.Request.CustomerShipTo.ShipToID
        );
    }

    [Test]
    public void Execute_Should_Populate_OrderImportRequest_Request_CustomerShipTo_ShipToID_With_Guest_Customer_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        var guestCustomer = Some.Customer().WithErpSequence("456").Build();

        this.WhenExists(guestCustomer);
        this.WhenGuestErpCustomerIdIs(guestCustomer.Id);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            guestCustomer.ErpSequence,
            result.OrderImportRequest.Request.CustomerShipTo.ShipToID
        );
    }

    [Test]
    public void Execute_Should__Not_Populate_OrderImportRequest_Request_CustomerShipTo_ShipToID_When_Customer_Order_Customer_And_Guest_Customer_ErpNumbers_Are_Null_Or_Empty()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.That(result.OrderImportRequest.Request.CustomerShipTo.ShipToID, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Populate_ShipToState_With_State_Abbreviation()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer())
            .WithSTState("Minnesota")
            .Build();

        var state = Some.State().WithAbbreviation("MN").Build();
        this.WhenGetStateByNameIs(customerOrder.STState, state);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            state.Abbreviation,
            result.OrderImportRequest.Request.CustomerShipTo.ShipToAddress.ShipToState
        );
    }

    [Test]
    public void Execute_Should_Populate_ShipToCountry_With_Country_Abbreviation()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer())
            .WithSTCountry("United States")
            .Build();

        var country = Some.Country().WithAbbreviation("MN").Build();
        this.WhenGetCountryByNameIs(customerOrder.STCountry, country);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            country.Abbreviation,
            result.OrderImportRequest.Request.CustomerShipTo.ShipToAddress.ShipToCountry
        );
    }

    protected override OrderImportResult GetDefaultResult()
    {
        return new OrderImportResult
        {
            OrderImportRequest = new OrderImport { Request = new Request() }
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

    protected void WhenGetStateByNameIs(string name, State state)
    {
        this.stateRepository.Setup(o => o.GetStateByName(name)).Returns(state);
    }

    protected void WhenGetCountryByNameIs(string name, Country country)
    {
        this.countryRepository.Setup(o => o.GetCountryByName(name)).Returns(country);
    }
}
