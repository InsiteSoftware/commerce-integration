namespace Insite.Integration.Connector.SXe.Tests.V61.Pipelines.Pipes.SFCustomerSummary;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;

[TestFixture]
public class CreateInitialRequestTests
    : BaseForPipeTests<SFCustomerSummaryParameter, SFCustomerSummaryResult>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_New_SFCustomerSummaryRequest()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.SFCustomerSummaryRequest);
    }
}
