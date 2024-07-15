
using System.Collections.Concurrent;

namespace UtilitiesCS
{
    public interface IFileSystemFolderPaths
    {
        ConcurrentDictionary<string, string> SpecialFolders { get; }
        string FldrAppData { get; }
        string FldrFlow { get; }
        string FldrMyDocuments { get; }
        string FldrPreReads { get; }
        string FldrOneDrive { get; }
        string FldrPythonStaging { get; }
        void Reload();
        IAppStagingFilenames Filenames { get; }
    }
}