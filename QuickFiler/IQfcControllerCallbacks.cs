using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QuickFiler
{
    internal interface IQfcControllerCallbacks
    {
        void ConvToggle_Group(List<MailItem> selItems, int intOrigPosition);
        void ConvToggle_UnGroup(List<MailItem> selItems, int intPosition, int ConvCt, object varList);
        void KeyboardHandler_KeyDown(object sender, KeyEventArgs e);
        void KeyboardHandler_KeyUp(object sender, KeyEventArgs e);
        void KeyboardHandler_KeyPress(object sender, KeyPressEventArgs e);
        void QFD_Minimize();
        void RemoveSpecificControlGroup(int intPosition);
        public bool BlShowInConversations { get; set; }
        void ExplConvView_ToggleOn();
        void ExplConvView_ToggleOff();  
    }
}