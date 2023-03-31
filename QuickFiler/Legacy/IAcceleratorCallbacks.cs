using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace QuickFiler
{
    internal interface IAcceleratorCallbacks
    {
        int ActivateByIndex(int intNewSelection, bool blExpanded);
        bool IsSelectionBelowMax(int intNewSelection);
        void MoveDownPix(int intPosition, int intPix);
        void ResetAcceleratorSilently();
        void ToggleKeyboardDialog();
        bool ToggleOffActiveItem(bool parentBlExpanded);
        void ToggleRemoteMouseLabels();
        void OpenQFMail(MailItem olMail);
        void RemoveSpecificControlGroup(int intPosition);
        IQfcItemController TryGetQfc(int index);
    }
}