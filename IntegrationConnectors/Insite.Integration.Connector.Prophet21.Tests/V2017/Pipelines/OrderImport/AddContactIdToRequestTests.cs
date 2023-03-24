namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.OrderImport;

using System;

using Moq;
using NUnit.Framework;

using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class AddContactIdToRequestTests : BaseForPipeTests<OrderImportParameter, OrderImportResult>
{
    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    public override Type PipeType => typeof(AddContactIdToRequest);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();

        this.pipeAssemblyFactory = this.container.GetMock<IPipeAssemblyFactory>();
    }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    public void Execute_Should_Bypass_Pipe_When_Prophet21OrderSubmitContactTreatment_Is_DoNotSubmitContact()
    {
        var customerOrder = Some.CustomerOrder().Build();

        var getContactResult = new GetContactResult { ContactId = "12345" };

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(
            Prophet21OrderSubmitContactTreatment.DoNotSubmitContact
        );
        this.WhenExecutePipelineIs(customerOrder, getContactResult);

        var result = this.RunExecute(parameter);

        this.VerifyExecutePipelineWasNotCalled();
    }

    [Test]
    public void Execute_Should_Return_Error_When_ExecutePipeline_Returns_Error()
    {
        var customerOrder = Some.CustomerOrder().Build();

        var getContactResult = new GetContactResult
        {
            ResultCode = ResultCode.Error,
            SubCode = SubCode.GeneralFailure,
            ContactId = "12345"
        };

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(
            Prophet21OrderSubmitContactTreatment.UseApiToLookupAndSubmit
        );
        this.WhenExecutePipelineIs(customerOrder, getContactResult);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
    }

    [TestCase(Prophet21OrderSubmitContactTreatment.ContactFromUser)]
    [TestCase(Prophet21OrderSubmitContactTreatment.ContactFromCustomer)]
    [TestCase(Prophet21OrderSubmitContactTreatment.UseApiToLookupAndSubmit)]
    public void Execute_Should_Get_ContactId_From_GetContact_Pipeline(
        Prophet21OrderSubmitContactTreatment orderSubmitContactTreatment
    )
    {
        var customerOrder = Some.CustomerOrder().Build();

        var getContactResult = new GetContactResult { ContactId = "12345" };

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenProphet21OrderSubmitContactTreatmentIs(orderSubmitContactTreatment);
        this.WhenExecutePipelineIs(customerOrder, getContactResult);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(getContactResult.ContactId, result.OrderImportRequest.Request.ContactID);
    }

    protected override OrderImportResult GetDefaultResult()
    {
        return new OrderImportResult
        {
            OrderImportRequest = new OrderImport { Request = new Request() }
        };
    }

    private void VerifyExecutePipelineWasNotCalled()
    {
        this.pipeAssemblyFactory.Verify(
            o => o.ExecutePipeline(It.IsAny<GetContactParameter>(), It.IsAny<GetContactResult>()),
            Times.Never
        );
    }

    protected void WhenProphet21OrderSubmitContactTreatmentIs(
        Prophet21OrderSubmitContactTreatment orderSubmitContactTreatment
    )
    {
        this.integrationConnectorSettings
            .Setup(o => o.Prophet21OrderSubmitContactTreatment)
            .Returns(orderSubmitContactTreatment);
    }

    private void WhenExecutePipelineIs(
        CustomerOrder customerOrder,
        GetContactResult getContactResult
    )
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.Is<GetContactParameter>(p => p.CustomerOrder == customerOrder),
                        It.IsAny<GetContactResult>()
                    )
            )
            .Returns(getContactResult);
    }
}
