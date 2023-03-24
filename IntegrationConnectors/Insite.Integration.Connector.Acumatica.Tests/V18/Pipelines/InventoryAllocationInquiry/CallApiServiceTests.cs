namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.InventoryAllocationInquiry;

using System;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Acumatica.Services;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.InventoryAllocationInquiry;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.InventoryAllocationInquiry;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<InventoryAllocationInquiryParameter, InventoryAllocationInquiryResult>
{
    private Mock<IAcumaticaApiService> acumaticaApiService;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.acumaticaApiService = this.container.GetMock<IAcumaticaApiService>();

        var acumaticaApiServiceFactory = this.container.GetMock<IAcumaticaApiServiceFactory>();
        acumaticaApiServiceFactory
            .Setup(o => o.GetAcumaticaApiService(null))
            .Returns(this.acumaticaApiService.Object);

        this.dependencyLocator
            .Setup(o => o.GetInstance<IAcumaticaApiServiceFactory>())
            .Returns(acumaticaApiServiceFactory.Object);
    }

    [Test]
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Call_Login()
    {
        this.RunExecute();

        this.VerifyLoginWasCalled();
    }

    [Test]
    public void Execute_Should_Call_InventoryAllocationInquiry_For_Each_InventoryAllocationInquiryRequest()
    {
        var inventoryAllocationInquiryRequest1 = new InventoryAllocationInquiry();
        var inventoryAllocationInquiryRequest2 = new InventoryAllocationInquiry();
        var inventoryAllocationInquiryResponse1 = new InventoryAllocationInquiry();
        var inventoryAllocationInquiryResponse2 = new InventoryAllocationInquiry();

        var result = this.GetDefaultResult();
        result.InventoryAllocationInquiryRequests.Add(inventoryAllocationInquiryRequest1);
        result.InventoryAllocationInquiryRequests.Add(inventoryAllocationInquiryRequest2);

        this.WhenInventoryAllocationInquiryIs(
            inventoryAllocationInquiryRequest1,
            inventoryAllocationInquiryResponse1
        );
        this.WhenInventoryAllocationInquiryIs(
            inventoryAllocationInquiryRequest2,
            inventoryAllocationInquiryResponse2
        );

        result = this.RunExecute(result);

        this.VerifyInventoryAllocationInquiryWasCalled(inventoryAllocationInquiryRequest1);
        this.VerifyInventoryAllocationInquiryWasCalled(inventoryAllocationInquiryRequest2);
        Assert.IsTrue(result.InventoryAllocationInquiryResponses.Count == 2);
        CollectionAssert.Contains(
            result.InventoryAllocationInquiryResponses,
            inventoryAllocationInquiryResponse1
        );
        CollectionAssert.Contains(
            result.InventoryAllocationInquiryResponses,
            inventoryAllocationInquiryResponse2
        );
    }

    [Test]
    public void Execute_Should_Call_Logout()
    {
        this.RunExecute();

        this.VerifyLogoutWasCalled();
    }

    private void WhenInventoryAllocationInquiryIs(
        InventoryAllocationInquiry inventoryAllocationInquiryRequest,
        InventoryAllocationInquiry inventoryAllocationInquiryResponse
    )
    {
        this.acumaticaApiService
            .Setup(o => o.InventoryAllocationInquiry(inventoryAllocationInquiryRequest))
            .Returns(inventoryAllocationInquiryResponse);
    }

    private void VerifyInventoryAllocationInquiryWasCalled(
        InventoryAllocationInquiry inventoryAllocationInquiryRequest
    )
    {
        this.acumaticaApiService.Verify(
            o => o.InventoryAllocationInquiry(inventoryAllocationInquiryRequest),
            Times.Once
        );
    }

    private void VerifyLoginWasCalled()
    {
        this.acumaticaApiService.Verify(o => o.Login(), Times.Once);
    }

    private void VerifyLogoutWasCalled()
    {
        this.acumaticaApiService.Verify(o => o.Logout(), Times.Once);
    }
}
