namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetContact;

using System;

using Moq;
using NUnit.Framework;

using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetContact;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

[TestFixture]
public class CreateContactTests : BaseForPipeTests<GetContactParameter, GetContactResult>
{
    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    private Mock<IProphet21ApiService> prophet21ApiService;

    public override Type PipeType => typeof(CreateContact);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();

        this.prophet21ApiService = this.container.GetMock<IProphet21ApiService>();

        this.dependencyLocator
            .Setup(o => o.GetInstance<IProphet21ApiService>())
            .Returns(this.prophet21ApiService.Object);
    }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [TestCase(Prophet21OrderSubmitContactTreatment.DoNotSubmitContact)]
    [TestCase(Prophet21OrderSubmitContactTreatment.ContactFromUser)]
    [TestCase(Prophet21OrderSubmitContactTreatment.ContactFromCustomer)]
    public void Execute_Should_Bypass_Pipe_When_Prophet21OrderSubmitContactTreatment_Is_Not_UseApiToLookupAndSubmit(
        Prophet21OrderSubmitContactTreatment orderSubmitContactTreatment
    )
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber("56789"))
            .WithPlacedByUserProfile(
                Some.UserProfile().WithFirstName("M").WithLastName("W").WithEmail("test@test.com")
            )
            .Build();

        var contact = new Contact { Id = "12345" };

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(orderSubmitContactTreatment);
        this.WhenProphet21CompanyIs("01");
        this.WhenCreateContactIs(customerOrder, contact);

        var result = this.GetDefaultResult();
        result.ContactId = string.Empty;
        result = this.RunExecute(parameter, result);

        Assert.That(result.ContactId, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Bypass_Pipe_When_Result_ContactId_Is_Already_Populated()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber("56789"))
            .WithPlacedByUserProfile(
                Some.UserProfile().WithFirstName("M").WithLastName("W").WithEmail("test@test.com")
            )
            .Build();

        var contact = new Contact { Id = "12345" };

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(
            Prophet21OrderSubmitContactTreatment.UseApiToLookupAndSubmit
        );
        this.WhenProphet21CompanyIs("01");
        this.WhenCreateContactIs(customerOrder, contact);

        var result = this.GetDefaultResult();
        result.ContactId = "Existing";
        result = this.RunExecute(parameter, result);

        Assert.AreEqual("Existing", result.ContactId);
    }

    [TestCase(null)]
    [TestCase("")]
    public void Execute_Should_Return_Error_When_CreateCustomer_ContactId_Is_Invalid(
        string contactId
    )
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber("56789"))
            .WithPlacedByUserProfile(
                Some.UserProfile().WithFirstName("M").WithLastName("W").WithEmail("test@test.com")
            )
            .Build();

        var contact = new Contact { Id = contactId };

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(
            Prophet21OrderSubmitContactTreatment.UseApiToLookupAndSubmit
        );
        this.WhenProphet21CompanyIs("01");
        this.WhenCreateContactIs(customerOrder, contact);

        var result = this.GetDefaultResult();
        result.ContactId = string.Empty;
        result = this.RunExecute(parameter, result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
    }

    [Test]
    public void Execute_Should_Get_CreateCustomer_ContactId()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber("56789"))
            .WithPlacedByUserProfile(
                Some.UserProfile().WithFirstName("M").WithLastName("W").WithEmail("test@test.com")
            )
            .Build();

        var contact = new Contact { Id = "12345" };

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(
            Prophet21OrderSubmitContactTreatment.UseApiToLookupAndSubmit
        );
        this.WhenProphet21CompanyIs("01");
        this.WhenCreateContactIs(customerOrder, contact);

        var result = this.GetDefaultResult();
        result.ContactId = string.Empty;
        result = this.RunExecute(parameter, result);

        Assert.AreEqual(contact.Id, result.ContactId);
    }

    protected void WhenProphet21OrderSubmitContactTreatmentIs(
        Prophet21OrderSubmitContactTreatment orderSubmitContactTreatment
    )
    {
        this.integrationConnectorSettings
            .Setup(o => o.Prophet21OrderSubmitContactTreatment)
            .Returns(orderSubmitContactTreatment);
    }

    protected void WhenProphet21CompanyIs(string prophet21Company)
    {
        this.integrationConnectorSettings.Setup(o => o.Prophet21Company).Returns(prophet21Company);
    }

    protected void WhenCreateContactIs(CustomerOrder customerOrder, Contact contact)
    {
        this.prophet21ApiService
            .Setup(
                o =>
                    o.CreateContact(
                        null,
                        It.Is<Contact>(
                            p =>
                                p.FirstName == customerOrder.PlacedByUserProfile.FirstName
                                && p.LastName == customerOrder.PlacedByUserProfile.LastName
                                && p.EmailAddress == customerOrder.PlacedByUserProfile.Email
                                && p.AddressId == customerOrder.Customer.ErpNumber
                        )
                    )
            )
            .Returns(contact);
    }
}
