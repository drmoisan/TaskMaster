using Microsoft.Office.Interop.Outlook;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UtilitiesCS;

namespace TaskMaster
{
    public interface IAppEvents
    {
        void Hook();
        void Unhook();
        Items OlToDoItems { get; }
        Task ProcessMailItemAsync(object item);
        //ConcurrentBag<IConditionalEngine<MailItemHelper>> InboxEngines { get; }
    }
}