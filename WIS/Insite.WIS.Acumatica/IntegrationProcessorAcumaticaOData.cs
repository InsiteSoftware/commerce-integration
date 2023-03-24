namespace Insite.WIS.Acumatica;

using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Insite.Common.Helpers;
using Insite.Integration.Enums;
using Insite.Integration.Attributes;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.WebIntegrationService;
using Newtonsoft.Json.Linq;
using Insite.Common.HttpUtilities;
using Insite.Common.Dependencies;
using System.Threading;
using Newtonsoft.Json;

[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.Acumatica)]
public class IntegrationProcessorAcumaticaOData : IIntegrationProcessor
{
    private readonly HttpClientProvider httpClientProvider;

    public IntegrationProcessorAcumaticaOData()
        : this(DependencyLocator.Current.GetInstance<HttpClientProvider>()) { }

    public IntegrationProcessorAcumaticaOData(HttpClientProvider httpClientProvider)
    {
        this.httpClientProvider = httpClientProvider;
    }

    public DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep
    )
    {
        var jobLogger = new IntegrationJobLogger(siteConnection, integrationJob);

        var httpClient = this.httpClientProvider.GetHttpClient(
            new Uri(integrationJob.JobDefinition.IntegrationConnection.Url)
        );

        var results = new List<ExpandoObject>();

        var page = 0;
        const int pageSize = 500;
        int recordCount;

        do
        {
            var requestUri = GetRequestUri(jobStep, page, pageSize);

            jobLogger.Debug($"Starting Executing Request: {requestUri}");

            var response = CallODataApi(httpClient, requestUri, integrationJob);

            var resultsPage = JObject.Parse(response)["value"].ToObject<List<ExpandoObject>>();
            results.AddRange(resultsPage);

            recordCount = resultsPage.Count;
            page++;
        } while (recordCount == pageSize);

        var dataTable = CreateDataTable(results);

        var dataTableName = string.Format(
            CultureInfo.InvariantCulture,
            "{0}{1}",
            jobStep.Sequence,
            jobStep.ObjectName
        );
        dataTable.TableName = dataTableName;

        var dataSet = new DataSet();
        dataSet.Tables.Add(dataTable);

        return dataSet;
    }

    private static string CallODataApi(
        HttpClient httpClient,
        string requestUri,
        IntegrationJob integrationJob
    )
    {
        HttpResponseMessage httpResponse;
        using (var request = new HttpRequestMessage())
        {
            request.RequestUri = new Uri(requestUri, UriKind.Relative);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = GetTokenRequestAuthenticationHeader(
                integrationJob.JobDefinition.IntegrationConnection
            );

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            httpResponse = httpClient.SendAsync(request, CancellationToken.None).Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        if (httpResponse == null)
        {
            throw new DataException("Error calling API. Response is null.");
        }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new DataException(
                $"Error calling API. Status Code: {httpResponse.StatusCode}. Reason Phrase: {httpResponse.ReasonPhrase}. Content: foobar"
            );
        }

        return responseContent;
    }

    private static AuthenticationHeaderValue GetTokenRequestAuthenticationHeader(
        IntegrationConnection integrationConnection
    )
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var password = EncryptionHelper.DecryptAes(integrationConnection.Password);
#pragma warning restore

        return new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{integrationConnection.LogOn}:{password}")
            )
        );
    }

    private static string GetRequestUri(JobDefinitionStep jobDefinitionStep, int page, int pageSize)
    {
        var uri = $"odata/{jobDefinitionStep.FromClause}?$format=json";

        if (!string.IsNullOrEmpty(jobDefinitionStep.SelectClause))
        {
            uri += $"&$select={jobDefinitionStep.SelectClause}";
        }

        if (!string.IsNullOrEmpty(jobDefinitionStep.WhereClause))
        {
            uri += $"&$filter={jobDefinitionStep.WhereClause}";
        }

        uri += $"&$top={pageSize}&$skip={page * pageSize}";

        return uri;
    }

    internal static DataTable CreateDataTable(List<ExpandoObject> results)
    {
        var dataTable = new DataTable();

        foreach (var result in results)
        {
            var dataRow = dataTable.NewRow();

            foreach (var property in result)
            {
                var dataColumn = dataTable.Columns[property.Key];
                if (dataColumn == null)
                {
                    dataColumn = new DataColumn(property.Key);
                    dataTable.Columns.Add(dataColumn);
                }

                dataRow[property.Key] = property.Value;
            }

            dataTable.Rows.Add(dataRow);
        }

        return dataTable;
    }
}
