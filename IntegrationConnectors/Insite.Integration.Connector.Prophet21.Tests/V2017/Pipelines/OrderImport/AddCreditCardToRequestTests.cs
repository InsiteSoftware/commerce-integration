namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.OrderImport;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Insite.Core.SystemSetting.Groups.Integration;
using Moq;
using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Core.Interfaces.EnumTypes;

[TestFixture]
public class AddCreditCardToRequestTests : BaseForPipeTests<OrderImportParameter, OrderImportResult>
{
    private Mock<IStateRepository> stateRepository;

    private Mock<ICountryRepository> countryRepository;

    private Mock<ISystemListRepository> systemListRepository;

    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    public override Type PipeType => typeof(AddCreditCardToRequest);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();

        this.stateRepository = this.container.GetMock<IStateRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<IStateRepository>())
            .Returns(this.stateRepository.Object);

        this.countryRepository = this.container.GetMock<ICountryRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<ICountryRepository>())
            .Returns(this.countryRepository.Object);

        this.systemListRepository = this.container.GetMock<ISystemListRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<ISystemListRepository>())
            .Returns(this.systemListRepository.Object);
    }

    [Test]
    public void Order_Is_700()
    {
        Assert.AreEqual(700, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Bypass_Pipe_When_CustomerOrder_Has_No_CreditCardTransactions()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = Some.CustomerOrder();

        var result = this.RunExecute(parameter);

        this.VerifyGetStateByNameWasNotCalled();
    }

    [TestCase("Visa", "P21-V", true)]
    [TestCase("Mastercard", "P21-MC", false)]
    [TestCase("American Express", "P21-AMEX", true)]
    public void Execute_Should_Populate_RequestCreditCard_Element_Properties(
        string cardType,
        string erpCardType,
        bool sendAuthCode
    )
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.CreditCardTransaction()
                    .WithCardType(cardType)
                    .WithAuthCode(Guid.NewGuid().ToString())
                    .WithCreditCardNumber("XXXXXXXXXXXX1111")
                    .WithExpirationDate("01/2023")
                    .WithToken2("Token 2")
                    .WithPNRef("PN Ref")
                    .WithAmount(12)
                    .WithResult("0")
            )
            .Build();

        this.WhenSystemListHas(SystemListValueTypes.CreditCardTypeMapping, cardType, erpCardType);
        this.WhenSendAuthCodeInOrderSubmitIs(sendAuthCode);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(erpCardType, result.OrderImportRequest.Request.CreditCard.CardType);
        Assert.AreEqual(
            sendAuthCode ? customerOrder.CreditCardTransactions.First().AuthCode : null,
            result.OrderImportRequest.Request.CreditCard.AuthorizationCode
        );
        Assert.AreEqual(
            MaskCardNumber(customerOrder.CreditCardTransactions.First().CreditCardNumber),
            result.OrderImportRequest.Request.CreditCard.CardNumber
        );
        Assert.AreEqual(
            GetParsedDatePart(customerOrder.CreditCardTransactions.First().ExpirationDate, "MM"),
            result.OrderImportRequest.Request.CreditCard.ExpirationMonth
        );
        Assert.AreEqual(
            GetParsedDatePart(customerOrder.CreditCardTransactions.First().ExpirationDate, "yy"),
            result.OrderImportRequest.Request.CreditCard.ExpirationYear
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().Token2,
            result.OrderImportRequest.Request.CreditCard.ElementPaymentAccountID
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().PNRef,
            result.OrderImportRequest.Request.CreditCard.ElementTransactionID
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().Amount,
            result.OrderImportRequest.Request.CreditCard.ChargeAmount
        );
    }

    [TestCase("", "", "")]
    [TestCase("Joe", "Joe", "")]
    [TestCase("Joe Schmo", "Joe", "Schmo")]
    public void Execute_Should_Populate_RequestCardHolder_FirstName_And_LastName(
        string name,
        string firstName,
        string lastName
    )
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.CreditCardTransaction().WithName(name).WithResult("0"))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            firstName,
            result.OrderImportRequest.Request.CreditCard.CardHolder.FirstName
        );
        Assert.AreEqual(lastName, result.OrderImportRequest.Request.CreditCard.CardHolder.LastName);
    }

    [Test]
    public void Execute_Should_Populate_RequestAddress_With_CreditCard_Billing_Address()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.CreditCardTransaction()
                    .WithAvsAddr("110 5th St N|Suite 800|Minneapolis|Minnesota|United States")
                    .WithAvsZip("55402")
                    .WithResult("0")
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "110 5th St N",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Address1
        );
        Assert.AreEqual(
            "Suite 800",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Address2
        );
        Assert.AreEqual(
            "Minneapolis",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.City
        );
        Assert.AreEqual(
            "Minnesota",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.State
        );
        Assert.AreEqual(
            "United States",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Country
        );
        Assert.AreEqual(
            "55402",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Zip
        );
    }

    [Test]
    public void Execute_Should_Populate_RequestAddress_With_CustomerOrder_Billing_Address()
    {
        var customerOrder = Some.CustomerOrder()
            .WithBTAddress1("110 5th St N")
            .WithBTAddress2("Suite 800")
            .WithBTCity("Minneapolis")
            .WithBTState("Minnesota")
            .WithBTCountry("United States")
            .WithBTPostalCode("55402")
            .With(Some.CreditCardTransaction().WithAvsAddr("Not pipe delimited").WithResult("0"))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "110 5th St N",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Address1
        );
        Assert.AreEqual(
            "Suite 800",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Address2
        );
        Assert.AreEqual(
            "Minneapolis",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.City
        );
        Assert.AreEqual(
            "Minnesota",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.State
        );
        Assert.AreEqual(
            "United States",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Country
        );
        Assert.AreEqual(
            "55402",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Zip
        );
    }

    [Test]
    public void Execute_Should_Populate_RequestAddress_State_And_Country_With_Repository_Objects_Abbreviation()
    {
        var state = Some.State().WithName("Minnesota").WithAbbreviation("MN").Build();

        var country = Some.Country().WithName("United States").WithAbbreviation("US").Build();

        var customerOrder = Some.CustomerOrder()
            .WithBTState(state.Name)
            .WithBTCountry(country.Name)
            .With(Some.CreditCardTransaction().WithAvsAddr("Not pipe delimited").WithResult("0"))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenGetStateByNameIs(state.Name, state);
        this.WhenGetCountryByNameIs(country.Name, country);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            state.Abbreviation,
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.State
        );
        Assert.AreEqual(
            country.Abbreviation,
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Country
        );
    }

    [Test]
    public void Execute_Should_Populate_RequestCreditCard_From_Success_Transaction()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.CreditCardTransaction()
                    .WithAvsAddr("220 5th St N|Suite 900|New York|New York|United States")
                    .WithAvsZip("55407")
                    .WithResult("1")
            )
            .With(
                Some.CreditCardTransaction()
                    .WithAvsAddr("110 5th St N|Suite 800|Minneapolis|Minnesota|United States")
                    .WithAvsZip("55402")
                    .WithResult("0")
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "110 5th St N",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Address1
        );
        Assert.AreEqual(
            "Suite 800",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Address2
        );
        Assert.AreEqual(
            "Minneapolis",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.City
        );
        Assert.AreEqual(
            "Minnesota",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.State
        );
        Assert.AreEqual(
            "United States",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Country
        );
        Assert.AreEqual(
            "55402",
            result.OrderImportRequest.Request.CreditCard.CardHolder.Address.Zip
        );
    }

    protected void VerifyGetStateByNameWasNotCalled()
    {
        this.stateRepository.Verify(o => o.GetStateByName(It.IsAny<string>()), Times.Never);
    }

    protected override OrderImportResult GetDefaultResult()
    {
        return new OrderImportResult
        {
            OrderImportRequest = new OrderImport { Request = new Request() }
        };
    }

    protected void WhenGetStateByNameIs(string name, State state)
    {
        this.stateRepository.Setup(o => o.GetStateByName(name)).Returns(state);
    }

    protected void WhenGetCountryByNameIs(string name, Country country)
    {
        this.countryRepository.Setup(o => o.GetCountryByName(name)).Returns(country);
    }

    protected static string MaskCardNumber(string creditCardNumber)
    {
        return creditCardNumber
            .Substring(creditCardNumber.Length - 4)
            .PadLeft(creditCardNumber.Length, '*');
    }

    protected static string GetParsedDatePart(string expirationDate, string dateFormat)
    {
        var isExpirationDateParsed = DateTime.TryParseExact(
            expirationDate,
            "MM/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var expirationDateTime
        );
        return isExpirationDateParsed ? expirationDateTime.ToString(dateFormat) : string.Empty;
    }

    protected void WhenSystemListHas(
        string creditCardTypeMapping,
        string cardType,
        string erpCardType
    )
    {
        this.systemListRepository
            .Setup(o => o.GetTableAsNoTracking())
            .Returns(
                new List<SystemList>()
                {
                    Some.SystemList()
                        .WithName(creditCardTypeMapping)
                        .With(
                            Some.SystemListValue().WithName(erpCardType).WithDescription(cardType)
                        )
                        .Build()
                }.AsQueryable()
            );
    }

    protected void WhenSendAuthCodeInOrderSubmitIs(bool sendAuthCodeInOrderSubmit)
    {
        this.integrationConnectorSettings
            .Setup(o => o.Prophet21SendAuthCodeInOrderSubmit)
            .Returns(sendAuthCodeInOrderSubmit);
    }
}
