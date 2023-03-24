namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.ARCustomerMntRequest;

using System;
using System.IO;
using System.Xml.Serialization;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.ARCustomerMnt;

public sealed class CallApiService : IPipe<ARCustomerMntParameter, ARCustomerMntResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 300;

    public ARCustomerMntResult Execute(
        IUnitOfWork unitOfWork,
        ARCustomerMntParameter parameter,
        ARCustomerMntResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");
        parameter.JobLogger?.Debug(
            $"{nameof(ARCustomerMntRequest)}: {this.GetSerializedValue(result.ARCustomerMntRequest)}"
        );

        result.ARCustomerMntResponse = this.dependencyLocator
            .GetInstance<ISXeApiServiceFactory>()
            .GetSXeApiServiceV61(parameter.IntegrationConnection)
            .ARCustomerMnt(result.ARCustomerMntRequest);

        var returnDataSuccessful =
            !string.IsNullOrEmpty(result.ARCustomerMntResponse.returnData)
            && result.ARCustomerMntResponse.returnData.ContainsIgnoreCase("Update Successful");

        // ARCustomerMntResponse may successfuly create customers but still return an error message
        if (
            !string.IsNullOrEmpty(result.ARCustomerMntResponse.errorMessage)
            && !returnDataSuccessful
        )
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages.Add(
                new ResultMessage { Message = result.ARCustomerMntResponse.errorMessage }
            );
        }

        parameter.JobLogger?.Debug(
            $"{nameof(ARCustomerMntResponse)}: {this.GetSerializedValue(result.ARCustomerMntResponse)}"
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
