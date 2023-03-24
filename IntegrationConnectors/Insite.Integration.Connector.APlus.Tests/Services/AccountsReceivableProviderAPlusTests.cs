namespace Insite.Integration.Connector.APlus.Tests.Services;

using Moq;
using NUnit.Framework;

using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.TestHelpers;
using Insite.Integration.Connector.APlus.Services;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class AccountsReceivableProviderAPlusTests
{
    private AccountsReceivableProviderAPlus accountsReceivableProviderAPlus;

    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.pipeAssemblyFactory = container.GetMock<IPipeAssemblyFactory>();

        this.accountsReceivableProviderAPlus = container.Resolve<AccountsReceivableProviderAPlus>();
    }

    [Test]
    public void Execute_Should_Call_Execute_Pipeline_With_Get_Aging_Buckets_Parameter()
    {
        var getAgingBucketsParameter = new GetAgingBucketsParameter();
        var getAgingBucketsResult = new GetAgingBucketsResult();

        var accountsReceivableSummaryResult = new AccountsReceivableSummaryResult()
        {
            GetAgingBucketsResult = getAgingBucketsResult
        };

        this.WhenExecutePipelineIs(accountsReceivableSummaryResult);

        var result = this.accountsReceivableProviderAPlus.GetAgingBuckets(getAgingBucketsParameter);

        this.VerifyExecutePipelineWasCalled(getAgingBucketsParameter);
        Assert.AreEqual(getAgingBucketsResult, result);
    }

    [Test]
    public void Execute_Should_Return_Error_Result_When_Pipeline_Returns_Error_Result()
    {
        var getAgingBucketsParameter = new GetAgingBucketsParameter();
        var getAgingBucketsResult = new GetAgingBucketsResult();

        var accountsReceivableSummaryResult = new AccountsReceivableSummaryResult
        {
            ResultCode = ResultCode.Error,
            GetAgingBucketsResult = getAgingBucketsResult
        };

        this.WhenExecutePipelineIs(accountsReceivableSummaryResult);

        var result = this.accountsReceivableProviderAPlus.GetAgingBuckets(getAgingBucketsParameter);

        this.VerifyExecutePipelineWasCalled(getAgingBucketsParameter);
        Assert.AreEqual(ResultCode.Error, result.ResultCode);
    }

    private void WhenExecutePipelineIs(
        AccountsReceivableSummaryResult accountsReceivableSummaryResult
    )
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.IsAny<AccountsReceivableSummaryParameter>(),
                        It.IsAny<AccountsReceivableSummaryResult>()
                    )
            )
            .Returns(accountsReceivableSummaryResult);
    }

    private void VerifyExecutePipelineWasCalled(GetAgingBucketsParameter getAgingBucketsParameter)
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<AccountsReceivableSummaryParameter>(
                        p => p.GetAgingBucketsParameter == getAgingBucketsParameter
                    ),
                    It.IsAny<AccountsReceivableSummaryResult>()
                ),
            Times.Once()
        );
    }
}
