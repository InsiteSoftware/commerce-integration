namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.CustomerSummary;

using System;
using Insite.Core.Plugins.Customer;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.CustomerSummary;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.CustomerSummary;
using NUnit.Framework;

[TestFixture]
public class AddBillToToRequestTests
    : BaseForPipeTests<CustomerSummaryParameter, CustomerSummaryResult>
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
        var parameter = new CustomerSummaryParameter
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
        var parameter = new CustomerSummaryParameter
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

        var parameter = new CustomerSummaryParameter
        {
            GetAgingBucketsParameter = new GetAgingBucketsParameter { Customer = customer }
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customer.ErpNumber, result.CustomerSummaryRequest.Request.Customer);
    }

    protected override CustomerSummaryResult GetDefaultResult()
    {
        return new CustomerSummaryResult
        {
            CustomerSummaryRequest = new CustomerSummaryRequest { Request = new Request() }
        };
    }
}
