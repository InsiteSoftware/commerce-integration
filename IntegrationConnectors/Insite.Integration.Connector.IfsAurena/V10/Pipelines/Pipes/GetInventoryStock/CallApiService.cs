namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetInventoryStock;

using Insite.Common.Dependencies;
using Insite.Common.Logging;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class CallApiService : IPipe<GetInventoryStockParameter, GetInventoryStockResult>
{
    private readonly IIfsAurenaClient ifsAurenaClient;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.ifsAurenaClient = dependencyLocator.GetInstance<IIfsAurenaClient>();
    }

    public int Order => 200;

    public GetInventoryStockResult Execute(
        IUnitOfWork unitOfWork,
        GetInventoryStockParameter parameter,
        GetInventoryStockResult result
    )
    {
        LogHelper
            .For(this)
            .Debug($"{nameof(InventoryPartInStock)} Request: {result.InventoryPartInStockRequest}");

        var getInventoryStockResult = this.ifsAurenaClient.GetInventoryStock(
            parameter.IntegrationConnection,
            result.InventoryPartInStockRequest
        );
        if (getInventoryStockResult.resultCode != ResultCode.Success)
        {
            return PipelineHelper.CreateErrorPipelineResult(
                result,
                SubCode.RealTimeInventoryGeneralFailure,
                getInventoryStockResult.response
            );
        }

        result.SerializedInventoryPartInStockResponse = getInventoryStockResult.response;

        LogHelper
            .For(this)
            .Debug(
                $"{nameof(InventoryPartInStock)} Response: {result.SerializedInventoryPartInStockResponse}"
            );

        return result;
    }
}
