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
public class SubmitCreditCardDetailsTests
    : BaseForPipeTests<SubmitOrderParameter, SubmitOrderResult>
{
    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    public override Type PipeType => typeof(SubmitCreditCardDetails);

    public override void SetUp()
    {
        this.pipeAssemblyFactory = this.container.GetMock<IPipeAssemblyFactory>();
    }

    [Test]
    public void Order_Is_800()
    {
        this.pipe.Order.Should().Be(800);
    }

    [Test]
    public void Execute_Should_Return_Error_When_SubmitCreditCardDetails_Returns_Error()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.GetDefaultResult();
        var submitCreditCardDetailsResult = new SubmitCreditCardDetailsResult
        {
            ResultCode = ResultCode.Error,
            SubCode = SubCode.GeneralFailure
        };

        this.WhenExecutePipelineIs(parameter, result, submitCreditCardDetailsResult);

        result = this.RunExecute(parameter, result);

        this.VerifyExecutePipelineWasCalled(parameter, result);
        result.ResultCode.Should().Be(submitCreditCardDetailsResult.ResultCode);
        result.SubCode.Should().Be(submitCreditCardDetailsResult.SubCode);
    }

    [Test]
    public void Execute_Should_Successfully_Call_SubmitCreditCardDetails()
    {
        var parameter = this.GetDefaultParameter();
        var result = this.GetDefaultResult();
        var submitCreditCardDetailsResult = new SubmitCreditCardDetailsResult
        {
            ResultCode = ResultCode.Success,
            SubCode = SubCode.Success
        };

        this.WhenExecutePipelineIs(parameter, result, submitCreditCardDetailsResult);

        result = this.RunExecute(parameter, result);

        this.VerifyExecutePipelineWasCalled(parameter, result);
        result.ResultCode.Should().Be(submitCreditCardDetailsResult.ResultCode);
        result.SubCode.Should().Be(submitCreditCardDetailsResult.SubCode);
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
        SubmitCreditCardDetailsResult submitCreditCardDetailsResult
    )
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.Is<SubmitCreditCardDetailsParameter>(
                            p =>
                                p.ErpOrderNumber == submitOrderResult.ErpOrderNumber
                                && p.CustomerOrder == submitOrderParameter.CustomerOrder
                                && p.IntegrationConnection
                                    == submitOrderParameter.IntegrationConnection
                                && p.JobLogger == submitOrderParameter.JobLogger
                        ),
                        It.IsAny<SubmitCreditCardDetailsResult>()
                    )
            )
            .Returns(submitCreditCardDetailsResult);
    }

    private void VerifyExecutePipelineWasCalled(
        SubmitOrderParameter submitOrderParameter,
        SubmitOrderResult submitOrderResult
    )
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<SubmitCreditCardDetailsParameter>(
                        p =>
                            p.ErpOrderNumber == submitOrderResult.ErpOrderNumber
                            && p.CustomerOrder == submitOrderParameter.CustomerOrder
                            && p.IntegrationConnection == submitOrderParameter.IntegrationConnection
                            && p.JobLogger == submitOrderParameter.JobLogger
                    ),
                    It.IsAny<SubmitCreditCardDetailsResult>()
                ),
            Times.Once
        );
    }
}
