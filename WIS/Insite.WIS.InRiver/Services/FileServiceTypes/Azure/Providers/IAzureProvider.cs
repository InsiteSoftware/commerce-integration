namespace Insite.WIS.InRiver.Services.FileServiceTypes.Azure.Providers;

using System.Collections.Generic;
using System.IO;

public interface IAzureProvider
{
    void MoveBlob(string source, string destination);

    void SafeDeleteBlob(string source);

    IEnumerable<string> GetMatchingBlobs(string sourceFolder, string filePattern);

    Stream OpenBlobFileStream(string source);
}
