namespace Insite.WIS.InRiver.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insite.WIS.Broker.WebIntegrationService;

public interface IFileService
{
    void ProcessBadFile(
        string processingFileName,
        string badFilesFolderName,
        IntegrationConnection integrationConnection
    );

    List<string> RetrieveFilesForProcessing(
        IntegrationJob integrationJob,
        JobDefinitionStep jobStep,
        IntegrationConnection integrationConnection
    );

    string MoveFileForProcessing(string fileName, IntegrationJob integrationJob);

    void MoveFileToProcessed(
        IntegrationJob integrationJob,
        string processingFileName,
        string fileName
    );

    Stream StreamFromFilePath(
        string processingFileName,
        IntegrationConnection integrationConnection
    );
}
