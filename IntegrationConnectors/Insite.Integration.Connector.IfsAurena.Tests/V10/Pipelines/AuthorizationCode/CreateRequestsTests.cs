namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.AuthorizationCode;

using System;
using FluentAssertions;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitAuthorizationCode;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;
using CustomerOrder = Insite.Data.Entities.CustomerOrder;

[TestFixture]
public class CreateRequestsTests
    : BaseForPipeTests<SubmitAuthorizationCodeParameter, SubmitAuthorizationCodeResult>
{
    public override Type PipeType => typeof(CreateRequests);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        this.pipe.Order.Should().Be(100);
    }

    [Test]
    public void Execute_Should_Add_All_Properties_To_Requests()
    {
        var parameter = this.GetDefaultParameter();

        var result = this.RunExecute(parameter);

        result.AuthorizationCodeRequest
            .Should()
            .BeEquivalentTo(
                new AuthorizationCode
                {
                    AuthCode = "auth",
                    OrderNo = parameter.ErpOrderNumber,
                    ReferenceId = "33323232"
                }
            );
        result.AuthorizationCodeRequest.OrderNo.Should().Be(parameter.ErpOrderNumber);
    }

    [Test]
    public void Execute_Should_Return_ExitPipeline_True_When_CustomerOrder_Doesnt_Have_CreditCardTransactions()
    {
        var parameter = new SubmitAuthorizationCodeParameter
        {
            CustomerOrder = Some.CustomerOrder().Build()
        };

        var result = this.RunExecute(parameter);

        result.ExitPipeline.Should().BeTrue();
    }

    protected override SubmitAuthorizationCodeParameter GetDefaultParameter()
    {
        var transaction = Some.CreditCardTransaction()
            .WithCardType("Visa")
            .WithCreditCardNumber("4111111111111111")
            .WithExpirationDate("09/2025")
            .WithAuthCode("auth")
            .WithToken1("token")
            .WithPNRef("33323232");

        var customerOrder = Some.CustomerOrder()
            .With(transaction)
            .With(Some.Currency().WithCurrencyCode("USD"))
            .WithCustomerNumber("123456")
            .Build();

        return new SubmitAuthorizationCodeParameter
        {
            ErpOrderNumber = "ERP123",
            CustomerOrder = customerOrder
        };
    }
}
