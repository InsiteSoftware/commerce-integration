namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;

using System.Collections.Generic;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

public sealed class CallApiService : IPipe<GetItemPriceParameter, GetItemPriceResult>
{
    private readonly IProphet21ApiService prophet21ApiService;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.prophet21ApiService = dependencyLocator.GetInstance<IProphet21ApiService>();
    }

    public int Order => 700;

    public GetItemPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetItemPriceParameter parameter,
        GetItemPriceResult result
    )
    {
        result.GetItemPriceReply = this.prophet21ApiService.GetItemPrice(
            parameter.IntegrationConnection,
            result.GetItemPriceRequest
        );

        if (result.GetItemPriceReply?.ReplyStatus?.Result != 0)
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = new List<ResultMessage>
            {
                new ResultMessage { Message = result.GetItemPriceReply?.ReplyStatus?.Message }
            };
        }

        return result;
    }
}
