#pragma warning disable CA1001
namespace Insite.Integration.Connector.Prophet21.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.TestHelpers;
using Insite.Core.TestHelpers.Builders;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.Services;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.RealTimePricing.SystemSettings;

[TestFixture]
public class ExternalPricingServiceProphet21Tests
{
    private ExternalPricingServiceProphet21 externalPricingServiceProphet21;

    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    private Mock<IProphet21IntegrationConnectionProvider> prophet21IntegrationConnectionProvider;

    private Mock<RealTimePricingSettings> realTimePricingSettings;

    private IList<IntegrationConnection> integrationConnections;

    private FakeUnitOfWork fakeUnitOfWork;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.pipeAssemblyFactory = container.GetMock<IPipeAssemblyFactory>();
        this.prophet21IntegrationConnectionProvider =
            container.GetMock<IProphet21IntegrationConnectionProvider>();
        this.realTimePricingSettings = container.GetMock<RealTimePricingSettings>();

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

        this.externalPricingServiceProphet21 = container.Resolve<ExternalPricingServiceProphet21>();

        this.WhenConnectionId(string.Empty);
    }

    [Test]
    public void GetPrice_Should_Call_Execute_Pipeline_With_Pricing_Service_Parameter()
    {
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        var getLineItemPriceAndAvailabilityResult = this.CreateGetItemPriceResult(
            pricingServiceParameter
        );

        this.WhenExecutePipelineIs(getLineItemPriceAndAvailabilityResult);

        var result = this.externalPricingServiceProphet21.GetPrice(pricingServiceParameter);

        this.VerifyExecutePipelineWasCalled(pricingServiceParameter, null);
        Assert.AreEqual(
            result,
            getLineItemPriceAndAvailabilityResult.PricingServiceResults.First().Value
        );
    }

    [Test]
    public void GetPrice_Should_Call_Execute_Pipeline_With_Pricing_Service_Parameters()
    {
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        var getLineItemPriceAndAvailabilityResult = this.CreateGetItemPriceResult(
            pricingServiceParameter
        );

        this.WhenExecutePipelineIs(getLineItemPriceAndAvailabilityResult);

        var result = this.externalPricingServiceProphet21.GetPrice(
            new List<PricingServiceParameter> { pricingServiceParameter }
        );

        this.VerifyExecutePipelineWasCalled(pricingServiceParameter, null);
        Assert.AreEqual(
            result.First().Value,
            getLineItemPriceAndAvailabilityResult.PricingServiceResults.First().Value
        );
    }

    [Test]
    public void GetPrice_Should_Call_Execute_Pipeline_With_Integration_Connection_When_RealTimePricingSettings_ConnectionId_Is_Set()
    {
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        var getLineItemPriceAndAvailabilityResult = this.CreateGetItemPriceResult(
            pricingServiceParameter
        );

        var integrationConnection = Some.IntegrationConnection().Build();

        this.WhenExecutePipelineIs(getLineItemPriceAndAvailabilityResult);
        this.WhenConnectionId(integrationConnection.Id.ToString());
        this.WhenGetIntegrationConnectionIs(integrationConnection, integrationConnection);
        this.WhenExists(integrationConnection);

        this.externalPricingServiceProphet21.GetPrice(
            new List<PricingServiceParameter> { pricingServiceParameter }
        );

        this.VerifyExecutePipelineWasCalled(pricingServiceParameter, integrationConnection);
    }

    [Test]
    public void GetPrice_Should_Call_Execute_Pipeline_With_Null_Integration_Connection_When_RealTimePricingSettings_ConnectionId_Is_Not_Set()
    {
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        var getLineItemPriceAndAvailabilityResult = this.CreateGetItemPriceResult(
            pricingServiceParameter
        );

        var integrationConnection = Some.IntegrationConnection().Build();

        this.WhenExecutePipelineIs(getLineItemPriceAndAvailabilityResult);
        this.WhenConnectionId(string.Empty);
        this.WhenExists(integrationConnection);

        this.externalPricingServiceProphet21.GetPrice(
            new List<PricingServiceParameter> { pricingServiceParameter }
        );

        this.VerifyExecutePipelineWasCalled(pricingServiceParameter, null);
    }

    private GetItemPriceResult CreateGetItemPriceResult(
        PricingServiceParameter pricingServiceParameter
    )
    {
        return new GetItemPriceResult
        {
            PricingServiceResults = new Dictionary<PricingServiceParameter, PricingServiceResult>
            {
                { pricingServiceParameter, new PricingServiceResult() }
            }
        };
    }

    protected void WhenConnectionId(string connectionId)
    {
        this.realTimePricingSettings.Setup(o => o.ConnectionId).Returns(connectionId);
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

    private void WhenExecutePipelineIs(GetItemPriceResult getItemPriceResult)
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.IsAny<GetItemPriceParameter>(),
                        It.IsAny<GetItemPriceResult>()
                    )
            )
            .Returns(getItemPriceResult);
    }

    private void VerifyExecutePipelineWasCalled(
        PricingServiceParameter pricingServiceParameter,
        IntegrationConnection integrationConnection
    )
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<GetItemPriceParameter>(
                        p =>
                            p.PricingServiceParameters.Contains(pricingServiceParameter)
                            && p.IntegrationConnection == integrationConnection
                    ),
                    It.IsAny<GetItemPriceResult>()
                ),
            Times.Once()
        );
    }
}
