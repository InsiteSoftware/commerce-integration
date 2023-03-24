namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.SalesOrder;

using System;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class CreateInitialRequestTests : BaseForPipeTests<SalesOrderParameter, SalesOrderResult>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_New_SalesOrderRequest()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.SalesOrderRequest);
    }
}
