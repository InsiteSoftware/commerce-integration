namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitOrderCharges;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderCharges;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class SerializeRequestsTests
    : BaseForPipeTests<SubmitOrderChargesParameter, SubmitOrderChargesResult>
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
        var result = new SubmitOrderChargesResult
        {
            CustomerOrderChargeRequests = new List<CustomerOrderCharge>
            {
                new CustomerOrderCharge
                {
                    ChargeType = "ChargeType1",
                    IntrastatExemptDb = false,
                    UnitChargeDb = false
                },
                new CustomerOrderCharge { ChargeType = "ChargeType2" }
            }
        };

        result = this.RunExecute(result);

        result.SerializedCustomerOrderChargeRequests.Should().NotBeEmpty();
        result.SerializedCustomerOrderChargeRequests
            .Should()
            .ContainEquivalentOf(
                IfsAurenaSerializationService.Serialize(result.CustomerOrderChargeRequests[0])
            );
        result.SerializedCustomerOrderChargeRequests
            .Should()
            .ContainEquivalentOf(
                IfsAurenaSerializationService.Serialize(result.CustomerOrderChargeRequests[1])
            );

        result.SerializedCustomerOrderChargeRequests[0]
            .Should()
            .Contain("\"IntrastatExempt\":false");
        result.SerializedCustomerOrderChargeRequests[0].Should().Contain("\"UnitCharge\":false");
    }
}
