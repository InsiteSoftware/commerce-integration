namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.CreateOrder;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class CreateInitialRequestTests : BaseForPipeTests<CreateOrderParameter, CreateOrderResult>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_New_CreateOrderRequest()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.CreateOrderRequest);
        Assert.AreEqual("CreateOrder", result.CreateOrderRequest.Name);
    }
}
