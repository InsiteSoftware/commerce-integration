namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetContact;

using System;

using Moq;
using NUnit.Framework;

using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetContact;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class GetContactFromUserTests : BaseForPipeTests<GetContactParameter, GetContactResult>
{
    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    public override Type PipeType => typeof(GetContactFromUser);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [TestCase(Prophet21OrderSubmitContactTreatment.DoNotSubmitContact)]
    [TestCase(Prophet21OrderSubmitContactTreatment.ContactFromCustomer)]
    [TestCase(Prophet21OrderSubmitContactTreatment.UseApiToLookupAndSubmit)]
    public void Execute_Should_Bypass_Pipe_When_Prophet21OrderSubmitContactTreatment_Is_Not_ContactFromUser(
        Prophet21OrderSubmitContactTreatment orderSubmitContactTreatment
    )
    {
        var customerOrder = Some.CustomerOrder()
            .WithPlacedByUserProfile(
                Some.UserProfile()
                    .With(Some.CustomProperty().WithName("ContactId").WithValue("12345"))
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(orderSubmitContactTreatment);

        var result = this.RunExecute(parameter);

        Assert.That(result.ContactId, Is.Null.Or.Empty);
    }

    [TestCase(null)]
    [TestCase("")]
    public void Execute_Should_Return_Error_When_UserProfile_ContactId_Is_Invalid(string contactId)
    {
        var customerOrder = Some.CustomerOrder()
            .WithPlacedByUserProfile(
                Some.UserProfile()
                    .With(Some.CustomProperty().WithName("ContactId").WithValue(contactId))
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(
            Prophet21OrderSubmitContactTreatment.ContactFromUser
        );

        var result = this.RunExecute(parameter);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
    }

    [Test]
    public void Execute_Should_Get_UserProfile_ContactId()
    {
        const string ContactId = "12345";
        var customerOrder = Some.CustomerOrder()
            .WithPlacedByUserProfile(
                Some.UserProfile()
                    .With(Some.CustomProperty().WithName("ContactId").WithValue(ContactId))
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(
            Prophet21OrderSubmitContactTreatment.ContactFromUser
        );

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
}
