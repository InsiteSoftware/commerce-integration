// Don't run code cleanup on this file as it causes issues.

namespace Insite.WIS.Epicor.Epicor9;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml;
using Insite.Common.Helpers;
using Insite.Integration.Enums;
using Insite.WIS.Epicor.Epicor9BomSearchService;
using Insite.WIS.Epicor.Epicor9ConfiguratorService;
using Insite.WIS.Epicor.Epicor9DynamicQueryService;
using Insite.WIS.Epicor.Epicor9PartService;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Plugins.Constants;
using Insite.WIS.Broker.WebIntegrationService;
using Microsoft.Web.Services3.Design;
using Microsoft.Web.Services3.Security;

using CallContextDataSetType = Insite.WIS.Epicor.Epicor9ConfiguratorService.CallContextDataSetType;

public class IntegrationProcessorConfigurationRefreshEpicor9 : IIntegrationProcessor
{
    private Policy policy;

    public string CertName;

    public string CompanyId;

    public string EpicorPassword;

    public bool EpicorSoapEncryption;

    public string EpicorUserName;

    public string IntegrationUserProfileId;

    public string TargetImagePath;

    public string UrlHead;

    protected Policy Policy
    {
        get
        {
            if (this.policy == null)
            {
                this.policy = this.GetPolicy();
            }

            return this.policy;
        }
    }

