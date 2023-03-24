namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetMyAccountOpenAR;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class CreateInitialRequestTests
    : BaseForPipeTests<GetMyAccountOpenARParameter, GetMyAccountOpenARResult>
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

        Assert.IsNotNull(result.GetMyAccountOpenARRequest);
        Assert.IsNotNull(result.GetMyAccountOpenARRequest.Request);
    }

    [Test]
    public void Execute_Should_Populate_GetItemPriceRequest_Request_B2BSellerVersion()
    {
        var result = this.RunExecute();

        Assert.AreEqual(
            "5",
            result.GetMyAccountOpenARRequest.Request.B2BSellerVersion.MajorVersion
        );
        Assert.AreEqual(
            "11",
            result.GetMyAccountOpenARRequest.Request.B2BSellerVersion.MinorVersion
        );
        Assert.AreEqual(
            "100",
            result.GetMyAccountOpenARRequest.Request.B2BSellerVersion.BuildNumber
        );
    }
}
