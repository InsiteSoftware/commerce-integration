namespace Insite.WIS.DistributorDataSolutions.Parsers;

using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using Insite.WIS.Broker.Parsers;
using Insite.WIS.Broker.WebIntegrationService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class FileParserDDS : FileParserBase
{
    public FileParserDDS(IntegrationJob integrationJob, JobDefinitionStep jobDefinitionStep)
        : base(integrationJob, jobDefinitionStep) { }

    public override DataTable ParseFile(Stream stream, string fileName)
    {
        var dataTable = new DataTable(
            string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}",
                this.JobDefinitionStep.Sequence,
                this.JobDefinitionStep.ObjectName
            )
        );

        using (var streamReader = new StreamReader(stream))
        {
            string json;
            while ((json = streamReader.ReadLine()) != null)
            {
                var dictionary = new Dictionary<string, object>();
                PopulateDictionaryFromJToken(
                    dictionary,
                    JsonConvert.DeserializeObject<JToken>(json),
                    string.Empty
                );
                AddDictionaryToDataTable(dictionary, dataTable);
            }
        }

        return dataTable;
    }

    private static void PopulateDictionaryFromJToken(
        Dictionary<string, object> dict,
        JToken token,
        string prefix
    )
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                foreach (var prop in token.Children<JProperty>())
                {
                    PopulateDictionaryFromJToken(dict, prop.Value, Join(prefix, prop.Name));
                }

                break;
            case JTokenType.Array:
                var index = 0;
                foreach (var value in token.Children())
                {
                    PopulateDictionaryFromJToken(dict, value, Join(prefix, (++index).ToString()));
                }

                break;
            default:
                dict.Add(prefix, ((JValue)token).Value);
                break;
        }
    }

    private static string Join(string prefix, string name)
    {
        return string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";
    }

    private static void AddDictionaryToDataTable(
        Dictionary<string, object> dictionary,
        DataTable dataTable
    )
    {
        var dataRow = dataTable.NewRow();

        foreach (var property in dictionary)
        {
            if (!dataTable.Columns.Contains(property.Key))
            {
                dataTable.Columns.Add(property.Key);
            }

            dataRow[property.Key] = property.Value;
        }

        dataTable.Rows.Add(dataRow);
    }
}
