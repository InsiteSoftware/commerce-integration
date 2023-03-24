namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.ARCustomerMnt;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.ARCustomerMntRequest;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.ARCustomerMnt;

[TestFixture]
public class GetErpNumberAndErpSequenceFromResponseTests
    : BaseForPipeTests<ARCustomerMntParameter, ARCustomerMntResult>
{
    public override Type PipeType => typeof(GetErpNumberAndErpSequenceFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [TestCase("<ReturnData>Customer #: 1234</ReturnData>")]
    [TestCase("Customer #: 1234")]
    [TestCase("Customer #: 1234, Ship To: 4321")]
    public void Execute_Should_Get_ErpNumber(string returnData)
    {
        var result = new ARCustomerMntResult
        {
            ARCustomerMntResponse = new ARCustomerMntResponse
            {
                Response = new Response { ReturnData = returnData }
            }
        };

        result = this.RunExecute(result);

        Assert.AreEqual("1234", result.ErpNumber);
    }

    [TestCase("<ReturnData>Customer #: 1234, Ship To: 4321</ReturnData>")]
    [TestCase("Customer #: 1234, Ship To: 4321")]
    public void Execute_Should_Get_ErpSequence(string returnData)
    {
        var result = new ARCustomerMntResult
        {
            ARCustomerMntResponse = new ARCustomerMntResponse
            {
                Response = new Response { ReturnData = returnData }
            }
        };

        result = this.RunExecute(result);

        Assert.AreEqual("4321", result.ErpSequence);
    }
}
