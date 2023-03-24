namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetCartSummary;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class CreateInitialRequestTests
    : BaseForPipeTests<GetCartSummaryParameter, GetCartSummaryResult>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_New_GetItemPriceRequest()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.GetCartSummaryRequest);
        Assert.IsNotNull(result.GetCartSummaryRequest.Request);
    }

    [Test]
    public void Execute_Should_Populate_GetItemPriceRequest_Request_B2BSellerVersion()
    {
        var result = this.RunExecute();

        Assert.AreEqual("5", result.GetCartSummaryRequest.Request.B2BSellerVersion.MajorVersion);
        Assert.AreEqual("11", result.GetCartSummaryRequest.Request.B2BSellerVersion.MinorVersion);
        Assert.AreEqual("100", result.GetCartSummaryRequest.Request.B2BSellerVersion.BuildNumber);
    }
}
