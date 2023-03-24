namespace Insite.WIS.InRiver;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Insite.Common.Helpers;
using Insite.WIS.InRiver.Interfaces;
using Insite.WIS.InRiver.Models;
using Insite.WIS.Broker;
using Insite.WIS.Broker.WebIntegrationService;
using TranslationDictionary = Insite.WIS.InRiver.Models.TranslationDictionary;

/// <summary>The integration processor for inRiver XML files.</summary>
public abstract class IntegrationProcessorInRiver : IIntegrationProcessorInRiver
{
    protected readonly IntegrationProcessorFileHelper FileHelper;

    protected IntegrationProcessorInRiver(IntegrationProcessorFileHelper fileHelper)
    {
        this.DefaultLanguage = string.Empty;
        this.InRiverObjectName = string.Empty;
        this.FileHelper = fileHelper;
    }

    protected DataTable CommonVocabularyTable { get; set; }

    protected DataTable LanguageLookupTable { get; set; }

    protected DataTable ProductStyleLookupTable { get; set; }

    protected DataTable WebsiteLookupTable { get; set; }

    /// <summary>Gets or sets the website lookup table.</summary>
    protected DataTable CategoryLookupTable { get; set; }

    protected string InRiverObjectName { get; set; }

    protected string DefaultLanguage { get; set; }

    protected IntegrationProcessorLogHelper LogHelper { get; set; }

    /// <summary>The ProcessInRiverCollection</summary>
    /// <param name="inRiverCollection">Enumerable list of <see cref="InRiverGenericObject"/>.</param>
    /// <param name="dataTableSchema">The table schema passed out of integration job.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are
    /// integrating to.</param>
    /// <param name="integrationJobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/>
    /// being executed.</param>
    /// <returns>The <see cref="DataTable"/>.</returns>
    public abstract DataTable ProcessInRiverCollection(
        IEnumerable<InRiverGenericObject> inRiverCollection,
        DataTable dataTableSchema,
        IntegrationJob integrationJob,
        SiteConnection siteConnection,
        JobDefinitionStep integrationJobStep
    );

    public virtual IEnumerable<InRiverGenericObject> ParseXmlFile(
        string processingFileName,
        IntegrationConnection integrationConnection
    )
    {
        using (
            var fileStream = this.FileHelper.StreamFromFilePath(
                processingFileName,
                integrationConnection
            )
        )
        {
            using (
                var reader =
                    fileStream != null
                        ? new StreamReader(fileStream, true)
                        : new StreamReader(processingFileName, true)
            )
            {
                var document = XDocument.Load(reader);
                var rootElement = document.Root;
                var xmlNamespace = rootElement.GetDefaultNamespace();

                var inRiverObjects = new List<InRiverGenericObject>();
                var xmlNodes = rootElement.Elements();
                foreach (var node in xmlNodes)
                {
                    var inRiverObject = new InRiverGenericObject
                    {
                        UniqueIdentifier = IntegrationProcessorXmlHelper.GetUniqueIdentifier(
                            node,
                            xmlNamespace
                        ),
                        Action = IntegrationProcessorXmlHelper.GetAttributeValue("Action", node),
                        Fields = new Dictionary<string, IEnumerable<TranslationDictionary>>(),
                        Parents = new List<InRiverGenericObject>(),
                        Children = new List<InRiverGenericObject>()
                    };

                    IntegrationProcessorXmlHelper.ProcessFields(
                        node,
                        xmlNamespace,
                        this.InRiverObjectName,
                        this.DefaultLanguage,
                        inRiverObject
                    );

                    var linksNode = node.Element(xmlNamespace + this.InRiverObjectName + "Links");
                    if (linksNode != null)
                    {
                        IntegrationProcessorXmlHelper.ProcessParentNode(
                            linksNode,
                            xmlNamespace,
                            this.InRiverObjectName,
                            inRiverObject
                        );
                        IntegrationProcessorXmlHelper.ProcessChildrenNodes(
                            linksNode,
                            xmlNamespace,
                            this.InRiverObjectName,
                            inRiverObject
                        );
                    }

                    inRiverObjects.Add(inRiverObject);
                }

                return inRiverObjects;
            }
        }
    }

