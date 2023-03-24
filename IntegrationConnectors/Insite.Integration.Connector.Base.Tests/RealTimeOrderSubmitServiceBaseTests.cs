#pragma warning disable CA1001
namespace Insite.Integration.Connector.Base.Tests;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Cart;
using Insite.Core.Plugins.Integration;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.TestHelpers;
using Insite.Core.TestHelpers.Builders;
using Insite.Data.Entities;
using Insite.Integration.WebService.Interfaces;
using Insite.Integration.WebService.SystemSettings;

[TestFixture]
public class RealTimeOrderSubmitServiceBaseTests
{
    private TestableRealTimeOrderSubmitServiceBase realTimeOrderSubmitServiceBase;

    private Mock<IIntegrationJobSchedulingService> integrationJobSchedulingService;

    private Mock<IWebServiceHandler> webServiceHandler;

    private Mock<IJobLogger> jobLogger;

    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    private Mock<IntegrationGeneralSettings> integrationGeneralSettings;

    private FakeUnitOfWork fakeUnitOfWork;

    private IList<CustomerOrder> customerOrders;

    private IList<OrderHistory> orderHistories;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.integrationJobSchedulingService =
            container.GetMock<IIntegrationJobSchedulingService>();

        this.webServiceHandler = container.GetMock<IWebServiceHandler>();

        this.jobLogger = container.GetMock<IJobLogger>();
        var jobLoggerFactory = container.GetMock<IJobLoggerFactory>();
        jobLoggerFactory
            .Setup(
                o =>
                    o.GetJobLogger(
                        It.IsAny<Guid>(),
                        It.IsAny<IUnitOfWorkFactory>(),
                        It.IsAny<IntegrationGeneralSettings>()
                    )
            )
            .Returns(this.jobLogger.Object);

        this.pipeAssemblyFactory = container.GetMock<IPipeAssemblyFactory>();

        this.integrationGeneralSettings = container.GetMock<IntegrationGeneralSettings>();

        var unitOfWork = container.GetMock<IUnitOfWork>();
        this.fakeUnitOfWork = new FakeUnitOfWork(unitOfWork.Object);
        var unitOfWorkFactory = container.GetMock<IUnitOfWorkFactory>();

        unitOfWorkFactory.Setup(o => o.GetUnitOfWork()).Returns(this.fakeUnitOfWork);

        this.customerOrders = new List<CustomerOrder>();
        this.fakeUnitOfWork.SetupEntityList(this.customerOrders);

        this.orderHistories = new List<OrderHistory>();
        this.fakeUnitOfWork.SetupEntityList(this.orderHistories);

