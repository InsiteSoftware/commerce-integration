namespace Insite.WIS.InRiver.Services.FileServiceTypes.Directory;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.InRiver.Interfaces;

public class DirectoryFileService : IFileService
{
    public void ProcessBadFile(
        string processingFileName,
        string badFilesFolderName,
        IntegrationConnection integrationConnection
    )
    {
        var badFolder = Path.Combine(Path.GetDirectoryName(processingFileName), "BadFiles");
        if (!Directory.Exists(badFolder))
        {
            Directory.CreateDirectory(badFolder);
        }

        File.Move(
            processingFileName,
            Path.Combine(badFolder, Path.GetFileName(processingFileName))
        );
    }

    public List<string> RetrieveFilesForProcessing(
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        IntegrationConnection integrationConnection
    )
    {
        var filePattern = jobStep.FromClause;
        var files = Directory.GetFiles(integrationConnection.Url, filePattern).ToList();

        // A Step may use the same file from a previous Step
        var alreadyProcessedFilePattern =
            jobStep.FromClause + "." + integrationJob.JobNumber + ".processed";
        files.AddRange(
            Directory.GetFiles(integrationConnection.Url, alreadyProcessedFilePattern).ToList()
        );

        return files.OrderBy(o => o).ToList();
    }

    public string MoveFileForProcessing(string fileName, IntegrationJob integrationJob)
    {
        string processingFileName;

        // If it's already been processed from a prior Step, don't need to rename it again
        if (fileName.EndsWith(integrationJob.JobNumber + ".processed"))
        {
            processingFileName = fileName;
        }
        else
        {
            processingFileName = fileName + "." + integrationJob.JobNumber + ".processing";
            if (File.Exists(processingFileName))
            {
                File.Delete(processingFileName);
            }

            File.Move(fileName, processingFileName);
        }

        return processingFileName;
    }

    public void MoveFileToProcessed(
        IntegrationJob integrationJob,
        string processingFileName,
        string fileName
    )
    {
        // For Preview Jobs, don't archive the file
        if (integrationJob.IsPreview)
        {
            File.Move(processingFileName, fileName);
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
                File.Move(processingFileName, processedFileName);
            }
        }
    }

    public Stream StreamFromFilePath(
        string processingFileName,
        IntegrationConnection integrationConnection
    )
    {
        return new FileStream(processingFileName, FileMode.Open);
    }
}
