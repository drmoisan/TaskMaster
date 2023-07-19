using Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Interfaces
{
    public interface IQfcExplorerController
    {
        bool BlShowInConversations { get; set; }
        void OpenQFItem(object ObjItem);
        void ExplConvView_ToggleOff();
        void ExplConvView_ToggleOn();
        void ExplConvView_Cleanup();
        void ExplConvView_ReturnState();
    }
}