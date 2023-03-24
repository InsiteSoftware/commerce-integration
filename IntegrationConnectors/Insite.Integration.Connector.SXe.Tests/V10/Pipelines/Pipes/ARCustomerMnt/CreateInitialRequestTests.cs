namespace Insite.Integration.Connector.SXe.Tests.V10.Pipelines.Pipes.ARCustomerMnt;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.ARCustomerMntRequest;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;

[TestFixture]
public class CreateInitialRequestTests
    : BaseForPipeTests<ARCustomerMntParameter, ARCustomerMntResult>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_ARCustomerMntRequest()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.ARCustomerMntRequest);
    }
}
