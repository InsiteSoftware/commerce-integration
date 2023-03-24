namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetMyAccountOpenAR;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class GetAgingBucketsFromResponseTests
    : BaseForPipeTests<GetMyAccountOpenARParameter, GetMyAccountOpenARResult>
{
    public override Type PipeType => typeof(GetAgingBucketsFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Aging_Buckets()
    {
        var result = new GetMyAccountOpenARResult
        {
            GetMyAccountOpenARReply = new GetMyAccountOpenAR
            {
                Reply = new Reply
                {
                    AgedAR = new ReplyAgedAR
                    {
                        CurrentAmount = 12,
                        Age1Amount = 20,
                        Age1ToAge2Amount = 14,
                        Age2ToAge3Amount = 30,
                        OverAge3Amount = 50
                    }
                }
            }
        };

        result = this.RunExecute(result);

        Assert.AreEqual(12, result.GetAgingBucketsResult.AgingBuckets[0]);
        Assert.AreEqual(20, result.GetAgingBucketsResult.AgingBuckets[1]);
        Assert.AreEqual(14, result.GetAgingBucketsResult.AgingBuckets[2]);
        Assert.AreEqual(30, result.GetAgingBucketsResult.AgingBuckets[3]);
        Assert.AreEqual(50, result.GetAgingBucketsResult.AgingBuckets[4]);
    }
}
