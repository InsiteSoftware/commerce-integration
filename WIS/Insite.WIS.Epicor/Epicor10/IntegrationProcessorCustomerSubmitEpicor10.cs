namespace Insite.WIS.Epicor.Epicor10;

using System;
using System.Data;
using System.Globalization;
using System.Linq;

using Insite.Common.Helpers;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.Plugins.Constants;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.Epicor.Epicor10CustCntService;
using Insite.WIS.Epicor.Epicor10CustomerService;
using Insite.WIS.Epicor.Epicor10ShipToService;

using ShipToRow = Insite.WIS.Epicor.Epicor10ShipToService.ShipToRow;

/// <summary>Epicor10 API Implementation of Customer Submit</summary>
public class IntegrationProcessorCustomerSubmitEpicor10
    : IntegrationProcessorCustomerSubmitBase,
        IIntegrationProcessor
{
#pragma warning disable SA1306
    protected IntegrationProcessorCoreEpicor10 IntegrationProcessorCore;
#pragma warning restore SA1306

    /// <summary>Executes the job associated with the passed in <see cref="IntegrationJob"/> and <see cref="JobDefinitionStep"/>.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <returns>The results of the passed in  <see cref="IntegrationJob"/> and <see cref="JobDefinitionStep"/></returns>
    public override DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep
    )
    {
        this.IntegrationProcessorCore = new IntegrationProcessorCoreEpicor10(
            siteConnection,
            integrationJob
        );

        try
        {
            var integrationConnection = integrationJob.JobDefinition.IntegrationConnection;
            var erpCompanyParameter = jobStep.JobDefinitionStepParameters.FirstOrDefault(
                p => p.Name.Equals(Parameters.ErpCompany, StringComparison.OrdinalIgnoreCase)
            );
            var companyId =
                (erpCompanyParameter != null) ? erpCompanyParameter.Value : string.Empty;
#pragma warning disable CS0618 // Type or member is obsolete
            this.IntegrationProcessorCore.Initialize(
                integrationConnection.Url,
                companyId,
                integrationConnection.LogOn,
                EncryptionHelper.DecryptAes(integrationConnection.Password)
            );
#pragma warning restore

            return base.Execute(siteConnection, integrationJob, jobStep);
        }
        finally
        {
            this.IntegrationProcessorCore.EndSession();
        }
    }

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
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        var customerNumber = initialDataset.Tables[Data.CustomerTable].Rows[0][
            Data.CustomerNumberColumn
        ].ToString();
        var customerSequence = initialDataset.Tables[Data.CustomerTable].Rows[0][
            Data.CustomerSequenceColumn
        ].ToString();

        if (isBillToCustomer)
        {
            return this.GetBillTo(customerNumber) != null;
        }

        var customerRow = this.GetBillTo(customerNumber);
        if (customerRow == null || customerRow.CustNum <= 0)
        {
            return false;
        }

        return this.FindShipTo(customerRow.CustNum, customerSequence) != null;
    }

    /// <summary>Method to create a new Customer or ShipTo in the ERP</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="isBillToCustomer">bool the defines whether or not the proposed customer to create is a billto or shipto</param>
    /// <param name="createCustomerType">The type of Customer create that should happen.</param>
    /// <param name="customerGenericId">The static customer on the ERP to assign the ShipTo to. The field will be blank unless you are
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
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        var sourceCustomerRow = initialDataset.Tables[Data.CustomerTable].Rows[0];
        var customerNumber = sourceCustomerRow[Data.CustomerNumberColumn].ToString();
        var customerSequence = sourceCustomerRow[Data.CustomerSequenceColumn].ToString();
        string erpCustomerNumber;
        string erpCustomerSequence;

        if (isBillToCustomer)
        {
            var destinationCustomerRow = this.CreateBillTo(customerNumber, sourceCustomerRow);
            erpCustomerNumber = destinationCustomerRow.CustNum.ToString(
                CultureInfo.InvariantCulture
            );
            erpCustomerSequence = string.Empty;
        }
        else
        {
            var customerRow = this.GetBillTo(customerNumber);
            var destinationShipToRow = this.CreateShipTo(
                customerRow.CustNum,
                customerSequence,
                sourceCustomerRow
            );
            erpCustomerNumber = destinationShipToRow.CustNum.ToString(CultureInfo.InvariantCulture);
            erpCustomerSequence = destinationShipToRow.ShipToNum;
        }

        // return both the isc and erp customernumber/customersequence back to the website so isc can be updated
        return new SubmitCustomerResponse
        {
            CustomerNumber = initialDataset.Tables[Data.CustomerTable].Rows[0][
                Data.CustomerNumberColumn
            ].ToString(),
            CustomerSequence = initialDataset.Tables[Data.CustomerTable].Rows[0][
                Data.CustomerSequenceColumn
            ].ToString(),
            ErpCustomerNumber = erpCustomerNumber,
            ErpCustomerSequence = erpCustomerSequence
        };
    }

    /// <summary>Method to update a customer in the ERP</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="isBillToCustomer">bool the defines whether or not the proposed customer to create is a billto or shipto.</param>
    /// <returns>The <see cref="SubmitCustomerResponse"/> containing information about what was updated for the customer.</returns>
    protected override SubmitCustomerResponse UpdateCustomer(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        bool isBillToCustomer
    )
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        // return both the isc and erp customernumber/customersequence back to the website so isc can be updated
        return new SubmitCustomerResponse
        {
            CustomerNumber = initialDataset.Tables[Data.CustomerTable].Rows[0][
                Data.CustomerNumberColumn
            ].ToString(),
            CustomerSequence = initialDataset.Tables[Data.CustomerTable].Rows[0][
                Data.CustomerSequenceColumn
            ].ToString(),
            ErpCustomerNumber = initialDataset.Tables[Data.CustomerTable].Rows[0][
                Data.CustomerNumberColumn
            ].ToString(),
            ErpCustomerSequence = initialDataset.Tables[Data.CustomerTable].Rows[0][
                Data.CustomerSequenceColumn
            ].ToString()
        };
    }

    /// <summary>Method to retrive Customer record from ERP</summary>
    /// <param name="customerNumber">The CustomerNumber to look up.</param>
    /// <returns>The <see cref="CustomerRow"/> matching the given CustomerNumber.</returns>
    protected virtual CustomerRow GetBillTo(string customerNumber)
    {
        using (
            var customerServiceClient = this.IntegrationProcessorCore.GetClient<
                CustomerSvcContractClient,
                CustomerSvcContract
            >(IntegrationProcessorCoreEpicor10.CustomerServicePath)
        )
        {
            customerServiceClient.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    this.IntegrationProcessorCore.SessionId,
                    this.IntegrationProcessorCore.Username,
                    this.IntegrationProcessorCore.SiteConnection,
                    this.IntegrationProcessorCore.IntegrationJob
                )
            );

            var whereClause = "CustID = '" + customerNumber + "'";

            var excludeClause = this.IntegrationProcessorCore.ExcludeClause;
            var customerTableset = customerServiceClient.GetRows(
                whereClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                excludeClause,
                0,
                0,
                out var morePages
            );

            return customerTableset?.Customer.FirstOrDefault(
                customerTable => customerTable.CustID == customerNumber
            );
        }
    }

    /// <summary>Method to create a new Customer record in the ERP</summary>
    /// <param name="customerNumber">The CustomerNumber to use for the new record in the ERP.</param>
    /// <param name="sourceCustomerRow">The source DataRow containing the Customer data.</param>
    /// <returns>The <see cref="CustomerRow"/> for the newly created Customer.</returns>
    protected virtual CustomerRow CreateBillTo(string customerNumber, DataRow sourceCustomerRow)
    {
        var customerTableset = new CustomerTableset();
        var destinationCustomerRow = new CustomerRow();

        using (
            var customerServiceClient = this.IntegrationProcessorCore.GetClient<
                CustomerSvcContractClient,
                CustomerSvcContract
            >(IntegrationProcessorCoreEpicor10.CustomerServicePath)
        )
        {
            customerServiceClient.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    this.IntegrationProcessorCore.SessionId,
                    this.IntegrationProcessorCore.Username,
                    this.IntegrationProcessorCore.SiteConnection,
                    this.IntegrationProcessorCore.IntegrationJob
                )
            );

            // Get initial Customer record from ERP
            customerServiceClient.GetNewCustomer(ref customerTableset);

            if (customerTableset.Customer.Count > 0 && customerTableset.Customer[0] != null)
            {
                destinationCustomerRow = customerTableset.Customer[0];
            }

            this.MapBillTo(customerNumber, sourceCustomerRow, destinationCustomerRow);

            customerServiceClient.GetCustomerTerritory(
                ref customerTableset,
                destinationCustomerRow.CustNum
            );
            customerServiceClient.Update(ref customerTableset);

            // Update customerRow to include data modfiications made by ERP
            if (customerTableset.Customer.Count > 0 && customerTableset.Customer[0] != null)
            {
                destinationCustomerRow = customerTableset.Customer[0];
            }

            // Create Contact and associate it with the new customer record
            this.CreateContact(destinationCustomerRow.CustNum, string.Empty, sourceCustomerRow);
        }

        return destinationCustomerRow;
    }

    /// <summary>Method to map source customer data to the new Customer record in the ERP</summary>
    /// <param name="customerNumber">The CustomerNumber to use for the new record in the ERP.</param>
    /// <param name="sourceCustomerRow">The source DataRow containing the Customer data.</param>
    /// <param name="destinationCustomerRow">The destination CustomerRow for the new Customer record in the ERP.</param>
    protected virtual void MapBillTo(
        string customerNumber,
        DataRow sourceCustomerRow,
        CustomerRow destinationCustomerRow
    )
    {
        // Basic settings for new customer.
        destinationCustomerRow.Company = this.IntegrationProcessorCore.CompanyId;
        destinationCustomerRow.CustID = customerNumber;
        destinationCustomerRow.CustomerType = "CUS";

        // Name/Address/Etc settings for new customer
        destinationCustomerRow.Name = sourceCustomerRow[Data.CompanyNameColumn].ToString();
        if (destinationCustomerRow.Name.Trim() == string.Empty)
        {
            destinationCustomerRow.Name =
                sourceCustomerRow[Data.FirstNameColumn]
                + " "
                + sourceCustomerRow[Data.LastNameColumn];
        }

        destinationCustomerRow.Address1 = sourceCustomerRow[Data.Address1Column].ToString();
        destinationCustomerRow.Address2 = sourceCustomerRow[Data.Address2Column].ToString();
        destinationCustomerRow.Address3 = sourceCustomerRow[Data.Address3Column].ToString();
        destinationCustomerRow.City = sourceCustomerRow[Data.CityColumn].ToString();
        destinationCustomerRow.State = sourceCustomerRow[Data.StateColumn].ToString();
        destinationCustomerRow.Zip = sourceCustomerRow[Data.PostalCodeColumn].ToString();
        destinationCustomerRow.Country = sourceCustomerRow[Data.CountryColumn].ToString();
        var countryNum = this.GetCountryNum(
            sourceCustomerRow.Table.DataSet.Tables[Data.CountryTable],
            destinationCustomerRow.Country
        );
        destinationCustomerRow.CountryNum = countryNum;
        destinationCustomerRow.EMailAddress = sourceCustomerRow[Data.EmailColumn].ToString();
        destinationCustomerRow.PhoneNum = sourceCustomerRow[Data.PhoneColumn].ToString();
        destinationCustomerRow.FaxNum = sourceCustomerRow[Data.FaxColumn].ToString();

        // Other settings for new customer
        destinationCustomerRow.TermsCode = sourceCustomerRow[Data.TermsCodeColumn].ToString();
        destinationCustomerRow.ShipViaCode = sourceCustomerRow[Data.ShipCodeColumn].ToString();
        var taxRegion = sourceCustomerRow[Data.TaxCode1Column].ToString();
        if (taxRegion.Trim() == "NT" || taxRegion.Trim() == string.Empty)
        {
            taxRegion = string.Empty;
        }

        destinationCustomerRow.TaxRegionCode = taxRegion;

        destinationCustomerRow.GroupCode = sourceCustomerRow[Data.CustomerTypeColumn].ToString();
        destinationCustomerRow.SalesRepCode = sourceCustomerRow[Data.SalespersonColumn].ToString();
        destinationCustomerRow.ShippingQualifier = Convert.ToBoolean(
            sourceCustomerRow[Data.ShipPartialColumn]
        )
            ? "L"
            : "O"; // L - Line Complete, O - Order Complete
        destinationCustomerRow.CreditHold = Convert.ToBoolean(
            sourceCustomerRow[Data.CreditHoldColumn]
        );
        destinationCustomerRow.CreditLimit = Convert.ToDecimal(
            sourceCustomerRow[Data.CreditLimitColumn]
        );
        destinationCustomerRow.DiscountPercent = Convert.ToDecimal(
            sourceCustomerRow[Data.DiscountColumn]
        );
        destinationCustomerRow.WebCustomer = true;
    }

    /// <summary>Method to retrive ShipTo record from ERP</summary>
    /// <param name="erpCustomerNumber">The ErpCustomerNumber matching the CustNum in the ERP.</param>
    /// <param name="customerSequence">The CustomerSequence matching the ShipToNum in the ERP.</param>
    /// <returns>The <see cref="ShipToRow"/> matching the given ErpCustomerNumber and CustomerSequence.</returns>
    protected virtual ShipToRow FindShipTo(int erpCustomerNumber, string customerSequence)
    {
        using (
            var shipToServiceClient = this.IntegrationProcessorCore.GetClient<
                ShipToSvcContractClient,
                ShipToSvcContract
            >(IntegrationProcessorCoreEpicor10.ShipToServicePath)
        )
        {
            shipToServiceClient.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    this.IntegrationProcessorCore.SessionId,
                    this.IntegrationProcessorCore.Username,
                    this.IntegrationProcessorCore.SiteConnection,
                    this.IntegrationProcessorCore.IntegrationJob
                )
            );

            var whereClause =
                "CustNum = '" + erpCustomerNumber + "' AND ShipToNum = '" + customerSequence + "'";

            var excludeClause = this.IntegrationProcessorCore.ExcludeClause;
            var shipToTableset = shipToServiceClient.GetRows(
                whereClause,
                excludeClause,
                excludeClause,
                0,
                0,
                out var morePages
            );

            return shipToTableset?.ShipTo.FirstOrDefault(
                shipToTable => shipToTable.ShipToNum == customerSequence
            );
        }
    }

    /// <summary>Method to create a new ShipTo record in the ERP</summary>
    /// <param name="erpCustomerNumber">The ErpCustomerNumber to use for the new record in the ERP.</param>
    /// <param name="customerSequence">The CustomerSequence to use for the new record in the ERP.</param>
    /// <param name="sourceCustomerRow">The source DataRow containing the Customer data.</param>
    /// <returns>The <see cref="ShipToRow"/> for the newly created ShipTo.</returns>
    protected virtual ShipToRow CreateShipTo(
        int erpCustomerNumber,
        string customerSequence,
        DataRow sourceCustomerRow
    )
    {
        var shipToTableset = new ShipToTableset();
        var destinationShipToRow = new ShipToRow();

        using (
            var shipToServiceClient = this.IntegrationProcessorCore.GetClient<
                ShipToSvcContractClient,
                ShipToSvcContract
            >(IntegrationProcessorCoreEpicor10.ShipToServicePath)
        )
        {
            shipToServiceClient.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    this.IntegrationProcessorCore.SessionId,
                    this.IntegrationProcessorCore.Username,
                    this.IntegrationProcessorCore.SiteConnection,
                    this.IntegrationProcessorCore.IntegrationJob
                )
            );

            // Get initial ShipTo record from ERP
            shipToServiceClient.GetNewShipTo(ref shipToTableset, erpCustomerNumber);

            if (shipToTableset.ShipTo.Count > 0 && shipToTableset.ShipTo[0] != null)
            {
                destinationShipToRow = shipToTableset.ShipTo[0];
            }

            this.MapShipTo(
                erpCustomerNumber,
                customerSequence,
                sourceCustomerRow,
                destinationShipToRow
            );

            shipToServiceClient.Update(ref shipToTableset);

            // Update shipToRow to include data modfiications made by ERP
            if (shipToTableset.ShipTo.Count > 0 && shipToTableset.ShipTo[0] != null)
            {
                destinationShipToRow = shipToTableset.ShipTo[0];
            }

            // Create Contact and associate it with the ShipTo
            this.CreateContact(erpCustomerNumber, customerSequence, sourceCustomerRow);
        }

        return destinationShipToRow;
    }

    /// <summary>Method to map source customer data to the new customer record in the ERP</summary>
    /// <param name="erpCustomerNumber">The ErpCustomerNumber to use for the new record in the ERP.</param>
    /// <param name="customerSequence">The CustomerSequence to use for the new record in the ERP.</param>
    /// <param name="sourceCustomerRow">The source DataRow containing the Customer data.</param>
    /// <param name="destinationShipToRow">The destination ShipToRow for the new Customer record in the ERP.</param>
    protected virtual void MapShipTo(
        int erpCustomerNumber,
        string customerSequence,
        DataRow sourceCustomerRow,
        ShipToRow destinationShipToRow
    )
    {
        // Basic settings for new ShipTo.
        destinationShipToRow.Company = this.IntegrationProcessorCore.CompanyId;
        destinationShipToRow.CustNum = erpCustomerNumber;
        destinationShipToRow.ShipToNum = customerSequence;

        // Name/Addres/Etc. settings for new ShipTo
        destinationShipToRow.Name = sourceCustomerRow[Data.CompanyNameColumn].ToString();
        if (destinationShipToRow.Name.Trim() == string.Empty)
        {
            destinationShipToRow.Name =
                sourceCustomerRow[Data.FirstNameColumn]
                + " "
                + sourceCustomerRow[Data.LastNameColumn];
        }

        destinationShipToRow.Address1 = sourceCustomerRow[Data.Address1Column].ToString();
        destinationShipToRow.Address2 = sourceCustomerRow[Data.Address2Column].ToString();
        destinationShipToRow.Address3 = sourceCustomerRow[Data.Address3Column].ToString();
        destinationShipToRow.City = sourceCustomerRow[Data.CityColumn].ToString();
        destinationShipToRow.State = sourceCustomerRow[Data.StateColumn].ToString();
        destinationShipToRow.ZIP = sourceCustomerRow[Data.PostalCodeColumn].ToString();
        destinationShipToRow.Country = sourceCustomerRow[Data.CountryColumn].ToString();
        var countryNum = this.GetCountryNum(
            sourceCustomerRow.Table.DataSet.Tables[Data.CountryTable],
            destinationShipToRow.Country
        );
        destinationShipToRow.CountryNum = countryNum;
        destinationShipToRow.EMailAddress = sourceCustomerRow[Data.EmailColumn].ToString();
        destinationShipToRow.PhoneNum = sourceCustomerRow[Data.PhoneColumn].ToString();
        destinationShipToRow.FaxNum = sourceCustomerRow[Data.FaxColumn].ToString();

        // Other settings for new customer
        destinationShipToRow.ShipViaCode = sourceCustomerRow[Data.ShipCodeColumn].ToString();
        var taxRegion = sourceCustomerRow[Data.TaxCode1Column].ToString();
        if (taxRegion.Trim() == "NT" || taxRegion.Trim() == string.Empty)
        {
            taxRegion = string.Empty;
        }

        destinationShipToRow.TaxRegionCode = taxRegion;
        destinationShipToRow.SalesRepCode = sourceCustomerRow[Data.SalespersonColumn].ToString();
    }

    /// <summary>Method to create a new Contact record in the ERP</summary>
    /// <param name="erpCustomerNumber">The ErpCustomerNumber to associate the new Contact record with in the ERP.</param>
    /// <param name="customerSequence">The CustomerSequence to associate the new Contact record with in the ERP.</param>
    /// <param name="sourceCustomerRow">The source DataRow containing the Customer data.</param>
    /// <returns>The <see cref="ShipToRow"/> for the newly created ShipTo.</returns>
    protected virtual CustCntRow CreateContact(
        int erpCustomerNumber,
        string customerSequence,
        DataRow sourceCustomerRow
    )
    {
        var custCntTableset = new CustCntTableset();
        var destinationCustCntRow = new CustCntRow();

        using (
            var custCntServiceClient = this.IntegrationProcessorCore.GetClient<
                CustCntSvcContractClient,
                CustCntSvcContract
            >(IntegrationProcessorCoreEpicor10.CustCntServicePath)
        )
        {
            custCntServiceClient.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    this.IntegrationProcessorCore.SessionId,
                    this.IntegrationProcessorCore.Username,
                    this.IntegrationProcessorCore.SiteConnection,
                    this.IntegrationProcessorCore.IntegrationJob
                )
            );

            // Get initial contact record from ERP
            custCntServiceClient.GetNewCustCnt(
                ref custCntTableset,
                erpCustomerNumber,
                customerSequence
            );

            if (custCntTableset.CustCnt.Count > 0 && custCntTableset.CustCnt[0] != null)
            {
                destinationCustCntRow = custCntTableset.CustCnt[0];
            }

            this.MapContact(
                erpCustomerNumber,
                customerSequence,
                sourceCustomerRow,
                destinationCustCntRow
            );

            custCntServiceClient.Update(ref custCntTableset);

            // Update the contact row to include data modfiications made by ERP
            if (custCntTableset.CustCnt.Count > 0 && custCntTableset.CustCnt[0] != null)
            {
                destinationCustCntRow = custCntTableset.CustCnt[0];
            }
        }

        return destinationCustCntRow;
    }

    /// <summary>The map contact.</summary>
    /// <param name="erpCustomerNumber">The erp customer number.</param>
    /// <param name="customerSequence">The customer sequence.</param>
    /// <param name="sourceCustomerRow">The source customer row.</param>
    /// <param name="destinationCustCntRow">The destination cust cnt row.</param>
    protected virtual void MapContact(
        int erpCustomerNumber,
        string customerSequence,
        DataRow sourceCustomerRow,
        CustCntRow destinationCustCntRow
    )
    {
        // Basic settings for new contact.
        destinationCustCntRow.Company = this.IntegrationProcessorCore.CompanyId;
        destinationCustCntRow.CustNum = erpCustomerNumber;
        destinationCustCntRow.ShipToNum = customerSequence;

        // Name/Address/Etc settings for new contact,
        destinationCustCntRow.FirstName = sourceCustomerRow["FirstName"].ToString();
        destinationCustCntRow.MiddleName = sourceCustomerRow["MiddleName"].ToString();
        destinationCustCntRow.LastName = sourceCustomerRow["LastName"].ToString();
        destinationCustCntRow.Name = sourceCustomerRow["ContactFullName"].ToString();
        if (destinationCustCntRow.Name == string.Empty)
        {
            destinationCustCntRow.Name = (
                destinationCustCntRow.FirstName + " " + destinationCustCntRow.LastName
            ).Trim();
        }

        destinationCustCntRow.EMailAddress = sourceCustomerRow["Email"].ToString();
        destinationCustCntRow.PhoneNum = sourceCustomerRow["Phone"].ToString();
        destinationCustCntRow.FaxNum = sourceCustomerRow["Fax"].ToString();

        if (string.IsNullOrEmpty(customerSequence))
        {
            destinationCustCntRow.PrimaryBilling = true;
            destinationCustCntRow.PrimaryPurchasing = true;
        }
        else
        {
            destinationCustCntRow.PrimaryShipping = true;
        }
    }

    /// <summary>The get country num.</summary>
    /// <param name="countryTable">The country table.</param>
    /// <param name="country">The country.</param>
    /// <returns>The <see cref="int"/>.</returns>
    protected virtual int GetCountryNum(DataTable countryTable, string country)
    {
        if (countryTable == null || countryTable.Rows.Count <= 0)
        {
            return 0;
        }

        var countryRow = countryTable.Rows[0];
        if (countryRow["ISONumber"] == null)
        {
            return 0;
        }

        return Convert.ToInt32(countryRow["ISONumber"]);
    }
}
