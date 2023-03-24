namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;

using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

public sealed class CallApiService : IPipe<OrderImportParameter, OrderImportResult>
{
    private readonly IProphet21ApiService prophet21ApiService;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.prophet21ApiService = dependencyLocator.GetInstance<IProphet21ApiService>();
    }

    public int Order => 900;

    public OrderImportResult Execute(
        IUnitOfWork unitOfWork,
        OrderImportParameter parameter,
        OrderImportResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");
        parameter.JobLogger?.Debug(
            $"OrderImportRequest: {this.GetSerializedValue(result.OrderImportRequest)}"
        );

        result.OrderImportReply = this.prophet21ApiService.OrderImport(
            parameter.IntegrationConnection,
            result.OrderImportRequest
        );

        if (result.OrderImportReply?.ReplyStatus?.Result != 0)
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = new List<ResultMessage>
            {
                new ResultMessage { Message = result.OrderImportReply?.ReplyStatus?.Message }
            };
        }

        if (result.OrderImportReply != null)
        {
            parameter.JobLogger?.Debug(
                $"OrderImportReply: {this.GetSerializedValue(result.OrderImportReply)}"
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
