namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.OrderImport;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class GetErpOrderNumberFromResponseTests
    : BaseForPipeTests<OrderImportParameter, OrderImportResult>
{
    private const string OrderNumber = "1234";

    public override Type PipeType => typeof(GetErpOrderNumberFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_1000()
    {
        Assert.AreEqual(1000, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Erp_Order_Number()
    {
        var result = this.RunExecute();

        Assert.AreEqual(OrderNumber, result.ErpOrderNumber);
    }

    protected override OrderImportParameter GetDefaultParameter()
    {
        return new OrderImportParameter { CustomerOrder = Some.CustomerOrder() };
    }

    protected override OrderImportResult GetDefaultResult()
    {
        return new OrderImportResult
        {
            OrderImportReply = new OrderImport { Reply = new Reply { OrderNumber = OrderNumber } }
        };
    }
}
