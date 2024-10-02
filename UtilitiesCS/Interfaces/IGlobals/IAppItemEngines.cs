using System.Collections.Concurrent;
using System.Threading.Tasks;


namespace UtilitiesCS
{
    public interface IAppItemEngines
    {
        ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>> InboxEngines { get; }

        Task InitAsync();
    }
}