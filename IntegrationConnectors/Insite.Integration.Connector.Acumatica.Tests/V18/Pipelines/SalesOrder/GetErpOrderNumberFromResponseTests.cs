namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.SalesOrder;

using System;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;
using NUnit.Framework;

[TestFixture]
public class GetErpOrderNumberFromResponseTests
    : BaseForPipeTests<SalesOrderParameter, SalesOrderResult>
{
    public override Type PipeType => typeof(GetErpOrderNumberFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_900()
    {
        Assert.AreEqual(900, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_ErpOrderNumber_From_SalesOrderResponse()
    {
        var result = this.GetDefaultResult();
        result.SalesOrderResponse = new SalesOrder() { OrderNbr = "ERP123" };

        result = this.RunExecute(result);

        Assert.AreEqual(result.SalesOrderResponse.OrderNbr, result.ErpOrderNumber);
    }
}
