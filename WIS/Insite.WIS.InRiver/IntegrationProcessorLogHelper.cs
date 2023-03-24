namespace Insite.WIS.InRiver;

using Insite.Common.Extensions;
using Insite.Integration.Enums;
using Insite.WIS.Broker;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.Broker.Plugins;

public class IntegrationProcessorLogHelper
{
    protected readonly IntegrationJob IntegrationJob;

    protected readonly SiteConnection SiteConnection;

#pragma warning disable SA1306
    protected IntegrationJobLogger JobLogger;
#pragma warning restore SA1306

    public IntegrationProcessorLogHelper(
        SiteConnection siteConnection,
        IntegrationJob integrationJob
    )
    {
        this.SiteConnection = siteConnection;
        this.IntegrationJob = integrationJob;
        this.JobLogger = new IntegrationJobLogger(siteConnection, integrationJob);
    }

    public void LogDebug(string message)
    {
        this.JobLogger.Debug(message);
    }

    public void LogFileProcessingStart(string fileName)
    {
        this.JobLogger.Debug($"Starting Reading File {fileName}");
    }

    public void LogFileProcessingFinish(string fileName)
    {
        this.JobLogger.Debug($"Finished Reading File {fileName}");
    }

    public void LogFileAccessException(string message, string fileName)
    {
        this.JobLogger.Warn(
            $"Exception {message} occurred trying to access file {fileName}, skipping this file"
        );
    }

    public void LogFileParseException(string message, string fileName)
    {
        this.JobLogger.Error(
            $"Exception Reading File {fileName} Moving to Bad Folder.  Message: {message}"
        );
    }

    public void LogProcessingInRiverObject(string objectName, string uniqueIdentifier)
    {
        this.JobLogger.Debug($"Processing inRiver {objectName}: Key = {uniqueIdentifier}");
    }

    public void LogFileFoundCount(JobDefinitionStep jobStep, int fileCount, string filePattern)
    {
        var message = $"No files found matching '{filePattern}'";
        if (fileCount == 0)
        {
            var errorType = jobStep.FlatFileErrorHandling.EnumParse<LookupErrorHandlingType>();
            switch (errorType)
            {
                case LookupErrorHandlingType.Error:
                    this.JobLogger.Error(message);
                    break;
                case LookupErrorHandlingType.Warning:
                    this.JobLogger.Warn(message);
                    break;
                case LookupErrorHandlingType.Ignore:
                    this.JobLogger.Info(message);
                    break;
                default:
                    this.JobLogger.Warn(message);
                    break;
            }
        }
        else
        {
            this.JobLogger.Info($"Found {fileCount} files matching '{filePattern}'");
        }
    }

    public void LogObjectProcessed(string objectName, string uniqueIdentifier)
    {
        this.JobLogger.Debug($"Processed inRiver {objectName}: Key = {uniqueIdentifier}");
    }

    public void LogObjectSkipped(
        string objectName,
        string uniqeIdentifier,
        string parentUniqueIdentifier
    )
    {
        this.JobLogger.Debug(
            $"{objectName}: Key = {uniqeIdentifier}, skipped because parent ({parentUniqueIdentifier}) is not mapped."
        );
    }
}
