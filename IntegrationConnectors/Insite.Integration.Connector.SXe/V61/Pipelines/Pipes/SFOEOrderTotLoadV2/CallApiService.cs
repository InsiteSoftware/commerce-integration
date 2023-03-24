namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System.IO;
using System.Xml.Serialization;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

public sealed class CallApiService : IPipe<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 900;

    public SFOEOrderTotLoadV2Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV2Parameter parameter,
        SFOEOrderTotLoadV2Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");
        parameter.JobLogger?.Debug(
            $"{nameof(SFOEOrderTotLoadV2Request)}: {this.GetSerializedValue(result.SFOEOrderTotLoadV2Request)}"
        );

        result.SFOEOrderTotLoadV2Response = this.dependencyLocator
            .GetInstance<ISXeApiServiceFactory>()
            .GetSXeApiServiceV61(parameter.IntegrationConnection)
            .SFOEOrderTotLoadV2(result.SFOEOrderTotLoadV2Request);

        if (!string.IsNullOrEmpty(result.SFOEOrderTotLoadV2Response?.errorMessage))
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages.Add(
                new ResultMessage { Message = result.SFOEOrderTotLoadV2Response.errorMessage }
            );
        }

        parameter.JobLogger?.Debug(
            $"{nameof(SFOEOrderTotLoadV2Response)}: {this.GetSerializedValue(result.SFOEOrderTotLoadV2Response)}"
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
