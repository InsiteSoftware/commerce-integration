namespace Insite.WIS.Sx;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using Insite.Common.Helpers;
using Insite.Integration.Enums;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.Plugins.Constants;
using Insite.WIS.Broker.WebIntegrationService;

/// <summary>
/// SX Implementation of Pricing Refresh. Ultimately queries the pdsc table on the database and builds a dataset in the exact format of the
/// ISC price matrix table.
/// </summary>
public class IntegrationProcessorPricingRefreshSx
    : IntegrationProcessorPricingRefreshBase,
        IIntegrationProcessor
{
#pragma warning disable SA1306
    protected IntegrationJobLogger JobLogger;
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
        this.JobLogger = new IntegrationJobLogger(siteConnection, integrationJob);

        var notFoundRecords = new List<string>();
        var pdscCounter = 0;
        var integrationProcessorPricingRefreshSxInfo = this.PopulatePricingRefreshParameters(
            siteConnection,
            integrationJob,
            jobStep
        );
        var dataTable = this.BuildPriceMatrixDataTable(jobStep.Sequence);

        // make a call out to the pdsc table to get pricing records
        var connectionString =
            jobStep.IntegrationConnectionOverride != null
                ? jobStep.IntegrationConnectionOverride.ConnectionString
                : integrationJob.JobDefinition.IntegrationConnection.ConnectionString;

        using (var conn = new OdbcConnection(connectionString))
        {
            conn.Open();
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Info,
                "Executing Pricing Sql: "
                    + this.BuildPricingQuery(integrationProcessorPricingRefreshSxInfo)
            );
#pragma warning disable CA2100
            var command = new OdbcCommand(
                this.BuildPricingQuery(integrationProcessorPricingRefreshSxInfo),
                conn
            );
#pragma warning restore CA2100
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (pdscCounter % 5000 == 0 && pdscCounter != 0)
                {
                    this.JobLogger.Debug(
                        string.Format(
                            "Processed pdsc rows {0}{1}Unable to Identify (only showing first 20 of total {3}):{1}{2}",
                            pdscCounter,
                            Environment.NewLine,
                            string.Join(Environment.NewLine, notFoundRecords.Take(20)),
                            notFoundRecords.Count
                        )
                    );
                    notFoundRecords.Clear();
                }

                pdscCounter++;

                var recordType = string.Empty;
                var productKeyPart = string.Empty;
                var customerKeyPart = string.Empty;
                var productKeyPartRequired = true;
                var customerKeyPartRequired = true;
                var initialDataset = XmlDatasetManager.ConvertXmlToDataset(
                    integrationJob.InitialData
                );
                switch (reader["levelcd"].ToString().Trim())
                {
                    case "1":
                        recordType = "Customer/Product";
                        productKeyPart = this.GetProductId(
                            reader["prod"].ToString().Trim(),
                            initialDataset
                        );
                        customerKeyPart = this.GetCustomerId(
                            reader["custno"].ToString().Trim(),
                            reader["custtype"].ToString().Trim(),
                            initialDataset
                        );
                        break;
                    case "2":
                        productKeyPart = reader["prod"].ToString().Trim();
                        customerKeyPart = this.GetCustomerId(
                            reader["custno"].ToString().Trim(),
                            reader["custtype"].ToString().Trim(),
                            initialDataset
                        );
                        if (productKeyPart.ToLower().Substring(0, 1) == "p")
                        {
                            recordType = "Customer/Product Price Code";
                        }
                        else if (productKeyPart.ToLower().Substring(0, 1) == "l")
                        {
                            recordType = "Customer/Product Price Line";
                        }
                        else if (productKeyPart.ToLower().Substring(0, 1) == "c")
                        {
                            recordType = "Customer/Product Price Category";
                        }
                        else
                        {
                            recordType = string.Empty;
                        }

                        break;
                    case "3":
                        recordType = "Customer Price Code/Product";
                        productKeyPart = this.GetProductId(
                            reader["prod"].ToString().Trim(),
                            initialDataset
                        );
                        customerKeyPart = reader["custtype"].ToString().Trim();
                        break;
                    case "4":
                        recordType = "Customer Price Code/Product Price Code";
                        productKeyPart = reader["prod"].ToString().Trim();
                        customerKeyPart = reader["custtype"].ToString().Trim();
                        break;
                    case "5":
                        productKeyPartRequired = false;
                        recordType = "Customer";
                        productKeyPart = string.Empty;
                        customerKeyPart = this.GetCustomerId(
                            reader["custno"].ToString().Trim(),
                            reader["custtype"].ToString().Trim(),
                            initialDataset
                        );
                        break;
                    case "6":
                        productKeyPartRequired = false;
                        recordType = "Customer Price Code";
                        productKeyPart = string.Empty;
                        customerKeyPart = reader["custtype"].ToString().Trim();
                        break;
                    case "7":
                        customerKeyPartRequired = false;
                        recordType = Convert.ToBoolean(reader["promofl"])
                            ? "Product Promotion"
                            : "Product";
                        productKeyPart = this.GetProductId(
                            reader["prod"].ToString().Trim(),
                            initialDataset
                        );
                        customerKeyPart = string.Empty;
                        break;
                    case "8":
                        customerKeyPartRequired = false;
                        recordType = Convert.ToBoolean(reader["promofl"])
                            ? "Product Price Code Promotion"
                            : "Product Price Code";
                        productKeyPart = reader["prod"].ToString().Trim();
                        customerKeyPart = string.Empty;
                        break;
                }

                // validate the recordType, productCode, customerCodes assigned above are valid.  if they are not skip the record.
                if (recordType.Equals(string.Empty))
                {
                    notFoundRecords.Add("Record Type for id " + reader["pdrecno"]);
                    continue;
                }

                if (customerKeyPartRequired && customerKeyPart.Equals(string.Empty))
                {
                    notFoundRecords.Add("Customer Code for id " + reader["pdrecno"]);
                    continue;
                }

                if (productKeyPartRequired && productKeyPart.Equals(string.Empty))
                {
                    notFoundRecords.Add("Product Code for id " + reader["pdrecno"]);
                    continue;
                }

                // add the datarow to the datatable. this should look the same as what we expect the pricematrix row to look like on the other side
                var dataRow = dataTable.NewRow();
                dataRow[Data.RecordTypeColumn] = recordType;
                dataRow[Data.CurrencyCodeColumn] = this.GetCurrencyCode(
                    reader["custno"].ToString().Trim(),
                    string.Empty,
                    initialDataset
                );
                dataRow[Data.WarehouseColumn] = reader["whse"].ToString().Trim();
                dataRow[Data.UnitOfMeasureColumn] = reader["units"].ToString().Trim();
                dataRow[Data.CustomerKeyPartColumn] = customerKeyPart;
                dataRow[Data.ProductKeyPartColumn] = productKeyPart;
                var startDate =
                    reader["startdt"] == DBNull.Value
                        ? DateTime.UtcNow
                        : Convert.ToDateTime(reader["startdt"]);
                dataRow[Data.ActivateOnColumn] = startDate;
                var endDate =
                    reader["enddt"] == DBNull.Value
                        ? new DateTime(2059, 12, 31)
                        : Convert.ToDateTime(reader["enddt"]);
                dataRow[Data.DeactivateOnColumn] = endDate;
                dataRow[Data.CalculationFlagsColumn] =
                    reader["pround"].ToString().Trim() + "|" + reader["ptarget"].ToString().Trim();

                var qtyBrk = reader["qtyBrk"]
                    .ToString()
                    .TrimEnd()
                    .Split(new[] { ";" }, StringSplitOptions.None);
                var prcMult = reader["prcMult"]
                    .ToString()
                    .TrimEnd()
                    .Split(new[] { ";" }, StringSplitOptions.None);
                var prcDisc = reader["prcDisc"]
                    .ToString()
                    .TrimEnd()
                    .Split(new[] { ";" }, StringSplitOptions.None);
                var qtyBrkType = reader["qtyBreakty"].ToString().TrimEnd();
                var priceType = reader["prcType"].ToString().TrimEnd();
                var priceOnTy = reader["priceOnTy"].ToString().TrimEnd();
                if (qtyBrkType.Equals(string.Empty))
                {
                    // Structure 1
                    if (priceType.ToLower() == "true")
                    {
                        // $
                        for (var priceStruct = 0; priceStruct < 9; priceStruct++)
                        {
                            var prcMultValue = Convert.ToDecimal(prcMult[priceStruct]);
                            var prcDiscValue = Convert.ToDecimal(prcDisc[priceStruct]);
                            var fieldExt = (priceStruct + 1)
                                .ToString(CultureInfo.InvariantCulture)
                                .Trim()
                                .PadLeft(2, '0');
                            dataRow["Amount" + fieldExt] = prcMultValue;
                            dataRow["AltAmount" + fieldExt] = prcDiscValue;
                            dataRow["PriceBasis" + fieldExt] = "CLO";
                            dataRow["AdjustmentType" + fieldExt] = "A";
                            dataRow["BreakQty" + fieldExt] = 0;
                        }
                    }
                    else if (priceType.ToLower() == "false")
                    {
                        // %
                        // Structure 2
                        for (var priceStruct = 0; priceStruct < 9; priceStruct++)
                        {
                            var prcMultValue = Convert.ToDecimal(prcMult[priceStruct]);
                            var prcDiscValue = Convert.ToDecimal(prcDisc[priceStruct]);
                            var fieldExt = (priceStruct + 1)
                                .ToString(CultureInfo.InvariantCulture)
                                .Trim()
                                .PadLeft(2, '0');
                            dataRow["Amount" + fieldExt] = prcMultValue;
                            dataRow["AltAmount" + fieldExt] = prcDiscValue;
                            dataRow["PriceBasis" + fieldExt] = "CL" + priceOnTy.ToUpper();
                            dataRow["AdjustmentType" + fieldExt] = "P";
                            dataRow["BreakQty" + fieldExt] = 0;
                        }
                    }
                }
                else if (qtyBrkType.ToLower() == "p")
                {
                    if (priceType.ToLower() == "true")
                    {
                        // $
                        // Structure 3
                        for (var priceStruct = 0; priceStruct < 9; priceStruct++)
                        {
                            var prcMultValue = Convert.ToDecimal(prcMult[priceStruct]);
                            var prcDiscValue = Convert.ToDecimal(prcDisc[priceStruct]);
                            var fieldExt = (priceStruct + 1)
                                .ToString(CultureInfo.InvariantCulture)
                                .Trim()
                                .PadLeft(2, '0');
                            dataRow["Amount" + fieldExt] = prcMultValue;
                            dataRow["AltAmount" + fieldExt] = prcDiscValue;
                            dataRow["PriceBasis" + fieldExt] = "O";
                            dataRow["AdjustmentType" + fieldExt] = "A";
                        }

                        dataRow["BreakQty01"] = 1;
                        for (var iQtyBrk = 0; iQtyBrk < qtyBrk.Length; iQtyBrk++)
                        {
                            var qtyBrkValue = Convert.ToDecimal(qtyBrk[iQtyBrk]);
                            var fieldExt = (iQtyBrk + 2)
                                .ToString(CultureInfo.InvariantCulture)
                                .Trim()
                                .PadLeft(2, '0');
                            dataRow["BreakQty" + fieldExt] = qtyBrkValue;
                        }
                    }
                    else if (priceType.ToLower() == "false")
                    {
                        // %
                        // Structure 4
                        for (var priceStruct = 0; priceStruct < 9; priceStruct++)
                        {
                            var prcMultValue = Convert.ToDecimal(prcMult[priceStruct]);
                            var prcDiscValue = Convert.ToDecimal(prcDisc[priceStruct]);
                            var fieldExt = (priceStruct + 1)
                                .ToString(CultureInfo.InvariantCulture)
                                .Trim()
                                .PadLeft(2, '0');
                            dataRow["Amount" + fieldExt] = prcMultValue;
                            dataRow["AltAmount" + fieldExt] = prcDiscValue;
                            dataRow["PriceBasis" + fieldExt] = priceOnTy.ToUpper();
                            dataRow["AdjustmentType" + fieldExt] = "P";
                        }

                        dataRow["BreakQty01"] = 1;
                        for (var iQtyBrk = 0; iQtyBrk < qtyBrk.Length; iQtyBrk++)
                        {
                            var qtyBrkValue = Convert.ToDecimal(qtyBrk[iQtyBrk]);
                            var fieldExt = (iQtyBrk + 2)
                                .ToString(CultureInfo.InvariantCulture)
                                .Trim()
                                .PadLeft(2, '0');
                            dataRow["BreakQty" + fieldExt] = qtyBrkValue;
                        }
                    }
                }
                else if (qtyBrkType.ToLower() == "d")
                {
                    if (priceType.ToLower() == "true")
                    {
                        // $
                        // Structure 5
                        for (var priceStruct = 0; priceStruct < 9; priceStruct++)
                        {
                            var prcMultValue = Convert.ToDecimal(prcMult[priceStruct]);
                            var prcDiscValue = Convert.ToDecimal(prcDisc[priceStruct]);
                            var fieldExt = (priceStruct + 1)
                                .ToString(CultureInfo.InvariantCulture)
                                .Trim()
                                .PadLeft(2, '0');
                            dataRow["Amount" + fieldExt] = prcMultValue;
                            dataRow["AltAmount" + fieldExt] = prcDiscValue;
                            dataRow["PriceBasis" + fieldExt] = "CLD";
                            dataRow["AdjustmentType" + fieldExt] = "A";
                        }

                        dataRow["BreakQty01"] = 1;
                        for (var iQtyBrk = 0; iQtyBrk < qtyBrk.Length; iQtyBrk++)
                        {
                            var qtyBrkValue = Convert.ToDecimal(qtyBrk[iQtyBrk]);
                            var fieldExt = (iQtyBrk + 2)
                                .ToString(CultureInfo.InvariantCulture)
                                .Trim()
                                .PadLeft(2, '0');
                            dataRow["BreakQty" + fieldExt] = qtyBrkValue;
                        }
                    }
                    else if (priceType.ToLower() == "false")
                    {
                        // %
                        // Structure 6
                        for (var priceStruct = 0; priceStruct < 9; priceStruct++)
                        {
                            var prcMultValue = Convert.ToDecimal(prcMult[priceStruct]);
                            var prcDiscValue = Convert.ToDecimal(prcDisc[priceStruct]);
                            var fieldExt = (priceStruct + 1)
                                .ToString(CultureInfo.InvariantCulture)
                                .Trim()
                                .PadLeft(2, '0');
                            dataRow["Amount" + fieldExt] = prcMultValue;
                            dataRow["AltAmount" + fieldExt] = prcDiscValue;
                            dataRow["PriceBasis" + fieldExt] = "CLD" + priceOnTy.ToUpper();
                            dataRow["AdjustmentType" + fieldExt] = "P";
                        }

                        dataRow["BreakQty01"] = 1;
                        for (var iQtyBrk = 0; iQtyBrk < qtyBrk.Length; iQtyBrk++)
                        {
                            var qtyBrkValue = Convert.ToDecimal(qtyBrk[iQtyBrk]);
                            var fieldExt = (iQtyBrk + 2)
                                .ToString(CultureInfo.InvariantCulture)
                                .Trim()
                                .PadLeft(2, '0');
                            dataRow["BreakQty" + fieldExt] = qtyBrkValue;
                        }
                    }
                }

                // make sure all 11 instances of each field has a value.  If they are null, the fieldmap post processor will have
                // problems with them.
                for (var i = 1; i <= 11; i++)
                {
                    var index = i.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
                    if (string.IsNullOrEmpty(dataRow["Amount" + index].ToString()))
                    {
                        dataRow["Amount" + index] = 0;
                    }

                    if (string.IsNullOrEmpty(dataRow["AltAmount" + index].ToString()))
                    {
                        dataRow["AltAmount" + index] = 0;
                    }

                    if (string.IsNullOrEmpty(dataRow["PriceBasis" + index].ToString()))
                    {
                        dataRow["PriceBasis" + index] = string.Empty;
                    }

                    if (string.IsNullOrEmpty(dataRow["AdjustmentType" + index].ToString()))
                    {
                        dataRow["AdjustmentType" + index] = string.Empty;
                    }

                    if (string.IsNullOrEmpty(dataRow["BreakQty" + index].ToString()))
                    {
                        dataRow["BreakQty" + index] = 0;
                    }
                }

                dataTable.Rows.Add(dataRow);
            }
        }

        var dataSet = new DataSet();
        dataSet.Tables.Add(dataTable);
        return dataSet;
    }

    /// <summary>Retrieves the ERP_Company parameter defined in the jobStep.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <returns>The <see cref="IntegrationProcessorPricingRefreshSxInfo"/> with all of the parameters that are relevent to the pricing refresh
    /// contained within.</returns>
    protected virtual IntegrationProcessorPricingRefreshSxInfo PopulatePricingRefreshParameters(
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
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Info,
                string.Format(Messages.ParameterNotFoundMessage, Parameters.ErpCompany)
            );
        }

        return new IntegrationProcessorPricingRefreshSxInfo
        {
            CompanyNumber = (parmCompanyNumber == null) ? string.Empty : parmCompanyNumber.Value
        };
    }

    /// <summary>Builds the pricing sql call to query the sx database with.</summary>
    /// <param name="integrationProcessorPricingRefreshSxInfo">The parameter information related to the call.</param>
    /// <returns>The pricing sql call to query the sx database with</returns>
    protected virtual string BuildPricingQuery(
        IntegrationProcessorPricingRefreshSxInfo integrationProcessorPricingRefreshSxInfo
    )
    {
        var dateTimeNowFormattedForSx =
            DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture)
            + "-"
            + DateTime.UtcNow.Month.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
            + "-"
            + DateTime.UtcNow.Day.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');

        return string.Format(
            @"select a.cono,
                    a.pdrecno,
                    a.whse,
                    a.startdt,
                    a.enddt,
                    a.prcType,
                    a.prcdisc,
                    a.priceonty,
                    a.qtybreakty,
                    a.qtybrk,
                    a.levelcd,
                    a.custno,
                    a.custtype,
                    a.prod ,
                    a.prcmult,
                    a.units,
                    a.statustype,
                    a.qtytype,
                    a.promofl,
                    a.refer,
                    a.pround,
                    a.ptarget
                from pub.pdsc a
                    inner join
                        (select b.cono,
                            b.levelcd,
                            b.whse,
                            b.custno,
                            b.custtype,
                            b.prod,
                            b.units,
                            MAX(b.startdt) as startdt
                        from pub.pdsc b
                        where b.cono = {0}
                            and b.statustype = 1
                            and b.startdt < '{1}'
                            and (b.enddt is null or b.enddt > '{1}')
                        group by b.cono,
                            b.levelcd,
                            b.whse,
                            b.custno,
                            b.custtype,
                            b.prod,
                            b.units) as c
                    on a.cono = c.cono
                        and a.levelcd = c.levelcd
                        and a.whse = c.whse
                        and a.custno = c.custno
                        and a.custtype = c.custtype
                        and a.prod = c.prod
                        and a.units = c.units
                        and a.startdt = c.startdt
                where a.cono = {0}
                    and a.statustype = 1
                    and a.startdt < '{1}'
                    and (a.enddt is null or a.enddt > '{1}')",
            integrationProcessorPricingRefreshSxInfo.CompanyNumber,
            dateTimeNowFormattedForSx
        );
    }
}
