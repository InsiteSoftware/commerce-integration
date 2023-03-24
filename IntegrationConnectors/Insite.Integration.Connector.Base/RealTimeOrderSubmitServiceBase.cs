namespace Insite.Integration.Connector.Base;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Cart;
using Insite.Core.Plugins.Integration;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Data.Entities;
using Insite.Integration.WebService.Interfaces;
using Insite.Integration.WebService.SystemSettings;

public class RealTimeOrderSubmitServiceBase
{
    private const string CustomerOrderPropertyName = "CustomerOrder";

    private const string IntegrationJobPropertyName = "IntegrationJob";

    private const string IntegrationConnectionPropertyName = "IntegrationConnection";

    private const string JobLoggerPropertyName = "JobLogger";

    private const string IsOrderSubmitPropertyName = "IsOrderSubmit";

    private const string ErpOrderNumberPropertyName = "ErpOrderNumber";

    private const string CustomerOrderNotFoundMessage =
        "Customer order not found. Order number: {0}.";

    private const string ErrorSchedulingIntegrationJobMessage =
        "Failure scheduling integration job. Job definition name: {0}. Order number: {1}.";

    private readonly IUnitOfWorkFactory unitOfWorkFactory;

    private readonly IIntegrationJobSchedulingService integrationJobSchedulingService;

    private readonly IWebServiceHandler webServiceHandler;

    private readonly IJobLoggerFactory jobLoggerFactory;

    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    private readonly IntegrationGeneralSettings integrationGeneralSettings;

    public RealTimeOrderSubmitServiceBase(
        IUnitOfWorkFactory unitOfWorkFactory,
        IIntegrationJobSchedulingService integrationJobSchedulingService,
        IWebServiceHandler webServiceHandler,
        IJobLoggerFactory jobLoggerFactory,
        IPipeAssemblyFactory pipeAssemblyFactory,
        IntegrationGeneralSettings integrationGeneralSettings
    )
    {
        this.unitOfWorkFactory = unitOfWorkFactory;
        this.integrationJobSchedulingService = integrationJobSchedulingService;
        this.webServiceHandler = webServiceHandler;
        this.jobLoggerFactory = jobLoggerFactory;
        this.pipeAssemblyFactory = pipeAssemblyFactory;
        this.integrationGeneralSettings = integrationGeneralSettings;
    }

    protected SubmitRealTimeOrderResult SubmitRealTimeOrder<TParameter, TResult>(
        SubmitRealTimeOrderParameter submitRealTimeOrderParameter
    )
        where TParameter : PipeParameterBase
        where TResult : PipeResultBase
    {
        var customerOrder = this.GetCustomerOrder(submitRealTimeOrderParameter.OrderNumber);
        var integrationJob = this.GetIntegrationJob(
            submitRealTimeOrderParameter.JobDefinitionName,
            submitRealTimeOrderParameter.OrderNumber
        );
        var jobLogger = this.jobLoggerFactory.GetJobLogger(
            integrationJob.Id,
            this.unitOfWorkFactory,
            this.integrationGeneralSettings
        );

        this.webServiceHandler.StartIntegrationJob(integrationJob.Id);

        var parameter = this.CreateParameter<TParameter>(customerOrder, integrationJob, jobLogger);
        var result = this.pipeAssemblyFactory.ExecutePipeline(
            parameter,
            Activator.CreateInstance<TResult>()
        );

        if (result.ResultCode == ResultCode.Success)
        {
            var erpOrderNumber = GetProperty<TResult, string>(result, ErpOrderNumberPropertyName);

            this.UpdateCustomerOrder(customerOrder, erpOrderNumber);
            this.UpdateOrderHistory(customerOrder, erpOrderNumber);
        }
        else
        {
            // must be fatal so web service handler sets correct integration job failed status
            jobLogger.Fatal(result.Message);
        }

        this.webServiceHandler.FinishIntegrationJob(integrationJob.Id);

        return new SubmitRealTimeOrderResult
        {
            ResultCode = result.ResultCode,
            SubCode = result.SubCode,
            Messages = result.Messages
        };
    }

