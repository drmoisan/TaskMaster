using Microsoft.Office.Interop.Outlook;

namespace TaskMaster
{
    public interface IAppEvents
    {
        void Hook();
        void Unhook();
        Items OlToDoItems { get; }
    }
}