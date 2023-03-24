namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetContact;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetContact;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

[TestFixture]
public class GetContactFromApiTests : BaseForPipeTests<GetContactParameter, GetContactResult>
{
    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    private Mock<IProphet21ApiService> prophet21ApiService;

    public override Type PipeType => typeof(GetContactFromApi);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();

        this.prophet21ApiService = this.container.GetMock<IProphet21ApiService>();

        this.dependencyLocator
            .Setup(o => o.GetInstance<IProphet21ApiService>())
            .Returns(this.prophet21ApiService.Object);
    }

    [Test]
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [TestCase(Prophet21OrderSubmitContactTreatment.DoNotSubmitContact)]
    [TestCase(Prophet21OrderSubmitContactTreatment.ContactFromUser)]
    [TestCase(Prophet21OrderSubmitContactTreatment.ContactFromCustomer)]
    public void Execute_Should_Bypass_Pipe_When_Prophet21OrderSubmitContactTreatment_Is_Not_UseApiToLookupAndSubmit(
        Prophet21OrderSubmitContactTreatment orderSubmitContactTreatment
    )
    {
        const string Email = "test@test.com";
        const string ErpNumber = "567890";

        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber(ErpNumber))
            .WithPlacedByUserProfile(Some.UserProfile().WithEmail(Email))
            .Build();

        var arrayOfContacts = new ArrayOfContact
        {
            Contacts = new List<Contact>
            {
                new Contact
                {
                    ContactLinks = new List<ContactLink>
                    {
                        new ContactLink { ContactLinkId = ErpNumber, ContactId = "12345" }
                    }
                }
            }
        };

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(orderSubmitContactTreatment);
        this.WhenGetContactIs(Email, arrayOfContacts);

        var result = this.RunExecute(parameter);

        Assert.That(result.ContactId, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Get_ContactId_From_Api()
    {
        const string Email = "test@test.com";
        const string ErpNumber = "567890";
        const string ContactId = "12345";

        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber(ErpNumber))
            .WithPlacedByUserProfile(Some.UserProfile().WithEmail(Email))
            .Build();

        var arrayOfContacts = new ArrayOfContact
        {
            Contacts = new List<Contact>
            {
                new Contact
                {
                    ContactLinks = new List<ContactLink>
                    {
                        new ContactLink { ContactLinkId = ErpNumber, ContactId = ContactId }
                    }
                }
            }
        };

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(
            Prophet21OrderSubmitContactTreatment.UseApiToLookupAndSubmit
        );
        this.WhenGetContactIs(Email, arrayOfContacts);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(ContactId, result.ContactId);
    }

    protected void WhenProphet21OrderSubmitContactTreatmentIs(
        Prophet21OrderSubmitContactTreatment orderSubmitContactTreatment
    )
    {
        this.integrationConnectorSettings
            .Setup(o => o.Prophet21OrderSubmitContactTreatment)
            .Returns(orderSubmitContactTreatment);
    }

    protected void WhenGetContactIs(string emailAddress, ArrayOfContact arrayOfContact)
    {
        this.prophet21ApiService
            .Setup(o => o.GetContacts(null, emailAddress))
            .Returns(arrayOfContact);
    }
}
