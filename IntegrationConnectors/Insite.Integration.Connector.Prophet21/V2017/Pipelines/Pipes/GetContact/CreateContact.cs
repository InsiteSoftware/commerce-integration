namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetContact;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

public sealed class CreateContact : IPipe<GetContactParameter, GetContactResult>
{
    private readonly IProphet21ApiService prophet21ApiService;

    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public CreateContact(
        IDependencyLocator dependencyLocator,
        IntegrationConnectorSettings integrationConnectorSettings
    )
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
        this.prophet21ApiService = dependencyLocator.GetInstance<IProphet21ApiService>();
    }

    public int Order => 400;

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

        // contact was found from api call in previous pipe
        if (!string.IsNullOrEmpty(result.ContactId))
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(CreateContact)} Started.");

        var contactRequest = this.CreateContactRequest(parameter.CustomerOrder);
        var contactResponse = this.prophet21ApiService.CreateContact(
            parameter.IntegrationConnection,
            contactRequest
        );

        result.ContactId = contactResponse?.Id;

        if (string.IsNullOrEmpty(result.ContactId))
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = new List<ResultMessage>
            {
                new ResultMessage
                {
                    Message =
                        $"Error attempting to create new contact. Contact id from api not found."
                }
            };
        }

        parameter.JobLogger?.Debug($"{nameof(CreateContact)} Finished.");

        return result;
    }

    private Contact CreateContactRequest(CustomerOrder customerOrder)
    {
        return new Contact
        {
            FirstName = customerOrder?.PlacedByUserProfile?.FirstName,
            LastName = customerOrder?.PlacedByUserProfile?.LastName,
            EmailAddress = customerOrder?.PlacedByUserProfile?.Email,
            AddressId = customerOrder?.Customer?.ErpNumber,
            ContactLinks = new List<ContactLink>
            {
                new ContactLink
                {
                    ContactLinkId = customerOrder?.Customer?.ErpNumber,
                    CompanyId = this.GetCompanyNumber(),
                    ContactType = "1203", // 1203=BillToContactType | 1417=ShipToContactType
                    Status = "704"
                }
            }
        };
    }

    private string GetCompanyNumber()
    {
        var companyNumbers = this.integrationConnectorSettings.Prophet21Company.Split(
            new[] { ',', ';', '|' },
            StringSplitOptions.RemoveEmptyEntries
        );

        return companyNumbers.Any() ? companyNumbers[0] : "01";
    }
}
