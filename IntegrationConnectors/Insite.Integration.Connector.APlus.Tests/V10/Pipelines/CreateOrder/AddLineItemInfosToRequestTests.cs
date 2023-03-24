namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.CreateOrder;

using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class AddLineItemInfosToRequestTests
    : BaseForPipeTests<CreateOrderParameter, CreateOrderResult>
{
    private const string OrderLineNoteLineItemType = "M";

    private const string OrderHeaderNoteLineItemType = "/";

    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    public override Type PipeType => typeof(AddLineItemInfosToRequest);

    public override void SetUp()
    {
        this.pipeAssemblyFactory = this.container.GetMock<IPipeAssemblyFactory>();

        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();

        this.WhenExecuteGetOrderNotesPipelineIs(
            string.Empty,
            OrderLineNoteLineItemType,
            new List<RequestLineItemInfo>(),
            ResultCode.Success
        );
        this.WhenExecuteGetOrderNotesPipelineIs(
            string.Empty,
            OrderHeaderNoteLineItemType,
            new List<RequestLineItemInfo>(),
            ResultCode.Success
        );
    }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Set_LineItemInfos_From_OrderLines()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.OrderLine()
                    .With(Some.Product().WithErpNumber("Prod1"))
                    .WithQtyOrdered(1)
                    .WithUnitOfMeasure("EA")
                    .With(Some.Warehouse().WithName("Warehouse1"))
                    .WithUnitNetPrice(11)
            )
            .With(
                Some.OrderLine()
                    .With(Some.Product().WithErpNumber("Prod2"))
                    .WithQtyOrdered(2)
                    .WithUnitOfMeasure("CS")
                    .WithUnitNetPrice(12)
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo.All(
                o =>
                    customerOrder.OrderLines.Any(
                        p =>
                            p.Product.ErpNumber == o.ItemNumber
                            && p.QtyOrdered.ToString() == o.OrderQty
                            && p.UnitOfMeasure == o.UnitOfMeasure
                            && p.UnitNetPrice.ToString() == o.ActualSellPrice
                    )
            )
        );
    }

    [Test]
    public void Execute_Should_Exit_Pipeline_When_OrderLine_GetOrderNotes_Pipeline_Returns_Error()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber(string.Empty))
            .With(Some.OrderLine().With(Some.Product()))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenExecuteGetOrderNotesPipelineIs(
            string.Empty,
            OrderLineNoteLineItemType,
            new List<RequestLineItemInfo>(),
            ResultCode.Error
        );

        var result = this.RunExecute(parameter);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
    }

    [Test]
    public void Execute_Should_Set_OrderLine_Notes_LineItemInfo_From_GetOrderNotes_Pipeline()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber(string.Empty))
            .With(Some.OrderLine().With(Some.Product()))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var lineItemInfos = new List<RequestLineItemInfo>
        {
            new RequestLineItemInfo(),
            new RequestLineItemInfo()
        };

        this.WhenExecuteGetOrderNotesPipelineIs(
            string.Empty,
            OrderLineNoteLineItemType,
            lineItemInfos,
            ResultCode.Success
        );

        var result = this.RunExecute(parameter);

        CollectionAssert.Contains(
            result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo,
            lineItemInfos[0]
        );
        CollectionAssert.Contains(
            result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo,
            lineItemInfos[1]
        );
    }

    [Test]
    public void Execute_Should_Exit_Pipline_When_OrderHeader_GetOrderNotes_Pipeline_Returns_Error()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber(string.Empty))
            .With(Some.OrderLine().With(Some.Product()))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenExecuteGetOrderNotesPipelineIs(
            string.Empty,
            OrderHeaderNoteLineItemType,
            new List<RequestLineItemInfo>(),
            ResultCode.Error
        );

        var result = this.RunExecute(parameter);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
    }

    [Test]
    public void Execute_Should_Set_OrderHeader_Notes_LineItemInfo_From_GetOrderNotes_Pipeline()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber(string.Empty))
            .With(Some.OrderLine().With(Some.Product()))
            .Build();
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var lineItemInfos = new List<RequestLineItemInfo>
        {
            new RequestLineItemInfo(),
            new RequestLineItemInfo()
        };

        this.WhenExecuteGetOrderNotesPipelineIs(
            string.Empty,
            OrderHeaderNoteLineItemType,
            lineItemInfos,
            ResultCode.Success
        );

        var result = this.RunExecute(parameter);

        CollectionAssert.Contains(
            result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo,
            lineItemInfos[0]
        );
        CollectionAssert.Contains(
            result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo,
            lineItemInfos[1]
        );
    }

    [Test]
    public void Execute_Should_Not_Add_Shipping_And_Handling_Charges_When_Charges_Are_Less_Than_Or_Equal_To_Zero()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = Some.CustomerOrder()
            .WithShippingCharges(0)
            .WithHandlingCharges(0);

        this.WhenAPlusFreightChargeCodeIs("ABC");

        var result = this.RunExecute(parameter);

        CollectionAssert.IsEmpty(result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo);
    }

    [Test]
    public void Execute_Should_Not_Add_Shipping_And_Handling_Charges_When_APlusFreightChargeCode_Is_Not_Set()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = Some.CustomerOrder()
            .WithShippingCharges(1)
            .WithHandlingCharges(1);

        this.WhenAPlusFreightChargeCodeIs(string.Empty);

        var result = this.RunExecute(parameter);

        CollectionAssert.IsEmpty(result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo);
    }

    [Test]
    public void Execute_Should_Add_Shipping_And_Handling_Charges()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = Some.CustomerOrder()
            .WithShippingCharges(1)
            .WithHandlingCharges(1);

        this.WhenAPlusFreightChargeCodeIs("ABC");

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            string.Empty,
            result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo.First().ItemNumber
        );
        Assert.AreEqual(
            "/",
            result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo.First().LineItemType
        );
        Assert.AreEqual(
            "Y",
            result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo.First().OverridePrice
        );
        Assert.AreEqual(
            "ABC",
            result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo.First().ChargeType
        );
        Assert.AreEqual(
            "2",
            result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo.First().ActualSellPrice
        );
    }

    protected override CreateOrderResult GetDefaultResult()
    {
        return new CreateOrderResult
        {
            CreateOrderRequest = new CreateOrderRequest
            {
                Orders = new List<RequestOrder> { new RequestOrder() }
            }
        };
    }

    private void WhenExecuteGetOrderNotesPipelineIs(
        string notes,
        string lineItemType,
        List<RequestLineItemInfo> lineItemInfos,
        ResultCode resultCode
    )
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.Is<GetOrderNotesParameter>(
                            p => p.Notes == notes && p.LineItemType == lineItemType
                        ),
                        It.IsAny<GetOrderNotesResult>()
                    )
            )
            .Returns(
                new GetOrderNotesResult { ResultCode = resultCode, LineItemInfos = lineItemInfos }
            );
    }

    private void WhenAPlusFreightChargeCodeIs(string aPlusFreightChargeCode)
    {
        this.integrationConnectorSettings
            .Setup(o => o.APlusFreightChargeCode)
            .Returns(aPlusFreightChargeCode);
    }
}
