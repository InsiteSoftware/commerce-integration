namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class SubmitCreditCardDetails : IPipe<SubmitOrderParameter, SubmitOrderResult>
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public SubmitCreditCardDetails(IPipeAssemblyFactory pipeAssemblyFactory)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public int Order => 800;

    public SubmitOrderResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderParameter parameter,
        SubmitOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(SubmitCreditCardDetails)} Started.");

        var submitCreditCardDetailsParameter = new SubmitCreditCardDetailsParameter
        {
            ErpOrderNumber = result.ErpOrderNumber,
            CustomerOrder = parameter.CustomerOrder,
            IntegrationConnection = parameter.IntegrationConnection,
            JobLogger = parameter.JobLogger
        };

        var submitCreditCardDetailsResult = this.pipeAssemblyFactory.ExecutePipeline(
            submitCreditCardDetailsParameter,
            new SubmitCreditCardDetailsResult()
        );
        if (submitCreditCardDetailsResult.ResultCode != ResultCode.Success)
        {
            return PipelineHelper.CopyResult(result, submitCreditCardDetailsResult);
        }

        parameter.JobLogger?.Debug($"{nameof(SubmitCreditCardDetails)} Finished.");

        return result;
    }
}