        this.realTimeOrderSubmitServiceBase =
            container.Resolve<TestableRealTimeOrderSubmitServiceBase>();
    }

    [Test]
    public void SubmitRealTimeOrder_Should_Throw_Exception_When_Customer_Order_Not_Found()
    {
        Assert.Throws<ArgumentException>(
            () =>
                this.realTimeOrderSubmitServiceBase.CallSubmitRealTimeOrder<
                    TestablePipeParameterBase,
                    TestablePipeResultBase
                >(this.SubmitRealTimeOrderParameter)
        );
    }

    [Test]
    public void SubmitRealTimeOrder_Should_Throw_Exception_When_Integration_Job_Scheduling_Service_Returns_Null_Integration_Job()
    {
        this.WhenExists(this.customerOrder);

        Assert.Throws<Exception>(
            () =>
                this.realTimeOrderSubmitServiceBase.CallSubmitRealTimeOrder<
                    TestablePipeParameterBase,
                    TestablePipeResultBase
                >(this.SubmitRealTimeOrderParameter)
        );
    }

    [Test]
    public void SubmitRealTimeOrder_Should_Call_Start_Integration_Job()
    {
        this.WhenExists(this.customerOrder);
        this.WhenScheduleBatchIntegrationJobIs(
            JobDefinitionName,
            this.customerOrder.OrderNumber,
            this.integrationJob
        );
        this.WhenExecutePipelineIs(new TestablePipeResultBase());

        this.realTimeOrderSubmitServiceBase.CallSubmitRealTimeOrder<
            TestablePipeParameterBase,
            TestablePipeResultBase
        >(this.SubmitRealTimeOrderParameter);

        this.VerifyStartIntegrationJobWasCalled(this.integrationJob.Id);
    }

    [Test]
    public void SubmitRealTimeOrder_Should_Execute_Pipeline()
    {
        this.WhenExists(this.customerOrder);
        this.WhenScheduleBatchIntegrationJobIs(
            JobDefinitionName,
            this.customerOrder.OrderNumber,
            this.integrationJob
        );
        this.WhenExecutePipelineIs(new TestablePipeResultBase());

        this.realTimeOrderSubmitServiceBase.CallSubmitRealTimeOrder<
            TestablePipeParameterBase,
            TestablePipeResultBase
        >(this.SubmitRealTimeOrderParameter);

        this.VerifyExecutePipelineWasCalled(
            this.customerOrder,
            this.integrationJob,
            this.jobLogger.Object
        );
    }

    [Test]
    public void SubmitRealTimeOrder_Should_Update_Customer_Order()
    {
        const string ErpOrderNumber = "Erp-123";

        this.WhenExists(this.customerOrder);
        this.WhenScheduleBatchIntegrationJobIs(
            JobDefinitionName,
            this.customerOrder.OrderNumber,
            this.integrationJob
        );
        this.WhenExecutePipelineIs(new TestablePipeResultBase { ErpOrderNumber = ErpOrderNumber });

        this.realTimeOrderSubmitServiceBase.CallSubmitRealTimeOrder<
            TestablePipeParameterBase,
            TestablePipeResultBase
        >(this.SubmitRealTimeOrderParameter);

        Assert.AreEqual(CustomerOrder.StatusType.Processing, this.customerOrder.Status);
        Assert.AreEqual(ErpOrderNumber, this.customerOrder.ErpOrderNumber);
    }

    [Test]
    public void SubmitRealTimeOrder_Should_Update_Order_History()
    {
        const string ErpOrderNumber = "Erp-123";

        this.WhenExists(this.customerOrder);
        this.WhenExists(this.orderHistory);
        this.WhenScheduleBatchIntegrationJobIs(
            JobDefinitionName,
            this.customerOrder.OrderNumber,
            this.integrationJob
        );
        this.WhenExecutePipelineIs(new TestablePipeResultBase { ErpOrderNumber = ErpOrderNumber });

        this.realTimeOrderSubmitServiceBase.CallSubmitRealTimeOrder<
            TestablePipeParameterBase,
            TestablePipeResultBase
        >(this.SubmitRealTimeOrderParameter);

        Assert.AreEqual(ErpOrderNumber, this.orderHistory.ErpOrderNumber);
    }

    [Test]
    public void SubmitRealTimeOrder_Should_Log_Fatal_Error_When_Execute_Pipeline_Returns_Error()
    {
        var getOEFullOrderMntV6Result = new TestablePipeResultBase
        {
            ResultCode = ResultCode.Error,
            Messages = new List<ResultMessage> { new ResultMessage { Message = "Test" } }
        };

        this.WhenExists(this.customerOrder);
        this.WhenScheduleBatchIntegrationJobIs(
            JobDefinitionName,
            this.customerOrder.OrderNumber,
            this.integrationJob
        );
        this.WhenExecutePipelineIs(getOEFullOrderMntV6Result);

        this.realTimeOrderSubmitServiceBase.CallSubmitRealTimeOrder<
            TestablePipeParameterBase,
            TestablePipeResultBase
        >(this.SubmitRealTimeOrderParameter);

        this.VerifyFatalErrorWasLogged("Test");
        this.VerifyFinishIntegrationJobWasCalled(this.integrationJob.Id);
    }

    [Test]
    public void SubmitRealTimeOrder_Should_Call_Finish_Integration_Job()
    {
        this.WhenExists(this.customerOrder);
        this.WhenScheduleBatchIntegrationJobIs(
            JobDefinitionName,
            this.customerOrder.OrderNumber,
            this.integrationJob
        );
        this.WhenExecutePipelineIs(new TestablePipeResultBase());

        this.realTimeOrderSubmitServiceBase.CallSubmitRealTimeOrder<
            TestablePipeParameterBase,
            TestablePipeResultBase
        >(this.SubmitRealTimeOrderParameter);

        this.VerifyFinishIntegrationJobWasCalled(this.integrationJob.Id);
    }

    private const string JobDefinitionName = "Test Job Definition Name";

    private CustomerOrder customerOrder = Some.CustomerOrder().WithOrderNumber("12345").Build();

    private OrderHistory orderHistory = Some.OrderHistory().WithWebOrderNumber("12345").Build();

    private IntegrationJob integrationJob = Some.IntegrationJob().Build();

    private SubmitRealTimeOrderParameter SubmitRealTimeOrderParameter =>
        new SubmitRealTimeOrderParameter
        {
            JobDefinitionName = JobDefinitionName,
            OrderNumber = this.customerOrder.OrderNumber
        };

    private void WhenExists(CustomerOrder customerOrder)
    {
        this.customerOrders.Add(customerOrder);
    }

    private void WhenExists(OrderHistory orderHistory)
    {
        this.orderHistories.Add(orderHistory);
    }

    private void WhenScheduleBatchIntegrationJobIs(
        string jobDefinitionName,
        string genericParameter,
        IntegrationJob integrationJob
    )
    {
        this.integrationJobSchedulingService
            .Setup(
                o =>
                    o.ScheduleBatchIntegrationJob(
                        jobDefinitionName,
                        It.IsAny<DataSet>(),
                        It.IsAny<Collection<JobDefinitionStepParameter>>(),
                        genericParameter,
                        It.IsAny<DateTime?>(),
                        It.IsAny<bool>()
                    )
            )
            .Returns(integrationJob);
    }

    private void WhenExecutePipelineIs(TestablePipeResultBase pipeResultBase)
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.IsAny<TestablePipeParameterBase>(),
                        It.IsAny<TestablePipeResultBase>()
                    )
            )
            .Returns(pipeResultBase);
    }

    private void VerifyExecutePipelineWasCalled(
        CustomerOrder customerOrder,
        IntegrationJob integrationJob,
        IJobLogger jobLogger
    )
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<TestablePipeParameterBase>(
                        p =>
                            p.CustomerOrder == customerOrder
                            && p.IntegrationJob == integrationJob
                            && p.JobLogger == jobLogger
                    ),
                    It.IsAny<TestablePipeResultBase>()
                ),
            Times.Once()
        );
    }

    private void VerifyStartIntegrationJobWasCalled(Guid integrationJobId)
    {
        this.webServiceHandler.Verify(o => o.StartIntegrationJob(integrationJobId), Times.Once);
    }

    private void VerifyFinishIntegrationJobWasCalled(Guid integrationJobId)
    {
        this.webServiceHandler.Verify(o => o.FinishIntegrationJob(integrationJobId), Times.Once);
    }

    private void VerifyFatalErrorWasLogged(string errorMessage)
    {
        this.jobLogger.Verify(o => o.Fatal(errorMessage), Times.Once);
    }

    public class TestablePipeParameterBase : PipeParameterBase
    {
        public CustomerOrder CustomerOrder { get; set; }

        public IntegrationJob IntegrationJob { get; set; }

        public IJobLogger JobLogger { get; set; }
    }

    public class TestablePipeResultBase : PipeResultBase
    {
        public string ErpOrderNumber { get; set; }
    }

    public class TestableRealTimeOrderSubmitServiceBase : RealTimeOrderSubmitServiceBase
    {
        public TestableRealTimeOrderSubmitServiceBase(
            IUnitOfWorkFactory unitOfWorkFactory,
            IIntegrationJobSchedulingService integrationJobSchedulingService,
            IWebServiceHandler webServiceHandler,
            IJobLoggerFactory jobLoggerFactory,
            IPipeAssemblyFactory pipeAssemblyFactory,
            IntegrationGeneralSettings integrationGeneralSettings
        )
            : base(
                unitOfWorkFactory,
                integrationJobSchedulingService,
                webServiceHandler,
                jobLoggerFactory,
                pipeAssemblyFactory,
                integrationGeneralSettings
            ) { }

        public SubmitRealTimeOrderResult CallSubmitRealTimeOrder<TParameter, TResult>(
            SubmitRealTimeOrderParameter submitRealTimeOrderParameter
        )
            where TParameter : PipeParameterBase
            where TResult : PipeResultBase
        {
            return this.SubmitRealTimeOrder<TParameter, TResult>(submitRealTimeOrderParameter);
        }
    }
}
