namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitCreditCardDetails;

using System;
using System.Linq;
using FluentAssertions;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitCreditCardDetails;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;
using CustomerOrder = Insite.Data.Entities.CustomerOrder;

[TestFixture]
public class CreateRequestsTests
    : BaseForPipeTests<SubmitCreditCardDetailsParameter, SubmitCreditCardDetailsResult>
{
    private Mock<ICustomerHelper> customerHelper;

    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    public override Type PipeType => typeof(CreateRequests);

    public override void SetUp()
    {
        this.customerHelper = this.container.GetMock<ICustomerHelper>();
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();
    }

    [Test]
    public void Order_Is_100()
    {
        this.pipe.Order.Should().Be(100);
    }

    [Test]
    public void Execute_Should_Add_All_Properties_To_Requests()
    {
        var parameter = this.GetDefaultParameter();
        var customer = Some.Customer().WithFirstName("fname").WithLastName("lname").Build();
        this.WhenGetBillToIs(parameter.CustomerOrder, customer);
        this.WhenIfsAurenaCompanyIs("ifs company");

        var result = this.RunExecute(parameter);

        result.CreditCardDetailsRequest
            .Should()
            .BeEquivalentTo(
                new CreditCardDetails
                {
                    Currency = "USD",
                    CustomerNo = "123456",
                    CardType = "Visa",
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Company = "ifs company",
                    CreditExpMonth = "Value09",
                    CreditExpYear = 2025,
                    DisplayCardNumber = "token",
                    OrderNo = parameter.ErpOrderNumber,
                    SingleOccurrence = false
                }
            );
        result.CreditCardDetailsRequest.OrderNo.Should().Be(parameter.ErpOrderNumber);
    }

    [Test]
    public void Execute_Should_Return_ExitPipeline_True_When_CustomerOrder_Doesnt_Have_CreditCardTransactions()
    {
        var parameter = new SubmitCreditCardDetailsParameter
        {
            CustomerOrder = Some.CustomerOrder().Build()
        };

        var result = this.RunExecute(parameter);

        result.ExitPipeline.Should().BeTrue();
    }

    protected override SubmitCreditCardDetailsParameter GetDefaultParameter()
    {
        var transaction = Some.CreditCardTransaction()
            .WithCardType("Visa")
            .WithExpirationDate("09/2025")
            .WithAuthCode("auth")
            .WithToken1("token");

        var customerOrder = Some.CustomerOrder()
            .With(transaction)
            .With(Some.Currency().WithCurrencyCode("USD"))
            .WithCustomerNumber("123456")
            .Build();

        return new SubmitCreditCardDetailsParameter
        {
            ErpOrderNumber = "ERP123",
            CustomerOrder = customerOrder
        };
    }

    private void WhenGetBillToIs(CustomerOrder customerOrder, Customer customer)
    {
        this.customerHelper
            .Setup(o => o.GetBillTo(this.fakeUnitOfWork, customerOrder))
            .Returns(customer);
    }

    private void WhenIfsAurenaCompanyIs(string ifsAurenaCompany)
    {
        this.integrationConnectorSettings.Setup(o => o.IfsAurenaCompany).Returns(ifsAurenaCompany);
    }
}
