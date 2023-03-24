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
using Insite.WIS.Epicor.Epicor10CashGrpService;
using Insite.WIS.Epicor.Epicor10CashRecService;
using Insite.WIS.Epicor.Epicor10CreditTranService;
using Insite.WIS.Epicor.Epicor10CustomerService;
using Insite.WIS.Epicor.Epicor10SalesOrderService;
using Insite.WIS.Epicor.Epicor10ShipToService;

using ShipToRow = Insite.WIS.Epicor.Epicor10ShipToService.ShipToRow;

public class IntegrationProcessorOrderSubmitEpicor10 : IIntegrationProcessor
{
#pragma warning disable SA1306
    protected IntegrationProcessorCoreEpicor10 IntegrationProcessorCore;
#pragma warning restore SA1306

#pragma warning disable SA1306
    protected IntegrationJobLogger JobLogger;
#pragma warning restore SA1306

    public virtual DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep
    )
    {
        this.JobLogger = new IntegrationJobLogger(siteConnection, integrationJob);

        this.IntegrationProcessorCore = new IntegrationProcessorCoreEpicor10(
            siteConnection,
            integrationJob
        );
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);
        try
        {
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
            )
            {
                throw new ArgumentException(Messages.InvalidInitialDataSetExceptionMessage);
            }

            // parse the incoming paramaters from the step and store into a more accessible class
            var integrationProcessorSubmitOrderEpicor10Info = this.PopulateOrderSubmitParameters(
                siteConnection,
                integrationJob,
                jobStep
            );
            var dataRowCustomerOrder = initialDataset.Tables[Data.CustomerOrderTable].Rows[0];
            var dataRowBillto = initialDataset.Tables[Data.CustomerTable].Rows[0];
            var dataRowShipto = initialDataset.Tables[Data.ShipToTable].Rows[0];

            // these checks was being made in the e9 order submit so im assuming its needed here too.
            if (!integrationProcessorSubmitOrderEpicor10Info.SubmitAllPaymentInfo)
            {
                throw new ArgumentException(Messages.SubmitAllPaymentInfoRequiredExceptionMessage);
            }

            if (!integrationProcessorSubmitOrderEpicor10Info.PromoMiscCharge)
            {
                throw new ArgumentException(Messages.PromoMiscChargeRequiredExceptionMessage);
            }

            // login/initialize
#pragma warning disable CS0618 // Type or member is obsolete
            this.IntegrationProcessorCore.Initialize(
                integrationJob.JobDefinition.IntegrationConnection.Url,
                integrationProcessorSubmitOrderEpicor10Info.CompanyNumber,
                integrationJob.JobDefinition.IntegrationConnection.LogOn,
                EncryptionHelper.DecryptAes(
                    integrationJob.JobDefinition.IntegrationConnection.Password
                )
            );
