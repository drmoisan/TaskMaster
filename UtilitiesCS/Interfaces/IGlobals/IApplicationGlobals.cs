
using System.Threading.Tasks;
using TaskMaster;

namespace UtilitiesCS
{
    public interface IApplicationGlobals
    {
        Task LoadAsync();
        IFileSystemFolderPaths FS { get; }
        IOlObjects Ol { get; }
        IToDoObjects TD { get; }
        IAppAutoFileObjects AF { get; }
        IAppEvents Events { get; }
        IAppQuickFilerSettings QfSettings { get; }
        IAppItemEngines Engines { get; }
    }
}