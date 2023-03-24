namespace Insite.WIS.InRiver;

using System;
using System.Collections.Generic;
using System.IO;
using Insite.Integration.Enums;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.InRiver.Interfaces;
using Insite.WIS.InRiver.Services.FileServiceTypes.Azure;
using Insite.WIS.InRiver.Services.FileServiceTypes.Directory;

public class IntegrationProcessorFileHelper
{
    public const string BadFilesFolderName = "BadFiles";

    public virtual void ProcessBadFile(
        string processingFileName,
        IntegrationConnection integrationConnection
    )
    {
        var fileService = GetFileService(integrationConnection.TypeName);
        fileService.ProcessBadFile(processingFileName, BadFilesFolderName, integrationConnection);
    }

    public virtual List<string> RetrieveFilesForProcessing(
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        IntegrationConnection integrationConnection
    )
    {
        var fileService = GetFileService(
            integrationJob.JobDefinition.IntegrationConnection.TypeName
        );
        return fileService.RetrieveFilesForProcessing(
            integrationJob,
            jobStep,
            integrationConnection
        );
    }

    public virtual string MoveFileForProcessing(string fileName, IntegrationJob integrationJob)
    {
        var fileService = GetFileService(
            integrationJob.JobDefinition.IntegrationConnection.TypeName
        );
        return fileService.MoveFileForProcessing(fileName, integrationJob);
    }

    public virtual void MoveFileToProcessed(
        IntegrationJob integrationJob,
        string processingFileName,
        string fileName
    )
    {
        var fileService = GetFileService(
            integrationJob.JobDefinition.IntegrationConnection.TypeName
        );
        fileService.MoveFileToProcessed(integrationJob, processingFileName, fileName);
    }

    public Stream StreamFromFilePath(
        string processingFileName,
        IntegrationConnection integrationConnection
    )
    {
        var fileService = GetFileService(integrationConnection.TypeName);
        return fileService.StreamFromFilePath(processingFileName, integrationConnection);
    }

    private static IFileService GetFileService(string typeName)
    {
        if (
            typeName.IsNotBlank()
            && typeName.Trim() == nameof(IntegrationConnectionType.AzureStorageFlatFile)
        )
        {
            return new AzureFileService();
        }
        else
        {
            return new DirectoryFileService();
        }
    }
}
