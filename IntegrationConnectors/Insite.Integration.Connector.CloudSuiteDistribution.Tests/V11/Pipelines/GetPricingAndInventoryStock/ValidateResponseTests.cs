namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetPricingAndInventoryStock;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class ValidateResponseTests
    : BaseForPipeTests<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
{
    public override Type PipeType => typeof(ValidateResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_500()
    {
        this.pipe.Order.Should().Be(500);
    }

    [Test]
    public void Execute_Should_Return_Error_If_Response_ErrorMessage_Is_Not_Empty()
    {
        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = new OePricingMultipleV4Response
            {
                Response = new Response { ErrorMessage = "Error Message" }
            }
        };

        result = this.RunExecute(result);

        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.GeneralFailure);
        result.Message.Should().Be("Error Message");
    }

    [Test]
    public void Execute_Should_Return_Error_If_Any_Response_PriceOutV2s_ErrorMessages_Are_Not_Empty()
    {
        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = new OePricingMultipleV4Response
            {
                Response = new Response
                {
                    ErrorMessage = string.Empty,
                    PriceOutV2Collection = new PriceOutV2Collection
                    {
                        PriceOutV2s = new List<PriceOutV2>
                        {
                            new PriceOutV2 { ErrorMess = "Error Message" }
                        }
                    }
                }
            }
        };

        result = this.RunExecute(result);

        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.GeneralFailure);
        result.Message.Should().Be("Error Message");
    }
}
