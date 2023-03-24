namespace Insite.WIS.CloudSuiteDistribution;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Insite.Common.Extensions;
using Insite.Common.HttpUtilities;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.Providers;
using Insite.WIS.Broker.WebIntegrationService;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;

[IntegrationConnector(IntegrationConnectorType.CloudSuiteDistribution)]
public class IntegrationProcessorCompassApiQuery : IIntegrationProcessor
{
    internal const int DefaultPageSize = 10000;

    internal const string CompassApiPageSize = "CompassApiBatchSize";

    internal const string RunningStatus = "RUNNING";

    internal const string FinishedStatus = "FINISHED";

    internal static readonly TimeSpan QueryPollingInterval = TimeSpan.FromSeconds(1);

    internal static TimeSpan IntegrationJobTimeout = TimeSpan.FromMinutes(30);

    internal const string InvalidSubmitJobResponseMessage = "Invalid Submit Job Response: {0}";

    internal const string InvalidJobStatusResponseMessage = "Invalid Job Status Response: {0}";

    internal const string InvalidJobResultsResponseMessage = "Invalid Job Results Response: {0}";

    private readonly HttpClientProvider httpClientProvider;

    private readonly OAuthTokenProvider oAuthTokenProvider;

    internal IIntegrationJobLogger IntegrationJobLogger;

    public IntegrationProcessorCompassApiQuery(
        HttpClientProvider httpClientProvider,
        OAuthTokenProvider oAuthTokenProvider
    )
    {
        this.httpClientProvider = httpClientProvider;
        this.oAuthTokenProvider = oAuthTokenProvider;
    }

