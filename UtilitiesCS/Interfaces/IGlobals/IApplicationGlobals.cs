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

        /// <summary>
        /// The store disable service (issue #261). Constructed in <c>LoadBasicMethod()</c>; reads the
        /// store model lazily per call so it is valid before the async store-load phase populates it.
        /// </summary>
        IStoreDisableService StoreDisable { get; }
    }
}
