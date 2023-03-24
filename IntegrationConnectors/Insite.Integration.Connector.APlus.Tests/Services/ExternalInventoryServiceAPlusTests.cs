#pragma warning disable CA1001
namespace Insite.Integration.Connector.APlus.Tests.Services;

using System.Collections.Generic;

using Insite.Common.Dependencies;
using Insite.Core.Context;

using Moq;
using NUnit.Framework;

using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.TestHelpers;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.Integration.Connector.APlus.Services;
using Insite.Data.Entities;
using Insite.RealTimeInventory.SystemSettings;
using Insite.Core.Interfaces.Data;
using Insite.Core.TestHelpers.Builders;

[TestFixture]
public class ExternalInventoryServiceAPlusTests
{
    private ExternalInventoryServiceAPlus externalInventoryServiceAPlus;

    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    private Mock<RealTimeInventorySettings> realTimeInventorySettings;

    private IList<IntegrationConnection> integrationConnections;

    private FakeUnitOfWork fakeUnitOfWork;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.pipeAssemblyFactory = container.GetMock<IPipeAssemblyFactory>();
        this.realTimeInventorySettings = container.GetMock<RealTimeInventorySettings>();

        var unitOfWork = container.GetMock<IUnitOfWork>();
        this.fakeUnitOfWork = new FakeUnitOfWork(unitOfWork.Object);
        var unitOfWorkFactory = container.GetMock<IUnitOfWorkFactory>();

        unitOfWorkFactory.Setup(o => o.GetUnitOfWork()).Returns(this.fakeUnitOfWork);

        this.integrationConnections = new List<IntegrationConnection>();
        this.fakeUnitOfWork.SetupEntityList(this.integrationConnections);

        this.externalInventoryServiceAPlus = container.Resolve<ExternalInventoryServiceAPlus>();

        TestHelper.MockSiteContext(new Mock<ISiteContext>(), new Mock<IDependencyLocator>());

        this.WhenConnectionId(string.Empty);
    }

    [Test]
    public void GetInventory_Should_Call_Execute_Pipeline_With_Get_Inventory_Parameter()
    {
        var getInventoryParameter = new GetInventoryParameter();
        var getInventoryResult = new GetInventoryResult();

        this.WhenExecutePipelineIs(getInventoryResult);

        var result = this.externalInventoryServiceAPlus.GetInventory(getInventoryParameter);

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
        this.WhenExists(integrationConnection);

        this.externalInventoryServiceAPlus.GetInventory(getInventoryParameter);

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

        this.externalInventoryServiceAPlus.GetInventory(getInventoryParameter);

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

    private void WhenExecutePipelineIs(GetInventoryResult getInventoryResult)
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.IsAny<LineItemPriceAndAvailabilityParameter>(),
                        It.IsAny<LineItemPriceAndAvailabilityResult>()
                    )
            )
            .Returns(
                new LineItemPriceAndAvailabilityResult { GetInventoryResult = getInventoryResult }
            );
    }

    private void VerifyExecutePipelineWasCalled(
        GetInventoryParameter getInventoryParameter,
        IntegrationConnection integrationConnection
    )
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<LineItemPriceAndAvailabilityParameter>(
                        p =>
                            p.GetInventoryParameter == getInventoryParameter
                            && p.IntegrationConnection == integrationConnection
                    ),
                    It.IsAny<LineItemPriceAndAvailabilityResult>()
                ),
            Times.Once()
        );
    }
}
