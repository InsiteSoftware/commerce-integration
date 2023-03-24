namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitOrderLines;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderLines;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class SerializeRequestsTests
    : BaseForPipeTests<SubmitOrderLinesParameter, SubmitOrderLinesResult>
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
        var result = new SubmitOrderLinesResult
        {
            CustomerOrderLineRequests = new List<CustomerOrderLine>
            {
                new CustomerOrderLine { CatalogNo = "Prod1" },
                new CustomerOrderLine { CatalogNo = "Prod2" }
            }
        };

        result = this.RunExecute(result);

        result.SerializedCustomerOrderLineRequests.Should().NotBeEmpty();
        result.SerializedCustomerOrderLineRequests
            .Should()
            .ContainEquivalentOf(
                IfsAurenaSerializationService.Serialize(result.CustomerOrderLineRequests[0])
            );
        result.SerializedCustomerOrderLineRequests
            .Should()
            .ContainEquivalentOf(
                IfsAurenaSerializationService.Serialize(result.CustomerOrderLineRequests[1])
            );
    }
}
