#pragma warning disable CA1001
namespace Insite.Integration.Connector.SXe.Tests.Services;

using System;
using System.Collections.Generic;
using System.Reflection;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers;
using Insite.Core.TestHelpers.Builders;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.Services;

[TestFixture]
public class SXeApiServiceFactoryTests
{
    private SXeApiServiceFactory sXeApiServiceFactory;

    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    private IList<IntegrationConnection> integrationConnections;

    private FakeUnitOfWork fakeUnitOfWork;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.integrationConnectorSettings = container.GetMock<IntegrationConnectorSettings>();

        var unitOfWork = container.GetMock<IUnitOfWork>();
        this.fakeUnitOfWork = new FakeUnitOfWork(unitOfWork.Object);
        var unitOfWorkFactory = container.GetMock<IUnitOfWorkFactory>();

        unitOfWorkFactory.Setup(o => o.GetUnitOfWork()).Returns(this.fakeUnitOfWork);

        this.integrationConnections = new List<IntegrationConnection>();
        this.fakeUnitOfWork.SetupEntityList(this.integrationConnections);

        var dependencyLocator = container.GetMock<IDependencyLocator>();
        dependencyLocator
            .Setup(o => o.GetInstance<IntegrationConnectorSettings>())
            .Returns(this.integrationConnectorSettings.Object);
        dependencyLocator
            .Setup(o => o.GetInstance<IUnitOfWorkFactory>())
            .Returns(unitOfWorkFactory.Object);

        this.sXeApiServiceFactory = container.Resolve<SXeApiServiceFactory>();
    }

    [Test]
    public void GetIntegrationConnection_Should_Return_Parameter_IntegrationConnection_If_Parameter_IntegrationConnection_Is_Not_Null()
    {
        var integrationConnection = Some.IntegrationConnection().Build();

        var result = this.CallGetIntegrationConnection(integrationConnection);

        Assert.AreEqual(integrationConnection, result);
    }

    [Test]
    public void GetIntegrationConnection_Should_Throw_Exception_If_Parameter_IntegrationConnection_Is_Null_And_IntegrationConnectionSettings_SXeIntegrationConnectionId_Is_Blank()
    {
        Assert.Throws<ArgumentException>(() => this.CallGetIntegrationConnection(null));
    }

    [Test]
    public void GetIntegrationConnection_Should_Throw_Exception_If_Parameter_IntegrationConnection_Is_Null_And_IntegrationConnectionSettings_SXeIntegrationConnectionId_Is_Not_Blank_And_IntegrationConnection_Is_Null()
    {
        this.WhenConnectionId(Guid.NewGuid().ToString());

        Assert.Throws<ArgumentException>(() => this.CallGetIntegrationConnection(null));
    }

    [Test]
    public void GetIntegrationConnection_Should_Return_IntegrationConnection_Matching_IntegrationConnectorSettings_SXeIntegrationConnectionId()
    {
        var integrationConnection = Some.IntegrationConnection().Build();

        this.WhenConnectionId(integrationConnection.Id.ToString());
        this.WhenExists(integrationConnection);

        var result = this.CallGetIntegrationConnection(null);

        Assert.AreEqual(integrationConnection, result);
    }

    protected IntegrationConnection CallGetIntegrationConnection(
        IntegrationConnection integrationConnection
    )
    {
        try
        {
            return (IntegrationConnection)
                typeof(SXeApiServiceFactory)
                    .GetMethod(
                        "GetIntegrationConnection",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    )
                    .Invoke(this.sXeApiServiceFactory, new object[] { integrationConnection });
        }
        catch (Exception exception)
        {
            throw exception.InnerException ?? exception;
        }
    }

    protected void WhenConnectionId(string connectionId)
    {
        this.integrationConnectorSettings
            .Setup(o => o.SXeIntegrationConnectionId)
            .Returns(connectionId);
    }

    private void WhenExists(IntegrationConnection integrationConnection)
    {
        this.integrationConnections.Add(integrationConnection);
    }
}
