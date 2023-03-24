namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.CreateOrder;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class AddBillToToRequestTests : BaseForPipeTests<CreateOrderParameter, CreateOrderResult>
{
    private const string DefaultCompanyNumber = "1";

    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    private Mock<CustomerDefaultsSettings> customerDefaultSettings;

    private IList<Customer> customers;

    public override Type PipeType => typeof(AddBillToToRequest);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();
        this.customerDefaultSettings = this.container.GetMock<CustomerDefaultsSettings>();

        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);
    }

    [Test]
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_CompanyNumber_From_IntegrationConnectorSettings_APlusCompany()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber("ABC123"))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenAPlusCompanyIs("2,3");

        var result = this.RunExecute(parameter);
        var expectedCustomerId = this.GetCustomerId("2", customerOrder.Customer.ErpNumber);

        Assert.AreEqual(
            expectedCustomerId,
            result.CreateOrderRequest.Orders[0].OrderHeader.CustomerID
        );
    }

    [Test]
    public void Execute_Should_Get_Default_CompanyNumber_When_IntegrationConnectorSettings_APlusCompany_Is_Not_Populated()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber("ABC123"))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenAPlusCompanyIs(string.Empty);

        var result = this.RunExecute(parameter);
        var expectedCustomerId = this.GetCustomerId(
            DefaultCompanyNumber,
            customerOrder.Customer.ErpNumber
        );

        Assert.AreEqual(
            expectedCustomerId,
            result.CreateOrderRequest.Orders[0].OrderHeader.CustomerID
        );
    }

    [Test]
    public void Execute_Should_Get_Guest_Customer_ErpNumber_When_CustomerOrder_Customer_ErpNumber_Is_Blank()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber(string.Empty))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var guestCustomer = Some.Customer().WithErpNumber("GuestErpNumber").Build();

        this.WhenGuestCustomerIdIs(guestCustomer.Id);
        this.WhenExists(guestCustomer);
        this.WhenAPlusCompanyIs(string.Empty);

        var result = this.RunExecute(parameter);
        var expectedCustomerId = this.GetCustomerId(DefaultCompanyNumber, guestCustomer.ErpNumber);

        Assert.AreEqual(
            expectedCustomerId,
            result.CreateOrderRequest.Orders[0].OrderHeader.CustomerID
        );
    }

    [Test]
    public void Execute_Should_Get_Empty_ErpNumber_When_CustomerOrder_Customer_ErpNumber_Is_Blank_And_No_Guest_Customer_Is_Set()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber(string.Empty))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenAPlusCompanyIs(string.Empty);

        var result = this.RunExecute(parameter);
        var expectedCustomerId = this.GetCustomerId(DefaultCompanyNumber, string.Empty);

        Assert.AreEqual(
            expectedCustomerId,
            result.CreateOrderRequest.Orders[0].OrderHeader.CustomerID
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

    protected string GetCustomerId(string companyNumber, string customerNumber)
    {
        return companyNumber.PadLeft(2, '0') + customerNumber.PadLeft(10, '0');
    }

    private void WhenAPlusCompanyIs(string aPlusCompany)
    {
        this.integrationConnectorSettings.Setup(o => o.APlusCompany).Returns(aPlusCompany);
    }

    private void WhenGuestCustomerIdIs(Guid id)
    {
        this.customerDefaultSettings.Setup(o => o.GuestErpCustomerId).Returns(id);
    }

    protected void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }
}
