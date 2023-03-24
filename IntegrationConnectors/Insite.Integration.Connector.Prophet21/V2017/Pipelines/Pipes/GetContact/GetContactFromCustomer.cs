namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetContact;

using System.Collections.Generic;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class GetContactFromCustomer : IPipe<GetContactParameter, GetContactResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public GetContactFromCustomer(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 100;

    public GetContactResult Execute(
        IUnitOfWork unitOfWork,
        GetContactParameter parameter,
        GetContactResult result
    )
    {
        if (
            this.integrationConnectorSettings.Prophet21OrderSubmitContactTreatment
            != Prophet21OrderSubmitContactTreatment.ContactFromCustomer
        )
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(GetContactFromCustomer)} Started.");

        result.ContactId = parameter.CustomerOrder?.Customer?.GetProperty("ContactId", null);

        if (string.IsNullOrEmpty(result.ContactId))
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = new List<ResultMessage>
            {
                new ResultMessage
                {
                    Message =
                        $"Customer ContactId not found. "
                        + $"Customer: {parameter.CustomerOrder?.Customer?.ErpNumber}."
                }
            };
        }

        parameter.JobLogger?.Debug($"{nameof(GetContactFromCustomer)} Finished.");

        return result;
    }
}