    /// <summary>Executes the job associated with the passed in <see cref="IntegrationJob"/> and<see cref="JobDefinitionStep"/>.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are
    /// integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="integrationJobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/>
    /// being executed.</param>
    /// <returns>The results of the passed in <see cref="IntegrationJob"/> and <see cref="JobDefinitionStep"/></returns>
    public virtual DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep integrationJobStep
    )
    {
        this.LogHelper = this.CreateLogHelper(siteConnection, integrationJob);
        var integrationConnection = integrationJob.JobDefinition.IntegrationConnection;

        this.ValidateDependancies(integrationJob, integrationJobStep);
        this.PopulatePreprocessorLookupTables(integrationJob);

        this.DefaultLanguage = this.GetDefaultLanguage();
        var files = this.FileHelper.RetrieveFilesForProcessing(
            integrationJob,
            integrationJobStep,
            integrationConnection
        );
        this.LogHelper.LogFileFoundCount(
            integrationJobStep,
            files.Count,
            integrationJobStep.FromClause
        );

        var inRiverObjects = this.ProcessFiles(integrationJob, files, integrationJobStep);

        var dataSetForPostProcessor = new DataSet();
        var table = this.CreateWorkingTable(
            inRiverObjects,
            siteConnection,
            integrationJob,
            integrationJobStep
        );
        dataSetForPostProcessor.Tables.Add(table);

        return dataSetForPostProcessor;
    }

    protected virtual IntegrationProcessorLogHelper CreateLogHelper(
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        return new IntegrationProcessorLogHelper(siteConnection, integrationJob);
    }

    private List<InRiverGenericObject> ProcessFiles(
        IntegrationJob integrationJob,
        IEnumerable<string> files,
        JobDefinitionStep integrationJobStep
    )
    {
        var inRiverObjects = new List<InRiverGenericObject>();
        var doNotArchiveParameter = integrationJobStep.JobDefinitionStepParameters.FirstOrDefault(
            o => o.Name.EqualsIgnoreCase("DoNotArchive")
        );
        var doNotArchive =
            doNotArchiveParameter != null && doNotArchiveParameter.Value.EqualsIgnoreCase("true");

        foreach (var fileName in files.Distinct())
        {
            var processingFileName = doNotArchive
                ? fileName
                : this.FileHelper.MoveFileForProcessing(fileName, integrationJob);

            try
            {
                this.LogHelper.LogFileProcessingStart(fileName);

                var objectList = this.ParseXmlFile(
                    processingFileName,
                    integrationJob.JobDefinition.IntegrationConnection
                );
                foreach (var inRiverGenericObject in objectList)
                {
                    var oldObject = inRiverObjects.SingleOrDefault(
                        o =>
                            o.UniqueIdentifier.Equals(
                                inRiverGenericObject.UniqueIdentifier,
                                StringComparison.InvariantCultureIgnoreCase
                            )
                    );
                    if (oldObject != null)
                    {
                        inRiverObjects.Remove(oldObject);
                    }

                    inRiverObjects.Add(inRiverGenericObject);
                }

                if (!doNotArchive)
                {
                    this.FileHelper.MoveFileToProcessed(
                        integrationJob,
                        processingFileName,
                        fileName
                    );
                }

                this.LogHelper.LogFileProcessingFinish(fileName);
            }
            catch (IOException ex)
            {
                this.LogHelper.LogFileAccessException(ex.Message, fileName);
            }
            catch (Exception exception)
            {
                this.LogHelper.LogFileParseException(exception.Message, fileName);
                this.FileHelper.ProcessBadFile(
                    processingFileName,
                    integrationJob.JobDefinition.IntegrationConnection
                );
            }
        }

        return inRiverObjects;
    }

    private DataTable CreateWorkingTable(
        IEnumerable<InRiverGenericObject> inRiverObjects,
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep integrationJobStep
    )
    {
        var schema = this.CreateDataTableSchema(integrationJobStep);

        var workingTable = integrationJobStep.ObjectName.Equals(
            "TranslationDictionary",
            StringComparison.OrdinalIgnoreCase
        )
            ? this.CreateTranslationDictionary(inRiverObjects, schema)
            : this.ProcessInRiverCollection(
                inRiverObjects,
                schema,
                integrationJob,
                siteConnection,
                integrationJobStep
            );

        return workingTable;
    }

    private DataTable CreateTranslationDictionary(
        IEnumerable<InRiverGenericObject> inRiverCollection,
        DataTable dataTableSchema
    )
    {
        var tableName = dataTableSchema.TableName;
        var integrationTable = new DataTable(tableName);
        integrationTable.Columns.Add("Key");
        integrationTable.Columns.Add("Language");
        integrationTable.Columns.Add("Value");

        foreach (var collectionObject in inRiverCollection)
        {
            this.LogHelper.LogProcessingInRiverObject(
                "Translation Dictionary",
                collectionObject.UniqueIdentifier
            );

            foreach (DataColumn column in dataTableSchema.Columns)
            {
                var translatedAttributeValue = this.GetTranslatedFieldValue(
                    collectionObject.Fields,
                    column.ColumnName,
                    this.DefaultLanguage
                );
                var translationDictionary = this.GetAllTranslatedFieldValues(
                    collectionObject.Fields,
                    column.ColumnName
                );
                foreach (
                    var fields in translationDictionary.Select(
                        o => new List<string> { translatedAttributeValue, o.Key, o.Value }
                    )
                )
                {
                    integrationTable.LoadDataRow(fields.ToArray(), true);
                }
            }
        }

        return integrationTable;
    }

    private DataTable CreateDataTableSchema(JobDefinitionStep jobStep)
    {
        var dataTableName = string.Format(
            CultureInfo.InvariantCulture,
            "{0}{1}",
            jobStep.Sequence,
            jobStep.ObjectName
        );
        var dataTableSchema = new DataTable(dataTableName);

        foreach (
            var columnName in jobStep.SelectClause
                .Split(',')
                .Where(columnName => !dataTableSchema.Columns.Contains(columnName.Trim()))
        )
        {
            if (!dataTableSchema.Columns.Contains(columnName.Trim()))
            {
                dataTableSchema.Columns.Add(columnName.Trim());
            }
        }

        return dataTableSchema;
    }

    private string GetDefaultLanguage()
    {
        var defaultLanguageRow = this.LanguageLookupTable.Select("IsDefault").FirstOrDefault();
        if (defaultLanguageRow == null)
        {
            throw new ArgumentException("There is no default language defined for website.");
        }

        var defaultLanguage = defaultLanguageRow["Language"].ToString();
        return defaultLanguage;
    }

    protected string GetTranslatedFieldValue(
        IDictionary<string, IEnumerable<TranslationDictionary>> fields,
        string fieldName,
        string languageCode
    )
    {
        var fieldNode = fields
            .SingleOrDefault(o => o.Key.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
            .Value;
        if (fieldNode == null)
        {
            return string.Empty;
        }

        var translatedNode = fieldNode.SingleOrDefault(
            o => o.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase)
        );
        if (translatedNode == null && languageCode.Contains("-"))
        {
            var alternateLanguageCode = languageCode.Split('-').First();
            translatedNode = fieldNode.SingleOrDefault(
                o =>
                    o.LanguageCode.Equals(alternateLanguageCode, StringComparison.OrdinalIgnoreCase)
            );
        }

        return translatedNode == null ? string.Empty : translatedNode.TranslatedValue;
    }

    protected Dictionary<string, string> GetAllTranslatedFieldValues(
        IDictionary<string, IEnumerable<TranslationDictionary>> fields,
        string fieldName
    )
    {
        var fieldNode = fields
            .SingleOrDefault(o => o.Key.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
            .Value;
        if (fieldNode == null)
        {
            return new Dictionary<string, string>();
        }

        var translatedFields = fieldNode.Where(
            o => this.LanguageLookupTable.Rows.Contains(o.LanguageCode)
        );

        return translatedFields.ToDictionary(o => o.LanguageCode, o => o.TranslatedValue);
    }

    protected string LookupCommonVocabularyKeyValue(
        string attributePrefix,
        string attributeName,
        string attributeValue
    )
    {
        // Lookup attribute value from the attribute name first i.e. ProductColor
        var commonVocabularyRow = this.CommonVocabularyTable
            .AsEnumerable()
            .FirstOrDefault(
                o =>
                    o.Field<string>("AttributeType")
                        .Equals(attributeName, StringComparison.OrdinalIgnoreCase)
                    && o.Field<string>("AttributeKey")
                        .Equals(attributeValue, StringComparison.OrdinalIgnoreCase)
            );

        // If not found, then lookup attribute value by stripping prefix i.e. Color
        if (commonVocabularyRow == null)
        {
            var attributeType = attributeName.Replace(attributePrefix, string.Empty);
            commonVocabularyRow = this.CommonVocabularyTable
                .AsEnumerable()
                .FirstOrDefault(
                    o =>
                        o.Field<string>("AttributeType")
                            .Equals(attributeType, StringComparison.OrdinalIgnoreCase)
                        && o.Field<string>("AttributeKey")
                            .Equals(attributeValue, StringComparison.OrdinalIgnoreCase)
                );
        }

        return commonVocabularyRow != null
            ? commonVocabularyRow["AttributeValue"].ToString()
            : attributeValue;
    }

    protected void ValidateDependancies(IntegrationJob integrationJob, JobDefinitionStep jobStep)
    {
        if (integrationJob == null)
        {
            throw new ArgumentNullException("integrationJob");
        }

        if (jobStep == null)
        {
            throw new ArgumentNullException("jobStep");
        }

        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        if (!initialDataset.Tables.Contains("LanguageLookup"))
        {
            throw new ArgumentException(
                "LanguageLookup Table not found in dataset. This integration processor requires the preprocessor inRiverLookupInformation."
            );
        }

        if (!initialDataset.Tables.Contains("CommonVocabulary"))
        {
            throw new ArgumentException(
                "CommonVocabulary Lookup Table not found in dataset. This integration processor requires the preprocessor inRiverLookupInformation."
            );
        }

        if (!initialDataset.Tables.Contains("ProductStyleLookup"))
        {
            throw new ArgumentException(
                "ProductStyleLookupTable Lookup Table not found in dataset. This integration processor requires the preprocessor inRiverLookupInformation."
            );
        }

        if (!initialDataset.Tables.Contains("WebsiteLookup"))
        {
            throw new ArgumentException(
                "WebsiteLookup Lookup Table not found in dataset. This integration processor requires the preprocessor inRiverLookupInformation."
            );
        }
    }

    protected void PopulatePreprocessorLookupTables(IntegrationJob integrationJob)
    {
        var initialDataset = XmlDatasetManager.ConvertXmlToDataset(integrationJob.InitialData);

        this.LanguageLookupTable = initialDataset.Tables["LanguageLookup"];
        this.CommonVocabularyTable = initialDataset.Tables["CommonVocabulary"];
        this.ProductStyleLookupTable = initialDataset.Tables["ProductStyleLookup"];
        this.WebsiteLookupTable = initialDataset.Tables["WebsiteLookup"];
        this.CategoryLookupTable = initialDataset.Tables["CategoryLookup"];
    }
}
