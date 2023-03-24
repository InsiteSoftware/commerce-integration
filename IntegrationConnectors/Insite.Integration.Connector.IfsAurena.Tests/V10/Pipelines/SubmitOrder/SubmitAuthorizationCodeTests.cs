namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitOrder;

using System;
using FluentAssertions;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class SubmitAuthorizationCodeTests
    : BaseForPipeTests<SubmitOrderParameter, SubmitOrderResult>
{
    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    public override Type PipeType => typeof(SubmitAuthorizationCode);

    public override void SetUp()
    {
        this.pipeAssemblyFactory = this.container.GetMock<IPipeAssemblyFactory>();
    }

    [Test]
    public void Order_Is_900()
    {
        this.pipe.Order.Should().Be(900);
    }

    [Test]
    public void Execute_Should_Return_Error_When_SubmitAuthorizationCode_Returns_Error()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.GetDefaultResult();
        var submitAuthorizationCodeResult = new SubmitAuthorizationCodeResult
        {
            ResultCode = ResultCode.Error,
            SubCode = SubCode.GeneralFailure
        };

        this.WhenExecutePipelineIs(parameter, result, submitAuthorizationCodeResult);

        result = this.RunExecute(parameter, result);

        this.VerifyExecutePipelineWasCalled(parameter, result);
        result.ResultCode.Should().Be(submitAuthorizationCodeResult.ResultCode);
        result.SubCode.Should().Be(submitAuthorizationCodeResult.SubCode);
    }

    [Test]
    public void Execute_Should_Successfully_Call_SubmitAuthorizationCode()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.GetDefaultResult();
        var submitAuthorizationCodeResult = new SubmitAuthorizationCodeResult
        {
            ResultCode = ResultCode.Success,
            SubCode = SubCode.Success
        };

        this.WhenExecutePipelineIs(parameter, result, submitAuthorizationCodeResult);

        result = this.RunExecute(parameter, result);

        this.VerifyExecutePipelineWasCalled(parameter, result);
        result.ResultCode.Should().Be(submitAuthorizationCodeResult.ResultCode);
        result.SubCode.Should().Be(submitAuthorizationCodeResult.SubCode);
    }

    protected override SubmitOrderParameter GetDefaultParameter()
    {
        return new SubmitOrderParameter
        {
            CustomerOrder = Some.CustomerOrder().Build(),
            IntegrationConnection = Some.IntegrationConnection().Build()
        };
    }

    protected override SubmitOrderResult GetDefaultResult()
    {
        return new SubmitOrderResult { ErpOrderNumber = "Erp123" };
    }

    private void WhenExecutePipelineIs(
        SubmitOrderParameter submitOrderParameter,
        SubmitOrderResult submitOrderResult,
        SubmitAuthorizationCodeResult submitAuthorizationCodeResult
    )
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.Is<SubmitAuthorizationCodeParameter>(
                            p =>
                                p.ErpOrderNumber == submitOrderResult.ErpOrderNumber
                                && p.CustomerOrder == submitOrderParameter.CustomerOrder
                                && p.IntegrationConnection
                                    == submitOrderParameter.IntegrationConnection
                                && p.JobLogger == submitOrderParameter.JobLogger
                        ),
                        It.IsAny<SubmitAuthorizationCodeResult>()
                    )
            )
            .Returns(submitAuthorizationCodeResult);
    }

    private void VerifyExecutePipelineWasCalled(
        SubmitOrderParameter submitOrderParameter,
        SubmitOrderResult submitOrderResult
    )
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<SubmitAuthorizationCodeParameter>(
                        p =>
                            p.ErpOrderNumber == submitOrderResult.ErpOrderNumber
                            && p.CustomerOrder == submitOrderParameter.CustomerOrder
                            && p.IntegrationConnection == submitOrderParameter.IntegrationConnection
                            && p.JobLogger == submitOrderParameter.JobLogger
                    ),
                    It.IsAny<SubmitAuthorizationCodeResult>()
                ),
            Times.Once
        );
    }
}
