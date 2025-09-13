
using System.Threading.Tasks;
using TaskMaster;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS
{
    public interface IApplicationGlobals
    {
        Task LoadAsync(bool parallel);
        IFileSystemFolderPaths FS { get; }
        IOlObjects Ol { get; }
        IToDoObjects TD { get; }
        IAppAutoFileObjects AF { get; }
        IAppEvents Events { get; }
        IAppQuickFilerSettings QfSettings { get; }
        IAppItemEngines Engines { get; }
        IntelligenceConfig IntelRes { get; }
    }
}