namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class SubmitOrderLines : IPipe<SubmitOrderParameter, SubmitOrderResult>
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public SubmitOrderLines(IPipeAssemblyFactory pipeAssemblyFactory)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public int Order => 600;

    public SubmitOrderResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderParameter parameter,
        SubmitOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(SubmitOrderLines)} Started.");

        var submitOrderLinesParameter = new SubmitOrderLinesParameter
        {
            ErpOrderNumber = result.ErpOrderNumber,
            CustomerOrder = parameter.CustomerOrder,
            IntegrationConnection = parameter.IntegrationConnection,
            JobLogger = parameter.JobLogger
        };

        var submitOrderLinesResult = this.pipeAssemblyFactory.ExecutePipeline(
            submitOrderLinesParameter,
            new SubmitOrderLinesResult()
        );
        if (submitOrderLinesResult.ResultCode != ResultCode.Success)
        {
            return PipelineHelper.CopyResult(result, submitOrderLinesResult);
        }

        parameter.JobLogger?.Debug($"{nameof(SubmitOrderLines)} Finished.");

        return result;
    }
}
