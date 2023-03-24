namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;

using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

public sealed class ValidateResponse
    : IPipe<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
{
    public int Order => 500;

    public GetPricingAndInventoryStockResult Execute(
        IUnitOfWork unitOfWork,
        GetPricingAndInventoryStockParameter parameter,
        GetPricingAndInventoryStockResult result
    )
    {
        var errorMessages = GetErrorMessages(result.OePricingMultipleV4Response);
        if (errorMessages.Any())
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = errorMessages;
        }

        return result;
    }

    private static ICollection<ResultMessage> GetErrorMessages(
        OePricingMultipleV4Response oePricingMultipleV4Response
    )
    {
        var resultMessages = new List<ResultMessage>();

        if (!string.IsNullOrEmpty(oePricingMultipleV4Response?.Response?.ErrorMessage))
        {
            resultMessages.Add(
                new ResultMessage { Message = oePricingMultipleV4Response.Response.ErrorMessage }
            );
        }

        oePricingMultipleV4Response.Response
            ?.PriceOutV2Collection?.PriceOutV2s?.Where(o => !string.IsNullOrEmpty(o.ErrorMess))
            .ToList()
            .ForEach(o => resultMessages.Add(new ResultMessage { Message = o.ErrorMess }));

        return resultMessages;
    }
}
