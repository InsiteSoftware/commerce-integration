namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.AccountsReceivableSummary;

using System;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.AccountsReceivableSummary;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.AccountsReceivableSummary;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.Integration.Connector.APlus.Services;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<AccountsReceivableSummaryParameter, AccountsReceivableSummaryResult>
{
    private Mock<IAPlusApiService> aPlusService;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.aPlusService = this.container.GetMock<IAPlusApiService>();

        var aPlusApiServiceFactory = this.container.GetMock<IAPlusApiServiceFactory>();
        aPlusApiServiceFactory
            .Setup(o => o.GetAPlusApiService(null))
            .Returns(this.aPlusService.Object);

        var dependencyLocator = this.container.GetMock<IDependencyLocator>();
        dependencyLocator
            .Setup(o => o.GetInstance<IAPlusApiServiceFactory>())
            .Returns(aPlusApiServiceFactory.Object);
    }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Response()
    {
        var accountsReceivableSummaryRequest = new AccountsReceivableSummaryRequest();
        var accountsReceivableSummaryResponse = new AccountsReceivableSummaryResponse();

        this.WhenAccountsReceivableSummaryIs(
            accountsReceivableSummaryRequest,
            accountsReceivableSummaryResponse
        );

        var result = new AccountsReceivableSummaryResult()
        {
            AccountsReceivableSummaryRequest = accountsReceivableSummaryRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(
            accountsReceivableSummaryResponse,
            result.AccountsReceivableSummaryResponse
        );
    }

    [Test]
    public void Execute_Should_Return_Error_When_AccountsReceivableSummary_Throws_Exception()
    {
        var accountsReceivableSummaryRequest = new AccountsReceivableSummaryRequest();
        var exceptionMessage = "error message";

        this.WhenAccountsReceivableSummaryThrowsException(
            accountsReceivableSummaryRequest,
            exceptionMessage
        );

        var result = new AccountsReceivableSummaryResult()
        {
            AccountsReceivableSummaryRequest = accountsReceivableSummaryRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, exceptionMessage);
    }

    protected void WhenAccountsReceivableSummaryIs(
        AccountsReceivableSummaryRequest accountsReceivableSummaryRequest,
        AccountsReceivableSummaryResponse accountsReceivableSummaryResponse
    )
    {
        this.aPlusService
            .Setup(o => o.AccountsReceivableSummary(accountsReceivableSummaryRequest))
            .Returns(accountsReceivableSummaryResponse);
    }

    protected void WhenAccountsReceivableSummaryThrowsException(
        AccountsReceivableSummaryRequest accountsReceivableSummaryRequest,
        string exceptionMessage
    )
    {
        this.aPlusService
            .Setup(o => o.AccountsReceivableSummary(accountsReceivableSummaryRequest))
            .Throws(new Exception(exceptionMessage));
    }
}
