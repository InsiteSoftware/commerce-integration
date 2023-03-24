namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.CreateOrder;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class GetErpOrderNumberFromResponseTests
    : BaseForPipeTests<CreateOrderParameter, CreateOrderResult>
{
    private const string OrderNumber = "1234";

    private const string OrderGeneration = "01";

    public override Type PipeType => typeof(GetErpOrderNumberFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_700()
    {
        Assert.AreEqual(700, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Bypass_Pipe_When_IsOrderSubmit_False()
    {
        var parameter = this.GetDefaultParameter();
        parameter.IsOrderSubmit = false;

        var result = this.RunExecute(parameter);

        Assert.IsNull(result.ErpOrderNumber);
    }

    [Test]
    public void Execute_Should_Get_Erp_Order_Number()
    {
        var result = this.RunExecute();

        Assert.AreEqual($"{OrderNumber}-{OrderGeneration}", result.ErpOrderNumber);
    }

    protected override CreateOrderParameter GetDefaultParameter()
    {
        return new CreateOrderParameter
        {
            CustomerOrder = Some.CustomerOrder().Build(),
            IsOrderSubmit = true
        };
    }

    protected override CreateOrderResult GetDefaultResult()
    {
        return new CreateOrderResult
        {
            CreateOrderResponse = new CreateOrderResponse
            {
                OrderHeader = new ResponseOrderHeader
                {
                    OrderNumber = OrderNumber,
                    OrderGeneration = OrderGeneration
                }
            }
        };
    }
}
