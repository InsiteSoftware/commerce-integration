namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitOrder;

using System;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class GetErpOrderNumberFromResponseTests
    : BaseForPipeTests<SubmitOrderParameter, SubmitOrderResult>
{
    public override Type PipeType => typeof(GetErpOrderNumberFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_500()
    {
        this.pipe.Order.Should().Be(500);
    }

    [Test]
    public void Execute_Should_Get_ErpOrderNumber_From_Response()
    {
        var result = new SubmitOrderResult
        {
            CustomerOrderResponse = new CustomerOrder { OrderNo = "Erp123" }
        };

        result = this.RunExecute(result);

        result.ErpOrderNumber.Should().Be(result.CustomerOrderResponse.OrderNo);
    }
}
