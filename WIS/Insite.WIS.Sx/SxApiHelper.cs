namespace Insite.WIS.Sx;

using System;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Insite.Common.Helpers;
using Insite.WIS.Broker;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.Broker.Plugins;

/// <summary>Common code used for the SX API integration</summary>
public static class SxApiHelper
{
    public static void ProcessResponse<T>(
        T response,
        string methodName,
        bool throwExceptionOnErrorMessage
    )
    {
        // null response is a fatal error and we need to throw an exception
        if (response == null)
        {
            throw new Exception(string.Format("{0} returned null", methodName));
        }

        if (throwExceptionOnErrorMessage)
        {
            // all of these responses have an errorMessage property, get it via reflection.  If its populated this is a fatal error and we
            // should throw an exception
            var prop = response.GetType().GetProperty("errorMessage");
            var errorMessage = prop.GetValue(response, null) as string;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new Exception(string.Format("{0} returned {1}", methodName, errorMessage));
            }
        }
    }

    /// <summary>Serialize the object passed into the method and log the entire xml as a debug message. Note that debugging enabled will either need to be enabled in the
    /// IntegrationConnection or JobDefinition for it to actually log.</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="value">The object to serialize</param>
    /// <param name="source">The identifier of the message to give some context around what we are logging.</param>
    public static void SerializeAndLog(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        object value,
        string source
    )
    {
        if (
            integrationJob.JobDefinition.IntegrationConnection.DebuggingEnabled
            || integrationJob.JobDefinition.DebuggingEnabled
        )
        {
            // Log the whole object for debugging purposes (notice the IntegrationJobLogType.Debug type)
            var serializer = new XmlSerializer(value.GetType());
            var writer = new StringWriter();
            serializer.Serialize(writer, value);
            var jobLogger = new IntegrationJobLogger(siteConnection, integrationJob);
            jobLogger.Debug("[" + source + "] " + writer, false);
        }
    }

    /// <summary>A method that allows you to externally set the api properties. You add field mappings in the MC for example;
    /// From: Customer.Address1
    /// To: OEFullOrderMntV5inputBillTo.address1</summary>
    /// <param name="siteConnection"><see cref="SiteConnection"/> which encapsules connection to the site which we are integrating to.</param>
    /// <param name="integrationJob">The <see cref="IntegrationJob"/> that is to be executed.</param>
    /// <param name="jobStep">The <see cref="JobDefinitionStep"/> of the <see cref="IntegrationJob"/> being executed.</param>
    /// <param name="value">The object whose property should be set</param>
    /// <param name="keyParms">Used to filter the source of the value we set to 1 row</param>
    public static void SetDynamicProperties(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        object value,
        NameValueCollection keyParms = null
    )
    {
        foreach (var fieldMap in jobStep.JobDefinitionStepFieldMaps)
        {
            // the FromProperty we need to essentially find in the dataset.  The FromProperty will look like this (TableName.ColumnName)
            var splitFromProperty = fieldMap.FromProperty.Split(
                new[] { '.' },
                StringSplitOptions.RemoveEmptyEntries
            );
            if (splitFromProperty.Count() == 2)
            {
                var initialDataset = XmlDatasetManager.ConvertXmlToDataset(
                    integrationJob.InitialData
                );
                var dataTable = initialDataset.Tables[splitFromProperty[0]];
                var dataColumn = dataTable?.Columns[splitFromProperty[1]];
                if (dataColumn != null)
                {
                    // if we made it this far both the TableName and ColumnName exist in the dataset. We need to somehow tie our mapping to a
                    // single row inside the table or else we dont know what the 'real' to property is.
                    DataRow filteredDataRow = null;
                    if (keyParms != null)
                    {
                        var isMatch = true;
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            foreach (var key in keyParms.AllKeys)
                            {
                                if (dataRow[key].ToString() != keyParms[key])
                                {
                                    isMatch = false;
                                }
                            }

                            if (isMatch)
                            {
                                filteredDataRow = dataRow;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (dataTable.Rows.Count == 1)
                        {
                            filteredDataRow = dataTable.Rows[0];
                        }
                    }

                    if (filteredDataRow != null)
                    {
                        // if there is only a single column then its a straight mapping
                        var fromValue = filteredDataRow[splitFromProperty[1]];

                        // now lets use reflection and try to get the ToProperty.
                        var splitToProperty = fieldMap.ToProperty.Split(
                            new[] { '.' },
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        if (splitToProperty.Count() > 1)
                        {
                            var toTypeName = string.Empty;
                            var toTypeChunkCounter = 0;
                            foreach (var toTypeChunk in splitToProperty)
                            {
                                if (toTypeChunkCounter == splitToProperty.Count() - 1)
                                {
                                    toTypeName = toTypeName.Substring(0, toTypeName.Length - 1);
                                }
                                else
                                {
                                    toTypeName = toTypeChunk + ".";
                                }

                                toTypeChunkCounter++;
                            }

                            var toType = Type.GetType(toTypeName);
                            var prop = toType?.GetProperty(
                                splitToProperty[splitToProperty.Count() - 1]
                            );
                            prop?.SetValue(value, fromValue, null);
                        }
                    }
                }
            }
        }
    }
}
