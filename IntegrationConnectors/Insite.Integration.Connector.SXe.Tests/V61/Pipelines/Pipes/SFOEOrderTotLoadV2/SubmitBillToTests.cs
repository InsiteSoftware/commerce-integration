namespace Insite.Integration.Connector.SXe.Tests.V61.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;

[TestFixture]
public class SubmitBillToTests
    : BaseForPipeTests<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    private Mock<StorefrontUserPermissionsSettings> storefrontUserPermissionsSettings;

    public override Type PipeType => typeof(SubmitBillTo);

    public override void SetUp()
    {
        this.pipeAssemblyFactory = this.container.GetMock<IPipeAssemblyFactory>();
        this.storefrontUserPermissionsSettings =
            this.container.GetMock<StorefrontUserPermissionsSettings>();

        this.WhenAllowCreateAccountIs(true);
    }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_AllowCreateAccount_Is_False()
    {
        var customerOrder = Some.CustomerOrder().With(Some.Customer()).Build();

        this.WhenAllowCreateAccountIs(false);

        var result = this.RunExecute(
            new SFOEOrderTotLoadV2Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.Customer);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_Customer_IsGuest()
    {
        var customerOrder = Some.CustomerOrder().With(Some.Customer().WithIsGuest(true)).Build();

        var result = this.RunExecute(
            new SFOEOrderTotLoadV2Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.Customer);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_Customer_ErpNumber_Is_Not_Blank()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithErpNumber("ERP123"))
            .Build();

        var result = this.RunExecute(
            new SFOEOrderTotLoadV2Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.Customer);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_Customer_Address1_Is_Blank()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithAddress1(string.Empty))
            .Build();

        var result = this.RunExecute(
            new SFOEOrderTotLoadV2Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.Customer);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_Customer_City_Is_Blank()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithCity(string.Empty))
            .Build();

        var result = this.RunExecute(
            new SFOEOrderTotLoadV2Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasNotCalled(customerOrder.Customer);
    }

    [Test]
    public void Execute_Should_Return_Error_When_ARCustomerMnt_Returns_Error()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithAddress1("123 ABC St").WithCity("Minneapolis"))
            .Build();

        var arCustomerMntResult = new ARCustomerMntResult
        {
            ResultCode = ResultCode.Error,
            SubCode = SubCode.GeneralFailure,
            Messages = new List<ResultMessage> { new ResultMessage { Message = "Error" } }
        };

        this.WhenExecutePipelineIs(customerOrder.Customer, arCustomerMntResult);

        var result = this.RunExecute(
            new SFOEOrderTotLoadV2Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasCalled(customerOrder.Customer);
        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(arCustomerMntResult.Message, result.Message);
    }

    [Test]
    public void Execute_Should_Set_Customer_ErpNumber_From_ARCustomerMntResult_ErpNumber()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Customer().WithAddress1("123 ABC St").WithCity("Minneapolis"))
            .Build();

        var arCustomerMntResult = new ARCustomerMntResult { ErpNumber = "ERP123" };

        this.WhenExecutePipelineIs(customerOrder.Customer, arCustomerMntResult);

        var result = this.RunExecute(
            new SFOEOrderTotLoadV2Parameter { CustomerOrder = customerOrder }
        );

        this.VerifyExecutePipelineWasCalled(customerOrder.Customer);
        Assert.AreEqual(arCustomerMntResult.ErpNumber, customerOrder.Customer.ErpNumber);
    }

    protected void WhenAllowCreateAccountIs(bool allowCreateAccount)
    {
        this.storefrontUserPermissionsSettings
            .Setup(o => o.AllowCreateAccount)
            .Returns(allowCreateAccount);
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
