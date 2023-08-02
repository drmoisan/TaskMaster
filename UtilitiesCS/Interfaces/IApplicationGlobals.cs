
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public interface IApplicationGlobals
    {
        Task LoadAsync();
        IFileSystemFolderPaths FS { get; }
        IOlObjects Ol { get; }
        IToDoObjects TD { get; }
        IAppAutoFileObjects AF { get; }
    }
}