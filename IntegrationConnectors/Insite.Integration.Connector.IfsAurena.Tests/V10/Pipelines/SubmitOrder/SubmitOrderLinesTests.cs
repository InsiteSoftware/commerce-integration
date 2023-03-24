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
public class SubmitOrderLinesTests : BaseForPipeTests<SubmitOrderParameter, SubmitOrderResult>
{
    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    public override Type PipeType => typeof(SubmitOrderLines);

    public override void SetUp()
    {
        this.pipeAssemblyFactory = this.container.GetMock<IPipeAssemblyFactory>();
    }

    [Test]
    public void Order_Is_600()
    {
        this.pipe.Order.Should().Be(600);
    }

    [Test]
    public void Execute_Should_Return_Error_When_SubmitOrderLines_Returns_Error()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.GetDefaultResult();
        var submitOrderLinesResult = new SubmitOrderLinesResult
        {
            ResultCode = ResultCode.Error,
            SubCode = SubCode.GeneralFailure
        };

        this.WhenExecutePipelineIs(parameter, result, submitOrderLinesResult);

        result = this.RunExecute(parameter, result);

        this.VerifyExecutePipelineWasCalled(parameter, result);
        result.ResultCode.Should().Be(submitOrderLinesResult.ResultCode);
        result.SubCode.Should().Be(submitOrderLinesResult.SubCode);
    }

    [Test]
    public void Execute_Should_Successfully_Call_SubmitOrderLines()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.GetDefaultResult();
        var submitOrderLinesResult = new SubmitOrderLinesResult
        {
            ResultCode = ResultCode.Success,
            SubCode = SubCode.Success
        };

        this.WhenExecutePipelineIs(parameter, result, submitOrderLinesResult);

        result = this.RunExecute(parameter, result);

        this.VerifyExecutePipelineWasCalled(parameter, result);
        result.ResultCode.Should().Be(submitOrderLinesResult.ResultCode);
        result.SubCode.Should().Be(submitOrderLinesResult.SubCode);
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
        SubmitOrderLinesResult submitOrderLinesResult
    )
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.Is<SubmitOrderLinesParameter>(
                            p =>
                                p.ErpOrderNumber == submitOrderResult.ErpOrderNumber
                                && p.CustomerOrder == submitOrderParameter.CustomerOrder
                                && p.IntegrationConnection
                                    == submitOrderParameter.IntegrationConnection
                                && p.JobLogger == submitOrderParameter.JobLogger
                        ),
                        It.IsAny<SubmitOrderLinesResult>()
                    )
            )
            .Returns(submitOrderLinesResult);
    }

    private void VerifyExecutePipelineWasCalled(
        SubmitOrderParameter submitOrderParameter,
        SubmitOrderResult submitOrderResult
    )
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<SubmitOrderLinesParameter>(
                        p =>
                            p.ErpOrderNumber == submitOrderResult.ErpOrderNumber
                            && p.CustomerOrder == submitOrderParameter.CustomerOrder
                            && p.IntegrationConnection == submitOrderParameter.IntegrationConnection
                            && p.JobLogger == submitOrderParameter.JobLogger
                    ),
                    It.IsAny<SubmitOrderLinesResult>()
                ),
            Times.Once
        );
    }
}
