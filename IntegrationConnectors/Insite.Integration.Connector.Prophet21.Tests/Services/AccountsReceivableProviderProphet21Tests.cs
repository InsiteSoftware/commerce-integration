namespace Insite.Integration.Connector.Prophet21.Tests.Services;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.TestHelpers;
using Insite.Integration.Connector.Prophet21.Services;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class AccountsReceivableProviderProphet21Tests
{
    private AccountsReceivableProviderProphet21 accountsReceivableProviderProphet21;

    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    private Mock<IProphet21IntegrationConnectionProvider> prophet21IntegrationConnectionProvider;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.pipeAssemblyFactory = container.GetMock<IPipeAssemblyFactory>();
        this.prophet21IntegrationConnectionProvider =
            container.GetMock<IProphet21IntegrationConnectionProvider>();

        container
            .GetMock<IDependencyLocator>()
            .Setup(o => o.GetInstance<IProphet21IntegrationConnectionProvider>())
            .Returns(this.prophet21IntegrationConnectionProvider.Object);

        this.accountsReceivableProviderProphet21 =
            container.Resolve<AccountsReceivableProviderProphet21>();
    }

    [Test]
    public void Execute_Should_Call_Execute_Pipeline_With_Get_Aging_Buckets_Parameter()
    {
        var getAgingBucketsParameter = new GetAgingBucketsParameter();
        var getAgingBucketsResult = new GetAgingBucketsResult();

        var getMyAccountOpenARResult = new GetMyAccountOpenARResult()
        {
            GetAgingBucketsResult = getAgingBucketsResult
        };

        this.WhenExecutePipelineIs(getMyAccountOpenARResult);

        var result = this.accountsReceivableProviderProphet21.GetAgingBuckets(
            getAgingBucketsParameter
        );

        this.VerifyExecutePipelineWasCalled(getAgingBucketsParameter);
        Assert.AreEqual(getAgingBucketsResult, result);
    }

    [Test]
    public void Execute_Should_Return_Error_Result_When_Pipeline_Returns_Error_Result()
    {
        var getAgingBucketsParameter = new GetAgingBucketsParameter();
        var getAgingBucketsResult = new GetAgingBucketsResult();

        var getMyAccountOpenARResult = new GetMyAccountOpenARResult
        {
            ResultCode = ResultCode.Error,
            GetAgingBucketsResult = getAgingBucketsResult
        };

        this.WhenExecutePipelineIs(getMyAccountOpenARResult);

        var result = this.accountsReceivableProviderProphet21.GetAgingBuckets(
            getAgingBucketsParameter
        );

        this.VerifyExecutePipelineWasCalled(getAgingBucketsParameter);
        Assert.AreEqual(ResultCode.Error, result.ResultCode);
    }

    private void WhenExecutePipelineIs(GetMyAccountOpenARResult getMyAccountOpenARResult)
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.IsAny<GetMyAccountOpenARParameter>(),
                        It.IsAny<GetMyAccountOpenARResult>()
                    )
            )
            .Returns(getMyAccountOpenARResult);
    }

    private void VerifyExecutePipelineWasCalled(GetAgingBucketsParameter getAgingBucketsParameter)
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<GetMyAccountOpenARParameter>(
                        p => p.GetAgingBucketsParameter == getAgingBucketsParameter
                    ),
                    It.IsAny<GetMyAccountOpenARResult>()
                ),
            Times.Once()
        );
    }
}
