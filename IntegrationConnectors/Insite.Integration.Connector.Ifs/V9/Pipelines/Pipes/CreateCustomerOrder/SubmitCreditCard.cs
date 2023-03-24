namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Insite.Core.Enums;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Integration;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class SubmitCreditCard
    : IPipe<CreateCustomerOrderParameter, CreateCustomerOrderResult>
{
    private readonly IIntegrationJobSchedulingService integrationJobSchedulingService;

    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public SubmitCreditCard(
        IIntegrationJobSchedulingService integrationJobSchedulingService,
        IntegrationConnectorSettings integrationConnectorSettings
    )
    {
        this.integrationJobSchedulingService = integrationJobSchedulingService;
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 900;

    public CreateCustomerOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateCustomerOrderParameter parameter,
        CreateCustomerOrderResult result
    )
    {
        if (!(parameter.CustomerOrder.CreditCardTransactions?.Any() ?? false))
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(SubmitCreditCard)} Started.");

        var creditCardSubmitJobDefinition = unitOfWork
            .GetTypedRepository<IJobDefinitionRepository>()
            .GetByStandardName(JobDefinitionStandardJobName.CreditCardSubmit.ToString());
        if (creditCardSubmitJobDefinition == null)
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages = new List<ResultMessage>
            {
                new ResultMessage
                {
                    Message =
                        $"Unable to find a JobDefinition for {JobDefinitionStandardJobName.CreditCardSubmit}."
                }
            };

            return result;
        }

        var jobDefinitionStepParameterValues = this.GetJobDefinitionStepParameterValues(
            parameter,
            result
        );
        var jobDefinitionStepParameters = GetJobDefinitionStepParameters(
            creditCardSubmitJobDefinition,
            jobDefinitionStepParameterValues
        );

        this.integrationJobSchedulingService.ScheduleBatchIntegrationJob(
            creditCardSubmitJobDefinition.Name,
            parameters: jobDefinitionStepParameters
        );

        parameter.JobLogger?.Debug($"{nameof(SubmitCreditCard)} Finished.");

        return result;
    }

    private Dictionary<string, string> GetJobDefinitionStepParameterValues(
        CreateCustomerOrderParameter parameter,
        CreateCustomerOrderResult result
    )
    {
        return new Dictionary<string, string>
        {
            { "ORDER_NO", result.ErpOrderNumber },
            { "CUSTOMER_NO", result.CustomerOrder.customerNo },
            { "COMPANY", this.integrationConnectorSettings.IfsCompany },
            { "CURRENCY", parameter.CustomerOrder.Currency?.CurrencyCode },
            { "CARDHOLDER_NAME", parameter.CustomerOrder.CreditCardTransactions.First().Name },
            {
                "CREDIT_EXP_MONTH",
                GetCreditCardTransactionExpirationMonth(
                    parameter.CustomerOrder.CreditCardTransactions.First()
                )
            },
            {
                "CREDIT_EXP_YEAR",
                GetCreditCardTransactionExpirationYear(
                    parameter.CustomerOrder.CreditCardTransactions.First()
                )
            },
            {
                "CARD_TYPE",
                GetCreditCardType(parameter.CustomerOrder.CreditCardTransactions.First().CardType)
            },
            { "CREDIT_CARD_NO", "BA27D7692B4EFDC3021158F64754305EAFCD7268A4CC43F8" },
            {
                "DISPLAY_CARD_NUMBER",
                parameter.CustomerOrder.CreditCardTransactions.First().CreditCardNumber
            },
            { "PN_REF", parameter.CustomerOrder.CreditCardTransactions.First().PNRef },
            { "AUTH_CODE", parameter.CustomerOrder.CreditCardTransactions.First().AuthCode }
        };
    }

    private static string GetCreditCardTransactionExpirationMonth(
        CreditCardTransaction creditCardTransaction
    )
    {
        if (creditCardTransaction.ExpirationDate.Length < 2)
        {
            return string.Empty;
        }

        return creditCardTransaction.ExpirationDate.Substring(0, 2);
    }

    private static string GetCreditCardTransactionExpirationYear(
        CreditCardTransaction creditCardTransaction
    )
    {
        if (creditCardTransaction.ExpirationDate.Length < 5)
        {
            return string.Empty;
        }

        return creditCardTransaction.ExpirationDate.Substring(3);
    }

    private static string GetCreditCardType(string cardType)
    {
        return
            cardType.Equals("americanexpress", StringComparison.OrdinalIgnoreCase)
            || cardType.Equals("american express", StringComparison.OrdinalIgnoreCase)
            ? "AMEX"
            : cardType;
    }

    private static Collection<JobDefinitionStepParameter> GetJobDefinitionStepParameters(
        JobDefinition creditCardSubmitJobDefinition,
        Dictionary<string, string> jobDefinitionStepParameterValues
    )
    {
        var jobDefinitionStepParameters = new Collection<JobDefinitionStepParameter>();

        foreach (
            var jobDefinitionStepParameter in creditCardSubmitJobDefinition.JobDefinitionSteps.SelectMany(
                o => o.JobDefinitionStepParameters
            )
        )
        {
            var jobDefinitionStepParameterValue = jobDefinitionStepParameterValues.FirstOrDefault(
                o =>
                    o.Key.Equals(
                        jobDefinitionStepParameter.Name,
                        StringComparison.OrdinalIgnoreCase
                    )
            );

            if (jobDefinitionStepParameterValue.Key.IsNotBlank())
            {
                jobDefinitionStepParameters.Add(
                    new JobDefinitionStepParameter
                    {
                        Id = jobDefinitionStepParameter.Id,
                        Value = jobDefinitionStepParameterValue.Value
                    }
                );
            }
        }

        return jobDefinitionStepParameters;
    }
}
