namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.OEPricingMultipleV4;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.OEPricingMultipleV4;

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
            .GetSXeApiServiceV10(parameter.IntegrationConnection)
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

        if (!string.IsNullOrEmpty(oePricingMultipleV4Response?.ErrorMessage))
        {
            resultMessages.Add(
                new ResultMessage { Message = oePricingMultipleV4Response.ErrorMessage }
            );
        }

        if (oePricingMultipleV4Response.Outprice == null)
        {
            return resultMessages;
        }

        foreach (var outPrice3 in oePricingMultipleV4Response.Outprice)
        {
            if (!string.IsNullOrEmpty(outPrice3.ErrorMessage))
            {
                resultMessages.Add(new ResultMessage { Message = outPrice3.ErrorMessage });
            }
        }

        return resultMessages;
    }

    private OEPricingMultipleV4Response SimulateOEPricingMultipleV4(
        OEPricingMultipleV4Request oePricingMultipleV4Request
    )
    {
        var random = new Random();

        var outprice3s = oePricingMultipleV4Request.Inproduct
            .Select(
                o =>
                    new Outprice3
                    {
                        ProductCode = o.ProductCode,
                        UnitOfMeasure = o.UnitOfMeasure,
                        Quantity = o.Quantity,
                        Price = random.Next(1, 1000),
                        NetAvailable = random.Next(16, 100)
                    }
            )
            .ToList();

        return new OEPricingMultipleV4Response { Outprice = outprice3s };
    }
}
