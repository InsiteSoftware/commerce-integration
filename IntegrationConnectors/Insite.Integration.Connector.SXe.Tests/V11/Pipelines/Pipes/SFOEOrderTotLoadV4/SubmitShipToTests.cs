namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;

[TestFixture]
public class SubmitShipToTests
    : BaseForPipeTests<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    private Mock<StorefrontUserPermissionsSettings> storefrontUserPermissionsSettings;

    public override Type PipeType => typeof(SubmitShipTo);

    public override void SetUp()
    {
        this.pipeAssemblyFactory = this.container.GetMock<IPipeAssemblyFactory>();
        this.storefrontUserPermissionsSettings =
            this.container.GetMock<StorefrontUserPermissionsSettings>();

        this.WhenAllowCreateAccountIs(true);
        this.WhenAllowCreateNewShipToAddressIs(true);
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_AllowCreateAccount_Is_False()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        this.WhenAllowCreateAccountIs(false);

        var result = this.RunExecute(
            new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.ShipTo);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_AllowCreateNewShipToAddress_Is_False()
    {
        var customerOrder = Some.CustomerOrder().WithShipTo(Some.Customer()).Build();

        this.WhenAllowCreateNewShipToAddressIs(false);

        var result = this.RunExecute(
            new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.ShipTo);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_FulfillmentMethod_Is_PickUp()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .Build();

        var result = this.RunExecute(
            new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.ShipTo);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_ShipTo_IsDropShip()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithIsDropShip(true))
            .Build();

        var result = this.RunExecute(
            new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.ShipTo);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_Customer_IsGuest()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithIsGuest(true))
            .Build();

        var result = this.RunExecute(
            new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.ShipTo);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_ShipTo_Equals_CustomerOrder_Customer()
    {
        var customer = Some.Customer().Build();
        var customerOrder = Some.CustomerOrder().Build();

        customerOrder.Customer = customer;
        customerOrder.ShipTo = customer;

        var result = this.RunExecute(
            new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.ShipTo);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_Customer_ErpSequence_Is_Not_Blank()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithErpSequence("ERP123"))
            .Build();

        var result = this.RunExecute(
            new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.ShipTo);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_ShipTo_Address1_Is_Blank()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithAddress1(string.Empty))
            .Build();

        var result = this.RunExecute(
            new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.ShipTo);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_ShipTo_City_Is_Blank()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithCity(string.Empty))
            .Build();

        var result = this.RunExecute(
            new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.ShipTo);
    }

    [Test]
    public void Execute_Should_Return_Error_When_ARCustomerMnt_Returns_Error()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithAddress1("123 ABC St").WithCity("Minneapolis"))
            .Build();

        var arCustomerMntResult = new ARCustomerMntResult
        {
            ResultCode = ResultCode.Error,
            SubCode = SubCode.GeneralFailure,
            Messages = new List<ResultMessage> { new ResultMessage { Message = "Error" } }
        };

        this.WhenExecutePipelineIs(customerOrder.ShipTo, arCustomerMntResult);

        var result = this.RunExecute(
            new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasCalled(customerOrder.ShipTo);
        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(arCustomerMntResult.Message, result.Message);
    }

    [Test]
    public void Execute_Should_Set_ShipTo_ErpNumber_And_ErpSequence_From_ARCustomerMntResult_ErpSequence()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithAddress1("123 ABC St").WithCity("Minneapolis"))
            .Build();

        var arCustomerMntResult = new ARCustomerMntResult
        {
            ErpNumber = "ERP123",
            ErpSequence = "ERP321"
        };

        this.WhenExecutePipelineIs(customerOrder.ShipTo, arCustomerMntResult);

        var result = this.RunExecute(
            new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasCalled(customerOrder.ShipTo);
        Assert.AreEqual(arCustomerMntResult.ErpNumber, customerOrder.ShipTo.ErpNumber);
        Assert.AreEqual(arCustomerMntResult.ErpSequence, customerOrder.ShipTo.ErpSequence);
    }

    protected void WhenAllowCreateAccountIs(bool allowCreateAccount)
    {
        this.storefrontUserPermissionsSettings
            .Setup(o => o.AllowCreateAccount)
            .Returns(allowCreateAccount);
    }

    protected void WhenAllowCreateNewShipToAddressIs(bool allowCreateNewShipToAddress)
    {
        this.storefrontUserPermissionsSettings
            .Setup(o => o.AllowCreateNewShipToAddress)
            .Returns(allowCreateNewShipToAddress);
    }

    protected void WhenExecutePipelineIs(Customer customer, ARCustomerMntResult arCustomerMntResult)
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.Is<ARCustomerMntParameter>(p => p.Customer == customer),
                        It.IsAny<ARCustomerMntResult>()
                    )
            )
            .Returns(arCustomerMntResult);
    }

    protected void VerifyExecutePipelineWasNotCalled(Customer customer)
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<ARCustomerMntParameter>(p => p.Customer == customer),
                    It.IsAny<ARCustomerMntResult>()
                ),
            Times.Never
        );
    }

    protected void VerifyExecutePipelineWasCalled(Customer customer)
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<ARCustomerMntParameter>(p => p.Customer == customer),
                    It.IsAny<ARCustomerMntResult>()
                ),
            Times.Once
        );
    }
}
