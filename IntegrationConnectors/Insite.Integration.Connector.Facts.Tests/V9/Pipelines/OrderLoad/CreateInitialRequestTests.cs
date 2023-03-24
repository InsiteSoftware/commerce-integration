namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderLoad;

using System;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;
using NUnit.Framework;

[TestFixture]
public class CreateInitialRequestTests : BaseForPipeTests<OrderLoadParameter, OrderLoadResult>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_Initial_Request()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.OrderLoadRequest);
        Assert.IsNotNull(result.OrderLoadRequest.Request);
        Assert.AreEqual("OrderLoad", result.OrderLoadRequest.Request.RequestID);
    }
}
