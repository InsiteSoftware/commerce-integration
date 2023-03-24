namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.AccountsReceivableSummary;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.AccountsReceivableSummary;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class CreateInitialRequestTests
    : BaseForPipeTests<AccountsReceivableSummaryParameter, AccountsReceivableSummaryResult>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_New_AccountsReceivableSummaryRequest()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.AccountsReceivableSummaryRequest);
    }
}
