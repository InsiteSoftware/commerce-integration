namespace Insite.WIS.InRiver;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.WIS.Broker;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.InRiver.Models;

/// <summary>The integration processor in river style class.</summary>
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.InRiver)]
internal class IntegrationProcessorInRiverStyleClass : IntegrationProcessorInRiver
{
    public IntegrationProcessorInRiverStyleClass(IntegrationProcessorFileHelper fileHelper)
        : base(fileHelper) { }

    public override DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep integrationJobStep
    )
    {
        this.InRiverObjectName = "Item";
        return base.Execute(siteConnection, integrationJob, integrationJobStep);
    }

    public override DataTable ProcessInRiverCollection(
        IEnumerable<InRiverGenericObject> inRiverCollection,
        DataTable dataTableSchema,
        IntegrationJob integrationJob,
        SiteConnection siteConnection,
        JobDefinitionStep integrationJobStep
    )
    {
        var integrationTable = new DataTable(dataTableSchema.TableName);
        this.AddRequiredColumnsToTable(integrationTable);

        foreach (var collectionObject in inRiverCollection)
        {
            this.LogHelper.LogProcessingInRiverObject("Item", collectionObject.UniqueIdentifier);

            var styleClass = this.StyleClass(
                collectionObject.Parents.FirstOrDefault()?.UniqueIdentifier
            );

            foreach (DataColumn column in dataTableSchema.Columns)
            {
                var fields = new List<string>();

                var translatedAttributeValue = this.GetTranslatedFieldValue(
                    collectionObject.Fields,
                    column.ColumnName,
                    this.DefaultLanguage
                );
                translatedAttributeValue = this.LookupCommonVocabularyKeyValue(
                    "Variant",
                    column.ColumnName,
                    translatedAttributeValue
                );

                var styleTrait = column.ColumnName.Replace("Variant", string.Empty);
                var styleTraitValue = translatedAttributeValue;
                fields.AddRange(new List<string> { styleClass, styleTrait, styleTraitValue });
                integrationTable.LoadDataRow(fields.ToArray(), true);
            }
        }

        return integrationTable;
    }

    protected void AddRequiredColumnsToTable(DataTable integrationTable)
    {
        integrationTable.Columns.Add("StyleClass");
        integrationTable.Columns.Add("StyleTrait");
        integrationTable.Columns.Add("StyleTraitValue");
    }

    private string StyleClass(string uniqueIdentifier)
    {
        return this.ProductStyleLookupTable.Rows
                .Cast<DataRow>()
                .Where(row => row["UniqueIdentifier"].Equals(uniqueIdentifier))
                .Select(row => row["StyleClass"].ToString())
                .FirstOrDefault() ?? string.Empty;
    }
}