    public virtual DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep
    )
    {
        siteConnection.AddLogMessage(
            integrationJob.Id.ToString(),
            IntegrationJobLogType.Info,
            "Execute Method"
        );

        // All the webservices connection information comes off the site connection
        this.UrlHead = integrationJob.JobDefinition.IntegrationConnection.Url;
        this.EpicorUserName = integrationJob.JobDefinition.IntegrationConnection.LogOn;
#pragma warning disable CS0618 // Type or member is obsolete
        this.EpicorPassword = EncryptionHelper.DecryptAes(
            integrationJob.JobDefinition.IntegrationConnection.Password
        );
#pragma warning restore
        if (
            string.IsNullOrEmpty(this.UrlHead)
            || string.IsNullOrEmpty(this.EpicorUserName)
            || string.IsNullOrEmpty(this.EpicorPassword)
        )
        {
            throw new ArgumentException(
                "Either the UrlHead, UserName or Password for the WebServices connection is blank or null."
            );
        }

        // store company number parm
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

        this.CompanyId = (parmCompanyNumber == null) ? string.Empty : parmCompanyNumber.Value;

        // store the TargetImagePath parm
        var parmTargetImagePath = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p =>
                p.Name.Equals(
                    "EpicorConfigurator_TargetImagePath",
                    StringComparison.OrdinalIgnoreCase
                )
        );
        if (parmTargetImagePath == null)
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Info,
                string.Format(
                    Messages.ParameterNotFoundMessage,
                    "EpicorConfigurator_TargetImagePath"
                )
            );
        }

        this.TargetImagePath =
            (parmTargetImagePath == null) ? string.Empty : parmTargetImagePath.Value;

        // store the EpicorSoapEncryption parm
        var parmEpicorSoapEncryption = jobStep.JobDefinitionStepParameters.FirstOrDefault(
            p => p.Name.Equals("ERP_EpicorX509Encryption", StringComparison.OrdinalIgnoreCase)
        );
        if (parmEpicorSoapEncryption == null)
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Info,
                string.Format(Messages.ParameterNotFoundMessage, "ERP_EpicorX509Encryption")
            );
        }

        this.EpicorSoapEncryption =
            (parmEpicorSoapEncryption != null) && Convert.ToBoolean(parmEpicorSoapEncryption.Value);

        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        // store the IntegrationUserProfileId because UploadContainerFile requires a userprofileid
        foreach (DataRow miscLookupDataRow in initialDataset.Tables["MiscLookup"].Rows)
        {
            if (miscLookupDataRow["Name"].ToString().Equals("IntegrationUserProfileId"))
            {
                this.IntegrationUserProfileId = miscLookupDataRow["Value"].ToString();
                break;
            }
        }

        // All of the code that exists in this class was cannabalized from the old integration. I kept all that code in place which means i need to create a
        // product dataset and pass them into the GetConfigurableProducts method the same way the original code did.  The lookupinformation preprocessor has all this stuff.
        var productDataSet = new DataSet();
        var productTable = productDataSet.Tables.Add("Product");
        var dataColumn = productTable.Columns.Add("ErpNumber");
        productTable.PrimaryKey = new[] { dataColumn };
        foreach (DataRow productLookupDataRow in initialDataset.Tables["ProductLookup"].Rows)
        {
            var productRow = productTable.NewRow();
            productRow["ErpNumber"] = productLookupDataRow["ErpNumber"].ToString();
            productTable.Rows.Add(productRow);
        }

        this.GetConfigurableProducts(productDataSet, string.Empty, siteConnection, integrationJob);

        return productDataSet;
    }

    protected virtual Policy GetPolicy()
    {
        PolicyAssertion policyAssertion = null;
        if (!this.EpicorSoapEncryption)
        {
            var assertion = new UsernameOverTransportAssertion();
            assertion.UsernameTokenProvider = new UsernameTokenProvider(
                this.EpicorUserName,
                this.EpicorPassword
            );
            policyAssertion = assertion;
        }
        else
        {
            var assertion = new UsernameForCertificateAssertion();
            assertion.EstablishSecurityContext = true;
            assertion.RenewExpiredSecurityContext = true;
            assertion.RequireSignatureConfirmation = false;
            assertion.MessageProtectionOrder =
                MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature;
            assertion.RequireDerivedKeys = true;

            assertion.TtlInSeconds = 300;

            assertion.X509TokenProvider = new X509TokenProvider(
                StoreLocation.LocalMachine,
                StoreName.My,
                string.Format("CN={0}", this.CertName),
                X509FindType.FindBySubjectDistinguishedName
            );

            assertion.Protection.Request.SignatureOptions =
                SignatureOptions.IncludeAddressing
                | SignatureOptions.IncludeTimestamp
                | SignatureOptions.IncludeSoapBody;

            assertion.Protection.Request.EncryptBody = true;

            assertion.Protection.Response.SignatureOptions =
                SignatureOptions.IncludeAddressing
                | SignatureOptions.IncludeTimestamp
                | SignatureOptions.IncludeSoapBody;

            assertion.Protection.Response.EncryptBody = true;

            assertion.Protection.Fault.SignatureOptions =
                SignatureOptions.IncludeAddressing
                | SignatureOptions.IncludeTimestamp
                | SignatureOptions.IncludeSoapBody;

            assertion.Protection.Fault.EncryptBody = false;

            assertion.UsernameTokenProvider = new UsernameTokenProvider(
                this.EpicorUserName,
                this.EpicorPassword
            );

            policyAssertion = assertion;
        }

        var policy = new Policy();
        policy.Assertions.Add(policyAssertion);

        return policy;
    }

    protected virtual void GetConfigurableProducts(
        DataSet productDataSet,
        string partNum,
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        var currentRev = string.Empty;
        PcStatusListDataSetTypePcStatusList plRow = null;
        try
        {
            var configurationTable = this.CreateConfigurationTable(productDataSet);
            var configurationPageTable = this.CreateConfigurationPageTable(productDataSet);
            var configurationOptionTable = this.CreateConfigurationOptionTable(productDataSet);
            var configurationOptionConditionTable = this.CreateConfigurationOptionConditionTable(
                productDataSet
            );
            var configurationConditionFilterTable = this.CreateConfigurationConditionFilterTable(
                productDataSet
            );
            var configurationQueryResultTable = this.CreateConfigurationQueryResultTable(
                productDataSet
            );

            var configuratorList = this.GetConfiguratorList(
                partNum,
                siteConnection,
                integrationJob
            );
            var processedParts = new HashSet<string>();
            if (configuratorList != null && configuratorList.PcStatusListDataSet != null)
            {
                foreach (var pl in configuratorList.PcStatusListDataSet)
                {
                    try
                    {
                        plRow = pl as PcStatusListDataSetTypePcStatusList;
                        if (plRow == null)
                        {
                            continue;
                        }

                        // Query is orderd by ApprovedDate Desc, so only process the latest of any PartNum
                        if (processedParts.Contains(plRow.PartNum))
                        {
                            continue;
                        }

                        // We need to determine the most current revision.
                        currentRev = this.ProcessConfigurationRevision(plRow.PartNum);

                        processedParts.Add(plRow.PartNum);

                        // Make sure this part number is in ProductDataSet.Tables["Product"], if not, do not process the configuration
                        if (this.TestActiveProduct(productDataSet, plRow.PartNum))
                        {
                            var configuratorDataSet = this.GetConfiguratorById(
                                plRow.PartNum,
                                currentRev
                            );
                            foreach (var cr in configuratorDataSet.ConfiguratorDataSet)
                            {
                                if (cr is ConfiguratorDataSetTypePcStatus pcStatusRow)
                                {
                                    this.ProcessConfiguration(
                                        configurationTable,
                                        plRow.PartNum,
                                        currentRev,
                                        plRow.DispConfSummary.ToString()
                                    );
                                    this.ProcessSubConfiguration(
                                        configurationPageTable,
                                        configurationOptionTable,
                                        configuratorList,
                                        plRow.PartNum,
                                        currentRev,
                                        pcStatusRow,
                                        siteConnection,
                                        integrationJob
                                    );
                                    continue;
                                }

                                if (cr is ConfiguratorDataSetTypePcPage pcPageRow)
                                {
                                    this.ProcessConfigurationPage(
                                        configurationPageTable,
                                        plRow,
                                        pcPageRow
                                    );
                                    continue;
                                }

                                if (cr is ConfiguratorDataSetTypePcInputs pcInputsRow)
                                {
                                    this.ProcessConfigurationOption(
                                        configuratorDataSet,
                                        configurationOptionTable,
                                        plRow,
                                        pcInputsRow,
                                        siteConnection,
                                        integrationJob
                                    );
                                    continue;
                                }

                                if (cr is ConfiguratorDataSetTypePcDynLst pcDynLstRow)
                                {
                                    this.ProcessConfigurationOptionCondition(
                                        configuratorDataSet,
                                        configurationOptionTable,
                                        configurationOptionConditionTable,
                                        configurationQueryResultTable,
                                        plRow,
                                        pcDynLstRow,
                                        siteConnection,
                                        integrationJob
                                    );
                                    continue;
                                }

                                if (
                                    cr
                                    is ConfiguratorDataSetTypePcDynLstCriteria pcDynLstCriteriaRow
                                )
                                {
                                    this.ProcessConfigurationConditionFilter(
                                        configuratorDataSet,
                                        configurationConditionFilterTable,
                                        plRow,
                                        pcDynLstCriteriaRow
                                    );
                                }
                            }

                            // foreach object in the configurator dataset.
                        }
                    }
                    catch (Exception exc)
                    {
                        siteConnection.AddLogMessage(
                            integrationJob.Id.ToString(),
                            IntegrationJobLogType.Error,
                            string.Format(
                                "An error was found in the GetConfigurableProducts method.  The error is {0}.  The part number is {1} and the revision is {2}.",
                                exc.Message,
                                plRow.PartNum,
                                currentRev
                            )
                        );
                    }
                }

                // foreach configuration.
            }
        }
        catch (Exception exc)
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Error,
                string.Format(
                    "An error was found in the GetConfigurableProducts method.  The error is {0}.  The part number is {1} and the revision is {2}.",
                    exc.Message,
                    plRow.PartNum,
                    currentRev
                )
            );
        }
    }

    protected virtual PcStatusListDataSetType GetConfiguratorList(
        string partNum,
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        PcStatusListDataSetType configuratorList;
        var whereClausePart = "Approved = true AND ApprovedDate <= TODAY ";
        if (partNum.Trim().Length > 0)
        {
            whereClausePart += "AND PartNum = '" + partNum + "' ";
        }

        whereClausePart += "BY ApprovedDate DESC";

        siteConnection.AddLogMessage(
            integrationJob.Id.ToString(),
            IntegrationJobLogType.Info,
            "Reaching ERP Webservice for GetConfiguratorList()"
        );

        using (var configuratorService = new ConfiguratorService())
        {
            configuratorService.Url = this.FixUrl(configuratorService.Url);
            configuratorService.SetPolicy(this.Policy);

            configuratorService.Timeout = 3000 * 1000; // Set in milliseconds

            configuratorList = configuratorService.GetList(
                this.CompanyId,
                whereClausePart,
                0,
                0,
                null,
                out var morePages,
                out var callContext
            );
        }

        if (configuratorList == null)
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Info,
                "Found 0 Configurations to Refresh"
            );
        }
        else
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Info,
                string.Format(
                    "Found {0} Configurations to Refresh",
                    configuratorList.PcStatusListDataSet.Length
                )
            );
        }

        return configuratorList;
    }

    protected virtual string ProcessConfigurationRevision(string partNum)
    {
        PartDataSetType partDS = null;
        PartDataSetTypePartRev targetRevision = null;
        try
        {
            // Get the part data set for the part number that was passed in.
            using (var partService = new PartService())
            {
                partService.Url = this.FixUrl(partService.Url);
                partService.SetPolicy(this.Policy);
                partService.Timeout = 3000 * 1000;
                partDS = partService.GetByID(this.CompanyId, partNum, null, out var callContext);
            }

            // There are lots of different objects in the part data set.  At this point, we are only interested in processing the part revision
            // information in the part data set.
            foreach (var objectPart in partDS.PartDataSet)
            {
                if (objectPart is not PartDataSetTypePartRev partRev)
                {
                    continue;
                }

                // We are now processing a part revision, so we will try to find the most current approved part that does not have an effective date in the future.
                // When we have processed all the parts, we will return the revision number determined to be the most current.
                if (partRev.Approved)
                {
                    // find the most current effective date without being in the future.
                    if (partRev.EffectiveDateSpecified)
                    {
                        if (partRev.EffectiveDate <= DateTime.Today)
                        {
                            if (targetRevision != null)
                            {
                                if (targetRevision.EffectiveDate < partRev.EffectiveDate)
                                {
                                    targetRevision = partRev;
                                }
                            }
                            else
                            {
                                targetRevision = partRev;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception exc)
        {
            throw new Exception(
                string.Format(
                    "Part number {0} could not be found during part revision assignment.  Error is {1}",
                    partNum,
                    exc.Message
                )
            );
        }

        return targetRevision.RevisionNum;
    }

    protected virtual bool TestActiveProduct(DataSet productDataSet, string testPartNumber)
    {
        var products = productDataSet.Tables["Product"];
        var activeProduct = false;
        if (products != null)
        {
            var foundProduct = products.Rows.Find(testPartNumber);
            activeProduct = foundProduct != null;
        }

        return activeProduct;
    }

    protected virtual ConfiguratorDataSetType GetConfiguratorById(string partNum, string revNum)
    {
        ConfiguratorDataSetType configuratorDataSet;
        using (var configuratorService = new ConfiguratorService())
        {
            configuratorService.Url = this.FixUrl(configuratorService.Url);
            configuratorService.SetPolicy(this.Policy);

            configuratorService.Timeout = 3000 * 1000; // Set in milliseconds

            configuratorDataSet = configuratorService.GetByID(
                this.CompanyId,
                partNum,
                revNum,
                null,
                out var callContext
            );
        }

        return configuratorDataSet;
    }

    protected virtual void ProcessConfiguration(
        DataTable configurationTable,
        string partNum,
        string revisionNum,
        string displaySummary
    )
    {
        configurationTable.Rows.Add(partNum, revisionNum, displaySummary);
    }

    protected virtual void ProcessSubConfiguration(
        DataTable configurationPageTable,
        DataTable configurationOptionTable,
        PcStatusListDataSetType configuratorList,
        string partNum,
        string revisionNum,
        ConfiguratorDataSetTypePcStatus pcStatusRow,
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        try
        {
            BomSearchDataSetType bomSearchDataSet;
            using (var bomSearchService = new BomSearchService())
            {
                bomSearchService.Url = this.FixUrl(bomSearchService.Url);
                bomSearchService.SetPolicy(this.Policy);

                bomSearchService.Timeout = 3000 * 1000; // Set in milliseconds

                bomSearchDataSet = bomSearchService.GetDatasetForTree(
                    this.CompanyId,
                    partNum,
                    revisionNum,
                    string.Empty,
                    true,
                    DateTime.Today,
                    false,
                    null,
                    out var callContext
                );
                foreach (
                    var mtl in bomSearchDataSet.BomSearchDataSet
                        .Where(b => b is BomSearchDataSetTypePartMtl)
                        .Select(b => b as BomSearchDataSetTypePartMtl)
                )
                {
                    var mtlPart = configuratorList.PcStatusListDataSet
                        .Where(c => c is PcStatusListDataSetTypePcStatusList)
                        .Select(c => c as PcStatusListDataSetTypePcStatusList)
                        .FirstOrDefault(
                            c => c.PartNum == mtl.MtlPartNum && c.RevisionNum == mtl.MtlRevisionNum
                        );

                    // Only create if it is a configured part
                    if (mtlPart != null)
                    {
                        var configurationPageRow = configurationPageTable.Rows.Find(
                            new object[] { partNum, revisionNum, 999 }
                        );
                        if (configurationPageRow == null)
                        {
                            configurationPageRow = configurationPageTable.Rows.Add(
                                partNum,
                                revisionNum,
                                999
                            );
                        }

                        configurationPageRow["SkipIfNoInputs"] = "true";

                        var configurationOptionRow = configurationOptionTable.Rows.Add(
                            partNum,
                            revisionNum,
                            999,
                            mtl.MtlPartNum
                        );
                        configurationOptionRow["Type"] = "SubconfigurationPart";
                        configurationOptionRow["IsVisible"] = false;
                        configurationOptionRow["SubconfigurationPartNumber"] = mtl.MtlPartNum;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Error,
                string.Format(
                    "ProcessSubConfiguration exception: {0}.  The part number is {1} and the revision number is {2}.",
                    ex.Message,
                    partNum,
                    revisionNum
                )
            );
        }
    }

    protected virtual void ProcessConfigurationPage(
        DataTable configurationPageTable,
        PcStatusListDataSetTypePcStatusList plRow,
        ConfiguratorDataSetTypePcPage pcPageRow
    )
    {
        var configurationPageRow = configurationPageTable.Rows.Add(
            plRow.PartNum,
            plRow.RevisionNum,
            pcPageRow.PageSeq
        );
        configurationPageRow["PageTitle"] = pcPageRow.PageTitle;
        configurationPageRow["SkipIfNoInputs"] = pcPageRow.SkipPageNoInputs;
    }

    protected virtual void ProcessConfigurationOption(
        ConfiguratorDataSetType configuratorDataSet,
        DataTable configurationOptionTable,
        PcStatusListDataSetTypePcStatusList plRow,
        ConfiguratorDataSetTypePcInputs pcInputsRow,
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        var imageFilePath = string.Empty;
        var consoleURL = string.Empty;
        var fileDirectory = string.Empty;
        var serverURL = string.Empty;
        var priceListAdjust = string.Empty;
        try
        {
            var configurationOptionRow = configurationOptionTable.Rows.Add(
                plRow.PartNum,
                plRow.RevisionNum,
                pcInputsRow.PageSeq,
                pcInputsRow.InputName
            );
            configurationOptionRow["Description"] = pcInputsRow.Comments;
            configurationOptionRow["Sequence"] = pcInputsRow.TabOrder;

            configurationOptionRow["TextIsMultiline"] = "false";
            if (pcInputsRow.ControlType.ToLower() == "text")
            {
                configurationOptionRow["Type"] = "Label";
            }
            else if (pcInputsRow.ControlType.ToLower() == "fill-in")
            {
                if (pcInputsRow.DataType.ToLower() == "character")
                {
                    configurationOptionRow["Type"] = "Text";
                }
                else if (pcInputsRow.DataType.ToLower() == "decimal")
                {
                    configurationOptionRow["Type"] = "Number";
                }
                else if (pcInputsRow.DataType.ToLower() == "date")
                {
                    configurationOptionRow["Type"] = "Date";
                }
            }
            else if (pcInputsRow.ControlType.ToLower() == "editor")
            {
                configurationOptionRow["Type"] = "Text";
                configurationOptionRow["TextIsMultiline"] = "true";
            }
            else if (pcInputsRow.ControlType.ToLower() == "toggle-box")
            {
                configurationOptionRow["Type"] = "CheckBox";
            }
            else if (pcInputsRow.ControlType.ToLower() == "combo-box")
            {
                configurationOptionRow["Type"] = "ComboBox";
                configurationOptionRow["SelectionListDisplays"] = pcInputsRow.ListItems;
                configurationOptionRow["SelectionListValues"] = pcInputsRow.ListItems;
                priceListAdjust = this.ProcessSelectionListPrice(
                    configuratorDataSet.ConfiguratorDataSet,
                    configurationOptionRow["SelectionListValues"].ToString(),
                    pcInputsRow.InputName
                );
                configurationOptionRow["SelectionListPriceAdjustment"] = priceListAdjust;
            }
            else if (pcInputsRow.ControlType.ToLower() == "radio-set")
            {
                configurationOptionRow["Type"] = "RadioButtons";
                configurationOptionRow["RadioButtonOrientation"] = pcInputsRow.Horizontal
                    ? "Horizontal"
                    : "Vertical";
                configurationOptionRow["SelectionListDisplays"] = string.Empty;
                configurationOptionRow["SelectionListValues"] = string.Empty;
                var vals = pcInputsRow.ListItems.Split(',');
                for (var i = 0; i < vals.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        configurationOptionRow["SelectionListDisplays"] += "," + vals[i];
                    }
                    else
                    {
                        configurationOptionRow["SelectionListValues"] += "," + vals[i];
                    }
                }

                configurationOptionRow["SelectionListDisplays"] = configurationOptionRow[
                    "SelectionListDisplays"
                ]
                    .ToString()
                    .TrimStart(',');
                configurationOptionRow["SelectionListValues"] = configurationOptionRow[
                    "SelectionListValues"
                ]
                    .ToString()
                    .TrimStart(',');
                priceListAdjust = this.ProcessSelectionListPrice(
                    configuratorDataSet.ConfiguratorDataSet,
                    configurationOptionRow["SelectionListValues"].ToString(),
                    pcInputsRow.InputName
                );
                configurationOptionRow["SelectionListPriceAdjustment"] = priceListAdjust;
            }
            else if (pcInputsRow.ControlType.ToLower() == "rectangle")
            {
                configurationOptionRow["Type"] = "Rectangle";
            }
            else if (pcInputsRow.ControlType.ToLower() == "browser")
            {
                configurationOptionRow["Type"] = "Container";

                // We need to determine the type of ISC container or Epicor Browser.  There are three types:  Image, Media or Link.
                if (string.IsNullOrEmpty(pcInputsRow.InitVal) == false)
                {
                    var currentFileExtension = Path.GetExtension(pcInputsRow.InitVal);
                    switch (currentFileExtension)
                    {
                        case ".png":
                        case ".gif":
                        case ".jpg":
                        case ".jpeg":
                        case ".bmp":
                        case ".tiff":
                        case ".tif":
                            configurationOptionRow["ContainerFileType"] = "Image";
                            break;
                        case ".wav":
                        case ".mov":
                        case ".mid":
                        case ".flv":
                        case ".mpg":
                        case ".mpg4":
                        case ".avi":
                            configurationOptionRow["ContainerFileType"] = "Media";
                            break;
                        default:
                            configurationOptionRow["ContainerFileType"] = "Link";
                            break;
                    }

                    configurationOptionRow["ContainerFileLocation"] = pcInputsRow.InitVal;

                    // If we do not have a known link that already exists somewhere out there in the internet, then we will look for this file on the hard drive.
                    if (pcInputsRow.InitVal.StartsWith("http") == false)
                    {
                        if (File.Exists(pcInputsRow.InitVal))
                        {
                            // Get the server where we will upload the file.
                            consoleURL = siteConnection.Url + "console";
                            serverURL = siteConnection.Url.ToString();

                            // Get the target path on the server to upload the file.
                            fileDirectory = this.TargetImagePath;

                            // Since fileDirectory is ultimately added manually, we will try to handle some very elementary variances in what could be added as a path.
                            if (fileDirectory.StartsWith("/") == false)
                            {
                                fileDirectory = fileDirectory.Insert(0, "/");
                            }

                            if (fileDirectory.EndsWith("/") == false)
                            {
                                fileDirectory = fileDirectory.Insert(fileDirectory.Length, "/");
                            }

                            imageFilePath = string.Format(
                                "{0}{1}{2}",
                                serverURL,
                                fileDirectory,
                                Path.GetFileName(pcInputsRow.InitVal)
                            );
                            configurationOptionRow["ContainerFileLocation"] = imageFilePath;
                            this.UploadContainerFile(
                                pcInputsRow.InitVal,
                                consoleURL,
                                fileDirectory,
                                siteConnection,
                                integrationJob
                            );
                        }
                        else
                        {
                            siteConnection.AddLogMessage(
                                integrationJob.Id.ToString(),
                                IntegrationJobLogType.Info,
                                $"ProcessConfigurationOption:  Could not find file {pcInputsRow.InitVal}."
                            );
                        }
                    }
                }
            }

            configurationOptionRow["IsVisible"] = !pcInputsRow.Invisible;
            configurationOptionRow["IsRequired"] = pcInputsRow.Required;
            configurationOptionRow["SetAsFutureDefault"] = pcInputsRow.IsGlobal;
            configurationOptionRow["XPosition"] = pcInputsRow.xPos;
            configurationOptionRow["YPosition"] = pcInputsRow.yPos;
            configurationOptionRow["Height"] = pcInputsRow.pHeight;
            configurationOptionRow["Width"] = pcInputsRow.pWidth;
            configurationOptionRow["LabelValue"] = pcInputsRow.SideLabel;
            configurationOptionRow["ToolTip"] = pcInputsRow.StatusText;
            configurationOptionRow["DefaultValue"] = pcInputsRow.InitVal;

            var fs = pcInputsRow.FormatString.Trim();
            configurationOptionRow["TextCharacterType"] = fs;

            if (configurationOptionRow["Type"].ToString().ToLower() == "text")
            {
                // We will set the TextCharacterType to a different value if we are working with Epicor 'Character' Inputs.  These correspond to ISC Text Inputs.
                // We will test the first letter of the Epicor 'Format string' to see what kind of limitations are being put on this text input.
                // if format string starts with N then the input only takes letters and numbers.  If A, then input only takes letters.  If !, then input only takes uppercase letters.
                // everything else should be any.
                if (fs.Length > 0)
                {
                    switch (fs[0].ToString().ToLower())
                    {
                        case "n":
                            configurationOptionRow["TextCharacterType"] = "Letters or Numbers";
                            break;
                        case "a":
                            configurationOptionRow["TextCharacterType"] = "Letters";
                            break;
                        case "!":
                            configurationOptionRow["TextCharacterType"] = "Uppercase Letters";
                            break;
                        default:
                            configurationOptionRow["TextCharacterType"] = "Any";
                            break;
                    }
                }
                else
                {
                    configurationOptionRow["TextCharacterType"] = "Any";
                }
            }

            configurationOptionRow["TextValidEntryList"] = pcInputsRow.ValList;
            if (pcInputsRow.DataType.ToLower() == "character")
            {
                configurationOptionRow["TextMinLength"] = "0";
                configurationOptionRow["TextMaxLength"] = Regex.Match(fs, @"\d+").Value;
            }
            else if (pcInputsRow.DataType.ToLower() == "decimal")
            {
                // The format string tells a user the format of the input of the decimal.  It goes like this.  if the format string looks like >>>,>>9.99 this corresponds to a decimal
                // number of 999,999.99.  This means that the decimal number can run from 0 to 999,999.99.  Another example is ->>>9.99, it corresponds to the decimal number of 9999.99.   The negative part
                // means that the number can actually go negative or -9999.99 to 9999.99.
                // We will get rid of any comma that comes in.  It is extraneous information.
                fs = fs.Replace(",", string.Empty);

                /*
                string minfs = fs.Replace(">", "").Replace("<","");
                configurationOptionRow["NumberMinValue"] = Convert.ToInt32(minfs);
                 */

                // Setting the minimum and maximum values of an input with a 'decimal' type.

                // One  needs to consider the minimum and maximum values as well as the format string of the input.
                // Per Tom F, this is what the mins and maxes of decimals should look like.
                /*
                    Epicor - Minimum    Epicor Maximum  Mask    ISC Minimum ISC Maximum
                    0                   0               ->>9.99 0           999.99
                    0                   0               ->>9.99 -999.99     999.99
                    5                   0               ->>9.99 5           999.99
                    0                   10              ->>9.99 0           10
                 */
                var acceptsNegatives = false;
                if (fs[0] == '-')
                {
                    acceptsNegatives = true;
                }

                var maxfs = fs.Replace("-", string.Empty).Replace(">", "9").Replace("<", "9");

                // Setting the ISC minimum decimal value.  If the Epicor decimal input has a minimum value specified, then use the minimum value for the ISC minimum value.  If the Epicor Minimum value is
                // not specified, then things get tricky.  One needs to look at the sign of the decimal input string and possibly the epicor maximum value to determine the value of the ISC minimum for the
                // decimal number.
                if (pcInputsRow.StartDec == 0)
                {
                    if (pcInputsRow.EndDec == 0)
                    {
                        if (acceptsNegatives)
                        {
                            configurationOptionRow["NumberMinValue"] = -Convert.ToDecimal(maxfs);
                        }
                        else
                        {
                            configurationOptionRow["NumberMinValue"] = 0;
                        }
                    }
                    else
                    {
                        configurationOptionRow["NumberMinValue"] = 0;
                    }
                }
                else
                {
                    configurationOptionRow["NumberMinValue"] = Convert.ToDecimal(
                        pcInputsRow.StartDec
                    );
                }

                // Setting the ISC max value.  If the Epicor decimal Input has no maximum value specified (or a zero value specified), use the information found in the Epicor Inputs 'formatstring' to determine
                // the maximum value (e.g. >>9.99 translates into a max of 999.99.)  If the Epicor decimal Input DOES have a maximum value specified, then use the maximum value to specify the
                // ISC max input value.
                if (pcInputsRow.EndDec == 0)
                {
                    configurationOptionRow["NumberMaxValue"] = Convert.ToDecimal(maxfs);
                }
                else
                {
                    configurationOptionRow["NumberMaxValue"] = Convert.ToDecimal(
                        pcInputsRow.EndDec
                    );
                }

                var parts = fs.Split('.').ToList();
                configurationOptionRow["NumberDecimals"] = parts.Count > 1 ? parts[1].Length : 0;
            }
        }
        catch (Exception exc)
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Error,
                string.Format(
                    "ProcessConfigurationOption exception: {0}.  The input is {1} and the type is {2}.",
                    exc.Message,
                    pcInputsRow.InputName,
                    pcInputsRow.ControlType
                )
            );
        }
    }

    protected virtual void ProcessConfigurationOptionCondition(
        ConfiguratorDataSetType configuratorDataSet,
        DataTable configurationOptionTable,
        DataTable configurationOptionConditionTable,
        DataTable configurationQueryResultTable,
        PcStatusListDataSetTypePcStatusList plRow,
        ConfiguratorDataSetTypePcDynLst pcDynLstRow,
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        try
        {
            var pcInputsRow = configuratorDataSet.ConfiguratorDataSet
                .Where(c => c is ConfiguratorDataSetTypePcInputs)
                .Select(c => c as ConfiguratorDataSetTypePcInputs)
                .FirstOrDefault(c => c.InputName == pcDynLstRow.InputName);

            var configurationOptionRow = configurationOptionTable.Rows.Find(
                new object[]
                {
                    plRow.PartNum,
                    plRow.RevisionNum,
                    pcDynLstRow.PageSeq,
                    pcDynLstRow.InputName
                }
            );
            if (configurationOptionRow != null)
            {
                configurationOptionRow["ComboConditionalList"] = true;
            }

            var configurationOptionConditionRow = configurationOptionConditionTable.Rows.Add(
                plRow.PartNum,
                plRow.RevisionNum,
                pcDynLstRow.PageSeq,
                pcDynLstRow.InputName,
                pcDynLstRow.ListSeq
            );

            configurationOptionConditionRow["ScriptConditionOriginal"] = pcDynLstRow.Condition;
            try
            {
                configurationOptionConditionRow["ScriptCondition"] =
                    this.ReplaceWordsWithJQuerySelectors(
                        pcDynLstRow.Condition,
                        pcDynLstRow.InputName,
                        siteConnection,
                        integrationJob
                    );
            }
            catch (Exception exc)
            {
                siteConnection.AddLogMessage(
                    integrationJob.Id.ToString(),
                    IntegrationJobLogType.Error,
                    string.Format(
                        "ProcessConfigurationOptionCondition-ScriptCondition exception: {0}.  The partnum is {1}. The input is {2}. The conditions is {3}.",
                        exc.Message,
                        plRow.PartNum,
                        pcDynLstRow.InputName,
                        pcDynLstRow.Condition
                    )
                );
            }

            configurationOptionConditionRow["QueryName"] = pcDynLstRow.BAQRunProgram
                ? pcDynLstRow.BAQProgramName
                : string.Empty;
            configurationOptionConditionRow["QueryDisplayField"] = pcDynLstRow.BAQDispValue;
            configurationOptionConditionRow["QueryInputField"] = pcDynLstRow.BAQInputVal;
            configurationOptionConditionRow["DefaultValue"] = pcDynLstRow.InitVal;
            configurationOptionConditionRow["SelectionListDisplays"] = string.Empty;
            configurationOptionConditionRow["SelectionListValues"] = string.Empty;
            if (pcInputsRow != null && pcInputsRow.ControlType.ToLower() == "radio-set")
            {
                var vals = pcDynLstRow.ListItems.Split(',');
                for (var i = 0; i < vals.Length - 1; i++)
                {
                    if (i % 2 == 0)
                    {
                        configurationOptionConditionRow["SelectionListDisplays"] += "," + vals[i];
                    }
                    else
                    {
                        configurationOptionConditionRow["SelectionListValues"] += "," + vals[i];
                    }
                }

                configurationOptionConditionRow["SelectionListDisplays"] =
                    configurationOptionConditionRow["SelectionListDisplays"]
                        .ToString()
                        .TrimStart(',');
                configurationOptionConditionRow["SelectionListValues"] =
                    configurationOptionConditionRow["SelectionListValues"]
                        .ToString()
                        .TrimStart(',');
            }
            else
            {
                configurationOptionConditionRow["SelectionListDisplays"] = pcDynLstRow.ListItems;
                configurationOptionConditionRow["SelectionListValues"] = pcDynLstRow.ListItems;
            }

            configurationOptionConditionRow["SelectionListPriceAdjustment"] =
                this.ProcessSelectionListPrice(
                    configuratorDataSet.ConfiguratorDataSet,
                    configurationOptionConditionRow["SelectionListValues"].ToString(),
                    pcDynLstRow.InputName
                );
            this.ProcessConfigurationQueryResult(
                configurationOptionConditionRow["QueryName"].ToString(),
                configurationQueryResultTable,
                siteConnection,
                integrationJob
            );
        }
        catch (Exception exc)
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Error,
                string.Format(
                    "ProcessConfigurationOptionCondition exception: {0}.  The input is {1} and the partnum is {2}.",
                    exc.Message,
                    pcDynLstRow.InputName,
                    plRow.PartNum
                )
            );
        }
    }

    protected virtual void ProcessConfigurationConditionFilter(
        ConfiguratorDataSetType configuratorDataSet,
        DataTable configurationConditionFilterTable,
        PcStatusListDataSetTypePcStatusList plRow,
        ConfiguratorDataSetTypePcDynLstCriteria pcDynLstCriteriaRow
    )
    {
        var configurationConditionFilterRow = configurationConditionFilterTable.Rows.Add(
            plRow.PartNum,
            plRow.RevisionNum,
            pcDynLstCriteriaRow.PageSeq,
            pcDynLstCriteriaRow.InputName,
            pcDynLstCriteriaRow.ListSeq,
            pcDynLstCriteriaRow.CriteriaSeq
        );
        configurationConditionFilterRow["ColumnName"] = pcDynLstCriteriaRow.ColumnName;
        configurationConditionFilterRow["Condition"] =
            pcDynLstCriteriaRow.Condition.ToLower() == "match"
                ? "Like"
                : pcDynLstCriteriaRow.Condition;
        var valueFrom = pcDynLstCriteriaRow.ValueFrom.ToLower();
        configurationConditionFilterRow["ValueFrom"] =
            valueFrom == "input"
                ? "OptionValue"
                : valueFrom == "const"
                    ? "Constant"
                    : "QueryField";
        configurationConditionFilterRow["AttributeValue"] =
            valueFrom == "input"
                ? pcDynLstCriteriaRow.InputValue
                : valueFrom == "const"
                    ? pcDynLstCriteriaRow.ConstantValue
                    : pcDynLstCriteriaRow.BAQColumn;
    }

    protected virtual void ProcessConfigurationQueryResult(
        string queryName,
        DataTable configurationQueryResultTable,
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        if (!string.IsNullOrEmpty(queryName))
        {
            var row = configurationQueryResultTable.Rows.Find(queryName);
            if (row != null)
            {
                return;
            }

            try
            {
                XmlNode dynamicQueryData;
                using (var dynamicQueryService = new DynamicQueryService())
                {
                    dynamicQueryService.Url = this.FixUrl(dynamicQueryService.Url);
                    dynamicQueryService.SetPolicy(this.Policy);

                    dynamicQueryService.Timeout = 3000 * 1000; // Set in milliseconds

                    dynamicQueryData = dynamicQueryService.ExecuteByID(
                        this.CompanyId,
                        queryName,
                        null,
                        out var callContext
                    );
                    var configurationQueryResultRow = configurationQueryResultTable.Rows.Add(
                        queryName,
                        dynamicQueryData.OuterXml
                    );
                }
            }
            catch (Exception ex)
            {
                siteConnection.AddLogMessage(
                    integrationJob.Id.ToString(),
                    IntegrationJobLogType.Error,
                    string.Format(
                        "ProcessConfigurationQueryResult exception: {0}.  QueryName is {1} ",
                        ex.Message,
                        queryName
                    )
                );
            }
        }
    }

    protected virtual string ProcessSelectionListPrice(
        object[] configuratorDataSet,
        string selectionListValues,
        string inputName
    )
    {
        var retVal = string.Empty;

        foreach (var inValue in selectionListValues.Split(','))
        {
            var pcInPriceRow = configuratorDataSet
                .Where(c => c is ConfiguratorDataSetTypePcInPrice)
                .Select(c => c as ConfiguratorDataSetTypePcInPrice)
                .FirstOrDefault(c => c.InputName == inputName && c.InValue == inValue);
            if (pcInPriceRow == null)
            {
                retVal += ",0";
            }
            else
            {
                retVal += "," + pcInPriceRow.ValuePrice;
            }
        }

        retVal = retVal.TrimStart(',');
        return retVal;
    }

    protected virtual void UploadContainerFile(
        string fileName,
        string uploadServer,
        string uploadPath,
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        try
        {
            var uploadUserId = new Guid(this.IntegrationUserProfileId);
            using (var client = new WebClient())
            {
                // Our upload system requires that you send in the place where you want to put the file as well as a valid ISC user.
                client.QueryString["Directory"] = uploadPath;
                client.QueryString["userProfileId"] = uploadUserId.ToString();
                client.UploadFile(uploadServer + "/fileupload.aspx", fileName);
            }
        }
        catch (Exception exc)
        {
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Error,
                string.Format(
                    "UploadContainerFile error is {0}. Getting Filename {1} from uploadserver {2} to uploadpath {3}",
                    exc.Message,
                    fileName,
                    uploadServer,
                    uploadPath
                )
            );
        }
    }

    protected virtual DataTable CreateConfigurationTable(DataSet productDataSet)
    {
        var configurationTable = productDataSet.Tables.Add("Configuration");
        var productName = configurationTable.Columns.Add("ProductName");
        var revision = configurationTable.Columns.Add("Revision");
        var displaySummary = configurationTable.Columns.Add("DisplaySummary");
        configurationTable.PrimaryKey = new[] { productName, revision };
        return configurationTable;
    }

    protected virtual DataTable CreateConfigurationPageTable(DataSet productDataSet)
    {
        var configurationPageTable = productDataSet.Tables.Add("ConfigurationPage");
        var productName = configurationPageTable.Columns.Add("ProductName");
        var revision = configurationPageTable.Columns.Add("Revision");
        var pageNumber = configurationPageTable.Columns.Add("PageNumber");
        configurationPageTable.Columns.Add("PageTitle");
        configurationPageTable.Columns.Add("SkipIfNoInputs");
        configurationPageTable.PrimaryKey = new[] { productName, revision, pageNumber };
        return configurationPageTable;
    }

    protected virtual DataTable CreateConfigurationOptionTable(DataSet productDataSet)
    {
        var configurationOptionTable = productDataSet.Tables.Add("ConfigurationOption");
        var productName = configurationOptionTable.Columns.Add("ProductName");
        var revision = configurationOptionTable.Columns.Add("Revision");
        var pageNumber = configurationOptionTable.Columns.Add("PageNumber");
        var name = configurationOptionTable.Columns.Add("Name");
        configurationOptionTable.Columns.Add("Description");
        configurationOptionTable.Columns.Add("Sequence");
        configurationOptionTable.Columns.Add("Type");
        configurationOptionTable.Columns.Add("IsVisible");
        configurationOptionTable.Columns.Add("IsRequired");
        configurationOptionTable.Columns.Add("SetAsFutureDefault");
        configurationOptionTable.Columns.Add("XPosition");
        configurationOptionTable.Columns.Add("YPosition");
        configurationOptionTable.Columns.Add("Height");
        configurationOptionTable.Columns.Add("Width");
        configurationOptionTable.Columns.Add("LabelValue");
        configurationOptionTable.Columns.Add("ToolTip");
        configurationOptionTable.Columns.Add("DefaultValue");
        configurationOptionTable.Columns.Add("TextMinLength");
        configurationOptionTable.Columns.Add("TextMaxLength");
        configurationOptionTable.Columns.Add("TextCharacterType");
        configurationOptionTable.Columns.Add("TextIsMultiline");
        configurationOptionTable.Columns.Add("TextValidEntryList");
        configurationOptionTable.Columns.Add("NumberMinValue");
        configurationOptionTable.Columns.Add("NumberMaxValue");
        configurationOptionTable.Columns.Add("NumberDecimals");
        configurationOptionTable.Columns.Add("RadioButtonOrientation");
        configurationOptionTable.Columns.Add("ComboConditionalList");
        configurationOptionTable.Columns.Add("SelectionListValues");
        configurationOptionTable.Columns.Add("SelectionListDisplays");
        configurationOptionTable.Columns.Add("SelectionListPriceAdjustment");
        configurationOptionTable.Columns.Add("SubconfigurationPartNumber");
        configurationOptionTable.Columns.Add("ContainerFileType");
        configurationOptionTable.Columns.Add("ContainerFileLocation");
        configurationOptionTable.PrimaryKey = new[] { productName, revision, pageNumber, name };
        return configurationOptionTable;
    }

    protected virtual DataTable CreateConfigurationOptionConditionTable(DataSet productDataSet)
    {
        var configurationOptionConditionTable = productDataSet.Tables.Add(
            "ConfigurationOptionCondition"
        );
        var productName = configurationOptionConditionTable.Columns.Add("ProductName");
        var revision = configurationOptionConditionTable.Columns.Add("Revision");
        var pageNumber = configurationOptionConditionTable.Columns.Add("PageNumber");
        var name = configurationOptionConditionTable.Columns.Add("Name");
        var sequence = configurationOptionConditionTable.Columns.Add("Sequence");
        configurationOptionConditionTable.Columns.Add("ScriptCondition");
        configurationOptionConditionTable.Columns.Add("ScriptConditionOriginal");
        configurationOptionConditionTable.Columns.Add("QueryName");
        configurationOptionConditionTable.Columns.Add("QueryDisplayField");
        configurationOptionConditionTable.Columns.Add("QueryInputField");
        configurationOptionConditionTable.Columns.Add("SelectionListDisplays");
        configurationOptionConditionTable.Columns.Add("SelectionListValues");
        configurationOptionConditionTable.Columns.Add("SelectionListPriceAdjustment");
        configurationOptionConditionTable.Columns.Add("DefaultValue");
        configurationOptionConditionTable.PrimaryKey = new[]
        {
            productName,
            revision,
            pageNumber,
            name,
            sequence
        };
        return configurationOptionConditionTable;
    }

    protected virtual DataTable CreateConfigurationConditionFilterTable(DataSet productDataSet)
    {
        var configurationConditionFilterTable = productDataSet.Tables.Add(
            "ConfigurationConditionFilter"
        );
        var productName = configurationConditionFilterTable.Columns.Add("ProductName");
        var revision = configurationConditionFilterTable.Columns.Add("Revision");
        var pageNumber = configurationConditionFilterTable.Columns.Add("PageNumber");
        var name = configurationConditionFilterTable.Columns.Add("Name");
        var conditionSequence = configurationConditionFilterTable.Columns.Add("ConditionSequence");
        var sequence = configurationConditionFilterTable.Columns.Add("Sequence");
        configurationConditionFilterTable.Columns.Add("ColumnName");
        configurationConditionFilterTable.Columns.Add("Condition");
        configurationConditionFilterTable.Columns.Add("ValueFrom");
        configurationConditionFilterTable.Columns.Add("AttributeValue");
        configurationConditionFilterTable.PrimaryKey = new[]
        {
            productName,
            revision,
            pageNumber,
            name,
            conditionSequence,
            sequence
        };
        return configurationConditionFilterTable;
    }

    protected virtual DataTable CreateConfigurationQueryResultTable(DataSet productDataSet)
    {
        var configurationQueryResultTable = productDataSet.Tables.Add("ConfigurationQueryResult");
        var queryName = configurationQueryResultTable.Columns.Add("QueryName");
        configurationQueryResultTable.Columns.Add("ResultDataSet");
        configurationQueryResultTable.PrimaryKey = new[] { queryName };
        return configurationQueryResultTable;
    }

    protected virtual string FixUrl(string url)
    {
        var urlTail = url.Substring(url.LastIndexOf('/'));
        var result = this.UrlHead + urlTail;
        return result;
    }

    protected virtual string ReplaceWordsWithJQuerySelectors(
        string providedString,
        string configurationOptionName,
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        var errorDescription = string.Empty;

        try
        {
            var fixedString = string.Empty;
            var exitParse = false;

            // FIX0: replace '<>' with '!='
            providedString = providedString.Replace("<>", "!=");

            // FIX1: remove leading and ending parentheses when they exist.
            if (
                providedString[providedString.Count() - 1] == ')'
                && providedString[providedString.Count() - 2] == ')'
                && providedString[0] == '('
                && providedString[1] == '('
            )
            {
                providedString = providedString.Remove(0, 1);
                providedString = providedString.Remove(providedString.LastIndexOf(")"));
            }

            // FIX2: replace single '=' char with double '=='  unless it is preceded or ends with a '<' or a '>'
            try
            {
                for (var i = 0; i < providedString.Length; i++)
                {
                    if (providedString[i].ToString() == "=")
                    {
                        if (
                            providedString[i - 1].ToString() != "<"
                            && providedString[i - 1].ToString() != ">"
                            && providedString[i + 1].ToString() != ">"
                            && providedString[i + 1].ToString() != "<"
                        )
                        {
                            if (
                                !(providedString[i + 1].ToString() == "=")
                                && !(providedString[i - 1].ToString() == "=")
                                && !(providedString[i + 1].ToString() == "!")
                                && !(providedString[i - 1].ToString() == "!")
                            )
                            {
                                providedString =
                                    providedString.Substring(0, i)
                                    + "=="
                                    + providedString.Substring(
                                        i + 1,
                                        (providedString.Length - 1) - i
                                    );
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                errorDescription += "creating '==' from '='.  ";
                siteConnection.AddLogMessage(
                    integrationJob.Id.ToString(),
                    IntegrationJobLogType.Error,
                    string.Format(
                        "An error was found in the replaceWordsWithJQuerySelectors method.  The error is {0}.  The ProvidedString is: '{1}' ",
                        errorDescription,
                        providedString
                    )
                );
            }

            // FIX3: sometimes the provided string is just "true" or "false", if so, we just need to create a conditional statment.
            // we are treating ConfiguraitonOptionCondition.ScriptCondition = true as ‘always run’ so it would be a script condition of 1 =1.
            try
            {
                if (providedString.ToLower() == "true")
                {
                    fixedString = "1==1";
                    exitParse = true;
                }

                if (providedString.ToLower() == "false")
                {
                    fixedString = "1==0";
                    exitParse = true;
                }
            }
            catch (Exception)
            {
                errorDescription += "cannot convert to simple conditional statement.  ";
                siteConnection.AddLogMessage(
                    integrationJob.Id.ToString(),
                    IntegrationJobLogType.Error,
                    string.Format(
                        "An error was found in the replaceWordsWithJQuerySelectors method.  The error is {0}.  The ProvidedString is: '{1}' ",
                        errorDescription,
                        providedString
                    )
                );
            }

            if (exitParse == false)
            {
                List<string> foundWords = null;

                try
                {
                    foundWords = this.BuildWordsFromString(providedString);
                }
                catch (Exception)
                {
                    errorDescription += "failed while building word list.  ";
                    siteConnection.AddLogMessage(
                        integrationJob.Id.ToString(),
                        IntegrationJobLogType.Error,
                        string.Format(
                            "An error was found in the replaceWordsWithJQuerySelectors method.  The error is {0}.  The ProvidedString is: '{1}' ",
                            errorDescription,
                            providedString
                        )
                    );
                }

                foundWords = foundWords.Distinct().ToList(); // remove duplicates
                fixedString = providedString;

                foreach (var unfixedWord in foundWords)
                {
                    if (!string.IsNullOrEmpty(unfixedWord))
                    {
                        if (unfixedWord.ToLower() is "true" or "false")
                        {
                            // FIX, make True or False lowercase for javascript,
                            fixedString = fixedString.Replace(unfixedWord, unfixedWord.ToLower());
                        }
                        else
                        {
                            if (unfixedWord.ToLower() == "and")
                            {
                                // FIX4: replace 'and' with '&&' only if its an isolated word, i.e. ' and ', not 'brAND'.
                                var fixedWord = "&&";

                                fixedString = fixedString.Replace(
                                    ' ' + unfixedWord + ' ',
                                    fixedWord
                                );
                            }
                            else
                            {
                                // FIX5: replace a non literal word with a jquery selector
                                // string fixedWord = "$('#" + unfixedWord + "').val()";
                                // string fixedWord = "configuration.GetOptionValue(\"" + unfixedWord + "\")";
                                var fixedWord = "Name == (\"" + unfixedWord + "\") && Value ";
                                fixedString = fixedString.Replace(unfixedWord, fixedWord);
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(errorDescription))
            {
                return fixedString;
            }

            return "/*Parser Error: '" + errorDescription + "' */" + fixedString;
        }
        catch (Exception exc)
        {
            var errorText =
                "/*Parser Error: "
                + errorDescription
                + ", here is the origional text: */"
                + providedString;
            siteConnection.AddLogMessage(
                integrationJob.Id.ToString(),
                IntegrationJobLogType.Error,
                $"An error was found in the replaceWordsWithJQuerySelectors method.  The error is {exc.Message}.  The provided string is {providedString} and the configurationOptionName is {configurationOptionName}.  Additional Info: {errorText}"
            );
            return errorText;
        }
    }

    protected virtual List<string> BuildWordsFromString(string providedString)
    {
        var wordWeAreWorkingOn = string.Empty;
        bool skipCurrentLetter;
        var wordIsntFinishedYet = false;

        char[] lettersInWords =
        {
            'a',
            'b',
            'c',
            'd',
            'e',
            'f',
            'g',
            'h',
            'i',
            'j',
            'k',
            'l',
            'm',
            'n',
            'o',
            'p',
            'q',
            'r',
            's',
            't',
            'u',
            'v',
            'w',
            'x',
            'y',
            'z',
            'A',
            'B',
            'C',
            'D',
            'E',
            'F',
            'G',
            'H',
            'I',
            'J',
            'K',
            'L',
            'M',
            'N',
            'O',
            'P',
            'Q',
            'R',
            'S',
            'T',
            'U',
            'V',
            'W',
            'X',
            'Y',
            'Z',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            '-'
        };
        char[] numbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        var foundWords = new List<string>();

        for (var i = 0; i < providedString.Length; i++)
        {
            skipCurrentLetter = false;

            foreach (var c in lettersInWords)
            {
                if (wordWeAreWorkingOn == string.Empty)
                {
                    // rule : word cant start with a number
                    foreach (var n in numbers)
                    {
                        if (c == n)
                        {
                            skipCurrentLetter = true;
                            break;
                        }
                    }
                }

                if (providedString[i] == c && skipCurrentLetter == false)
                {
                    wordWeAreWorkingOn += c;
                    wordIsntFinishedYet = false;
                    break;
                }
            }

            var wordPosition = providedString.IndexOf(wordWeAreWorkingOn);
            var totalLength = wordPosition + wordWeAreWorkingOn.Length;

            foreach (var c in lettersInWords)
            {
                // check next char to find end of word
                if (providedString.Count() > (i + 1))
                {
                    if (providedString[i + 1] == c)
                    {
                        wordIsntFinishedYet = true;
                        break;
                    }
                }
            }

            // only add word if it isn't preceeded by a number or a double quote
            if ((wordIsntFinishedYet == false) && (!string.IsNullOrEmpty(wordWeAreWorkingOn)))
            {
                var skipAddingWord = false;
                wordPosition = providedString.IndexOf(wordWeAreWorkingOn);

                foreach (var n in numbers)
                {
                    if (wordPosition - 1 >= 0)
                    {
                        if (providedString[wordPosition - 1].ToString() == n.ToString())
                        {
                            skipAddingWord = true;
                        }
                    }
                }

                if ((wordPosition - 1) > 0 && wordPosition <= providedString.Length)
                {
                    if (providedString[wordPosition - 1].ToString() == "\"")
                    {
                        skipAddingWord = true;
                    }
                }

                if (wordWeAreWorkingOn == "true")
                {
                    skipAddingWord = true;
                }

                if (skipAddingWord == false)
                {
                    // we have built a word.
                    foundWords.Add(wordWeAreWorkingOn);
                    wordWeAreWorkingOn = string.Empty;
                }
                else
                {
                    wordWeAreWorkingOn = string.Empty;
                }
            }
        }

        return foundWords;
    }
}
