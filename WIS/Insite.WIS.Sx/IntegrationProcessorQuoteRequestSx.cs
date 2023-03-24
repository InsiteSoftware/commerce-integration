namespace Insite.WIS.Sx;

using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using Insite.Common.Helpers;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.Plugins.Constants;
using Insite.WIS.Broker.WebIntegrationService;

/// <summary>SX API Implementation of Quote Request. Ultimately calls the SFOEOrderTotLoad method of the SxApiService to request a quote.</summary>
public abstract class IntegrationProcessorQuoteRequestSx : IIntegrationProcessor
{
#pragma warning disable SA1306
    protected IntegrationJobLogger JobLogger;
#pragma warning restore SA1306

    /// <summary>Executes the job associated with the passed in <see cref="IntegrationJob"/> and <see cref="JobDefinitionStep"/>.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <returns>The results of the passed in  <see cref="IntegrationJob"/> and <see cref="JobDefinitionStep"/></returns>
    public virtual DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep
    )
    {
        this.JobLogger = new IntegrationJobLogger(siteConnection, integrationJob);

        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        // make sure the initial dataset is atleast usable.
        if (
            (initialDataset == null)
            || (
                !initialDataset.Tables.Contains(Data.CustomerOrderTable)
                || initialDataset.Tables[Data.CustomerOrderTable].Rows.Count == 0
            )
            || (
                !initialDataset.Tables.Contains(Data.OrderLineTable)
                || initialDataset.Tables[Data.OrderLineTable].Rows.Count == 0
            )
            || (
                !initialDataset.Tables.Contains(Data.CustomerTable)
                || initialDataset.Tables[Data.CustomerTable].Rows.Count == 0
            )
        )
        {
            throw new ArgumentException(Messages.InvalidInitialDataSetExceptionMessage);
        }

        this.StandardApiCallWithLogging<LoginRequest, LoginResponse>(
            siteConnection,
            integrationJob,
            new LoginRequest(),
            "Login"
        );

        // parse the incoming sx specific paramaters from the step and store into a more accessible class
        var integrationProcessorSubmitOrderSxInfo = this.PopulateGetQuoteParameters(
            siteConnection,
            integrationJob,
            jobStep
        );

        // submit the quote request to SX
        var sfOeOrderTotLoadRequest = new SFOEOrderTotLoadRequest();
        this.AddOrderHeaderToRequest(
            siteConnection,
            integrationJob,
            jobStep,
            sfOeOrderTotLoadRequest,
            integrationProcessorSubmitOrderSxInfo
        );
        this.JobLogger.Debug(Messages.AddOrderHeaderToRequestCompletedMessage, false);

        this.AddOrderLinesToRequest(
            siteConnection,
            integrationJob,
            jobStep,
            sfOeOrderTotLoadRequest,
            integrationProcessorSubmitOrderSxInfo
        );
        this.JobLogger.Debug(Messages.AddOrderLinesToRequestCompletedMessage, false);

        var sfOeOrderTotLoadResponse = this.StandardApiCallWithLogging<
            SFOEOrderTotLoadRequest,
            SFOEOrderTotLoadResponse
        >(siteConnection, integrationJob, sfOeOrderTotLoadRequest, "SFOEOrderTotLoad");

        // return the data we need back to the client. At the initial time this was written, i am just returning
        // the tax amount, but i could forsee this being expanded to include other info.
        return this.CreateResponseDataSet(sfOeOrderTotLoadResponse);
    }

    /// <summary>Retrieves the ERP_Company, ERP_Warehouse parameters defined in the jobStep.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <returns>The <see cref="IntegrationProcessorQuoteRequestSxInfo"/> with all of the parameters that are relevent to the quote request
    /// contained within.</returns>
    protected virtual IntegrationProcessorQuoteRequestSxInfo PopulateGetQuoteParameters(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep
    )
    {
        var parmCompanyNumber = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.ErpCompany, StringComparison.OrdinalIgnoreCase)
        );
        if (parmCompanyNumber == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpCompany),
                false
            );
        }

        var parmCustomerDefaultWarehouse = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.ErpWarehouse, StringComparison.OrdinalIgnoreCase)
        );
        if (parmCustomerDefaultWarehouse == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpWarehouse),
                false
            );
        }

        return new IntegrationProcessorQuoteRequestSxInfo
        {
            CompanyNumber = (parmCompanyNumber == null) ? string.Empty : parmCompanyNumber.Value,
            CustomerDefaultWarehouse =
                (parmCustomerDefaultWarehouse == null)
                    ? string.Empty
                    : parmCustomerDefaultWarehouse.Value
        };
    }

    /// <summary>Populate the SFOEOrderTotLoadinputInheader object and attach it to the SFOEOrderTotLoadRequest.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="sfOeOrderTotLoadRequest">The full request object representing the customer order that is passed into the SFOEOrderTotLoad
    /// method of the SxApiService.</param>
    /// <param name="integrationProcessorQuoteRequestSxInfo">Parameters relevant for the quote request.</param>
    protected virtual void AddOrderHeaderToRequest(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        SFOEOrderTotLoadRequest sfOeOrderTotLoadRequest,
        IntegrationProcessorQuoteRequestSxInfo integrationProcessorQuoteRequestSxInfo
    )
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);
        var dataRowCustomer = initialDataset.Tables[Data.CustomerTable].Rows[0];
        var dataRowCustomerOrder = initialDataset.Tables[Data.CustomerOrderTable].Rows[0];
        var sfOeOrderTotLoadInputInheader = new SFOEOrderTotLoadinputInheader
        {
            customerID =
                integrationProcessorQuoteRequestSxInfo.CompanyNumber.PadLeft(4, '0')
                + dataRowCustomer[Data.CustomerNumberColumn].ToString().PadLeft(12, '0'),
            warehouseID = string.IsNullOrEmpty(
                dataRowCustomerOrder[Data.DefaultWarehouseName]?.ToString()
            )
                ? integrationProcessorQuoteRequestSxInfo.CustomerDefaultWarehouse
                : dataRowCustomerOrder[Data.DefaultWarehouseName].ToString(),
            poNumber = dataRowCustomerOrder[Data.CustomerPoColumn].ToString(),
            ordNumber = dataRowCustomerOrder[Data.OrderNumberColumn].ToString(),
            billToCity = dataRowCustomerOrder[Data.BtCityColumn].ToString(),
            billToState = dataRowCustomerOrder[Data.BtStateColumn].ToString(),
            billToZipCode = dataRowCustomerOrder[Data.BtPostalCodeColumn].ToString(),
            billToPhone = dataRowCustomerOrder[Data.BtPhoneColumn].ToString(),
            carrierCode = dataRowCustomerOrder[Data.ShipCodeColumn].ToString(),
            customerAddress1 = dataRowCustomerOrder[Data.BtAddress1Column].ToString(),
            customerAddress2 = dataRowCustomerOrder[Data.BtAddress2Column].ToString(),
            customerAddress3 = dataRowCustomerOrder[Data.BtAddress3Column].ToString(),
            customerAddress4 = dataRowCustomerOrder[Data.BtAddress4Column].ToString(),
            customerName =
                dataRowCustomerOrder[Data.BtFirstNameColumn]
                + " "
                + dataRowCustomerOrder[Data.BtLastNameColumn],
            customerCountry = dataRowCustomerOrder[Data.BtCountryColumn].ToString(),
            shipToAddress1 = dataRowCustomerOrder[Data.StAddress1Column].ToString(),
            shipToAddress2 = dataRowCustomerOrder[Data.StAddress2Column].ToString(),
            shipToAddress3 = dataRowCustomerOrder[Data.StAddress3Column].ToString(),
            shipToAddress4 = dataRowCustomerOrder[Data.StAddress4Column].ToString(),
            shipToCity = dataRowCustomerOrder[Data.StCityColumn].ToString(),
            shipToCountry = dataRowCustomerOrder[Data.StCountryColumn].ToString(),
            shipToName =
                dataRowCustomerOrder[Data.StFirstNameColumn]
                + " "
                + dataRowCustomerOrder[Data.StLastNameColumn],
            shipToNumber = dataRowCustomerOrder[Data.CustomerSequenceColumn].ToString(),
            shipToState = dataRowCustomerOrder[Data.StStateColumn].ToString(),
            shipToPhone = dataRowCustomerOrder[Data.StPhoneColumn].ToString(),
            shipToZipCode = dataRowCustomerOrder[Data.StPostalCodeColumn].ToString(),
            webTransactionType = "TSF" // TSF = Order Total, LSF = Order Load
        };
        SxApiHelper.SetDynamicProperties(
            siteConnection,
            integrationJob,
            jobStep,
            sfOeOrderTotLoadInputInheader
        );
        sfOeOrderTotLoadRequest.arrayInheader = new[] { sfOeOrderTotLoadInputInheader };
    }

    /// <summary>Populate the SFOEOrderTotLoadinputInline object and attach it to the SFOEOrderTotLoadRequest.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="sfOeOrderTotLoadRequest">The full request object representing the customer order that is passed into the SFOEOrderTotLoad
    /// method of the SxApiService.</param>
    /// <param name="integrationProcessorQuoteRequestSxInfo">Parameters relevant for the quote request.</param>
    protected virtual void AddOrderLinesToRequest(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        SFOEOrderTotLoadRequest sfOeOrderTotLoadRequest,
        IntegrationProcessorQuoteRequestSxInfo integrationProcessorQuoteRequestSxInfo
    )
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);
        var sfOeOrderTotLoadInputInlines = new SFOEOrderTotLoadinputInline[
            initialDataset.Tables[Data.OrderLineTable].Rows.Count
        ];
        var itemCount = 0;
        foreach (DataRow dataRowOrderLine in initialDataset.Tables[Data.OrderLineTable].Rows)
        {
            double.TryParse(dataRowOrderLine[Data.QtyOrderedColumn].ToString(), out var orderQty);
            double.TryParse(
                dataRowOrderLine[Data.UnitNetPriceColumn].ToString(),
                out var actualSellPrice
            );
            double.TryParse(
                dataRowOrderLine[Data.UnitRegularPriceColumn].ToString(),
                out var unitRegularPrice
            );
            var sfOeOrderTotLoadInputInline = new SFOEOrderTotLoadinputInline
            {
                itemNumber = dataRowOrderLine[Data.ErpNumberColumn].ToString(),
                orderQty = orderQty,
                unitOfMeasure = dataRowOrderLine[Data.UnitOfMeasureColumn].ToString(),
                warehouseID = string.IsNullOrEmpty(
                    dataRowOrderLine[Data.WarehouseColumn]?.ToString()
                )
                    ? integrationProcessorQuoteRequestSxInfo.CustomerDefaultWarehouse
                    : dataRowOrderLine[Data.WarehouseColumn].ToString(),
                itemDescription1 = dataRowOrderLine[Data.DescriptionColumn].ToString(),
                actualSellPrice = actualSellPrice,
                cost = unitRegularPrice,
                listPrice = dataRowOrderLine[Data.UnitRegularPriceColumn].ToString()
            };
            SxApiHelper.SetDynamicProperties(
                siteConnection,
                integrationJob,
                jobStep,
                sfOeOrderTotLoadInputInline,
                new NameValueCollection
                {
                    { Data.ErpNumberColumn, dataRowOrderLine[Data.ErpNumberColumn].ToString() }
                }
            );
            sfOeOrderTotLoadInputInlines[itemCount] = sfOeOrderTotLoadInputInline;
            itemCount++;
        }

        sfOeOrderTotLoadRequest.arrayInline = sfOeOrderTotLoadInputInlines;
    }

    /// <summary>Create the response dataset to send back to the website for processing</summary>
    /// <param name="sfOeOrderTotLoadResponse">The <see cref="SFOEOrderTotLoadResponse"/> containing information about what was updated for the order.</param>
    /// <returns>The response dataset to send back to the website for processing.</returns>
    protected virtual DataSet CreateResponseDataSet(
        SFOEOrderTotLoadResponse sfOeOrderTotLoadResponse
    )
    {
        var dataTable = new DataTable(Data.QuoteRequestTable);
        dataTable.Columns.Add(Data.SalesTaxAmountColumn);
        dataTable.Columns.Add(Data.UnitNetPriceColumn);
        dataTable.Columns.Add(Data.ProductNumberColumn);
        dataTable.Columns.Add(Data.UnitOfMeasureColumn);

        double salesTaxAmount = 0;
        if (sfOeOrderTotLoadResponse.arrayOuttotal.Any())
        {
            var totals = sfOeOrderTotLoadResponse.arrayOuttotal.FirstOrDefault();
            salesTaxAmount = totals?.salesTaxAmount ?? 0;
        }

        // get the pricing totals
        foreach (var outline in sfOeOrderTotLoadResponse.arrayOutline)
        {
            double regularPrice = 0;
            if (outline.orderQty != 0)
            {
                regularPrice = outline.lineAmount / outline.orderQty;
            }

            dataTable.Rows.Add(
                salesTaxAmount,
                regularPrice,
                outline.itemNumber,
                outline.unitOfMeasure
            );
        }

        var dataSet = new DataSet();
        dataSet.Tables.Add(dataTable);
        return dataSet;
    }

    protected abstract TU StandardApiCallWithLogging<T, TU>(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        T request,
        string methodName,
        bool throwExceptionOnErrorMessage = true
    );
}
