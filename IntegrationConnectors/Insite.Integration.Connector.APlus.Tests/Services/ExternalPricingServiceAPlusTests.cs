#pragma warning disable CA1001

namespace Insite.Integration.Connector.APlus.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.TestHelpers;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.Services;
using Insite.Data.Entities;
using Insite.RealTimePricing.SystemSettings;
using Insite.Core.Interfaces.Data;
using Insite.Core.TestHelpers.Builders;

[TestFixture]
public class ExternalPricingServiceAPlusTests
{
    private ExternalPricingServiceAPlus externalPricingServiceAPlus;

    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    private Mock<RealTimePricingSettings> realTimePricingSettings;

    private IList<IntegrationConnection> integrationConnections;

    private FakeUnitOfWork fakeUnitOfWork;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.pipeAssemblyFactory = container.GetMock<IPipeAssemblyFactory>();
        this.realTimePricingSettings = container.GetMock<RealTimePricingSettings>();

        var unitOfWork = container.GetMock<IUnitOfWork>();
        this.fakeUnitOfWork = new FakeUnitOfWork(unitOfWork.Object);
        var unitOfWorkFactory = container.GetMock<IUnitOfWorkFactory>();

        unitOfWorkFactory.Setup(o => o.GetUnitOfWork()).Returns(this.fakeUnitOfWork);

        this.integrationConnections = new List<IntegrationConnection>();
        this.fakeUnitOfWork.SetupEntityList(this.integrationConnections);

        this.externalPricingServiceAPlus = container.Resolve<ExternalPricingServiceAPlus>();

        this.WhenConnectionId(string.Empty);
    }

    [Test]
    public void GetPrice_Should_Call_Execute_Pipeline_With_Pricing_Service_Parameter()
    {
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        var getLineItemPriceAndAvailabilityResult =
            this.CreateGetLineItemPriceAndAvailabilityResult(pricingServiceParameter);

        this.WhenExecutePipelineIs(getLineItemPriceAndAvailabilityResult);

        var result = this.externalPricingServiceAPlus.GetPrice(pricingServiceParameter);

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

        var getLineItemPriceAndAvailabilityResult =
            this.CreateGetLineItemPriceAndAvailabilityResult(pricingServiceParameter);

        this.WhenExecutePipelineIs(getLineItemPriceAndAvailabilityResult);

        var result = this.externalPricingServiceAPlus.GetPrice(
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

        var getLineItemPriceAndAvailabilityResult =
            this.CreateGetLineItemPriceAndAvailabilityResult(pricingServiceParameter);

        var integrationConnection = Some.IntegrationConnection().Build();

        this.WhenExecutePipelineIs(getLineItemPriceAndAvailabilityResult);
        this.WhenConnectionId(integrationConnection.Id.ToString());
        this.WhenExists(integrationConnection);

        this.externalPricingServiceAPlus.GetPrice(
            new List<PricingServiceParameter> { pricingServiceParameter }
        );

        this.VerifyExecutePipelineWasCalled(pricingServiceParameter, integrationConnection);
    }

    [Test]
    public void GetPrice_Should_Call_Execute_Pipeline_With_Null_Integration_Connection_When_RealTimePricingSettings_ConnectionId_Is_Not_Set()
    {
        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty);

        var getLineItemPriceAndAvailabilityResult =
            this.CreateGetLineItemPriceAndAvailabilityResult(pricingServiceParameter);

        var integrationConnection = Some.IntegrationConnection().Build();

        this.WhenExecutePipelineIs(getLineItemPriceAndAvailabilityResult);
        this.WhenConnectionId(string.Empty);
        this.WhenExists(integrationConnection);

        this.externalPricingServiceAPlus.GetPrice(
            new List<PricingServiceParameter> { pricingServiceParameter }
        );

        this.VerifyExecutePipelineWasCalled(pricingServiceParameter, null);
    }

    private LineItemPriceAndAvailabilityResult CreateGetLineItemPriceAndAvailabilityResult(
        PricingServiceParameter pricingServiceParameter
    )
    {
        return new LineItemPriceAndAvailabilityResult
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

    private void WhenExecutePipelineIs(
        LineItemPriceAndAvailabilityResult lineItemPriceAndAvailabilityResult
    )
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.IsAny<LineItemPriceAndAvailabilityParameter>(),
                        It.IsAny<LineItemPriceAndAvailabilityResult>()
                    )
            )
            .Returns(lineItemPriceAndAvailabilityResult);
    }

    private void VerifyExecutePipelineWasCalled(
        PricingServiceParameter pricingServiceParameter,
        IntegrationConnection integrationConnection
    )
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<LineItemPriceAndAvailabilityParameter>(
                        p =>
                            p.PricingServiceParameters.Contains(pricingServiceParameter)
                            && p.IntegrationConnection == integrationConnection
                    ),
                    It.IsAny<LineItemPriceAndAvailabilityResult>()
                ),
            Times.Once()
        );
    }
}
