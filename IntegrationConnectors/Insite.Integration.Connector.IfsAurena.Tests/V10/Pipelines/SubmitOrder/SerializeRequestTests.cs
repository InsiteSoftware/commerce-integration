namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitOrder;

using System;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class SerializeRequestTests : BaseForPipeTests<SubmitOrderParameter, SubmitOrderResult>
{
    public override Type PipeType => typeof(SerializeRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_200()
    {
        this.pipe.Order.Should().Be(200);
    }

    [Test]
    public void Execute_Should_Serialize_Request()
    {
        var result = new SubmitOrderResult
        {
            CustomerOrderRequest = new CustomerOrder { OrderNo = "Erp123" }
        };

        result = this.RunExecute(result);

        result.SerializedCustomerOrderRequest
            .Should()
            .BeEquivalentTo(IfsAurenaSerializationService.Serialize(result.CustomerOrderRequest));
    }
}
