namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.OEPricingMultipleV4;

using System.Collections.Generic;
using System.Linq;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;

public sealed class CallApiService : IPipe<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 500;

    public OEPricingMultipleV4Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Parameter parameter,
        OEPricingMultipleV4Result result
    )
    {
        result.OEPricingMultipleV4Response = this.dependencyLocator
            .GetInstance<ISXeApiServiceFactory>()
            .GetSXeApiServiceV11(parameter.IntegrationConnection)
            .OEPricingMultipleV4(result.OEPricingMultipleV4Request);

        var errorResultMessages = this.GetErrorResultMessages(result.OEPricingMultipleV4Response);
        if (errorResultMessages.Any())
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = errorResultMessages;
        }

        return result;
    }

    private ICollection<ResultMessage> GetErrorResultMessages(
        OEPricingMultipleV4Response oePricingMultipleV4Response
    )
    {
        var resultMessages = new List<ResultMessage>();

        if (!string.IsNullOrEmpty(oePricingMultipleV4Response?.Response?.ErrorMessage))
        {
            resultMessages.Add(
                new ResultMessage { Message = oePricingMultipleV4Response.Response.ErrorMessage }
            );
        }

        if (oePricingMultipleV4Response?.Response?.PriceOutV2Collection == null)
        {
            return resultMessages;
        }

        foreach (
            var priceOutV2 in oePricingMultipleV4Response
                ?.Response
                ?.PriceOutV2Collection
                .PriceOutV2s
        )
        {
            if (!string.IsNullOrEmpty(priceOutV2.Errormess))
            {
                resultMessages.Add(new ResultMessage { Message = priceOutV2.Errormess });
            }
        }

        return resultMessages;
    }
}
