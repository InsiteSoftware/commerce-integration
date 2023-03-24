namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;

using System.Collections.Generic;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.Services;

public sealed class CallApiService : IPipe<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 600;

    public GetCustomerPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetCustomerPriceParameter parameter,
        GetCustomerPriceResult result
    )
    {
        result.CustomerPriceResponse = this.dependencyLocator
            .GetInstance<IIfsApiServiceFactory>()
            .GetIfsApiService(parameter.IntegrationConnection)
            .GetCustomerPrice(result.CustomerPriceRequest);

        if (!string.IsNullOrEmpty(result.CustomerPriceResponse?.errorText))
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = new List<ResultMessage>
            {
                new ResultMessage { Message = result.CustomerPriceResponse.errorText }
            };
        }

        return result;
    }
}
