namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.CreateCustomerOrder;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Core.Enums;
using Insite.Core.Plugins.Integration;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;

[TestFixture]
public class SubmitCreditCardTests
    : BaseForPipeTests<CreateCustomerOrderParameter, CreateCustomerOrderResult>
{
    private const string ErpOrderNumber = "ERP_123";

    private const string CustomerNumber = "Cust_321";

    private Mock<IJobDefinitionRepository> jobDefinitionRepository;

    private Mock<IIntegrationJobSchedulingService> integrationJobSchedulingService;

    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    public override Type PipeType => typeof(SubmitCreditCard);

    public override void SetUp()
    {
        this.jobDefinitionRepository = this.container.GetMock<IJobDefinitionRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<IJobDefinitionRepository>())
            .Returns(this.jobDefinitionRepository.Object);

        this.integrationJobSchedulingService =
            this.container.GetMock<IIntegrationJobSchedulingService>();
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();
    }

    [Test]
    public void Order_Is_900()
    {
        Assert.AreEqual(900, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Bypass_Pipe_When_CustomerOrder_Does_Not_Have_Any_CreditCardTransactions()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = Some.CustomerOrder();

        Assert.DoesNotThrow(() => this.RunExecute(parameter));
    }

    [Test]
    public void Execute_Should_Return_Error_When_CreditCardSubmit_Job_Is_Not_Found()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = Some.CustomerOrder().With(Some.CreditCardTransaction());

        this.WhenGetByStandardNameIs(
            JobDefinitionStandardJobName.CreditCardSubmit.ToString(),
            null
        );

        var result = this.RunExecute(parameter);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
    }

    [Test]
    public void Execute_Should_Schedule_Integration_Job()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = Some.CustomerOrder().With(Some.CreditCardTransaction());

        this.WhenGetByStandardNameIs(
            JobDefinitionStandardJobName.CreditCardSubmit.ToString(),
            this.creditCardSubmitJobDefinition
        );
        this.WhenIfsCompanyIs("1");

        this.RunExecute(parameter);

        this.VerifyScheduleBatchIntegrationJobWasCalled(
            this.creditCardSubmitJobDefinition.Name,
            this.creditCardSubmitJobDefinition.JobDefinitionSteps
                .First()
                .JobDefinitionStepParameters
        );
    }

    [Test]
    public void GetJobDefinitionStepParameterValues_Should_Return_Dictionary_Of_Parameter_Names_And_Values()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = Some.CustomerOrder()
            .With(Some.Currency().WithCurrencyCode("USD"))
            .With(
                Some.CreditCardTransaction()
                    .WithName("Joe Snuffy")
                    .WithExpirationDate("12/2018")
                    .WithCardType("Visa")
                    .WithCreditCardNumber("xxxxxxxxxxxx1234")
                    .WithPNRef("12345")
                    .WithAuthCode("123")
            );

        var result = this.GetDefaultResult();

        this.WhenIfsCompanyIs("1");

        var jobDefinitionStepParameterValues =
            (Dictionary<string, string>)
                this.RunPrivateMethod("GetJobDefinitionStepParameterValues", parameter, result);

        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(
                o => o.Key == "ORDER_NO" && o.Value == result.ErpOrderNumber
            )
        );
        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(
                o => o.Key == "CUSTOMER_NO" && o.Value == result.CustomerOrder.customerNo
            )
        );
        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(o => o.Key == "COMPANY" && o.Value == "1")
        );
        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(
                o => o.Key == "CURRENCY" && o.Value == parameter.CustomerOrder.Currency.CurrencyCode
            )
        );
        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(
                o => o.Key == "CARDHOLDER_NAME" && o.Value == "Joe Snuffy"
            )
        );
        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(
                o => o.Key == "CREDIT_EXP_MONTH" && o.Value == "12"
            )
        );
        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(
                o => o.Key == "CREDIT_EXP_YEAR" && o.Value == "2018"
            )
        );
        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(o => o.Key == "CARD_TYPE" && o.Value == "Visa")
        );
        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(
                o =>
                    o.Key == "CREDIT_CARD_NO"
                    && o.Value == "BA27D7692B4EFDC3021158F64754305EAFCD7268A4CC43F8"
            )
        );
        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(
                o => o.Key == "DISPLAY_CARD_NUMBER" && o.Value == "xxxxxxxxxxxx1234"
            )
        );
        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(o => o.Key == "PN_REF" && o.Value == "12345")
        );
        Assert.IsTrue(
            jobDefinitionStepParameterValues.Any(o => o.Key == "AUTH_CODE" && o.Value == "123")
        );
    }

    [TestCase("", "")]
    [TestCase("1", "")]
    [TestCase("12", "12")]
    [TestCase("12/", "12")]
    [TestCase("12/18", "12")]
    [TestCase("12/2018", "12")]
    public void GetCreditCardTransactionExpirationMonth_Should_Return_Expiration_Month(
        string expirationDate,
        string expirationMonth
    )
    {
        var creditCardTransaction = Some.CreditCardTransaction()
            .WithExpirationDate(expirationDate)
            .Build();
        var expirationMonthResult = (string)
            this.RunPrivateMethod("GetCreditCardTransactionExpirationMonth", creditCardTransaction);

        Assert.AreEqual(expirationMonth, expirationMonthResult);
    }

    [TestCase("", "")]
    [TestCase("12", "")]
    [TestCase("12/", "")]
    [TestCase("12/18", "18")]
    [TestCase("12/2018", "2018")]
    public void GetCreditCardTransactionExpirationYear_Should_Return_Expiration_Year(
        string expirationDate,
        string expirationYear
    )
    {
        var creditCardTransaction = Some.CreditCardTransaction()
            .WithExpirationDate(expirationDate)
            .Build();
        var expirationYearResult = (string)
            this.RunPrivateMethod("GetCreditCardTransactionExpirationYear", creditCardTransaction);

        Assert.AreEqual(expirationYear, expirationYearResult);
    }

    [TestCase("AmericanExpress", "AMEX")]
    [TestCase("American Express", "AMEX")]
    [TestCase("Visa", "Visa")]
    [TestCase("Mastercard", "Mastercard")]
    [TestCase("Discover", "Discover")]
    public void GetCreditCardType_Should_Return_Card_Type(string cardTypeIn, string cardTypeOut)
    {
        var result = (string)this.RunPrivateMethod("GetCreditCardType", cardTypeIn);

        Assert.AreEqual(cardTypeOut, result);
    }

    [Test]
    public void GetJobDefinitionStepParameters_Should_Return_JobDefinitionStepParameters()
    {
        var jobDefinitionStepParameterValues = new Dictionary<string, string>
        {
            { "ORDER_NO", ErpOrderNumber },
            { "NOT_VALID", "Invalid" },
            { "COMPANY", "1" }
        };

        var result =
            (Collection<JobDefinitionStepParameter>)
                this.RunPrivateMethod(
                    "GetJobDefinitionStepParameters",
                    this.creditCardSubmitJobDefinition,
                    jobDefinitionStepParameterValues
                );

        Assert.IsTrue(result.Count == 2);
        Assert.IsTrue(
            result.Any(
                o =>
                    o.Id
                        == this.creditCardSubmitJobDefinition.JobDefinitionSteps
                            .First()
                            .JobDefinitionStepParameters.First(p => p.Name == "ORDER_NO")
                            .Id
                    && o.Value == ErpOrderNumber
            )
        );
        Assert.IsTrue(
            result.Any(
                o =>
                    o.Id
                        == this.creditCardSubmitJobDefinition.JobDefinitionSteps
                            .First()
                            .JobDefinitionStepParameters.First(p => p.Name == "COMPANY")
                            .Id
                    && o.Value == "1"
            )
        );
    }

    protected override CreateCustomerOrderResult GetDefaultResult()
    {
        return new CreateCustomerOrderResult
        {
            CustomerOrder = new customerOrder { customerNo = CustomerNumber },
            ErpOrderNumber = ErpOrderNumber
        };
    }

    protected JobDefinition creditCardSubmitJobDefinition = Some.JobDefinition()
        .WithName("Credit Card Submit")
        .With(
            Some.JobDefinitionStep()
                .With(Some.JobDefinitionStepParameter().WithName("ORDER_NO"))
                .With(Some.JobDefinitionStepParameter().WithName("CUSTOMER_NO"))
                .With(Some.JobDefinitionStepParameter().WithName("COMPANY"))
                .With(Some.JobDefinitionStepParameter().WithName("CURRENCY"))
                .With(Some.JobDefinitionStepParameter().WithName("CARDHOLDER_NAME"))
                .With(Some.JobDefinitionStepParameter().WithName("CREDIT_EXP_MONTH"))
                .With(Some.JobDefinitionStepParameter().WithName("CREDIT_EXP_YEAR"))
                .With(Some.JobDefinitionStepParameter().WithName("CARD_TYPE"))
                .With(Some.JobDefinitionStepParameter().WithName("CREDIT_CARD_NO"))
                .With(Some.JobDefinitionStepParameter().WithName("DISPLAY_CARD_NUMBER"))
                .With(Some.JobDefinitionStepParameter().WithName("PN_REF"))
                .With(Some.JobDefinitionStepParameter().WithName("AUTH_CODE"))
        );

    private void WhenGetByStandardNameIs(
        string jobDefinitionStandardJobName,
        JobDefinition jobDefinition
    )
    {
        this.jobDefinitionRepository
            .Setup(o => o.GetByStandardName(jobDefinitionStandardJobName))
            .Returns(jobDefinition);
    }

    private void WhenIfsCompanyIs(string ifsCompany)
    {
        this.integrationConnectorSettings.Setup(o => o.IfsCompany).Returns(ifsCompany);
    }

    private void VerifyScheduleBatchIntegrationJobWasCalled(
        string jobDefinitionName,
        ICollection<JobDefinitionStepParameter> jobDefinitionStepParameters
    )
    {
        this.integrationJobSchedulingService.Verify(
            o =>
                o.ScheduleBatchIntegrationJob(
                    jobDefinitionName,
                    null,
                    It.Is<Collection<JobDefinitionStepParameter>>(
                        p => p.Count == jobDefinitionStepParameters.Count
                    ),
                    null,
                    null,
                    false
                ),
            Times.Once
        );
    }
}
