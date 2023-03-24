namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetMyAccountOpenAR;

using System.Collections.Generic;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

public sealed class CallApiService : IPipe<GetMyAccountOpenARParameter, GetMyAccountOpenARResult>
{
    private readonly IProphet21ApiService prophet21ApiService;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.prophet21ApiService = dependencyLocator.GetInstance<IProphet21ApiService>();
    }

    public int Order => 400;

    public GetMyAccountOpenARResult Execute(
        IUnitOfWork unitOfWork,
        GetMyAccountOpenARParameter parameter,
        GetMyAccountOpenARResult result
    )
    {
        result.GetMyAccountOpenARReply = this.prophet21ApiService.GetMyAccountOpenAR(
            parameter.IntegrationConnection,
            result.GetMyAccountOpenARRequest
        );

        if (result.GetMyAccountOpenARReply?.ReplyStatus?.Result != 0)
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = new List<ResultMessage>
            {
                new ResultMessage { Message = result.GetMyAccountOpenARReply?.ReplyStatus?.Message }
            };
        }

        return result;
    }
}
