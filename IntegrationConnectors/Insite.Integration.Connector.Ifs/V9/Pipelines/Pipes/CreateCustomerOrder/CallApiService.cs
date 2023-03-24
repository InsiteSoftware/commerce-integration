namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;

using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.Services;

public sealed class CallApiService : IPipe<CreateCustomerOrderParameter, CreateCustomerOrderResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 700;

    public CreateCustomerOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateCustomerOrderParameter parameter,
        CreateCustomerOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");
        parameter.JobLogger?.Debug(
            $"{nameof(customerOrder)}: {this.GetSerializedValue(result.CustomerOrder)}"
        );

        result.OrderResponse = this.dependencyLocator
            .GetInstance<IIfsApiServiceFactory>()
            .GetIfsApiService(parameter.IntegrationConnection)
            .CreateCustomerOrder(result.CustomerOrder);

        if (!string.IsNullOrEmpty(result.OrderResponse?.errorMessage))
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = new List<ResultMessage>
            {
                new ResultMessage { Message = result.OrderResponse.errorMessage }
            };
        }

        parameter.JobLogger?.Debug(
            $"{nameof(orderResponse)}: {this.GetSerializedValue(result.OrderResponse)}"
        );
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
