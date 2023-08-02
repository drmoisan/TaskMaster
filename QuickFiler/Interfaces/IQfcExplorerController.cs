using Microsoft.Office.Interop.Outlook;
using System.Threading.Tasks;

namespace QuickFiler.Interfaces
{
    public interface IQfcExplorerController
    {
        bool BlShowInConversations { get; set; }
        Task OpenQFItem(MailItem mailItem);
        void ExplConvView_ToggleOff();
        void ExplConvView_ToggleOn();
        void ExplConvView_Cleanup();
        void ExplConvView_ReturnState();
    }
}