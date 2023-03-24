namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetPartAvailability;

using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities.Dtos;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.Services;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<GetPartAvailabilityParameter, GetPartAvailabilityResult>
{
    private Mock<IIfsApiService> ifsApiService;

    private Mock<IWarehouseRepository> warehouseRepository;

    private IList<WarehouseDto> warehousesDtos;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.ifsApiService = this.container.GetMock<IIfsApiService>();

        var ifsApiServiceFactory = this.container.GetMock<IIfsApiServiceFactory>();
        ifsApiServiceFactory
            .Setup(o => o.GetIfsApiService(null))
            .Returns(this.ifsApiService.Object);

        this.dependencyLocator
            .Setup(o => o.GetInstance<IIfsApiServiceFactory>())
            .Returns(ifsApiServiceFactory.Object);

        this.warehouseRepository = this.container.GetMock<IWarehouseRepository>();
        this.warehousesDtos = new List<WarehouseDto>();

        this.warehouseRepository.Setup(o => o.GetCachedWarehouses()).Returns(this.warehousesDtos);
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<IWarehouseRepository>())
            .Returns(this.warehouseRepository.Object);
    }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_PartAvailabilityResponse()
    {
        var partAvailabilityRequest = new partAvailabilityRequest();
        var partAvailabilityResponse = new partAvailabilityResponse { errorText = string.Empty };

        this.WhenGetPartAvailabilityIs(partAvailabilityRequest, null, partAvailabilityResponse);

        var result = new GetPartAvailabilityResult
        {
            PartAvailabilityRequest = partAvailabilityRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(partAvailabilityResponse, result.PartAvailabilityResponse);
    }

    [Test]
    public void Execute_Should_Return_Error_When_GetCustomerPrice_Returns_Error()
    {
        var partAvailabilityRequest = new partAvailabilityRequest();
        var partAvailabilityResponse = new partAvailabilityResponse { errorText = "Error Message" };

        this.WhenGetPartAvailabilityIs(partAvailabilityRequest, null, partAvailabilityResponse);

        var result = new GetPartAvailabilityResult
        {
            PartAvailabilityRequest = partAvailabilityRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(partAvailabilityResponse.errorText, result.Message);
    }

    [Test]
    public void Execute_Should_Get_Inventory_For_All_Warehouses_When_GetInventoryParameter_GetWarehouses_Is_True()
    {
        var warehouseOne = new WarehouseDto { Name = "01" };
        var warehouseTwo = new WarehouseDto { Name = "02" };
        var warehouseThree = new WarehouseDto { Name = "03" };

        var partAvailabilityRequest = new partAvailabilityRequest { site = warehouseOne.Name };

        var partAvailabilityResponseOne = new partAvailabilityResponse { site = warehouseOne.Name };
        var partAvailabilityResponseTwo = new partAvailabilityResponse { site = warehouseTwo.Name };
        var partAvailabilityResponseThree = new partAvailabilityResponse
        {
            site = warehouseThree.Name
        };

        this.WhenGetPartAvailabilityIs(
            partAvailabilityRequest,
            warehouseOne.Name,
            partAvailabilityResponseOne
        );
        this.WhenGetPartAvailabilityIs(
            partAvailabilityRequest,
            warehouseTwo.Name,
            partAvailabilityResponseTwo
        );
        this.WhenGetPartAvailabilityIs(
            partAvailabilityRequest,
            warehouseThree.Name,
            partAvailabilityResponseThree
        );
        this.WhenExists(warehouseTwo);
        this.WhenExists(warehouseThree);

        var parameter = new GetPartAvailabilityParameter
        {
            GetInventoryParameter = new GetInventoryParameter { GetWarehouses = true }
        };
        var result = new GetPartAvailabilityResult
        {
            PartAvailabilityRequest = partAvailabilityRequest
        };
        result = this.RunExecute(parameter, result);

        this.VerifyGetPartAvailabilityWasCalled(partAvailabilityRequest, warehouseOne.Name);
        this.VerifyGetPartAvailabilityWasCalled(partAvailabilityRequest, warehouseTwo.Name);
        this.VerifyGetPartAvailabilityWasCalled(partAvailabilityRequest, warehouseThree.Name);
    }

    [Test]
    public void Execute_Should_Return_Error_Messages_For_All_Response_Errors()
    {
        var warehouseOne = new WarehouseDto { Name = "01" };
        var warehouseTwo = new WarehouseDto { Name = "02" };
        var warehouseThree = new WarehouseDto { Name = "03" };

        var partAvailabilityRequest = new partAvailabilityRequest { site = warehouseOne.Name };

        var partAvailabilityResponseOne = new partAvailabilityResponse
        {
            site = warehouseOne.Name,
            errorText = $"Error{warehouseOne.Name}"
        };
        var partAvailabilityResponseTwo = new partAvailabilityResponse
        {
            site = warehouseTwo.Name,
            errorText = $"Error{warehouseTwo.Name}"
        };
        var partAvailabilityResponseThree = new partAvailabilityResponse
        {
            site = warehouseThree.Name,
            errorText = $"Error{warehouseThree.Name}"
        };

        this.WhenGetPartAvailabilityIs(
            partAvailabilityRequest,
            warehouseOne.Name,
            partAvailabilityResponseOne
        );
        this.WhenGetPartAvailabilityIs(
            partAvailabilityRequest,
            warehouseTwo.Name,
            partAvailabilityResponseTwo
        );
        this.WhenGetPartAvailabilityIs(
            partAvailabilityRequest,
            warehouseThree.Name,
            partAvailabilityResponseThree
        );
        this.WhenExists(warehouseTwo);
        this.WhenExists(warehouseThree);

        var parameter = new GetPartAvailabilityParameter
        {
            GetInventoryParameter = new GetInventoryParameter { GetWarehouses = true }
        };
        var result = new GetPartAvailabilityResult
        {
            PartAvailabilityRequest = partAvailabilityRequest
        };
        result = this.RunExecute(parameter, result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.IsTrue(result.Messages.Any(o => o.Message == $"Error{warehouseOne.Name}"));
        Assert.IsTrue(result.Messages.Any(o => o.Message == $"Error{warehouseTwo.Name}"));
        Assert.IsTrue(result.Messages.Any(o => o.Message == $"Error{warehouseThree.Name}"));
    }

    [Test]
    public void Execute_Should_Assemble_Availability_From_All_Response_Inventory()
    {
        var warehouseOne = new WarehouseDto { Name = "01" };
        var warehouseTwo = new WarehouseDto { Name = "02" };
        var warehouseThree = new WarehouseDto { Name = "03" };

        var partAvailabilityRequest = new partAvailabilityRequest { site = warehouseOne.Name };

        var partAvailabilityResponseOne = new partAvailabilityResponse
        {
            site = warehouseOne.Name,
            partsAvailabile = new List<partAvailabilityResData>
            {
                new partAvailabilityResData
                {
                    partsAvailableSite = warehouseOne.Name,
                    quantityAvailable = 41
                }
            }
        };
        var partAvailabilityResponseTwo = new partAvailabilityResponse
        {
            site = warehouseTwo.Name,
            partsAvailabile = new List<partAvailabilityResData>
            {
                new partAvailabilityResData
                {
                    partsAvailableSite = warehouseTwo.Name,
                    quantityAvailable = 12
                }
            }
        };
        var partAvailabilityResponseThree = new partAvailabilityResponse
        {
            site = warehouseThree.Name,
            partsAvailabile = new List<partAvailabilityResData>
            {
                new partAvailabilityResData
                {
                    partsAvailableSite = warehouseThree.Name,
                    quantityAvailable = 88
                }
            }
        };

        this.WhenGetPartAvailabilityIs(
            partAvailabilityRequest,
            warehouseOne.Name,
            partAvailabilityResponseOne
        );
        this.WhenGetPartAvailabilityIs(
            partAvailabilityRequest,
            warehouseTwo.Name,
            partAvailabilityResponseTwo
        );
        this.WhenGetPartAvailabilityIs(
            partAvailabilityRequest,
            warehouseThree.Name,
            partAvailabilityResponseThree
        );
        this.WhenExists(warehouseTwo);
        this.WhenExists(warehouseThree);

        var parameter = new GetPartAvailabilityParameter
        {
            GetInventoryParameter = new GetInventoryParameter { GetWarehouses = true }
        };
        var result = new GetPartAvailabilityResult
        {
            PartAvailabilityRequest = partAvailabilityRequest
        };
        result = this.RunExecute(parameter, result);

        Assert.IsTrue(
            result.PartAvailabilityResponse.partsAvailabile.Any(
                o => o.partsAvailableSite == warehouseOne.Name && o.quantityAvailable == 41
            )
        );
        Assert.IsTrue(
            result.PartAvailabilityResponse.partsAvailabile.Any(
                o => o.partsAvailableSite == warehouseTwo.Name && o.quantityAvailable == 12
            )
        );
        Assert.IsTrue(
            result.PartAvailabilityResponse.partsAvailabile.Any(
                o => o.partsAvailableSite == warehouseThree.Name && o.quantityAvailable == 88
            )
        );
    }

    protected void WhenGetPartAvailabilityIs(
        partAvailabilityRequest partAvailabilityRequest,
        string warehouse,
        partAvailabilityResponse partAvailabilityResponse
    )
    {
        this.ifsApiService
            .Setup(
                o =>
                    o.GetPartAvailability(
                        It.Is<partAvailabilityRequest>(
                            p =>
                                p.addressId == partAvailabilityRequest.addressId
                                && p.customerNo == partAvailabilityRequest.customerNo
                                && p.custOwnAddressId == partAvailabilityRequest.custOwnAddressId
                                && p.partsAvailabile == partAvailabilityRequest.partsAvailabile
                                && p.site == warehouse
                        )
                    )
            )
            .Returns(partAvailabilityResponse);
    }

    protected void VerifyGetPartAvailabilityWasCalled(
        partAvailabilityRequest partAvailabilityRequest,
        string warehouse
    )
    {
        this.ifsApiService.Verify(
            o =>
                o.GetPartAvailability(
                    It.Is<partAvailabilityRequest>(
                        p =>
                            p.addressId == partAvailabilityRequest.addressId
                            && p.customerNo == partAvailabilityRequest.customerNo
                            && p.custOwnAddressId == partAvailabilityRequest.custOwnAddressId
                            && p.partsAvailabile == partAvailabilityRequest.partsAvailabile
                            && p.site == warehouse
                    )
                ),
            Times.Once
        );
    }

    protected void WhenExists(WarehouseDto warehouseDto)
    {
        this.warehousesDtos.Add(warehouseDto);
    }
}
