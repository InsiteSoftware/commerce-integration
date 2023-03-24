using Azure.Storage.Blobs;

namespace Insite.WIS.InRiver.Services.FileServiceTypes.Azure.Providers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public sealed class AzureProvider : IAzureProvider
{
    public const string InvalidAzureStorageConnectionStringException =
        "The Azure Storage Connection String is invalid.";

    private readonly BlobContainerClient container;

    public AzureProvider(string azureStorageConnectionString, string containerName)
    {
        this.container = new BlobContainerClient(azureStorageConnectionString, containerName);
    }

    public void MoveBlob(string source, string destination)
    {
        var destinationBlob = this.container.GetBlobClient(destination);

        // Move (copy and delete)
        var sourceBlob = this.container.GetBlobClient(source);

        var operation = destinationBlob.StartCopyFromUri(sourceBlob.Uri);

        operation.WaitForCompletion();

        sourceBlob.Delete();
    }

    public void SafeDeleteBlob(string source)
    {
        this.container.GetBlobClient(source).DeleteIfExists();
    }

    public IEnumerable<string> GetMatchingBlobs(string sourceFolder, string filePattern)
    {
        // Standardize filePattern to '/'s
        var standardizedFilePattern = filePattern.Replace('\\', '/');

        // Find matching blobs based on RegEx conversion of Windows api pattern
        var regex = new Regex(WildcardToRegex(standardizedFilePattern));

        return this.container
            .GetBlobsByHierarchy(prefix: sourceFolder, delimiter: "/")
            .Where(o => o.IsBlob)
            .Select(o => o.Blob.Name)
            .Where(o => regex.IsMatch(o))
            .ToList();
    }

    public Stream OpenBlobFileStream(string source)
    {
        // Get blob and return stream
        var blob = this.container.GetBlobClient(source);
        return blob.OpenRead();
    }

    private static string WildcardToRegex(string pattern)
    {
        return ".*" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
    }
}
