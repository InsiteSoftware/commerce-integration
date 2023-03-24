namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class SubmitOrderCharges : IPipe<SubmitOrderParameter, SubmitOrderResult>
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public SubmitOrderCharges(IPipeAssemblyFactory pipeAssemblyFactory)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public int Order => 700;

    public SubmitOrderResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderParameter parameter,
        SubmitOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(SubmitOrderCharges)} Started.");

        var submitOrderChargesParameter = new SubmitOrderChargesParameter
        {
            ErpOrderNumber = result.ErpOrderNumber,
            CustomerOrder = parameter.CustomerOrder,
            IntegrationConnection = parameter.IntegrationConnection,
            JobLogger = parameter.JobLogger
        };

        var submitOrderChargesResult = this.pipeAssemblyFactory.ExecutePipeline(
            submitOrderChargesParameter,
            new SubmitOrderChargesResult()
        );
        if (submitOrderChargesResult.ResultCode != ResultCode.Success)
        {
            return PipelineHelper.CopyResult(result, submitOrderChargesResult);
        }

        parameter.JobLogger?.Debug($"{nameof(SubmitOrderCharges)} Finished.");

        return result;
    }
}
