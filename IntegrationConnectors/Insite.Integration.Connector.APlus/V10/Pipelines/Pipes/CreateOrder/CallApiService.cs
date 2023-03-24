namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;

using System;
using System.IO;
using System.Xml.Serialization;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.Integration.Connector.APlus.Services;

public sealed class CallApiService : IPipe<CreateOrderParameter, CreateOrderResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 600;

    public CreateOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateOrderParameter parameter,
        CreateOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");
        parameter.JobLogger?.Debug(
            $"CreateOrderRequest: {this.GetSerializedValue(result.CreateOrderRequest)}"
        );

        try
        {
            result.CreateOrderResponse = this.dependencyLocator
                .GetInstance<IAPlusApiServiceFactory>()
                .GetAPlusApiService(parameter.IntegrationConnection)
                .CreateOrder(result.CreateOrderRequest);
        }
        catch (Exception exception)
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages.Add(new ResultMessage { Message = exception.Message });

            parameter.JobLogger?.Error(exception.Message);
        }

        if (result.CreateOrderResponse != null)
        {
            parameter.JobLogger?.Debug(
                $"CreateOrderResponse: {this.GetSerializedValue(result.CreateOrderResponse)}"
            );
        }

        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Finished.");

        return result;
    }

    private string GetSerializedValue(object value)
    {
        var serializer = new XmlSerializer(value.GetType());
        var writer = new StringWriter();

        serializer.Serialize(writer, value);

        return writer.ToString();
    }
}
