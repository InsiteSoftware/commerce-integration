namespace Insite.WIS.InRiver.Services.FileServiceTypes.Azure;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.InRiver.Interfaces;
using Insite.WIS.InRiver.Services.FileServiceTypes.Azure.Factories;

public class AzureFileService : IFileService
{
    private readonly IAzureProviderFactory azureProviderFactory;

    public AzureFileService()
    {
        this.azureProviderFactory = new AzureProviderFactory();
    }

    internal AzureFileService(IAzureProviderFactory azureProviderFactory)
    {
        this.azureProviderFactory = azureProviderFactory;
    }

    public void ProcessBadFile(
        string processingFileName,
        string badFilesFolderName,
        IntegrationConnection integrationConnection
    )
    {
        var azureProvider = this.azureProviderFactory.CreateAzureProvider(
            integrationConnection.ConnectionString,
            integrationConnection.SystemNumber
        );
        var processingFileNameSegment = new Uri(processingFileName).Segments.Last();
        var badDestinationSegment = $"{badFilesFolderName}/{processingFileNameSegment}";
        var badDestinationRoot = processingFileName.Remove(
            processingFileName.LastIndexOf(processingFileNameSegment, StringComparison.Ordinal)
        );
        var destination = $"{badDestinationRoot}{badDestinationSegment}";

        azureProvider.MoveBlob(processingFileName, destination);
    }

    public List<string> RetrieveFilesForProcessing(
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        IntegrationConnection integrationConnection
    )
    {
        var azureProvider = this.azureProviderFactory.CreateAzureProvider(
            integrationConnection.ConnectionString,
            integrationConnection.SystemNumber
        );
        var inputFolder = integrationConnection.Url;
        var filePattern = jobStep.FromClause;
        var filesForProcessing = new List<string>();
        var matchingBlobs = azureProvider.GetMatchingBlobs(inputFolder, filePattern).ToList();

        if (matchingBlobs.Any())
        {
            filesForProcessing.AddRange(matchingBlobs);
        }

        // A Step may use the same file from a previous Step
        var alreadyProcessedFilePattern =
            jobStep.FromClause + "." + integrationJob.JobNumber + ".processed";
        var additionalMatchingBlobs = azureProvider
            .GetMatchingBlobs(inputFolder, alreadyProcessedFilePattern)
            .ToList();

        if (additionalMatchingBlobs.Any())
        {
            filesForProcessing.AddRange(additionalMatchingBlobs);
        }

        return filesForProcessing;
    }

    public string MoveFileForProcessing(string fileName, IntegrationJob integrationJob)
    {
        var azureProvider = this.azureProviderFactory.CreateAzureProvider(
            integrationJob.JobDefinition.IntegrationConnection.ConnectionString,
            integrationJob.JobDefinition.IntegrationConnection.SystemNumber
        );
        string processingFileName;

        // If it's already been processed from a prior Step, don't need to rename it again
        if (fileName.EndsWith(integrationJob.JobNumber + ".processed"))
        {
            processingFileName = fileName;
        }
        else
        {
            // Set processing file name
            processingFileName = fileName + "." + integrationJob.JobNumber + ".processing";

            // Delete if processing file name already exists
            azureProvider.SafeDeleteBlob(processingFileName);

            // Move (copy and delete)
            azureProvider.MoveBlob(fileName, processingFileName);
        }

        return processingFileName;
    }

    public void MoveFileToProcessed(
        IntegrationJob integrationJob,
        string processingFileName,
        string fileName
    )
    {
        var azureProvider = this.azureProviderFactory.CreateAzureProvider(
            integrationJob.JobDefinition.IntegrationConnection.ConnectionString,
            integrationJob.JobDefinition.IntegrationConnection.SystemNumber
        );

        // For Preview Jobs, don't archive the file
        if (integrationJob.IsPreview)
        {
            azureProvider.MoveBlob(processingFileName, fileName);
        }
        else
        {
            // Only move to Processed if it's one that hasn't already been moved from a prior Step
            if (
                !fileName.EndsWith(
                    integrationJob.JobNumber + ".processed",
                    StringComparison.Ordinal
                )
            )
            {
                var processedFileName = fileName + "." + integrationJob.JobNumber + ".processed";

                var integrationConnection = integrationJob.JobDefinition.IntegrationConnection;
                var archiveFolder = integrationConnection.ArchiveFolder;

                if (!string.IsNullOrWhiteSpace(archiveFolder))
                {
                    var importFolder = integrationConnection.Url; // Url in code, Import Folder in the admin console...
                    if (!processedFileName.Contains(importFolder))
                    {
                        // This provider is built around Azure-resolved absolute addresses, which may not exactly match the connection set-up if there are casing differences.
                        throw new InvalidOperationException(
                            $"{fileName} does not have a case sensitive match for the input path of {importFolder}. Check connection settings."
                        );
                    }

                    processedFileName = processedFileName.Replace(importFolder, archiveFolder);
                }

                azureProvider.MoveBlob(processingFileName, processedFileName);
            }
        }
    }

    public Stream StreamFromFilePath(
        string processingFileName,
        IntegrationConnection integrationConnection
    )
    {
        var azureProvider = this.azureProviderFactory.CreateAzureProvider(
            integrationConnection.ConnectionString,
            integrationConnection.SystemNumber
        );
        var blobFileStream = azureProvider.OpenBlobFileStream(processingFileName);

        return blobFileStream;
    }
}
