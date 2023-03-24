namespace Insite.WIS.Sx;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using Insite.Common.Helpers;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.Plugins.Constants;
using Insite.WIS.Broker.WebIntegrationService;

/// <summary>SX API Implementation of Order Submit. Ultimately calls the OEFullOrderMntV5 method of the SxApiService to submit the order.</summary>
public abstract class IntegrationProcessorOrderSubmitSx : IIntegrationProcessor
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
            || (
                !initialDataset.Tables.Contains(Data.ShipToTable)
                || initialDataset.Tables[Data.ShipToTable].Rows.Count == 0
            )
            || (
                !initialDataset.Tables.Contains(Data.ShipViaTable)
                || initialDataset.Tables[Data.ShipViaTable].Rows.Count == 0
            )
            || (
                !initialDataset.Tables.Contains(Data.CurrencyTable)
                || initialDataset.Tables[Data.CurrencyTable].Rows.Count == 0
            )
            || (
                !initialDataset.Tables.Contains(Data.ShipToStateTable)
                || initialDataset.Tables[Data.ShipToStateTable].Rows.Count == 0
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

        // parse the incoming paramaters from the step and store into a more accessible class
        var integrationProcessorSubmitOrderSxInfo = this.PopulateOrderSubmitParameters(
            siteConnection,
            integrationJob,
            jobStep
        );
        var dataRowCustomerOrder = initialDataset.Tables[Data.CustomerOrderTable].Rows[0];
        var dataRowBillto = initialDataset.Tables[Data.CustomerTable].Rows[0];
        var dataRowShipVia = initialDataset.Tables[Data.ShipViaTable].Rows[0];

        // in the original code i copied the order submit from, there was logic that allowed you to define an application setting that would disallow using
        // the same po if you have used it already in the past.
        if (!integrationProcessorSubmitOrderSxInfo.AllowDuplicateCustomerPo)
        {
            if (dataRowCustomerOrder[Data.CustomerPoColumn].ToString().Length > 0)
            {
                this.JobLogger.Debug(Messages.DetermineIfDuplicateCustomerPoAllowedMessage, false);
                var oeGetListOfOrdersRequest = new OEGetListOfOrdersRequest
                {
                    customerNumber = Convert.ToDouble(
                        dataRowBillto[Data.CustomerNumberColumn].ToString()
                    ),
                    customerPurchaseOrder = dataRowCustomerOrder[Data.CustomerPoColumn].ToString()
                };
                var oeGetListOfOrdersResponse = this.StandardApiCallWithLogging<
                    OEGetListOfOrdersRequest,
                    OEGetListOfOrdersResponse
                >(siteConnection, integrationJob, oeGetListOfOrdersRequest, "OEGetListOfOrders");
                if (
                    oeGetListOfOrdersResponse.arrayOrder != null
                    && oeGetListOfOrdersResponse.arrayOrder.Length > 0
                )
                {
                    throw new Exception(
                        string.Format(
                            Messages.DuplicatePoNumberErrorExceptionMessage,
                            dataRowCustomerOrder[Data.CustomerPoColumn],
                            dataRowCustomerOrder[Data.OrderNumberColumn],
                            oeGetListOfOrdersResponse.arrayOrder[0].orderNumber
                                + "-"
                                + oeGetListOfOrdersResponse.arrayOrder[0].orderSuffix
                        )
                    );
                }

                this.JobLogger.Debug(Messages.AllowDuplicateCustomerPoInfoMessage, false);
            }
        }

        // transform the isc shipvia into an sx shipcode by doing a lookup. if it doesnt exist, im throwing an exception.
        this.JobLogger.Debug(Messages.DetermineIfShipCodeExistsMessage, false);
        var erpShipCode = dataRowShipVia[Data.ErpShipCodeColumn].ToString();
        var saGetShipViaListResponse = this.StandardApiCallWithLogging<
            SAGetShipViaListRequest,
            SAGetShipViaListResponse
        >(siteConnection, integrationJob, new SAGetShipViaListRequest(), "SAGetShipViaList");
        if (
            saGetShipViaListResponse.arrayShipVia == null
            || saGetShipViaListResponse.arrayShipVia.Length == 0
        )
        {
            throw new Exception(
                string.Format(Messages.SaGetShipViaListErrorExceptionMessage, "no ShipVias")
            );
        }

        // set the erpshipcode to use if we were able to make a match on sx
        var saGetShipViaListoutputShipVia = saGetShipViaListResponse.arrayShipVia.FirstOrDefault(
            x => x.code.Equals(erpShipCode, StringComparison.OrdinalIgnoreCase)
        );
        if (saGetShipViaListoutputShipVia != null)
        {
            erpShipCode = saGetShipViaListoutputShipVia.code;
            this.JobLogger.Debug(
                string.Format(Messages.GotErpShipCodeInfoMessage, erpShipCode),
                false
            );
        }

        // When addToOrder is flagged the address information is put into the order directly on order submit.  A customer isnt created, its just attached to the order.
        var isGuest = Convert.ToBoolean(
            initialDataset.Tables[Data.CustomerTable].Rows[0][Data.IsGuestColumn]
        );
        var isDropShip = Convert.ToBoolean(
            initialDataset.Tables[Data.CustomerTable].Rows[0][Data.IsDropShipColumn]
        );
        var addToOrder =
            (
                isGuest
                && integrationProcessorSubmitOrderSxInfo.CustomerIsGuestAction.Equals(
                    "AddToOrder",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            || (
                isDropShip
                && integrationProcessorSubmitOrderSxInfo.CustomerIsDropShipAction.Equals(
                    "AddToOrder",
                    StringComparison.OrdinalIgnoreCase
                )
            );
        this.JobLogger.Debug(string.Format(Messages.AddToOrderIsInfoMessage, addToOrder), false);

        var oeFullOrderMntV5Request = new OEFullOrderMntV5Request();
        this.AddOrderHeaderToRequest(
            oeFullOrderMntV5Request,
            initialDataset,
            integrationProcessorSubmitOrderSxInfo.CompanyNumber,
            integrationProcessorSubmitOrderSxInfo.CustomerDefaultWarehouse,
            erpShipCode,
            integrationProcessorSubmitOrderSxInfo.OrderNumberField
        );
        this.JobLogger.Debug(Messages.AddOrderHeaderToRequestCompletedMessage, false);

        this.AddBillToRequest(siteConnection, integrationJob, jobStep, oeFullOrderMntV5Request);
        this.JobLogger.Debug(Messages.AddBillToRequestCompletedMessage, false);

        this.AddCustomerToRequest(siteConnection, integrationJob, jobStep, oeFullOrderMntV5Request);
        this.JobLogger.Debug(Messages.AddCustomerToRequestCompletedMessage, false);

        this.AddShipToToRequest(
            siteConnection,
            integrationJob,
            jobStep,
            oeFullOrderMntV5Request,
            addToOrder
        );
        this.JobLogger.Debug(Messages.AddShipToToRequestCompletedMessage, false);

        this.AddMiscInfoToRequest(oeFullOrderMntV5Request, initialDataset);
        this.JobLogger.Debug(Messages.AddMiscInfoToRequestCompletedMessage, false);

        this.AddOrderLinesToRequest(
            siteConnection,
            integrationJob,
            jobStep,
            oeFullOrderMntV5Request
        );
        this.JobLogger.Debug(Messages.AddOrderLinesToRequestCompletedMessage, false);

        var oeFullOrderMntV5Response = this.StandardApiCallWithLogging<
            OEFullOrderMntV5Request,
            OEFullOrderMntV5Response
        >(siteConnection, integrationJob, oeFullOrderMntV5Request, "OEFullOrderMntV5");

        // return both the isc and sx erp ordernumber back to the website so isc can be updated
        return this.CreateResponseDataSet(
            oeFullOrderMntV5Response,
            dataRowCustomerOrder[Data.OrderNumberColumn].ToString()
        );
    }

    /// <summary>Retrieves the ERP_Company, ERP_AllowDuplicateCustomerPO, ERP_tWarehouse, ERP_OrderNumberField,
    /// ERP_CustomerIsDropShipAction, ERP_CustomerIsGuestAction parameters defined in the jobStep.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <returns>The <see cref="IntegrationProcessorCustomerSubmitInfo"/> with all of the parameters that are relevent to the order
    /// submit contained within.</returns>
    protected virtual IntegrationProcessorSubmitOrderSxInfo PopulateOrderSubmitParameters(
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

        var parmAllowDuplicateCustomerPo = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p =>
                p.Name.Equals(
                    Parameters.ErpAllowDuplicateCustomerPo,
                    StringComparison.OrdinalIgnoreCase
                )
        );
        if (parmAllowDuplicateCustomerPo == null)
        {
            this.JobLogger.Debug(
                string.Format(
                    Messages.ParameterNotFoundMessage,
                    Parameters.ErpAllowDuplicateCustomerPo
                ),
                false
            );
        }
        else if (parmAllowDuplicateCustomerPo.Value == null)
        {
            parmAllowDuplicateCustomerPo.Value = "True";
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

        var parmOrderNumberField = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.ErpOrderNumberField, StringComparison.OrdinalIgnoreCase)
        );
        if (parmOrderNumberField == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpOrderNumberField),
                false
            );
        }

        var parmCustomerIsDropShipAction = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p =>
                p.Name.Equals(
                    Parameters.ErpCustomerIsDropShipAction,
                    StringComparison.OrdinalIgnoreCase
                )
        );
        if (parmCustomerIsDropShipAction == null)
        {
            this.JobLogger.Debug(
                string.Format(
                    Messages.ParameterNotFoundMessage,
                    Parameters.ErpCustomerIsDropShipAction
                ),
                false
            );
        }

        var parmCustomerIsGuestAction = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p =>
                p.Name.Equals(
                    Parameters.ErpCustomerIsGuestAction,
                    StringComparison.OrdinalIgnoreCase
                )
        );
        if (parmCustomerIsGuestAction == null)
        {
            this.JobLogger.Debug(
                string.Format(
                    Messages.ParameterNotFoundMessage,
                    Parameters.ErpCustomerIsGuestAction
                ),
                false
            );
        }

        return new IntegrationProcessorSubmitOrderSxInfo
        {
            CompanyNumber = (parmCompanyNumber == null) ? string.Empty : parmCompanyNumber.Value,
            AllowDuplicateCustomerPo =
                parmAllowDuplicateCustomerPo != null
                && parmAllowDuplicateCustomerPo.Value.Equals(
                    "True",
                    StringComparison.OrdinalIgnoreCase
                ),
            CustomerDefaultWarehouse =
                (parmCustomerDefaultWarehouse == null)
                    ? string.Empty
                    : parmCustomerDefaultWarehouse.Value,
            OrderNumberField =
                (parmOrderNumberField == null) ? string.Empty : parmOrderNumberField.Value,
            CustomerIsDropShipAction =
                (parmCustomerIsDropShipAction == null)
                    ? string.Empty
                    : parmCustomerIsDropShipAction.Value,
            CustomerIsGuestAction =
                (parmCustomerIsGuestAction == null) ? string.Empty : parmCustomerIsGuestAction.Value
        };
    }

    /// <summary>Populate the OEFullOrderMntV5inputOrder object and attach it to the OEFullOrderMntV5Request.</summary>
    /// <param name="oeFullOrderMntV5Request">The full request object representing the customer order that is passed into the OEFullOrderMntV5
    /// method of the </param>
    /// <param name="dataSet">The customer order dataset from ISC.</param>
    /// <param name="companyNumber">The cono in context.</param>
    /// <param name="customerDefaultWarehouse">The default warehouse to use if its not defined in the ISC customer order.</param>
    /// <param name="erpShipCode">The ShipVia Code.</param>
    /// <param name="orderNumberField">The field on oeeh that we populate the ISC order # with. It typically is user3 but we allow variations.</param>
    protected virtual void AddOrderHeaderToRequest(
        OEFullOrderMntV5Request oeFullOrderMntV5Request,
        DataSet dataSet,
        string companyNumber,
        string customerDefaultWarehouse,
        string erpShipCode,
        string orderNumberField
    )
    {
        var dataRowCustomerOrder = dataSet.Tables[Data.CustomerOrderTable].Rows[0];
        var dataRowCurrency = dataSet.Tables[Data.CurrencyTable].Rows[0];
        double.TryParse(
            dataRowCustomerOrder[Data.DiscountAmountColumn].ToString(),
            out var parseDiscountAmount
        );
        double.TryParse(
            dataRowCustomerOrder[Data.ShippingColumn].ToString(),
            out var parseShipping
        );
        double.TryParse(
            dataRowCustomerOrder[Data.HandlingColumn].ToString(),
            out var parseHandling
        );

        var oeFullOrderMntV5InputOrder = new OEFullOrderMntV5inputOrder
        {
            actionType = "STOREFRONT",
            companyNumber = companyNumber,
            warehouse = string.IsNullOrEmpty(
                dataRowCustomerOrder[Data.DefaultWarehouseName].ToString()
            )
                ? customerDefaultWarehouse
                : dataRowCustomerOrder[Data.DefaultWarehouseName].ToString(),
            currencyCode = dataRowCurrency[Data.CurrencyCodeColumn].ToString(),
            shipVia = erpShipCode,
            buyer = dataRowCustomerOrder[Data.BuyerColumn].ToString(),
            purchaseOrderNumber = dataRowCustomerOrder[Data.CustomerPoColumn].ToString(),
            approvalType = "h",
            transactionType = "SO",
            orderDisposition = string.Empty,
            wholeOrderDiscountAmount = parseDiscountAmount,
            wholeOrderDiscountType = "$",
            addonAmount2 = parseShipping,
            addonNumber2 = 2,
            addonType2 = "$",
            addonAmount1 = parseHandling,
            addonNumber1 = 1,
            addonType1 = "$"
        };

        // the isc order number is stuck in a field that is dynamically defined as an application setting.
        try
        {
            var orderNumberProperty = oeFullOrderMntV5InputOrder
                .GetType()
                .GetProperty(orderNumberField.ToLower());
            orderNumberProperty.SetValue(
                oeFullOrderMntV5InputOrder,
                dataRowCustomerOrder[Data.OrderNumberColumn].ToString(),
                null
            );
        }
        catch (Exception)
        {
            // Could not find the property referenced by the application setting ERP_ISC_OrderNumberField.  Defaulting to User3
            oeFullOrderMntV5InputOrder.user3 = dataRowCustomerOrder[
                Data.OrderNumberColumn
            ].ToString();
        }

        oeFullOrderMntV5Request.arrayOrder = new[] { oeFullOrderMntV5InputOrder };
    }

    /// <summary>Populate the OEFullOrderMntV5inputBillTo object and attach it to the OEFullOrderMntV5Request.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="oeFullOrderMntV5Request">The full request object representing the customer order that is passed into the OEFullOrderMntV5
    /// method of the </param>
    protected virtual void AddBillToRequest(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        OEFullOrderMntV5Request oeFullOrderMntV5Request
    )
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        var dataRowBillto = initialDataset.Tables[Data.CustomerTable].Rows[0];
        var oeFullOrderMntV5InputBillTo = new OEFullOrderMntV5inputBillTo
        {
            billToNumber = dataRowBillto[Data.CustomerNumberColumn].ToString()
        };
        SxApiHelper.SetDynamicProperties(
            siteConnection,
            integrationJob,
            jobStep,
            oeFullOrderMntV5InputBillTo,
            new NameValueCollection { { Data.CustomerSequenceColumn, string.Empty } }
        );
        oeFullOrderMntV5Request.arrayBillTo = new[] { oeFullOrderMntV5InputBillTo };
    }

    /// <summary>Populate the OEFullOrderMntV5inputCustomer object and attach it to the OEFullOrderMntV5Request.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="oeFullOrderMntV5Request">The full request object representing the customer order that is passed into the OEFullOrderMntV5
    /// method of the </param>
    protected virtual void AddCustomerToRequest(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        OEFullOrderMntV5Request oeFullOrderMntV5Request
    )
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        var dataRowBillto = initialDataset.Tables[Data.CustomerTable].Rows[0];
        var oeFullOrderMntV5InputCustomer = new OEFullOrderMntV5inputCustomer
        {
            customerNumber = dataRowBillto[Data.CustomerNumberColumn].ToString()
        };
        SxApiHelper.SetDynamicProperties(
            siteConnection,
            integrationJob,
            jobStep,
            oeFullOrderMntV5InputCustomer,
            new NameValueCollection { { Data.CustomerSequenceColumn, string.Empty } }
        );
        oeFullOrderMntV5Request.arrayCustomer = new[] { oeFullOrderMntV5InputCustomer };
    }

    /// <summary>Populate the OEFullOrderMntV5inputShipTo object and attach it to the OEFullOrderMntV5Request.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="oeFullOrderMntV5Request">The full request object representing the customer order that is passed into the OEFullOrderMntV5
    /// method of the </param>
    /// <param name="addToOrder">Flags whether or not shipping information should be stuck on the order itself (and subsequently no customer cretaed)</param>
    protected virtual void AddShipToToRequest(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        OEFullOrderMntV5Request oeFullOrderMntV5Request,
        bool addToOrder
    )
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        var dataRowShipTo = initialDataset.Tables[Data.ShipToTable].Rows[0];
        var dataRowShipToState = initialDataset.Tables[Data.ShipToStateTable].Rows[0];
        var oeFullOrderMntV5InputShipTo = new OEFullOrderMntV5inputShipTo
        {
            shipToNumber = dataRowShipTo[Data.CustomerSequenceColumn].ToString()
        };
        if (addToOrder)
        {
            oeFullOrderMntV5InputShipTo.name = dataRowShipTo[Data.CompanyNameColumn].ToString();
            oeFullOrderMntV5InputShipTo.address1 = dataRowShipTo[Data.Address1Column].ToString();
            oeFullOrderMntV5InputShipTo.address2 = dataRowShipTo[Data.Address2Column].ToString();
            oeFullOrderMntV5InputShipTo.city = dataRowShipTo[Data.CityColumn].ToString();
            oeFullOrderMntV5InputShipTo.state = dataRowShipToState[
                Data.StateAbbreviationColumn
            ].ToString();
            oeFullOrderMntV5InputShipTo.postalCode = dataRowShipTo[
                Data.PostalCodeColumn
            ].ToString();
            oeFullOrderMntV5InputShipTo.phone = dataRowShipTo[Data.PhoneColumn].ToString();
        }

        SxApiHelper.SetDynamicProperties(
            siteConnection,
            integrationJob,
            jobStep,
            oeFullOrderMntV5InputShipTo,
            new NameValueCollection
            {
                {
                    Data.CustomerSequenceColumn,
                    dataRowShipTo[Data.CustomerSequenceColumn].ToString()
                }
            }
        );
        oeFullOrderMntV5Request.arrayShipTo = new[] { oeFullOrderMntV5InputShipTo };
    }

    /// <summary>Populate the OEFullOrderMntV5inputHeaderExtra and OEFullOrderMntV5inputTerms objects and attach them to the OEFullOrderMntV5Request.</summary>
    /// <param name="oeFullOrderMntV5Request">The full request object representing the customer order that is passed into the OEFullOrderMntV5
    /// method of the </param>
    /// <param name="dataSet">The customer order dataset from ISC.</param>
    protected virtual void AddMiscInfoToRequest(
        OEFullOrderMntV5Request oeFullOrderMntV5Request,
        DataSet dataSet
    )
    {
        // Notes
        var dataRowCustomerOrder = dataSet.Tables[Data.CustomerOrderTable].Rows[0];
        var headerExtras = new List<OEFullOrderMntV5inputHeaderExtra>();
        var orderNotes = dataRowCustomerOrder[Data.NotesColumn].ToString().Trim();
        for (var i = 0; i < (orderNotes.Length / 60) + 1; i++)
        {
            var oeFullOrderMntV5InputHeaderExtra = new OEFullOrderMntV5inputHeaderExtra
            {
                fieldName = "notes"
            };
            var remainingStringLength = orderNotes.Length - (i * 60);
            oeFullOrderMntV5InputHeaderExtra.fieldValue = string.Format(
                    "{0,-59}",
                    orderNotes.Substring(
                        i * 60,
                        remainingStringLength > 60 ? 60 : remainingStringLength
                    )
                )
                .Trim();
            oeFullOrderMntV5InputHeaderExtra.sequenceNumber = i + 1;
            headerExtras.Add(oeFullOrderMntV5InputHeaderExtra);
        }

        // Credit Card Payment
        if (
            dataSet.Tables.Contains(Data.CreditCardTransactionTable)
            && dataSet.Tables[Data.CreditCardTransactionTable].Rows.Count > 0
        )
        {
            var dataRowCreditCardTransaction = dataSet.Tables[Data.CreditCardTransactionTable].Rows[
                0
            ];
            var oeFullOrderMntV5InputHeaderExtra = new OEFullOrderMntV5inputHeaderExtra
            {
                fieldName = "authccdata",
                fieldValue = string.Format(
                    "acct={0}\texpdate={1}\tamt={2}\tpaytype={3}\tauthcode={4}\tpnref={5}",
                    dataRowCreditCardTransaction[Data.CreditCardNumberColumn],
                    dataRowCreditCardTransaction[Data.ExpirationDateColumn],
                    dataRowCreditCardTransaction[Data.AmountColumn],
                    this.TransformCardTypeToPayType(
                        dataRowCreditCardTransaction[Data.CardTypeColumn].ToString()
                    ),
                    dataRowCreditCardTransaction[Data.AuthCodeColumn],
                    dataRowCreditCardTransaction[Data.PnRefColumn]
                ),
                sequenceNumber = headerExtras.Count + 1
            };
            headerExtras.Add(oeFullOrderMntV5InputHeaderExtra);
        }

        if (headerExtras.Count > 0)
        {
            oeFullOrderMntV5Request.arrayHeaderExtra = headerExtras.ToArray();
        }

        // Terms
        var oeFullOrderMntV5InputTerms = new OEFullOrderMntV5inputTerms
        {
            sxEnterpriseTermsCode = dataRowCustomerOrder[Data.TermsCodeColumn].ToString()
        };
        oeFullOrderMntV5Request.arrayTerms = new[] { oeFullOrderMntV5InputTerms };
    }

    /// <summary>Populate the OEFullOrderMntV5inputItem's representing the order lines and attach them to the OEFullOrderMntV5Request.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="oeFullOrderMntV5Request">The full request object representing the customer order that is passed into the OEFullOrderMntV5
    /// method of the </param>
    protected virtual void AddOrderLinesToRequest(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        OEFullOrderMntV5Request oeFullOrderMntV5Request
    )
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        var oeFullOrderMntV5InputItems = new OEFullOrderMntV5inputItem[
            initialDataset.Tables[Data.OrderLineTable].Rows.Count
        ];
        var itemCount = 0;
        foreach (DataRow dataRowOrderLine in initialDataset.Tables[Data.OrderLineTable].Rows)
        {
            double.TryParse(
                dataRowOrderLine[Data.ProductDiscountPerEachColumn].ToString(),
                out var parseProductDiscount
            );

            double.TryParse(
                dataRowOrderLine[Data.UnitNetPriceColumn].ToString(),
                out var fullUnitCost
            );
            fullUnitCost += parseProductDiscount;

            var oeFullOrderMntV5InputItem = new OEFullOrderMntV5inputItem
            {
                lineIdentifier = (itemCount + 1).ToString(CultureInfo.InvariantCulture),
                sellerProductCode = dataRowOrderLine[Data.ErpNumberColumn].ToString(),
                quantityOrdered = dataRowOrderLine[Data.QtyOrderedColumn].ToString(),
                unitOfMeasure = dataRowOrderLine[Data.UnitOfMeasureColumn].ToString(),
                lineComments = dataRowOrderLine[Data.NotesColumn].ToString(),
                unitCost = fullUnitCost.ToString(CultureInfo.InvariantCulture),
                discountAmount = parseProductDiscount,
                discountType = true // true means amount, false means percent
            };
            SxApiHelper.SetDynamicProperties(
                siteConnection,
                integrationJob,
                jobStep,
                oeFullOrderMntV5InputItem,
                new NameValueCollection
                {
                    { Data.LineColumn, dataRowOrderLine[Data.LineColumn].ToString() }
                }
            );
            oeFullOrderMntV5InputItems[itemCount] = oeFullOrderMntV5InputItem;
            itemCount++;
        }

        oeFullOrderMntV5Request.arrayItem = oeFullOrderMntV5InputItems;
    }

    /// <summary>Create the response dataset to send back to the website for processing</summary>
    /// <param name="submitOrderResponse">The <see cref="OEFullOrderMntV5Response"/> containing information about what was updated for the order.</param>
    /// <param name="orderNumber">The ISC order number.</param>
    /// <returns>The response dataset to send back to the website for processing.</returns>
    protected virtual DataSet CreateResponseDataSet(
        OEFullOrderMntV5Response submitOrderResponse,
        string orderNumber
    )
    {
        var erpOrderNumber = string.Empty;
        if (submitOrderResponse.arrayHeader != null && submitOrderResponse.arrayHeader.Any())
        {
            erpOrderNumber =
                submitOrderResponse.arrayHeader[0].invoiceNumber
                + "-"
                + submitOrderResponse.arrayHeader[0].invoiceSuffix;
        }

        var dataTable = new DataTable(Data.OrderSubmitTable);
        dataTable.Columns.Add(Data.OrderNumberColumn);
        dataTable.Columns.Add(Data.ErpOrderNumberColumn);
        dataTable.Rows.Add(orderNumber, erpOrderNumber);

        var dataSet = new DataSet();
        dataSet.Tables.Add(dataTable);
        return dataSet;
    }

    /// <summary>Transform CreditCardTransaction.CardType into a PayType that SX expects for payment submit.</summary>
    /// <param name="cardType">The CreditCardTransaction.CardType associated with the order.</param>
    /// <returns>Translated 3,4,5,6 PayType used in SX payment submittal.</returns>
    protected virtual string TransformCardTypeToPayType(string cardType)
    {
        switch (cardType.ToLower())
        {
            case "mastercard":
                return "3";
            case "americanexpress":
                return "4";
            case "discover":
                return "5";
            case "visa":
                return "6";
            default:
                throw new ArgumentException(
                    string.Format(
                        Messages.UnableToTransformCardTypeToPayTypeExceptionMessage,
                        cardType
                    )
                );
        }
    }

    protected abstract TU StandardApiCallWithLogging<T, TU>(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        T request,
        string methodName,
        bool throwExceptionOnErrorMessage = true
    );
}