    public DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobDefinitionStep
    )
    {
        this.IntegrationJobLogger ??= new IntegrationJobLogger(siteConnection, integrationJob);

        var dataSet = new DataSet();

        try
        {
            var validateIntegrationQueryResults =
                IntegrationProcessorQueryBase.ValidateIntegrationQuery(
                    jobDefinitionStep.IntegrationQuery
                );
            if (validateIntegrationQueryResults.IsNotBlank())
            {
                this.IntegrationJobLogger.Fatal(validateIntegrationQueryResults);
                return dataSet;
            }

            var integrationConnection =
                jobDefinitionStep.IntegrationConnectionOverride
                ?? integrationJob.JobDefinition.IntegrationConnection;
            var resource = integrationConnection.Client;

            var httpClient = this.httpClientProvider.GetHttpClient(
                new Uri(integrationConnection.Url)
            );

            var sw = Stopwatch.StartNew();
            this.IntegrationJobLogger.Debug($"Start getting authorization headers");
            var headers = IntegrationProcessorApiRefreshBase.GetAuthorizationAndAcceptHeaders(
                this.oAuthTokenProvider,
                integrationConnection,
                "*/*"
            );
            headers.Add("Accept-Encoding", "identity");
            this.IntegrationJobLogger.Debug(
                $"Finished getting authorization headers ({sw.ElapsedMilliseconds} milliseconds)"
            );

            IntegrationProcessorQueryBase.LogIntegrationQuery(
                this.IntegrationJobLogger,
                jobDefinitionStep.IntegrationQuery
            );

            sw.Restart();
            this.IntegrationJobLogger.Debug($"Start submitting job");
            var (queryId, submitErrorMessage) = SubmitJob(
                httpClient,
                resource,
                jobDefinitionStep.IntegrationQuery,
                headers
            );
            if (submitErrorMessage.IsNotBlank())
            {
                this.IntegrationJobLogger.Fatal(
                    $"{submitErrorMessage} ({sw.ElapsedMilliseconds} milliseconds)"
                );
                return dataSet;
            }

            this.IntegrationJobLogger.Debug(
                $"Finished submitting job got query id {queryId} ({sw.ElapsedMilliseconds} milliseconds)"
            );

            sw.Restart();
            this.IntegrationJobLogger.Debug($"Start polling for job status");
            var (rowCount, pollingErrorMessage) = PollForJobCompletion(
                httpClient,
                resource,
                queryId,
                headers
            );
            if (pollingErrorMessage.IsNotBlank())
            {
                this.IntegrationJobLogger.Fatal(
                    $"{pollingErrorMessage} ({sw.ElapsedMilliseconds} milliseconds)"
                );
                return dataSet;
            }

            this.IntegrationJobLogger.Debug(
                $"Finished polling for job status got row count {rowCount} ({sw.ElapsedMilliseconds} milliseconds)"
            );

            var pageSize = GetPageSize(integrationJob, jobDefinitionStep);
            sw.Restart();
            this.IntegrationJobLogger.Debug(
                $"Start getting job results for {rowCount} rows using page size {pageSize}"
            );
            var (dataTable, resultsErrorMessage) = GetJobResults(
                httpClient,
                resource,
                queryId,
                rowCount,
                pageSize,
                $"{jobDefinitionStep.Sequence}{jobDefinitionStep.ObjectName}",
                this.IntegrationJobLogger,
                headers
            );
            if (resultsErrorMessage.IsNotBlank())
            {
                this.IntegrationJobLogger.Fatal(
                    $"{resultsErrorMessage} ({sw.ElapsedMilliseconds} milliseconds)"
                );
                return dataSet;
            }

            this.IntegrationJobLogger.Debug(
                $"Finished getting job results ({sw.ElapsedMilliseconds} milliseconds)"
            );

            dataSet.Tables.Add(dataTable);

            return dataSet;
        }
        catch (Exception e)
        {
            this.IntegrationJobLogger.Fatal($"Unhandled Exception {e.GetFullErrorMessage()}");
            return dataSet;
        }
    }

    internal static (string, string) SubmitJob(
        HttpClient httpClient,
        string resource,
        string integrationQuery,
        IDictionary<string, string> headers
    )
    {
        var response = IntegrationProcessorApiRefreshBase.DoRequest(
            httpClient,
            resource,
            HttpMethod.Post,
            new StringContent(integrationQuery, Encoding.UTF8, "text/plain"),
            headers
        );

        JObject parsedResponse;
        try
        {
            parsedResponse = JObject.Parse(response);
        }
        catch (Exception e)
        {
            return (
                null,
                string.Format(
                    InvalidSubmitJobResponseMessage,
                    $"Error parsing JSON with exception {e.GetFullErrorMessage()}"
                )
            );
        }

        parsedResponse.TryGetValue("status", out var statusToken);
        var status = statusToken?.Value<string>() ?? "null";
        if (!status.Equals(RunningStatus, StringComparison.OrdinalIgnoreCase))
        {
            return (
                null,
                string.Format(
                    InvalidSubmitJobResponseMessage,
                    $"Invalid status '{status}', expecting '{RunningStatus}'"
                )
            );
        }

        parsedResponse.TryGetValue("queryId", out var queryIdToken);
        var queryId = queryIdToken?.Value<string>() ?? string.Empty;
        if (string.IsNullOrEmpty(queryId))
        {
            return (
                null,
                string.Format(
                    InvalidSubmitJobResponseMessage,
                    $"Invalid query id, query id was null or empty"
                )
            );
        }

        return (queryId, null);
    }

    internal static (long, string) PollForJobCompletion(
        HttpClient httpClient,
        string resource,
        string queryId,
        IDictionary<string, string> headers,
        bool callJobResultsOnInvalidStatus = true
    )
    {
        var pollingTimer = Stopwatch.StartNew();
        while (true)
        {
            var response = IntegrationProcessorApiRefreshBase.DoRequest(
                httpClient,
                $"{resource}/{queryId}/status",
                HttpMethod.Get,
                null,
                headers
            );

            JObject parsedResponse;
            try
            {
                parsedResponse = JObject.Parse(response);
            }
            catch (Exception e)
            {
                return (
                    -1,
                    string.Format(
                        InvalidJobStatusResponseMessage,
                        $"Error parsing JSON with exception {e.GetFullErrorMessage()}"
                    )
                );
            }

            parsedResponse.TryGetValue("status", out var statusToken);
            var status = statusToken?.Value<string>() ?? "null";

            if (
                !status.Equals(FinishedStatus, StringComparison.OrdinalIgnoreCase)
                && !status.Equals(RunningStatus, StringComparison.OrdinalIgnoreCase)
            )
            {
                var errorMessage = string.Format(
                    InvalidJobStatusResponseMessage,
                    $"Invalid status '{status}', expecting {RunningStatus} or {FinishedStatus}"
                );
                if (!callJobResultsOnInvalidStatus)
                {
                    return (-1, errorMessage);
                }

                var (success, _, textFieldParser) = GetJobResultPage(
                    httpClient,
                    resource,
                    queryId,
                    1,
                    0,
                    headers
                );
                if (!success)
                {
                    return (-1, errorMessage);
                }

                while (!textFieldParser.EndOfData)
                {
                    var fields = textFieldParser.ReadFields();
                    if (fields == null || fields.Length < 6)
                    {
                        continue;
                    }

                    errorMessage += $"{Environment.NewLine}API Error Message: {fields[5]}";
                }

                return (-1, errorMessage);
            }

            if (status.Equals(FinishedStatus, StringComparison.OrdinalIgnoreCase))
            {
                parsedResponse.TryGetValue("rowCount", out var rowCountToken);
                var rowCount = rowCountToken?.Value<long?>() ?? -1;
                if (rowCount == -1)
                {
                    return (
                        -1,
                        string.Format(
                            InvalidJobStatusResponseMessage,
                            "Invalid row count returned from JobStatus request"
                        )
                    );
                }

                return (rowCount, null);
            }

            // Running status
            if (pollingTimer.Elapsed > IntegrationJobTimeout)
            {
                return (
                    -1,
                    string.Format(
                        InvalidJobStatusResponseMessage,
                        $"Timeout of {IntegrationJobTimeout.TotalMinutes} minutes has been reached waiting for the query to finish"
                    )
                );
            }

            Thread.Sleep(QueryPollingInterval);
        }
    }

    internal static (DataTable, string) GetJobResults(
        HttpClient httpClient,
        string resource,
        string queryId,
        long rowCount,
        int pageSize,
        string dataTableName,
        IIntegrationJobLogger integrationJobLogger,
        IDictionary<string, string> headers
    )
    {
        var offset = 0;
        var dataTable = new DataTable(dataTableName);
        var resultsTimer = Stopwatch.StartNew();
        while (dataTable.Rows.Count < rowCount)
        {
            var (success, columnNames, textFieldParser) = GetJobResultPage(
                httpClient,
                resource,
                queryId,
                pageSize,
                offset,
                headers
            );
            if (!success)
            {
                return (
                    dataTable,
                    string.Format(InvalidJobResultsResponseMessage, "response is null")
                );
            }

            if (dataTable.Rows.Count == 0)
            {
                if (columnNames == null || columnNames.Length == 0)
                {
                    integrationJobLogger.Warn("No rows returned");
                    return (dataTable, null);
                }

                // on first call, create datatable columns
                dataTable.Columns.AddRange(columnNames.Select(o => new DataColumn(o)).ToArray());
            }

            while (!textFieldParser.EndOfData)
            {
                var fields = textFieldParser.ReadFields();
                if (fields == null)
                {
                    continue;
                }

                if (fields.Length != dataTable.Columns.Count)
                {
                    integrationJobLogger.Warn(
                        $"Number of values {fields.Length} does not match number of columns {dataTable.Columns.Count}"
                    );
                    continue;
                }

                dataTable.LoadDataRow(fields, LoadOption.Upsert);
            }

            offset += pageSize;

            integrationJobLogger.Info(
                $"Processed rows {dataTable.Rows.Count} in {Math.Round(resultsTimer.Elapsed.TotalSeconds, 2)} seconds"
            );

            if (resultsTimer.Elapsed > IntegrationJobTimeout)
            {
                return (
                    dataTable,
                    string.Format(
                        InvalidJobResultsResponseMessage,
                        $"Timeout of {IntegrationJobTimeout.TotalMinutes} minutes has been reached reading query results, read {dataTable.Rows.Count}, expecting {rowCount}"
                    )
                );
            }
        }

        return (dataTable, null);
    }

    internal static (bool, string[], TextFieldParser) GetJobResultPage(
        HttpClient httpClient,
        string resource,
        string queryId,
        int pageSize,
        int offset,
        IDictionary<string, string> headers
    )
    {
        var response = IntegrationProcessorApiRefreshBase.DoRequest(
            httpClient,
            $"{resource}/{queryId}/result?limit={pageSize}&offset={offset}",
            HttpMethod.Get,
            null,
            headers
        );
        if (response == null)
        {
            return (false, null, null);
        }

        var textFieldParser = new TextFieldParser(new StringReader(response))
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            Delimiters = new[] { "," }
        };

        // first row always contains column names
        var columnNames = textFieldParser.ReadFields();

        return (true, columnNames, textFieldParser);
    }

    internal static int GetPageSize(
        IntegrationJob integrationJob,
        JobDefinitionStep jobDefinitionStep
    )
    {
        var jobParameterId =
            jobDefinitionStep.JobDefinitionStepParameters
                ?.FirstOrDefault(o => o.Name.EqualsIgnoreCase(CompassApiPageSize))
                ?.Id
            ?? integrationJob.JobDefinition
                ?.JobDefinitionParameters?.FirstOrDefault(
                    o => o.Name.EqualsIgnoreCase(CompassApiPageSize)
                )
                ?.Id;
        if (!jobParameterId.HasValue)
        {
            return DefaultPageSize;
        }

        var integrationJobParameter = integrationJob.IntegrationJobParameters.FirstOrDefault(
            o => o.JobDefinitionStepParameterId == jobParameterId.Value
        );
        if (integrationJobParameter == null)
        {
            return DefaultPageSize;
        }

        return int.TryParse(integrationJobParameter.Value, out var pageSize)
            ? pageSize
            : DefaultPageSize;
    }
}
