namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitCreditCardDetails;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitCreditCardDetails;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class SerializeRequestsTests
    : BaseForPipeTests<SubmitCreditCardDetailsParameter, SubmitCreditCardDetailsResult>
{
    public override Type PipeType => typeof(SerializeRequests);

    public override void SetUp() { }

    [Test]
    public void Order_Is_200()
    {
        this.pipe.Order.Should().Be(200);
    }

    [Test]
    public void Execute_Should_Serialize_Requests()
    {
        var result = new SubmitCreditCardDetailsResult()
        {
            CreditCardDetailsRequest = new CreditCardDetails
            {
                DisplayCardNumber = "1234567885434321323",
                OrderNo = "21321321321"
            }
        };

        result = this.RunExecute(result);

        result.SerializedCreditCardDetailsRequest
            .Should()
            .ContainEquivalentOf(
                IfsAurenaSerializationService.Serialize(result.CreditCardDetailsRequest)
            );
    }
}
