namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.AuthorizationCode;

using System;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitAuthorizationCode;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class SerializeRequestsTests
    : BaseForPipeTests<SubmitAuthorizationCodeParameter, SubmitAuthorizationCodeResult>
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
        var result = new SubmitAuthorizationCodeResult
        {
            AuthorizationCodeRequest = new AuthorizationCode
            {
                AuthCode = "1234567885434321323",
                OrderNo = "21321321321"
            }
        };

        result = this.RunExecute(result);

        result.SerializedAuthorizationCodeRequest
            .Should()
            .ContainEquivalentOf(
                IfsAurenaSerializationService.Serialize(result.AuthorizationCodeRequest)
            );
    }
}
