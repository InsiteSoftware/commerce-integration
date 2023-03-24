namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetContact;

using System;
using System.Linq;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

public sealed class GetContactFromApi : IPipe<GetContactParameter, GetContactResult>
{
    private readonly IProphet21ApiService prophet21ApiService;

    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public GetContactFromApi(
        IDependencyLocator dependencyLocator,
        IntegrationConnectorSettings integrationConnectorSettings
    )
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
        this.prophet21ApiService = dependencyLocator.GetInstance<IProphet21ApiService>();
    }

    public int Order => 300;

    public GetContactResult Execute(
        IUnitOfWork unitOfWork,
        GetContactParameter parameter,
        GetContactResult result
    )
    {
        if (
            this.integrationConnectorSettings.Prophet21OrderSubmitContactTreatment
            != Prophet21OrderSubmitContactTreatment.UseApiToLookupAndSubmit
        )
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(GetContactFromApi)} Started.");

        var arrayOfContacts = this.prophet21ApiService.GetContacts(
            parameter.IntegrationConnection,
            parameter.CustomerOrder.PlacedByUserProfile.Email
        );

        foreach (var contactLink in arrayOfContacts?.Contacts?.SelectMany(o => o.ContactLinks))
        {
            if (
                contactLink.ContactLinkId.Equals(
                    parameter.CustomerOrder?.Customer?.ErpNumber,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                result.ContactId = contactLink.ContactId;
                break;
            }
        }

        parameter.JobLogger?.Debug($"{nameof(GetContactFromApi)} Finished.");

        return result;
    }
}
