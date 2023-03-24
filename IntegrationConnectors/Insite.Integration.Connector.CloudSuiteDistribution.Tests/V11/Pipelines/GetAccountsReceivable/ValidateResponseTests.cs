namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetAccountsReceivable;

using System;
using FluentAssertions;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.SfCustomerSummary;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetAccountsReceivable;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class ValidateResponseTests
    : BaseForPipeTests<GetAccountsReceivableParameter, GetAccountsReceivableResult>
{
    public override Type PipeType => typeof(ValidateResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_500()
    {
        this.pipe.Order.Should().Be(500);
    }

    [Test]
    public void Execute_Should_Bypass_Pipe_If_ErrorMessage_Is_Empty()
    {
        var result = new GetAccountsReceivableResult
        {
            SfCustomerSummaryResponse = new SfCustomerSummaryResponse
            {
                Response = new Response { ErrorMessage = string.Empty }
            }
        };

        result = this.RunExecute(result);

        result.ResultCode.Should().Be(ResultCode.Success);
        result.SubCode.Should().Be(SubCode.Success);
    }

    [Test]
    public void Execute_Should_Return_Error_If_Response_ErrorMessage_Is_Not_Empty()
    {
        var result = new GetAccountsReceivableResult
        {
            SfCustomerSummaryResponse = new SfCustomerSummaryResponse
            {
                Response = new Response { ErrorMessage = "Error Message" }
            }
        };

        result = this.RunExecute(result);

        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.GeneralFailure);
        result.Message.Should().Be("Error Message");
    }
}
