
namespace UtilitiesVB
{
    public interface IFileSystemFolderPaths
    {
        string FldrAppData { get; }
        string FldrFlow { get; }
        string FldrMyD { get; }
        string FldrPreReads { get; }
        string FldrRoot { get; }
        string FldrStaging { get; }
        string FldrPythonStaging { get; }
        void Reload();
        IAppStagingFilenames Filenames { get; }
    }
}