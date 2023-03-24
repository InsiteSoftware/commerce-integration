namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetContact;

using System.Collections.Generic;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class GetContactFromUser : IPipe<GetContactParameter, GetContactResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public GetContactFromUser(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 200;

    public GetContactResult Execute(
        IUnitOfWork unitOfWork,
        GetContactParameter parameter,
        GetContactResult result
    )
    {
        if (
            this.integrationConnectorSettings.Prophet21OrderSubmitContactTreatment
            != Prophet21OrderSubmitContactTreatment.ContactFromUser
        )
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(GetContactFromUser)} Started.");

        result.ContactId = parameter.CustomerOrder?.PlacedByUserProfile?.GetProperty(
            "ContactId",
            null
        );

        if (string.IsNullOrEmpty(result.ContactId))
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = new List<ResultMessage>
            {
                new ResultMessage
                {
                    Message =
                        $"User ContactId not found. "
                        + $"UserName: {parameter.CustomerOrder?.PlacedByUserProfile?.UserName}. "
                }
            };
        }

        parameter.JobLogger?.Debug($"{nameof(GetContactFromUser)} Finished.");

        return result;
    }
}
