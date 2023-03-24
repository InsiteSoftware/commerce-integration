namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;

using System.Collections.Generic;
using System.Linq;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;

public sealed class CallApiService : IPipe<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 500;

    public OEPricingMultipleV3Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV3Parameter parameter,
        OEPricingMultipleV3Result result
    )
    {
        result.OEPricingMultipleV3Response = this.dependencyLocator
            .GetInstance<ISXeApiServiceFactory>()
            .GetSXeApiServiceV61(parameter.IntegrationConnection)
            .OEPricingMultipleV3(result.OEPricingMultipleV3Request);

        var errorResultMessages = this.GetErrorResultMessages(result.OEPricingMultipleV3Response);
        if (errorResultMessages.Any())
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = errorResultMessages;
        }

        return result;
    }

    private ICollection<ResultMessage> GetErrorResultMessages(
        OEPricingMultipleV3Response oePricingMultipleV3Response
    )
    {
        var resultMessages = new List<ResultMessage>();

        if (!string.IsNullOrEmpty(oePricingMultipleV3Response?.errorMessage))
        {
            resultMessages.Add(
                new ResultMessage { Message = oePricingMultipleV3Response.errorMessage }
            );
        }

        if (oePricingMultipleV3Response.arrayPrice == null)
        {
            return resultMessages;
        }

        foreach (var outPrice3 in oePricingMultipleV3Response.arrayPrice)
        {
            if (!string.IsNullOrEmpty(outPrice3.errorMessage))
            {
                resultMessages.Add(new ResultMessage { Message = outPrice3.errorMessage });
            }
        }

        return resultMessages;
    }
}
