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
public class DeserializeResponseTests : BaseForPipeTests<SubmitOrderParameter, SubmitOrderResult>
{
    public override Type PipeType => typeof(DeserializeResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_400()
    {
        this.pipe.Order.Should().Be(400);
    }

    [Test]
    public void Execute_Should_Deserialize_Response()
    {
        var customerOrder = new CustomerOrder { OrderNo = "Erp123" };

        var result = new SubmitOrderResult
        {
            SerializedCustomerOrderResponse = IfsAurenaSerializationService.Serialize(customerOrder)
        };

        result = this.RunExecute(result);

        result.CustomerOrderResponse.Should().BeEquivalentTo(customerOrder);
    }
}
