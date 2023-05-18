
namespace UtilitiesVB
{
    public interface IApplicationGlobals
    {
        IFileSystemFolderPaths FS { get; }
        IOlObjects Ol { get; }
        IToDoObjects TD { get; }
        IAppAutoFileObjects AF { get; }
    }
}