using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Interfaces
{
    public interface IFilerFormController
    {

        void ButtonCancel_Click();
        void ButtonOK_Click();
        void ButtonCancel_Click(object sender, EventArgs e);
        void ButtonOK_Click(object sender, EventArgs e);
        void Cleanup();
        void MaximizeFormViewer();
        void MinimizeFormViewer();
        void ToggleOffNavigation(bool async);
        void ToggleOnNavigation(bool async);
        IntPtr FormHandle { get; }
        
        // Removed methods never called through the interface so they
        // are unnecessary in the promise

        //IQfcCollectionController Groups { get; }
        //void FormResize(bool Force = false); // might not be necessary
        //void ButtonUndo_Click();
        //void ButtonUndo_Click(object sender, EventArgs e);
        //void SpnEmailPerLoad_ValueChanged(object sender, EventArgs e);
        //void Viewer_Activate();
        //int SpaceForEmail { get; }
        //int ItemsPerIteration { get; }
        //void LoadItems(IList<MailItem> listObjects);
    }
}