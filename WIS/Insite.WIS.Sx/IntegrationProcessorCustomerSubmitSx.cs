namespace Insite.WIS.Sx;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Insite.Common.Helpers;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.Plugins.Constants;
using Insite.WIS.Broker.WebIntegrationService;

/// <summary>SX API Implementation of Customer Submit</summary>
public abstract class IntegrationProcessorCustomerSubmitSx
    : IntegrationProcessorCustomerSubmitBase,
        IIntegrationProcessor
{
    /// <summary>Method that determines whether the customer supplied in the initialDataSet already exists in the ERP.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="isBillToCustomer">bool the defines whether or not the proposed customer to create is a billto or shipto</param>
    /// <returns>bool value specifying whether or not the customer already exists.</returns>
    protected override bool CustomerAlreadyExists(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        bool isBillToCustomer
    )
    {
        var customerRowIndex = jobStep.Sequence - 1;

        this.StandardApiCallWithLogging<LoginRequest, LoginResponse>(
            siteConnection,
            integrationJob,
            new LoginRequest(),
            "Login"
        );

        // Check if customer already exists by retrieving the customer
        this.JobLogger.Debug(Messages.DetermineIfCustomerAlreadyExistsMessage, false);

        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);
        var customerNumber = Convert.ToDouble(
            initialDataset.Tables[Data.CustomerTable].Rows[customerRowIndex][
                Data.ErpNumberColumn
            ].ToString()
        );
        var customerSequence = initialDataset.Tables[Data.CustomerTable].Rows[customerRowIndex][
            Data.CustomerSequenceColumn
        ].ToString();

        var arGetCustomerDataGeneralRequest = new ARGetCustomerDataGeneralRequest
        {
            customerNumber = customerNumber,
            shipTo = customerSequence
        };
        var arGetCustomerDataGeneralResponse = this.StandardApiCallWithLogging<
            ARGetCustomerDataGeneralRequest,
            ARGetCustomerDataGeneralResponse
        >(
            siteConnection,
            integrationJob,
            arGetCustomerDataGeneralRequest,
            "ARGetCustomerDataGeneral",
            false
        );

        // i dont see a good way to see if the customer already exists or not.
        var customerAlreadyExists = arGetCustomerDataGeneralResponse.errorMessage.Length == 0;
        this.JobLogger.Debug(
            string.Format(
                Messages.CustomerAlreadyExistsMessage,
                customerNumber,
                customerSequence,
                customerAlreadyExists ? string.Empty : " not"
            ),
            false
        );

        return customerAlreadyExists;
    }

    protected override SubmitCustomerResponse ProcessCustomerIsNewAction(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep integrationJobStep,
        string customerIsNewAction,
        string customerGenericId,
        bool isBillToCustomer
    )
    {
        var submitCustomerResponse = base.ProcessCustomerIsNewAction(
            siteConnection,
            integrationJob,
            integrationJobStep,
            customerIsNewAction,
            customerGenericId,
            isBillToCustomer
        );

        this.UpdateInitialDataSetWithCustomerErpData(
            integrationJob,
            isBillToCustomer,
            submitCustomerResponse
        );

        return submitCustomerResponse;
    }

    /// <summary>Method to create a new customer or ship to in the ERP</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="isBillToCustomer">bool the defines whether or not the proposed customer to create is a billto or shipto</param>
    /// <param name="createCustomerType">The type of Customer create that should happen.</param>
    /// <param name="customerGenericId">The static customer on the ERP to assign the ship to for. The field will be blank unless you are
    /// suppose to assign it to the generic customer.</param>
    /// <returns>The <see cref="SubmitCustomerResponse"/> containing information about what was updated for the customer.</returns>
    protected override SubmitCustomerResponse CreateCustomer(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        bool isBillToCustomer,
        CreateCustomerType createCustomerType,
        string customerGenericId = ""
    )
    {
        var customerRowIndex = jobStep.Sequence - 1;

        this.StandardApiCallWithLogging<LoginRequest, LoginResponse>(
            siteConnection,
            integrationJob,
            new LoginRequest(),
            "Login"
        );

        Func<string, string> getNextAvailableShipTo = parmCustomerNumber =>
        {
            // Retrieve all shiptos for the customer
            this.JobLogger.Debug(Messages.DetermineNextAvailableShipToMessage, false);
            var arGetShiptoListRequest = new ARGetShipToListRequest
            {
                customerNumber = Convert.ToDouble(parmCustomerNumber)
            };
            var arGetShipToListResponse = this.StandardApiCallWithLogging<
                ARGetShipToListRequest,
                ARGetShipToListResponse
            >(siteConnection, integrationJob, arGetShiptoListRequest, "ARGetShipToList");

            // find the next shipto (customersequence) available
            var shipToCounter = 1;
            var returnCustomerSequence = shipToCounter.ToString(CultureInfo.InvariantCulture);
            if (arGetShipToListResponse.arrayShipto != null)
            {
                while (true)
                {
                    if (
                        !arGetShipToListResponse.arrayShipto.Any(
                            x =>
                                x.shipTo.Equals(
                                    shipToCounter.ToString(CultureInfo.InvariantCulture)
                                )
                        )
                    )
                    {
                        returnCustomerSequence = shipToCounter.ToString(
                            CultureInfo.InvariantCulture
                        );
                        break;
                    }

                    shipToCounter++;
                }
            }

            this.JobLogger.Debug(
                string.Format(Messages.GotNextAvailableShipToMessage, returnCustomerSequence),
                false
            );
            return returnCustomerSequence;
        };

        // determine which customer number and customer sequence we are creating
        var customerNumber = string.Empty;
        var customerSequence = string.Empty;
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        switch (createCustomerType)
        {
            case CreateCustomerType.SuppliedCustomerNumberSuppliedCustomerSequence:
                customerNumber = initialDataset.Tables[Data.CustomerTable].Rows[customerRowIndex][
                    Data.CustomerNumberColumn
                ].ToString();
                customerSequence = initialDataset.Tables[Data.CustomerTable].Rows[customerRowIndex][
                    Data.CustomerSequenceColumn
                ].ToString();
                break;
            case CreateCustomerType.SuppliedCustomerNumberNextAvailableCustomerSequence:
                customerNumber = initialDataset.Tables[Data.CustomerTable].Rows[customerRowIndex][
                    Data.CustomerNumberColumn
                ].ToString();
                customerSequence = getNextAvailableShipTo(customerNumber);
                break;
            case CreateCustomerType.GenericCustomerNumberNextAvailableCustomerSequence:
                customerNumber = customerGenericId;
                customerSequence = getNextAvailableShipTo(customerNumber);
                break;
        }

        // create the customer
        this.JobLogger.Debug(
            string.Format(Messages.CreateNewCustomerMessage, customerNumber, customerSequence),
            false
        );
        var arCustomerMntRequest = new ARCustomerMntRequest
        {
            arrayFieldModification = this.CreateFieldModificationArray(
                initialDataset.Tables[Data.CustomerTable].Rows[0],
                "add",
                customerNumber,
                customerSequence
            )
        };
        this.StandardApiCallWithLogging<ARCustomerMntRequest, ARCustomerMntResponse>(
            siteConnection,
            integrationJob,
            arCustomerMntRequest,
            "ARCustomerMnt"
        );

        // return both the isc and sx erp customernumber/customersequence back to the website so isc can be updated
        return new SubmitCustomerResponse
        {
            CustomerNumber = initialDataset.Tables[Data.CustomerTable].Rows[0][
                Data.CustomerNumberColumn
            ].ToString(),
            CustomerSequence = initialDataset.Tables[Data.CustomerTable].Rows[0][
                Data.CustomerSequenceColumn
            ].ToString(),
            ErpCustomerNumber = customerNumber,
            ErpCustomerSequence = customerSequence
        };
    }

    /// <summary>Method to update a customer in the ERP</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="isBillToCustomer">bool the defines whether or not the proposed customer to create is a billto or shipto</param>
    /// <returns>The <see cref="SubmitCustomerResponse"/> containing information about what was updated for the customer.</returns>
    protected override SubmitCustomerResponse UpdateCustomer(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        bool isBillToCustomer
    )
    {
        this.StandardApiCallWithLogging<LoginRequest, LoginResponse>(
            siteConnection,
            integrationJob,
            new LoginRequest(),
            "Login"
        );

        // the customer we update will always be the one passed in the dataset
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);
        var customerNumber = initialDataset.Tables[Data.CustomerTable].Rows[0][
            Data.CustomerNumberColumn
        ].ToString();
        var customerSequence = initialDataset.Tables[Data.CustomerTable].Rows[0][
            Data.CustomerSequenceColumn
        ].ToString();

        // update the customer
        this.JobLogger.Debug(
            string.Format(Messages.UpdateExistingCustomerMessage, customerNumber, customerSequence),
            false
        );
        var arCustomerMntRequest = new ARCustomerMntRequest
        {
            arrayFieldModification = this.CreateFieldModificationArray(
                initialDataset.Tables[Data.CustomerTable].Rows[0],
                "chg",
                customerNumber,
                customerSequence
            )
        };
        this.StandardApiCallWithLogging<ARCustomerMntRequest, ARCustomerMntResponse>(
            siteConnection,
            integrationJob,
            arCustomerMntRequest,
            "ARCustomerMnt"
        );

        // return both the isc and sx erp customernumber/customersequence back to the website so isc can be updated
        return new SubmitCustomerResponse
        {
            CustomerNumber = customerNumber,
            CustomerSequence = customerSequence,
            ErpCustomerNumber = customerNumber,
            ErpCustomerSequence = customerSequence
        };
    }

    /// <summary>Build the Array of information used to add/update the Customer via the SX API</summary>
    /// <param name="dataRow">The DataRow representing the Customer that needs to be added or updated.</param>
    /// <param name="updateMode">add = Add, chg = Update</param>
    /// <param name="customerNumber">The ERP Customer Number that that we should add/update.</param>
    /// <param name="customerSequence">The ERP Customer Sequence that that we should add/update.</param>
    /// <returns>A <see cref="ARCustomerMntinputFieldModification"/> array.</returns>
    protected virtual ARCustomerMntinputFieldModification[] CreateFieldModificationArray(
        DataRow dataRow,
        string updateMode,
        string customerNumber,
        string customerSequence
    )
    {
        const int setNumber = 1;
        var sequenceNumber = 0;

        // define the field mappings (parm1 = SX, parm2 = ISC)
        var customerMapping = new List<Tuple<string, string>>
        {
            Tuple.Create("custtype", Data.CustomerTypeColumn),
            Tuple.Create("name", Data.CompanyNameColumn),
            Tuple.Create("email", Data.EmailColumn),
            Tuple.Create("phoneno", Data.PhoneColumn),
            Tuple.Create("faxphoneno", Data.FaxColumn),
            Tuple.Create("termstype", Data.TermsCodeColumn),
            Tuple.Create("pricetype", Data.PriceCodeColumn),
            Tuple.Create("currencyty", Data.CurrencyCodeColumn),
            Tuple.Create("whse", Data.DefaultWarehouseName),
            Tuple.Create("slsrepout", Data.PrimarySalespersonNumber),
            Tuple.Create("addr1", Data.Address1Column),
            Tuple.Create("addr2", Data.Address2Column),
            Tuple.Create("city", Data.CityColumn),
            Tuple.Create("state", Data.StateColumn),
            Tuple.Create("zipcd", Data.PostalCodeColumn),
            Tuple.Create("shipviaty", Data.ShipCodeColumn),
            Tuple.Create("bankno", Data.BankCodeColumn),
            Tuple.Create("credlim", Data.CreditLimitColumn),
            Tuple.Create("statustype", Data.IsActiveColumn)
        };

        // loop through the field mappings and create the ARCustomerMntinputFieldModification array with the mapped values
        var fieldMofications = new ARCustomerMntinputFieldModification[customerMapping.Count];
        foreach (var mapping in customerMapping)
        {
            fieldMofications[sequenceNumber] = new ARCustomerMntinputFieldModification
            {
                updateMode = updateMode,
                setNumber = setNumber,
                sequenceNumber = sequenceNumber++,
                key1 = customerNumber,
                key2 = customerSequence,
                fieldName = mapping.Item1,
                fieldValue = dataRow[mapping.Item2]
                    .ToString()
                    .Equals("True", StringComparison.OrdinalIgnoreCase)
                    ? "Active"
                    : dataRow[mapping.Item2]
                        .ToString()
                        .Equals("False", StringComparison.OrdinalIgnoreCase)
                        ? "InActive"
                        : dataRow[mapping.Item2].ToString()
            };
        }

        return fieldMofications;
    }

    protected abstract TU StandardApiCallWithLogging<T, TU>(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        T request,
        string methodName,
        bool throwExceptionOnErrorMessage = true
    );
}
