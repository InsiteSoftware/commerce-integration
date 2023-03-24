namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;

using System;
using System.Collections.Generic;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Acumatica.Services;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Newtonsoft.Json;

public sealed class CallApiService : IPipe<SalesOrderParameter, SalesOrderResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 800;

    public SalesOrderResult Execute(
        IUnitOfWork unitOfWork,
        SalesOrderParameter parameter,
        SalesOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");
        parameter.JobLogger?.Debug($"{nameof(SalesOrder)}: {Serialize(result.SalesOrderRequest)}");

        var acumaticaApiService = this.dependencyLocator
            .GetInstance<IAcumaticaApiServiceFactory>()
            .GetAcumaticaApiService(parameter.IntegrationConnection);

        acumaticaApiService.Login();

        try
        {
            result.SalesOrderResponse = acumaticaApiService.SalesOrder(result.SalesOrderRequest);
        }
        catch (Exception exception)
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = new List<ResultMessage>
            {
                new ResultMessage { Message = exception.Message }
            };
        }

        acumaticaApiService.Logout();

        parameter.JobLogger?.Debug($"{nameof(SalesOrder)}: {Serialize(result.SalesOrderResponse)}");
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Finished.");

        return result;
    }

    private static string Serialize<T>(T request)
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(request, settings);
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"Error serializing {nameof(T)} to Json request. Message: {exception.Message}."
            );
        }
    }
}
