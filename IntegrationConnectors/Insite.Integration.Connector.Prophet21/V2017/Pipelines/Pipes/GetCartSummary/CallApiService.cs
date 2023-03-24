namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;

using System.Collections.Generic;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

public sealed class CallApiService : IPipe<GetCartSummaryParameter, GetCartSummaryResult>
{
    private readonly IProphet21ApiService prophet21ApiService;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.prophet21ApiService = dependencyLocator.GetInstance<IProphet21ApiService>();
    }

    public int Order => 700;

    public GetCartSummaryResult Execute(
        IUnitOfWork unitOfWork,
        GetCartSummaryParameter parameter,
        GetCartSummaryResult result
    )
    {
        result.GetCartSummaryReply = this.prophet21ApiService.GetCartSummary(
            parameter.IntegrationConnection,
            result.GetCartSummaryRequest
        );

        if (result.GetCartSummaryReply?.ReplyStatus?.Result != 0)
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = new List<ResultMessage>
            {
                new ResultMessage { Message = result.GetCartSummaryReply?.ReplyStatus?.Message }
            };
        }

        return result;
    }
}
