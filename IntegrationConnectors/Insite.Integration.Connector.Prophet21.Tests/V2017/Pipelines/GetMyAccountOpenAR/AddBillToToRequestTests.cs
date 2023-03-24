namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetMyAccountOpenAR;

using System;

using NUnit.Framework;

using Insite.Core.Plugins.Customer;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class AddBillToToRequestTests
    : BaseForPipeTests<GetMyAccountOpenARParameter, GetMyAccountOpenARResult>
{
    public override Type PipeType => typeof(AddBillToToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Error_Message_If_Customer_Is_Null()
    {
        var parameter = new GetMyAccountOpenARParameter
        {
            GetAgingBucketsParameter = new GetAgingBucketsParameter { Customer = null }
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
    }

    [Test]
    public void Execute_Should_Bypass_Pipeline_If_Customer_Erp_Number_Is_Blank()
    {
        var parameter = new GetMyAccountOpenARParameter
        {
            GetAgingBucketsParameter = new GetAgingBucketsParameter
            {
                Customer = Some.Customer().WithErpNumber(string.Empty).Build()
            }
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.IsTrue(result.ExitPipeline);
    }

    [Test]
    public void Execute_Should_Populate_Customer_Number()
    {
        var customer = Some.Customer().WithErpNumber("ABC123").Build();

        var parameter = new GetMyAccountOpenARParameter
        {
            GetAgingBucketsParameter = new GetAgingBucketsParameter { Customer = customer }
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customer.ErpNumber, result.GetMyAccountOpenARRequest.Request.CustomerCode);
    }

    protected override GetMyAccountOpenARResult GetDefaultResult()
    {
        return new GetMyAccountOpenARResult
        {
            GetMyAccountOpenARRequest = new GetMyAccountOpenAR { Request = new Request() }
        };
    }
}
