using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading.Tasks;


namespace UtilitiesCS
{
    public interface IAppItemEngines
    {
        ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>> InboxEngines { get; }
        Task ToggleEngineAsync(string engineName);
        Task<bool> EngineActiveAsync(string engineName);
        void ShowSaveInfo(string engineName);
        Task ShowDiskDialog(string engineName, bool local);
        Task RestartEngineAsync(string engineName);

        Task InitAsync();
    }
}