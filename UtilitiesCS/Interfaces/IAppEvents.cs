using Microsoft.Office.Interop.Outlook;
using System.Threading.Tasks;

namespace TaskMaster
{
    public interface IAppEvents
    {
        void Hook();
        void Unhook();
        Items OlToDoItems { get; }
        Task ProcessMailItemAsync(object item);
    }
}