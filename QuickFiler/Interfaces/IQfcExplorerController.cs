using Microsoft.Office.Interop.Outlook;

namespace QuickFiler
{
    public interface IQfcExplorerController
    {
        bool BlShowInConversations { get; set; }
        void OpenQFMail(MailItem OlMail);
        void ExplConvView_ToggleOff();
        void ExplConvView_ToggleOn();
        void ExplConvView_Cleanup();
        void ExplConvView_ReturnState();
    }
}