using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;
using System.Threading.Tasks;
using UtilitiesCS.Interfaces.IWinForm;


namespace QuickFiler.Interfaces
{
    public interface IFilerFormController
    {
        Task ActionCancelAsync();
        Task ActionOkAsync();
        void ButtonCancel_Click(object sender, EventArgs e);
        void ButtonOK_Click(object sender, EventArgs e);
        void Cleanup();
        void MaximizeFormViewer();
        void MinimizeFormViewer();
        void ToggleOffNavigation(bool async);
        Task ToggleOffNavigationAsync();
        void ToggleOnNavigation(bool async);
        Task ToggleOnNavigationAsync();

        IntPtr FormHandle { get; }
        
    }
}