    protected virtual IntegrationConnection GetIntegrationConnection()
    {
        return null;
    }

    private TParameter CreateParameter<TParameter>(
        CustomerOrder customerOrder,
        IntegrationJob integrationJob,
        IJobLogger jobLogger
    )
        where TParameter : PipeParameterBase
    {
        var parameter = Activator.CreateInstance<TParameter>();

        SetProperty<TParameter>(parameter, CustomerOrderPropertyName, customerOrder);
        SetProperty<TParameter>(parameter, IntegrationJobPropertyName, integrationJob);
        SetProperty<TParameter>(
            parameter,
            IntegrationConnectionPropertyName,
            this.GetIntegrationConnection()
        );
        SetProperty<TParameter>(parameter, JobLoggerPropertyName, jobLogger);
        SetProperty<TParameter>(parameter, IsOrderSubmitPropertyName, true);

        return parameter;
    }

    private static TOut GetProperty<TIn, TOut>(object source, string propertyName)
        where TIn : class
        where TOut : class
    {
        var propertyInfo = typeof(TIn)
            .GetProperties()
            .FirstOrDefault(o => o.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        if (propertyInfo == null)
        {
            return default(TOut);
        }

        return (TOut)propertyInfo.GetValue(source);
    }

    private static void SetProperty<T>(object destination, string propertyName, object value)
        where T : class
    {
        var propertyInfo = typeof(T)
            .GetProperties()
            .FirstOrDefault(o => o.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        if (propertyInfo == null)
        {
            return;
        }

        propertyInfo.SetValue(destination, value);
    }

    private CustomerOrder GetCustomerOrder(string orderNumber)
    {
        var customerOrder = this.unitOfWorkFactory
            .GetUnitOfWork()
            .GetRepository<CustomerOrder>()
            .GetByNaturalKey(orderNumber);
        if (customerOrder == null)
        {
            throw new ArgumentException(string.Format(CustomerOrderNotFoundMessage, orderNumber));
        }

        return customerOrder;
    }

    private IntegrationJob GetIntegrationJob(string jobDefinitionName, string orderNumber)
    {
        var integrationJob = this.integrationJobSchedulingService.ScheduleBatchIntegrationJob(
            jobDefinitionName,
            genericParameter: orderNumber,
            scheduleDateTime: DateTime.MaxValue.AddYears(-1)
        );

        if (integrationJob == null)
        {
            throw new Exception(
                string.Format(ErrorSchedulingIntegrationJobMessage, jobDefinitionName, orderNumber)
            );
        }

        return integrationJob;
    }

    private void UpdateCustomerOrder(CustomerOrder customerOrder, string erpOrderNumber)
    {
        customerOrder.ErpOrderNumber = erpOrderNumber;
        customerOrder.Status = CustomerOrder.StatusType.Processing;
    }

    private void UpdateOrderHistory(CustomerOrder customerOrder, string erpOrderNumber)
    {
        /* UpdateCartHandler.SubmitOrderToErp occurs BEFORE UpdateCartHandler.CreateOrderHistory.
          So if an order is submitted to the erp in realtime and succeeds, an order history will not yet exist here.
          However, this code must remain in the case of a realtime submit failure and a subsequent resubmit. */

        var orderHistory = this.unitOfWorkFactory
            .GetUnitOfWork()
            .GetRepository<OrderHistory>()
            .GetTable()
            .FirstOrDefault(
                o =>
                    o.WebOrderNumber.Equals(
                        customerOrder.OrderNumber,
                        StringComparison.OrdinalIgnoreCase
                    )
            );

        if (orderHistory != null)
        {
            orderHistory.ErpOrderNumber = erpOrderNumber;
        }
    }
}
