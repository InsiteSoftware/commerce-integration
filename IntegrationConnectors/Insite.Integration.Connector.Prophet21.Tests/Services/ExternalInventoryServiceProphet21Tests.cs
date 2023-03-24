#pragma warning disable CA1001
namespace Insite.Integration.Connector.Prophet21.Tests.Services;

using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.TestHelpers;
using Insite.Core.TestHelpers.Builders;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.Services;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.RealTimeInventory.SystemSettings;

[TestFixture]
public class ExternalInventoryServiceProphet21Tests
{
    private ExternalInventoryServiceProphet21 externalInventoryServiceProphet21;

    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    private Mock<IProphet21IntegrationConnectionProvider> prophet21IntegrationConnectionProvider;

    private Mock<RealTimeInventorySettings> realTimeInventorySettings;

    private IList<IntegrationConnection> integrationConnections;

    private FakeUnitOfWork fakeUnitOfWork;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.pipeAssemblyFactory = container.GetMock<IPipeAssemblyFactory>();
        this.prophet21IntegrationConnectionProvider =
            container.GetMock<IProphet21IntegrationConnectionProvider>();
        this.realTimeInventorySettings = container.GetMock<RealTimeInventorySettings>();

        container
            .GetMock<IDependencyLocator>()
            .Setup(o => o.GetInstance<IProphet21IntegrationConnectionProvider>())
            .Returns(this.prophet21IntegrationConnectionProvider.Object);

        var unitOfWork = container.GetMock<IUnitOfWork>();
        this.fakeUnitOfWork = new FakeUnitOfWork(unitOfWork.Object);
        var unitOfWorkFactory = container.GetMock<IUnitOfWorkFactory>();

        unitOfWorkFactory.Setup(o => o.GetUnitOfWork()).Returns(this.fakeUnitOfWork);

        this.integrationConnections = new List<IntegrationConnection>();
        this.fakeUnitOfWork.SetupEntityList(this.integrationConnections);

        this.externalInventoryServiceProphet21 =
            container.Resolve<ExternalInventoryServiceProphet21>();

        TestHelper.MockSiteContext(new Mock<ISiteContext>(), new Mock<IDependencyLocator>());

        this.WhenConnectionId(string.Empty);
    }

    [Test]
    public void GetInventory_Should_Call_Execute_Pipeline_With_Get_Inventory_Parameter()
    {
        var getInventoryParameter = new GetInventoryParameter();
        var getInventoryResult = new GetInventoryResult();

        this.WhenExecutePipelineIs(getInventoryResult);

        var result = this.externalInventoryServiceProphet21.GetInventory(getInventoryParameter);

        this.VerifyExecutePipelineWasCalled(getInventoryParameter, null);
        Assert.AreEqual(result, getInventoryResult);
    }

    [Test]
    public void GetPrice_Should_Call_Execute_Pipeline_With_Integration_Connection_When_RealTimePricingSettings_ConnectionId_Is_Set()
    {
        var getInventoryParameter = new GetInventoryParameter();
        var getInventoryResult = new GetInventoryResult();

        var integrationConnection = Some.IntegrationConnection().Build();

        this.WhenExecutePipelineIs(getInventoryResult);
        this.WhenConnectionId(integrationConnection.Id.ToString());
        this.WhenGetIntegrationConnectionIs(integrationConnection, integrationConnection);
        this.WhenExists(integrationConnection);

        this.externalInventoryServiceProphet21.GetInventory(getInventoryParameter);

        this.VerifyExecutePipelineWasCalled(getInventoryParameter, integrationConnection);
    }

    [Test]
    public void GetPrice_Should_Call_Execute_Pipeline_With_Null_Integration_Connection_When_RealTimePricingSettings_ConnectionId_Is_Not_Set()
    {
        var getInventoryParameter = new GetInventoryParameter();
        var getInventoryResult = new GetInventoryResult();

        var integrationConnection = Some.IntegrationConnection().Build();

        this.WhenExecutePipelineIs(getInventoryResult);
        this.WhenConnectionId(string.Empty);
        this.WhenExists(integrationConnection);

        this.externalInventoryServiceProphet21.GetInventory(getInventoryParameter);

        this.VerifyExecutePipelineWasCalled(getInventoryParameter, null);
    }

    protected void WhenConnectionId(string connectionId)
    {
        this.realTimeInventorySettings.Setup(o => o.ConnectionId).Returns(connectionId);
    }

    private void WhenExists(IntegrationConnection integrationConnection)
    {
        this.integrationConnections.Add(integrationConnection);
    }

    private void WhenGetIntegrationConnectionIs(
        IntegrationConnection integrationConnectionIn,
        IntegrationConnection integrationConnectionOut
    )
    {
        this.prophet21IntegrationConnectionProvider
            .Setup(o => o.GetIntegrationConnection(integrationConnectionIn))
            .Returns(integrationConnectionOut);
    }

    private void WhenExecutePipelineIs(GetInventoryResult getInventoryResult)
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.IsAny<GetItemPriceParameter>(),
                        It.IsAny<GetItemPriceResult>()
                    )
            )
            .Returns(new GetItemPriceResult { GetInventoryResult = getInventoryResult });
    }

    private void VerifyExecutePipelineWasCalled(
        GetInventoryParameter getInventoryParameter,
        IntegrationConnection integrationConnection
    )
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<GetItemPriceParameter>(
                        p =>
                            p.GetInventoryParameter == getInventoryParameter
                            && p.IntegrationConnection == integrationConnection
                    ),
                    It.IsAny<GetItemPriceResult>()
                ),
            Times.Once()
        );
    }
}
