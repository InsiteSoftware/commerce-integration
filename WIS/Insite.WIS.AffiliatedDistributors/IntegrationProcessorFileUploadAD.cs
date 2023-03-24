namespace Insite.WIS.AffiliatedDistributors;

using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.WebIntegrationService;

[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.AffiliatedDistributors)]
public class IntegrationProcessorFileUploadAD : IntegrationProcessorFileUpload
{
    public bool IsRespectDirectory { get; set; }

    public override DataSet Execute(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        JobDefinitionStep jobDefinitionStep
    )
    {
        var parameters = this.IntegrationJob.IntegrationJobParameters;
        this.IsRespectDirectory =
            parameters
                .FirstOrDefault(
                    p => p.JobDefinitionParameter.Name.EqualsIgnoreCase("RespectDirectory")
                )
                ?.Value.EqualsIgnoreCase("true") ?? false;
        return base.Execute(siteConnection, integrationJob, jobDefinitionStep);
    }

    protected override string GetDestinationFilePath(
        FileUploadParameter fileUploadParameter,
        string sourceFilePath
    )
    {
        var destinationFilePath = base.GetDestinationFilePath(fileUploadParameter, sourceFilePath);

        var directoryName = Path.GetDirectoryName(destinationFilePath);
        var fileName = Path.GetFileName(destinationFilePath);
        var alphabeticalSubdirectoryName = fileName.Substring(0, 1).ToLower();

        return this.FileProvider.CombinePaths(
            directoryName,
            alphabeticalSubdirectoryName,
            fileName
        );
    }

    protected override void UnzipAndUploadFile(
        FileUploadParameter fileUploadParameter,
        string sourceFilePath
    )
    {
        if (!this.IsRespectDirectory)
        {
            base.UnzipAndUploadFile(fileUploadParameter, sourceFilePath);
            return;
        }

        using (var stream = this.FileProvider.GetFile(sourceFilePath))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.Name.IsBlank())
                {
                    continue;
                }

                var unzippedSourceFilePath = this.FileProvider.CombinePaths(
                    fileUploadParameter.SourceDirectory,
                    entry.FullName
                );

                this.UploadStream(fileUploadParameter, entry.Open(), unzippedSourceFilePath);
            }
        }
    }
}
