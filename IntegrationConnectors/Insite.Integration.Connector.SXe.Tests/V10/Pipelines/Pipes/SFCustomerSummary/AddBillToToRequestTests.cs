namespace Insite.Integration.Connector.SXe.Tests.V10.Pipelines.Pipes.SFCustomerSummary;

using System;

using NUnit.Framework;

using Insite.Core.Plugins.Customer;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFCustomerSummary;

[TestFixture]
public class AddBillToToRequestTests
    : BaseForPipeTests<SFCustomerSummaryParameter, SFCustomerSummaryResult>
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
        var parameter = new SFCustomerSummaryParameter
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
        var parameter = new SFCustomerSummaryParameter
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
    public void Execute_Should_Return_Error_If_Customer_Erp_Number_Is_Not_Decimal()
    {
        var parameter = new SFCustomerSummaryParameter
        {
            GetAgingBucketsParameter = new GetAgingBucketsParameter
            {
                Customer = Some.Customer().WithErpNumber("ABC").Build()
            }
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
    }

    [Test]
    public void Execute_Should_Set_Customer_Number()
    {
        var parameter = new SFCustomerSummaryParameter
        {
            GetAgingBucketsParameter = new GetAgingBucketsParameter
            {
                Customer = Some.Customer().WithErpNumber("123").Build()
            }
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            decimal.Parse(parameter.GetAgingBucketsParameter.Customer.ErpNumber),
            result.SFCustomerSummaryRequest.CustomerNumber
        );
    }

    protected override SFCustomerSummaryResult GetDefaultResult()
    {
        return new SFCustomerSummaryResult
        {
            SFCustomerSummaryRequest = new SFCustomerSummaryRequest()
        };
    }
}