#pragma warning restore

            // determine if the order already exists in the epicor10 erp.  If it does not proceed with the submit, otherwise update the
            // ISC website with the allocated erpnumber.
            var orderNumber = dataRowCustomerOrder[Data.OrderNumberColumn].ToString();
            var orderAlreadyExists = this.OrderAlreadyExists(orderNumber);
            if (orderAlreadyExists.Item1)
            {
                this.JobLogger.Debug(
                    string.Format(Messages.OrderAlreadyExistsMessage, orderNumber),
                    false
                );
                return this.CreateResponseDataSet(orderNumber, orderAlreadyExists.Item2);
            }

            // lookup both the customer and shipto to be used in the population of the order submit.
            // im assuming in step 1 of the order submit, the customer/shipto was already created.
            CustomerRow customerRow;
            ShipToRow shipToRow;
            if (integrationProcessorSubmitOrderEpicor10Info.UseStaticCustomer)
            {
                var customerTableset = this.GetCustomer(
                    integrationProcessorSubmitOrderEpicor10Info.StaticCustomerNumber
                );
                if (customerTableset.Customer.Count == 0)
                {
                    throw new ArgumentException(
                        string.Format(
                            Messages.StaticCustomerNotFoundExceptionMessage,
                            integrationProcessorSubmitOrderEpicor10Info.StaticCustomerNumber
                        )
                    );
                }

                customerRow = customerTableset.Customer[0];
                var erpCustomerNumber = customerRow.CustNum;
                var erpCustomerSequence = dataRowShipto[Data.CustomerSequenceColumn]
                    .ToString()
                    .Equals(string.Empty)
                    ? dataRowShipto[Data.CustomerNumberColumn].ToString()
                    : dataRowShipto[Data.CustomerNumberColumn]
                        + ":"
                        + dataRowShipto[Data.CustomerSequenceColumn];
                var shipToTableset = this.GetShipTo(erpCustomerNumber, erpCustomerSequence);
                if (shipToTableset.ShipTo.Count == 0)
                {
                    throw new ArgumentException(
                        string.Format(
                            Messages.CustomerAlreadyExistsMessage,
                            erpCustomerNumber,
                            erpCustomerSequence,
                            " not"
                        )
                    );
                }

                shipToRow = shipToTableset.ShipTo[0];
            }
            else
            {
                var customerTableset = this.GetCustomer(
                    dataRowBillto[Data.CustomerNumberColumn].ToString()
                );
                customerRow = customerTableset.Customer[0];
                var erpCustomerNumber = customerRow.CustNum;
                var erpCustomerSequence = dataRowShipto[Data.CustomerSequenceColumn].ToString();
                var shipToTableset = this.GetShipTo(erpCustomerNumber, erpCustomerSequence);
                if (shipToTableset.ShipTo.Count == 0)
                {
                    throw new ArgumentException(
                        string.Format(
                            Messages.CustomerAlreadyExistsMessage,
                            erpCustomerNumber,
                            erpCustomerSequence,
                            " not"
                        )
                    );
                }

                shipToRow = shipToTableset.ShipTo[0];
            }

            // submit the order
            var salesOrderTableset = new SalesOrderTableset();
            using (
                var client = this.IntegrationProcessorCore.GetClient<
                    SalesOrderSvcContractClient,
                    SalesOrderSvcContract
                >(IntegrationProcessorCoreEpicor10.SalesOrderServicePath)
            )
            {
                try
                {
                    client.Endpoint.EndpointBehaviors.Add(
                        new HookServiceBehavior(
                            this.IntegrationProcessorCore.SessionId,
                            this.IntegrationProcessorCore.Username,
                            this.IntegrationProcessorCore.SiteConnection,
                            this.IntegrationProcessorCore.IntegrationJob
                        )
                    );
                    this.AddOrderHeaderToRequest(
                        client,
                        ref salesOrderTableset,
                        customerRow,
                        shipToRow,
                        integrationJob,
                        integrationProcessorSubmitOrderEpicor10Info
                    );
                    this.JobLogger.Debug(Messages.AddOrderHeaderToRequestCompletedMessage, false);

                    this.AddMiscInfoToRequest(
                        client,
                        ref salesOrderTableset,
                        integrationJob,
                        integrationProcessorSubmitOrderEpicor10Info
                    );
                    this.JobLogger.Debug(Messages.AddMiscInfoToRequestCompletedMessage, false);

                    this.AddOrderLinesToRequest(
                        client,
                        ref salesOrderTableset,
                        customerRow,
                        integrationJob,
                        integrationProcessorSubmitOrderEpicor10Info
                    );
                    this.JobLogger.Debug(Messages.AddOrderLinesToRequestCompletedMessage, false);

                    this.AddPaymentToRequest(
                        client,
                        ref salesOrderTableset,
                        integrationJob,
                        integrationProcessorSubmitOrderEpicor10Info
                    );
                    this.JobLogger.Debug(Messages.AddPaymentToRequestCompletedMessage, false);

                    this.FinalizeOrder(client, ref salesOrderTableset);
                    this.JobLogger.Debug(Messages.FinalizeOrderCompletedMessage, false);
                }
                catch (Exception)
                {
                    this.JobLogger.Debug(Messages.RollingBackOrderDueToExceptionMessage, false);
                    this.RollbackSubmitOrder(client, ref salesOrderTableset);
                    throw;
                }
            }

            return this.CreateResponseDataSet(
                orderNumber,
                salesOrderTableset.OrderHed[0].OrderNum.ToString(CultureInfo.InvariantCulture)
            );
        }
        finally
        {
            this.IntegrationProcessorCore.EndSession();
        }
    }

    /// <summary>The populate order submit parameters.</summary>
    /// <param name="siteConnection">The site connection.</param>
    /// <param name="integrationJob">The integration job.</param>
    /// <param name="jobStep">The job step.</param>
    /// <returns>The <see cref="IntegrationProcessorSubmitOrderEpicor10Info"/>.</returns>
    protected virtual IntegrationProcessorSubmitOrderEpicor10Info PopulateOrderSubmitParameters(
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

        var parmSubmitAllPaymentInfo = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.SubmitAllPaymentInfo, StringComparison.OrdinalIgnoreCase)
        );
        if (parmSubmitAllPaymentInfo == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.SubmitAllPaymentInfo),
                false
            );
        }

        var parmSubmitToReviewStatus = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.SubmitToReviewStatus, StringComparison.OrdinalIgnoreCase)
        );
        if (parmSubmitToReviewStatus == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.SubmitToReviewStatus),
                false
            );
        }

        var parmPromoMiscCharge = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.ErpPromoMiscCharge, StringComparison.OrdinalIgnoreCase)
        );
        if (parmPromoMiscCharge == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpPromoMiscCharge),
                false
            );
        }

        var parmFreightEstimated = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.ErpFreightEstimated, StringComparison.OrdinalIgnoreCase)
        );
        if (parmFreightEstimated == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpFreightEstimated),
                false
            );
        }

        var parmSubmitSaleTransaction = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.SubmitSaleTransaction, StringComparison.OrdinalIgnoreCase)
        );
        if (parmSubmitSaleTransaction == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.SubmitSaleTransaction),
                false
            );
        }

        var parmFreightCode = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.ErpFreightCode, StringComparison.OrdinalIgnoreCase)
        );
        if (parmFreightCode == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpFreightCode),
                false
            );
        }

        var parmPromoMiscChargeCode = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p =>
                p.Name.Equals(Parameters.ErpPromoMiscChargeCode, StringComparison.OrdinalIgnoreCase)
        );
        if (parmPromoMiscChargeCode == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpPromoMiscChargeCode),
                false
            );
        }

        var parmUseStaticCustomer = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.ErpUseStaticCustomer, StringComparison.OrdinalIgnoreCase)
        );
        if (parmUseStaticCustomer == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpUseStaticCustomer),
                false
            );
        }

        var parmStaticCustomerNumber = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p =>
                p.Name.Equals(
                    Parameters.ErpStaticCustomerNumber,
                    StringComparison.OrdinalIgnoreCase
                )
        );
        if (parmStaticCustomerNumber == null)
        {
            this.JobLogger.Debug(
                string.Format(
                    Messages.ParameterNotFoundMessage,
                    Parameters.ErpStaticCustomerNumber
                ),
                false
            );
        }

        var parmCreditCardCashAccountId = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p =>
                p.Name.Equals(
                    Parameters.ErpCreditCardCashAccountId,
                    StringComparison.OrdinalIgnoreCase
                )
        );
        if (parmCreditCardCashAccountId == null)
        {
            this.JobLogger.Debug(
                string.Format(
                    Messages.ParameterNotFoundMessage,
                    Parameters.ErpCreditCardCashAccountId
                ),
                false
            );
        }

        var parmCreditCardChart = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals(Parameters.ErpCreditCardChart, StringComparison.OrdinalIgnoreCase)
        );
        if (parmCreditCardChart == null)
        {
            this.JobLogger.Debug(
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpCreditCardChart),
                false
            );
        }

        return new IntegrationProcessorSubmitOrderEpicor10Info
        {
            CompanyNumber = (parmCompanyNumber == null) ? string.Empty : parmCompanyNumber.Value,
            SubmitAllPaymentInfo =
                parmSubmitAllPaymentInfo != null
                && parmSubmitAllPaymentInfo.Value.Equals(
                    "True",
                    StringComparison.OrdinalIgnoreCase
                ),
            SubmitToReviewStatus =
                parmSubmitToReviewStatus != null
                && parmSubmitToReviewStatus.Value.Equals(
                    "True",
                    StringComparison.OrdinalIgnoreCase
                ),
            PromoMiscCharge =
                parmPromoMiscCharge != null
                && parmPromoMiscCharge.Value.Equals("True", StringComparison.OrdinalIgnoreCase),
            FreightEstimated =
                parmFreightEstimated != null
                && parmFreightEstimated.Value.Equals("True", StringComparison.OrdinalIgnoreCase),
            SubmitSaleTransaction =
                parmSubmitSaleTransaction != null
                && parmSubmitSaleTransaction.Value.Equals(
                    "True",
                    StringComparison.OrdinalIgnoreCase
                ),
            FreightCode = (parmFreightCode == null) ? string.Empty : parmFreightCode.Value,
            PromoMiscChargeCode =
                (parmPromoMiscChargeCode == null) ? string.Empty : parmPromoMiscChargeCode.Value,
            UseStaticCustomer =
                parmUseStaticCustomer != null
                && parmUseStaticCustomer.Value.Equals("True", StringComparison.OrdinalIgnoreCase),
            StaticCustomerNumber =
                (parmStaticCustomerNumber == null) ? string.Empty : parmStaticCustomerNumber.Value,
            CreditCardCashAccountId =
                (parmCreditCardCashAccountId == null)
                    ? string.Empty
                    : parmCreditCardCashAccountId.Value,
            CreditCardChart =
                (parmCreditCardChart == null) ? string.Empty : parmCreditCardChart.Value
        };
    }

    protected virtual Tuple<bool, string> OrderAlreadyExists(string orderNumber)
    {
        using (
            var client = this.IntegrationProcessorCore.GetClient<
                SalesOrderSvcContractClient,
                SalesOrderSvcContract
            >(IntegrationProcessorCoreEpicor10.SalesOrderServicePath)
        )
        {
            client.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    this.IntegrationProcessorCore.SessionId,
                    this.IntegrationProcessorCore.Username,
                    this.IntegrationProcessorCore.SiteConnection,
                    this.IntegrationProcessorCore.IntegrationJob
                )
            );
            var whereClause = "WebEntryPerson = '" + orderNumber + "'";
            var excludeClause = this.IntegrationProcessorCore.ExcludeClause;
            var salesOrderTableset = client.GetRows(
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
                0,
                0,
                out var morePages
            );
            return salesOrderTableset.OrderHed.Count == 0
                ? Tuple.Create(false, string.Empty)
                : Tuple.Create(
                    true,
                    salesOrderTableset.OrderHed[0].OrderNum.ToString(CultureInfo.InvariantCulture)
                );
        }
    }

    protected virtual CustomerTableset GetCustomer(string customerNumber)
    {
        using (
            var client = this.IntegrationProcessorCore.GetClient<
                CustomerSvcContractClient,
                CustomerSvcContract
            >(IntegrationProcessorCoreEpicor10.CustomerServicePath)
        )
        {
            client.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    this.IntegrationProcessorCore.SessionId,
                    this.IntegrationProcessorCore.Username,
                    this.IntegrationProcessorCore.SiteConnection,
                    this.IntegrationProcessorCore.IntegrationJob
                )
            );
            var whereClause = "CustID = '" + customerNumber + "'";
            var excludeClause = this.IntegrationProcessorCore.ExcludeClause;
            var customerTableset = client.GetRows(
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
            return customerTableset;
        }
    }

    protected virtual ShipToTableset GetShipTo(int erpCustomerNumber, string customerSequence)
    {
        using (
            var client = this.IntegrationProcessorCore.GetClient<
                ShipToSvcContractClient,
                ShipToSvcContract
            >(IntegrationProcessorCoreEpicor10.ShipToServicePath)
        )
        {
            client.Endpoint.EndpointBehaviors.Add(
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
            var shipToTableset = client.GetRows(
                whereClause,
                excludeClause,
                excludeClause,
                0,
                0,
                out var morePages
            );
            return shipToTableset;
        }
    }

    protected virtual CashGrpTableset GetOrCreateCashGroup(string groupId, string bankAcctId)
    {
        using (
            var client = this.IntegrationProcessorCore.GetClient<
                CashGrpSvcContractClient,
                CashGrpSvcContract
            >(IntegrationProcessorCoreEpicor10.CashGroupServicePath)
        )
        {
            client.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    this.IntegrationProcessorCore.SessionId,
                    this.IntegrationProcessorCore.Username,
                    this.IntegrationProcessorCore.SiteConnection,
                    this.IntegrationProcessorCore.IntegrationJob
                )
            );
            var whereClause = "GroupID = '" + groupId + "'";
            var cashGrpTableset = client.GetRows(whereClause, 0, 0, out var morePages);
            if (cashGrpTableset.CashGrp.Count == 0)
            {
                client.GetNewCashGrp(ref cashGrpTableset);
                var cashGrp = cashGrpTableset.CashGrp[0];
                cashGrp.GroupID = groupId;
                cashGrp.TranDate = DateTime.Today;
                cashGrp.BankAcctID = bankAcctId;
                cashGrpTableset.CashGrp[0] = cashGrp;
            }

            client.Update(ref cashGrpTableset);
            return cashGrpTableset;
        }
    }

    protected virtual void SubmitCashReceipt(
        CashGrpRow cashGrpRow,
        OrderHedRow orderHedRow,
        string groupId,
        DataRow creditCardTransactionRow,
        string creditCardChart
    )
    {
        const string transType = "Deposit";
        var creditCardAmount = Convert.ToDecimal(creditCardTransactionRow[Data.AmountColumn]);
        using (
            var client = this.IntegrationProcessorCore.GetClient<
                CashRecSvcContractClient,
                CashRecSvcContract
            >(IntegrationProcessorCoreEpicor10.CashRecServicePath)
        )
        {
            client.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    this.IntegrationProcessorCore.SessionId,
                    this.IntegrationProcessorCore.Username,
                    this.IntegrationProcessorCore.SiteConnection,
                    this.IntegrationProcessorCore.IntegrationJob
                )
            );

            var cashRecTableset = new CashRecTableset();
            client.GetNewCashHeadType(ref cashRecTableset, groupId, transType);

            var cashHeadType = cashRecTableset.CashHead[0];
            cashHeadType.GroupID = groupId;
            cashHeadType.TranType = transType;
            cashHeadType.OrderNum = orderHedRow.OrderNum;
            cashHeadType.CustNum = orderHedRow.CustNum;
            cashHeadType.CustID = orderHedRow.CustomerCustID;
            cashHeadType.TranDate = orderHedRow.OrderDate;
            cashHeadType.FiscalPeriod = cashGrpRow.FiscalPeriod;
            cashHeadType.FiscalYear = cashGrpRow.FiscalYear;
            cashHeadType.BankAcctID = cashGrpRow.BankAcctID;
            cashHeadType.CheckRef = orderHedRow.WebEntryPerson;
            cashHeadType.TranAmt = creditCardAmount;
            cashHeadType.DocTranAmt = creditCardAmount;
            cashRecTableset.CashHead[0] = cashHeadType;

            var cashHeadTglc = cashRecTableset.CashHeadTGLC[0];
            cashHeadTglc.GLAccount = creditCardChart;
            var creditCardChareSegments = creditCardChart.Split('|');
            cashHeadTglc.SegValue1 = creditCardChareSegments[0];
            cashHeadTglc.SegValue2 = creditCardChareSegments[1];
            cashHeadTglc.SegValue3 = creditCardChareSegments[2];
            cashRecTableset.CashHeadTGLC[0] = cashHeadTglc;

            client.Update(ref cashRecTableset);
        }
    }

    protected virtual void SubmitPaymentAuthorization(
        SalesOrderTableset salesOrderTableset,
        DataRow customerOrderRow,
        DataRow creditCardTransactionRow,
        string company
    )
    {
        var orderHedRow = salesOrderTableset.OrderHed[0];
        var transactionDate = Convert.ToDateTime(
            creditCardTransactionRow[Data.TransactionDateColumn]
        );
        var span = transactionDate.TimeOfDay;
        var transactionTime = (((span.Hours * 60) + span.Minutes) * 60) + span.Seconds;
        var token1 = creditCardTransactionRow[Data.Token1Column].ToString();
        var token2 = creditCardTransactionRow[Data.Token2Column].ToString();
        var creditCardAmount = Convert.ToDecimal(creditCardTransactionRow[Data.AmountColumn]);
        var freight = Convert.ToDecimal(customerOrderRow[Data.ShippingAndHandlingColumn]);
        var tax = Convert.ToDecimal(customerOrderRow[Data.TotalTaxColumn]);
        var subTotal = creditCardAmount - freight - tax;

        CreditTranRow creditTran;
        using (
            var client = this.IntegrationProcessorCore.GetClient<
                CreditTranSvcContractClient,
                CreditTranSvcContract
            >(IntegrationProcessorCoreEpicor10.CreditTranServicePath)
        )
        {
            client.Endpoint.EndpointBehaviors.Add(
                new HookServiceBehavior(
                    this.IntegrationProcessorCore.SessionId,
                    this.IntegrationProcessorCore.Username,
                    this.IntegrationProcessorCore.SiteConnection,
                    this.IntegrationProcessorCore.IntegrationJob
                )
            );

            var creditTranTableset = new CreditTranTableset();
            client.GetNewCreditTran(ref creditTranTableset, transactionDate, transactionTime);
            creditTran = creditTranTableset.CreditTran[0];
            creditTran.Company = company;
            creditTran.OrderNum = orderHedRow.OrderNum;
            creditTran.CustNum = orderHedRow.CustNum;
            creditTran.CustID = orderHedRow.CustomerCustID;
            creditTran.TranType = "A";
            creditTran.TranTypeDesc = "Authorize";
            creditTran.CardNumber = string.IsNullOrEmpty(token1)
                ? creditCardTransactionRow[Data.CreditCardNumberColumn].ToString()
                : token1.Split('|')[0];
            creditTran.CardStore = token2;
            creditTran.TranTotal = creditCardAmount;
            creditTran.DocTranTotal = creditCardAmount;
            creditTran.Freight = freight;
            creditTran.DocFreight = freight;
            creditTran.Tax = tax;
            creditTran.DocTax = tax;
            creditTran.Amount = subTotal;
            creditTran.DocAmount = subTotal;
            creditTran.CardType = creditCardTransactionRow[Data.RequestStringColumn].ToString(); // using RequestString to store the card type
            creditTran.CardMemberName = creditCardTransactionRow[Data.NameColumn].ToString();
            creditTran.ExpMonth = Convert.ToInt32(
                creditCardTransactionRow[Data.ExpirationDateColumn].ToString().Substring(0, 2)
            );
            creditTran.ExpYear =
                2000
                + Convert.ToInt32(
                    creditCardTransactionRow[Data.ExpirationDateColumn].ToString().Substring(2, 2)
                );
            creditTran.StAddress = customerOrderRow[Data.BtAddress1Column].ToString();
            creditTran.Zip = customerOrderRow[Data.BtPostalCodeColumn].ToString();
            creditTran.PNRef = creditCardTransactionRow[Data.PnRefColumn].ToString();
            creditTran.AuthCode = creditCardTransactionRow[Data.AuthCodeColumn].ToString();
            creditTran.Result = creditCardTransactionRow[Data.ResultColumn].ToString();
            creditTran.ResponseMsg = creditCardTransactionRow[Data.RespMsgColumn].ToString();
            creditTran.AVSAddr = creditCardTransactionRow[Data.AvsAddrColumn].ToString();
            creditTran.AVSZip = creditCardTransactionRow[Data.AvsZipColumn].ToString();
            creditTran.CSCMatch = creditCardTransactionRow[Data.Cvv2MatchColumn].ToString();
            creditTran.Rpt1TranTotal = creditCardAmount;
            creditTran.Rpt2TranTotal = creditCardAmount;
            creditTran.Rpt3TranTotal = creditCardAmount;
            creditTran.Rpt1Amount = subTotal;
            creditTran.Rpt2Amount = subTotal;
            creditTran.Rpt3Amount = subTotal;
            creditTran.Rpt1Tax = tax;
            creditTran.Rpt2Tax = tax;
            creditTran.Rpt3Tax = tax;
            creditTran.Rpt1Freight = freight;
            creditTran.Rpt2Freight = freight;
            creditTran.Rpt3Freight = freight;
            creditTran.TranSuccess = true;
            creditTran.CurrencyCode = orderHedRow.CurrencyCode;
            creditTranTableset.CreditTran[0] = creditTran;
            client.Update(ref creditTranTableset);
        }

        orderHedRow.CreditCardOrder = true;
        orderHedRow.CCTotal = creditCardAmount;
        orderHedRow.CCDocTotal = creditCardAmount;
        orderHedRow.CCFreight = freight;
        orderHedRow.CCDocFreight = freight;
        orderHedRow.CCTax = tax;
        orderHedRow.CCDocTax = tax;
        orderHedRow.CCAmount = subTotal;
        orderHedRow.CCDocAmount = subTotal;
        orderHedRow.CardMemberName = creditTran.CardMemberName;
        orderHedRow.CardType = creditTran.CardType;
        orderHedRow.CardStore = token1;
        orderHedRow.CCCSCIDToken = token2;
        orderHedRow.ExpirationMonth = creditTran.ExpMonth;
        orderHedRow.ExpirationYear = creditTran.ExpYear;
        orderHedRow.CCStreetAddr = customerOrderRow[Data.BtAddress1Column].ToString();
        orderHedRow.CCZip = customerOrderRow[Data.BtPostalCodeColumn].ToString();
        orderHedRow.CardNumber = creditTran.CardNumber;
        orderHedRow.CCCSCID = "****";
        orderHedRow.CCResponse = creditTran.ResponseMsg;
        orderHedRow.CCApprovalNum = creditTran.AuthCode;
        orderHedRow.AVSAddr = creditTran.AVSAddr;
        orderHedRow.AVSZip = creditTran.AVSZip;
        orderHedRow.CSCResult = creditTran.CSCMatch;
        orderHedRow.Rpt1CCTotal = creditCardAmount;
        orderHedRow.Rpt2CCTotal = creditCardAmount;
        orderHedRow.Rpt3CCTotal = creditCardAmount;
        orderHedRow.Rpt1CCAmount = subTotal;
        orderHedRow.Rpt2CCAmount = subTotal;
        orderHedRow.Rpt3CCAmount = subTotal;
        orderHedRow.Rpt1CCTax = tax;
        orderHedRow.Rpt2CCTax = tax;
        orderHedRow.Rpt3CCTax = tax;
        orderHedRow.Rpt1CCFreight = freight;
        orderHedRow.Rpt2CCFreight = freight;
        orderHedRow.Rpt3CCFreight = freight;
        orderHedRow.CCTranID = creditTran.TranNum.ToString(CultureInfo.InvariantCulture);
    }

    protected virtual void SubmitConfiguration(
        SalesOrderSvcContractClient client,
        ref SalesOrderTableset salesOrderTableset
    )
    {
        // per tom we dont worry about this until somebody actually uses it.
    }

    protected virtual DataSet CreateResponseDataSet(string orderNumber, string erpOrderNumber)
    {
        var dataTable = new DataTable(Data.OrderSubmitTable);
        dataTable.Columns.Add(Data.OrderNumberColumn);
        dataTable.Columns.Add(Data.ErpOrderNumberColumn);
        dataTable.Rows.Add(orderNumber, erpOrderNumber);

        var dataSet = new DataSet();
        dataSet.Tables.Add(dataTable);
        return dataSet;
    }

    protected virtual void AddOrderHeaderToRequest(
        SalesOrderSvcContractClient client,
        ref SalesOrderTableset salesOrderTableset,
        CustomerRow customerRow,
        ShipToRow shipToRow,
        IntegrationJob integrationJob,
        IntegrationProcessorSubmitOrderEpicor10Info integrationProcessorSubmitOrderEpicor10Info
    )
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        var dataRowCustomerOrder = initialDataset.Tables[Data.CustomerOrderTable].Rows[0];
        DataTable customerOrderPropertyTable = null;
        if (initialDataset.Tables.Contains(Data.CustomerOrderPropertyTable))
        {
            customerOrderPropertyTable = initialDataset.Tables[Data.CustomerOrderPropertyTable];
        }

        var orderNumber = dataRowCustomerOrder[Data.OrderNumberColumn].ToString();

        client.GetNewOrderHed(ref salesOrderTableset);
        var orderHed = salesOrderTableset.OrderHed[0];

        orderHed.CustomerCustID = customerRow.CustID;
        client.ChangeSoldToID(ref salesOrderTableset);
        orderHed.CustNum = customerRow.CustNum;
        client.ChangeCustomer(ref salesOrderTableset);
        orderHed.ShipToNum = shipToRow.ShipToNum;
        client.ChangeShipToID(ref salesOrderTableset);

        orderHed.Company = integrationProcessorSubmitOrderEpicor10Info.CompanyNumber;
        orderHed.CustNum = customerRow.CustNum;
        orderHed.PrcConNum = customerRow.PrimPCon;
        orderHed.BTCustNum = customerRow.CustNum;
        orderHed.BTConNum = customerRow.PrimBCon;
        orderHed.ShipToNum = shipToRow.ShipToNum;
        orderHed.ShpConNum = shipToRow.PrimSCon;
        orderHed.ShipToCustNum = customerRow.CustNum;
        orderHed.WebEntryPerson = orderNumber;
        orderHed.OrderDate = Convert.ToDateTime(dataRowCustomerOrder[Data.OrderDateColumn]);
        orderHed.PONum = dataRowCustomerOrder[Data.CustomerPoColumn].ToString();
        if (orderHed.PONum == string.Empty)
        {
            orderHed.PONum = dataRowCustomerOrder[Data.OrderNumberColumn].ToString();
        }

        orderHed.WebOrder = true;
        orderHed.ApplyOrderBasedDisc = false;
        orderHed.CurrencyCode = dataRowCustomerOrder[Data.CurrencyCodeColumn].ToString();
        orderHed.EntryProcess = "web";
        orderHed.TermsCode = dataRowCustomerOrder[Data.TermsCodeColumn].ToString();
        orderHed.ShipViaCode = dataRowCustomerOrder[Data.ErpShipCodeColumn].ToString();
        var taxRegion = dataRowCustomerOrder[Data.TaxCode1Column].ToString();
        if (taxRegion.Trim() == "NT" || taxRegion.Trim() == string.Empty)
        {
            taxRegion = string.Empty;
        }

        orderHed.TaxRegionCode = taxRegion;
        orderHed.OrderComment = dataRowCustomerOrder[Data.NotesColumn].ToString();
        orderHed.ResDelivery = Convert.ToBoolean(dataRowCustomerOrder[Data.ResidentialColumn]);
        if (!Convert.IsDBNull(dataRowCustomerOrder[Data.RequestedShipDateColumn]))
        {
            orderHed.RequestDate = Convert.ToDateTime(
                dataRowCustomerOrder[Data.RequestedShipDateColumn]
            );
            orderHed.NeedByDate = orderHed.RequestDate;
        }

        if (integrationProcessorSubmitOrderEpicor10Info.SubmitToReviewStatus)
        {
            orderHed.OrderHeld = true;
        }
        else
        {
            orderHed.OpenOrder = true;
        }

        var collectAccountNumber = dataRowCustomerOrder[Data.CollectAccountNumberColumn].ToString();
        if (!string.IsNullOrEmpty(collectAccountNumber))
        {
            orderHed.PayAccount = collectAccountNumber;
        }
        else if (!integrationProcessorSubmitOrderEpicor10Info.FreightEstimated)
        {
            orderHed.PayFlag = "SHIP";
        }
        else
        {
            // When we are estimating freight and there is a free freight promotion, we need to set the PayFlag to "SHIP".
            // I am identifying this case by a non-zero PromotionShippingDiscountTotal with a zero "ShippingAndHandling".
            if (
                Convert.ToDecimal(dataRowCustomerOrder[Data.PromotionShippingDiscountTotalColumn])
                    > 0
                && Convert.ToDecimal(dataRowCustomerOrder[Data.ShippingAndHandlingColumn]) == 0
            )
            {
                orderHed.PayFlag = "SHIP";
            }
        }

        if (customerOrderPropertyTable != null)
        {
            foreach (DataRow customerOrderPropertyRow in customerOrderPropertyTable.Rows)
            {
                if (
                    customerOrderPropertyRow[Data.NameColumn]
                        .ToString()
                        .Equals("FOB", StringComparison.OrdinalIgnoreCase)
                )
                {
                    orderHed.FOB = customerOrderPropertyRow[Data.ValueColumn].ToString();
                    break;
                }
            }
        }

        salesOrderTableset.OrderHed[0] = orderHed;
        client.Update(ref salesOrderTableset);
    }

    protected virtual void AddMiscInfoToRequest(
        SalesOrderSvcContractClient client,
        ref SalesOrderTableset salesOrderTableset,
        IntegrationJob integrationJob,
        IntegrationProcessorSubmitOrderEpicor10Info integrationProcessorSubmitOrderEpicor10Info
    )
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        var dataRowCustomerOrder = initialDataset.Tables[Data.CustomerOrderTable].Rows[0];

        // shipping and handling
        if (!integrationProcessorSubmitOrderEpicor10Info.FreightEstimated)
        {
            var shippingAndHandling = Convert.ToDecimal(
                dataRowCustomerOrder[Data.ShippingAndHandlingColumn]
            );
            if (shippingAndHandling > 0)
            {
                client.GetNewOHOrderMsc(
                    ref salesOrderTableset,
                    salesOrderTableset.OrderHed[0].OrderNum,
                    0
                );
                var orderMsc = salesOrderTableset.OHOrderMsc[0];
                orderMsc.Company = integrationProcessorSubmitOrderEpicor10Info.CompanyNumber;
                orderMsc.OrderLine = 0;
                orderMsc.SeqNum = 1;
                orderMsc.MiscCode = integrationProcessorSubmitOrderEpicor10Info.FreightCode;
                orderMsc.Description = "Freight";
                orderMsc.FreqCode = "F"; // F = First time only
                orderMsc.Type = "A"; // A = Amount
                orderMsc.MiscAmt = shippingAndHandling;
                orderMsc.DocMiscAmt = shippingAndHandling;
                salesOrderTableset.OHOrderMsc[0] = orderMsc;
            }
        }

        // promotions
        if (integrationProcessorSubmitOrderEpicor10Info.PromoMiscCharge)
        {
            var promotionGrandTotal =
                Convert.ToDecimal(dataRowCustomerOrder[Data.PromotionTotalColumn])
                - Convert.ToDecimal(
                    dataRowCustomerOrder[Data.PromotionShippingDiscountTotalColumn]
                );
            if (promotionGrandTotal > 0)
            {
                client.GetNewOHOrderMsc(
                    ref salesOrderTableset,
                    salesOrderTableset.OrderHed[0].OrderNum,
                    0
                );
                var orderMsc = salesOrderTableset.OHOrderMsc[1];
                orderMsc.Company = integrationProcessorSubmitOrderEpicor10Info.CompanyNumber;
                orderMsc.OrderLine = 0;
                orderMsc.SeqNum = 2;
                orderMsc.MiscCode = integrationProcessorSubmitOrderEpicor10Info.PromoMiscChargeCode;
                orderMsc.Description = "Promotional Discount";
                orderMsc.FreqCode = "F"; // F = First time only
                orderMsc.Type = "A"; // A = Amount
                orderMsc.MiscAmt = -promotionGrandTotal;
                orderMsc.DocMiscAmt = -promotionGrandTotal;
                salesOrderTableset.OHOrderMsc[1] = orderMsc;
            }
        }

        client.Update(ref salesOrderTableset);
    }

    protected virtual void AddOrderLinesToRequest(
        SalesOrderSvcContractClient client,
        ref SalesOrderTableset salesOrderTableset,
        CustomerRow customerRow,
        IntegrationJob integrationJob,
        IntegrationProcessorSubmitOrderEpicor10Info integrationProcessorSubmitOrderEpicor10Info
    )
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        var orderLineRows = initialDataset.Tables[Data.OrderLineTable].Rows;
        var productTable = initialDataset.Tables[Data.ProductTable];
        var productPropertyTable = initialDataset.Tables[Data.ProductPropertyTable];

        Func<string, DataRow> getProductDataRow = productId =>
        {
            foreach (DataRow productRow in productTable.Rows)
            {
                if (
                    productRow[Data.ProductIdColumn]
                        .ToString()
                        .Equals(productId, StringComparison.OrdinalIgnoreCase)
                )
                {
                    return productRow;
                }
            }

            return null;
        };

        Func<string, string, string> getProductPropertyValue = (productId, name) =>
        {
            foreach (DataRow productPropertyRow in productPropertyTable.Rows)
            {
                if (
                    productPropertyRow[Data.ProductIdColumn]
                        .ToString()
                        .Equals(productId, StringComparison.OrdinalIgnoreCase)
                    && productPropertyRow[Data.NameColumn]
                        .ToString()
                        .Equals(name, StringComparison.OrdinalIgnoreCase)
                )
                {
                    return productPropertyRow[Data.ValueColumn].ToString();
                }
            }

            return string.Empty;
        };

        var line = 0;
        foreach (DataRow orderLineRow in orderLineRows)
        {
            var productRow = getProductDataRow(orderLineRow[Data.ProductIdColumn].ToString());
            var inventoryUomValue = getProductPropertyValue(
                orderLineRow[Data.ProductIdColumn].ToString(),
                "InventoryUOM"
            );
            var partTypeValue = getProductPropertyValue(
                orderLineRow[Data.ProductIdColumn].ToString(),
                "PartType"
            );
            var inventoryUom = inventoryUomValue.Equals(string.Empty)
                ? orderLineRow[Data.UnitOfMeasureColumn].ToString()
                : inventoryUomValue;
            var isKit = partTypeValue.Equals("K", StringComparison.OrdinalIgnoreCase);

            client.GetNewOrderDtl(ref salesOrderTableset, salesOrderTableset.OrderHed[0].OrderNum);
            var orderDtl = salesOrderTableset.OrderDtl[line];

            orderDtl.Company = integrationProcessorSubmitOrderEpicor10Info.CompanyNumber;
            orderDtl.PartNum = productRow[Data.ErpNumberColumn].ToString();
            client.ChangePartNum(ref salesOrderTableset, false, inventoryUom);

            orderDtl.OrderLine = line + 1;
            orderDtl.CustNum = customerRow.CustNum;
            orderDtl.PartNum = productRow[Data.ErpNumberColumn].ToString();
            if (
                orderLineRow.Table.Columns.Contains(Data.RevisionColumn)
                && !string.IsNullOrEmpty(orderLineRow[Data.RevisionColumn].ToString())
            )
            {
                orderDtl.RevisionNum = orderLineRow[Data.RevisionColumn].ToString();
                orderDtl.BasePartNum = orderDtl.PartNum;
                orderDtl.BaseRevisionNum = orderDtl.RevisionNum;
                orderDtl.Configured = "Off";
            }

            orderDtl.LineDesc = orderLineRow[Data.DescriptionColumn].ToString();
            orderDtl.ProdCode = productRow[Data.ProductCodeColumn].ToString();
            orderDtl.SellingQuantity = Convert.ToDecimal(orderLineRow[Data.QtyOrderedColumn]);
            orderDtl.OrderQty = orderDtl.SellingQuantity;
            orderDtl.UnitPrice =
                Convert.ToDecimal(orderLineRow[Data.UnitNetPriceColumn])
                + Convert.ToDecimal(orderLineRow[Data.ProductDiscountPerEachColumn]);
            orderDtl.TaxCatID = productRow[Data.TaxCategoryColumn].ToString().Trim();
            orderDtl.OrderComment = orderLineRow[Data.NotesColumn].ToString();
            orderDtl.DocUnitPrice = orderDtl.UnitPrice;
            orderDtl.SalesUM = orderLineRow[Data.UnitOfMeasureColumn].ToString();
            orderDtl.IUM = inventoryUom;
            if (!Convert.IsDBNull(orderLineRow[Data.DueDateColumn]))
            {
                orderDtl.RequestDate = Convert.ToDateTime(orderLineRow[Data.DueDateColumn]);
            }

            orderDtl.POLine = orderLineRow[Data.CustomerPoLineColumn].ToString();
            orderDtl.LockPrice = true;
            orderDtl.OverridePriceList = false;
            if (isKit)
            {
                orderDtl.KitAllowUpdate = true;
            }

            salesOrderTableset.OrderDtl[line] = orderDtl;
            client.Update(ref salesOrderTableset);

            // orderRel appears to get magically created after orderdtl is updated.
            var orderRel = salesOrderTableset.OrderRel.FirstOrDefault(
                x => x.OrderLine == orderDtl.OrderLine
            );
            if (orderRel != null)
            {
                orderRel.WarehouseCode = orderLineRow[Data.WarehouseColumn].ToString();
                orderRel.Plant = orderLineRow[Data.ShipSiteColumn].ToString();
            }

            // does nothing right now
            this.SubmitConfiguration(client, ref salesOrderTableset);

            // if the line is a parent kit, explode the kits into the order.
            if (
                orderDtl.KitFlagDescription.Equals(
                    "Parent",
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
            {
                var errorMessage = string.Empty;
                var orderHed = salesOrderTableset.OrderHed[0];
                client.GetKitComponents(
                    orderDtl.PartNum,
                    orderDtl.RevisionNum,
                    string.Empty,
                    0,
                    orderHed.OrderNum,
                    orderDtl.OrderLine,
                    false,
                    true,
                    ref errorMessage,
                    ref salesOrderTableset
                );
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    throw new ApplicationException(errorMessage);
                }
            }

            client.Update(ref salesOrderTableset);

            line = salesOrderTableset.OrderDtl.Count;
        }
    }

    protected virtual void AddPaymentToRequest(
        SalesOrderSvcContractClient client,
        ref SalesOrderTableset salesOrderTableset,
        IntegrationJob integrationJob,
        IntegrationProcessorSubmitOrderEpicor10Info integrationProcessorSubmitOrderEpicor10Info
    )
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        decimal creditCardAmount = 0;
        DataRow creditCardTransactionRow = null;
        if (
            initialDataset.Tables.Contains(Data.CreditCardTransactionTable)
            && initialDataset.Tables[Data.CreditCardTransactionTable].Rows.Count > 0
        )
        {
            creditCardTransactionRow = initialDataset.Tables[Data.CreditCardTransactionTable].Rows[
                0
            ];
            creditCardAmount = Convert.ToDecimal(creditCardTransactionRow[Data.AmountColumn]);
        }

        var customerOrderRow = initialDataset.Tables[Data.CustomerOrderTable].Rows[0];

        // A non-credit card order (terms order) will have a credit card amount of 0. Return immediately if that is the case.
        if (creditCardAmount == 0)
        {
            return;
        }

        if (integrationProcessorSubmitOrderEpicor10Info.SubmitSaleTransaction)
        {
            var groupId =
                "EC"
                + DateTime.Today.Month.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
                + DateTime.Today.Day.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
            var cashGrpTableset = this.GetOrCreateCashGroup(
                groupId,
                integrationProcessorSubmitOrderEpicor10Info.CreditCardCashAccountId
            );
            this.SubmitCashReceipt(
                cashGrpTableset.CashGrp[0],
                salesOrderTableset.OrderHed[0],
                groupId,
                creditCardTransactionRow,
                integrationProcessorSubmitOrderEpicor10Info.CreditCardChart
            );
        }
        else
        {
            this.SubmitPaymentAuthorization(
                salesOrderTableset,
                customerOrderRow,
                creditCardTransactionRow,
                integrationProcessorSubmitOrderEpicor10Info.CompanyNumber
            );
            client.UpdateExistingOrder(salesOrderTableset);
        }
    }

    protected virtual void FinalizeOrder(
        SalesOrderSvcContractClient client,
        ref SalesOrderTableset salesOrderTableset
    )
    {
        var orderHed = salesOrderTableset.OrderHed[0];
        orderHed.ReadyToCalc = true;
        client.Update(ref salesOrderTableset);
    }

    protected virtual void RollbackSubmitOrder(
        SalesOrderSvcContractClient client,
        ref SalesOrderTableset salesOrderTableset
    )
    {
        try
        {
            const string deleteRowMod = "[Delete]";
            if (salesOrderTableset != null)
            {
                foreach (var orderHed in salesOrderTableset.OrderHed)
                {
                    orderHed.RowMod = deleteRowMod;
                }

                foreach (var orderDtl in salesOrderTableset.OrderDtl)
                {
                    orderDtl.RowMod = deleteRowMod;
                }

                foreach (var orderRel in salesOrderTableset.OrderRel)
                {
                    orderRel.RowMod = deleteRowMod;
                }

                foreach (var orderRepComm in salesOrderTableset.OrderRepComm)
                {
                    orderRepComm.RowMod = deleteRowMod;
                }

                foreach (var orderMsc in salesOrderTableset.OrderMsc)
                {
                    orderMsc.RowMod = deleteRowMod;
                }

                client.Update(ref salesOrderTableset);
            }
        }
        catch (Exception)
        {
            // swallow this exception because we are throwing the original exception that caused us to attempt the rollback in the first place
        }
    }
}
