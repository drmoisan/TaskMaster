using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Interfaces
{
    public interface IQfcFormController
    {

        //void FormResize(bool Force = false); // might not be necessary
        void ButtonCancel_Click();
        void ButtonOK_Click();
        void ButtonUndo_Click();
        void ButtonCancel_Click(object sender, EventArgs e);
        void ButtonOK_Click(object sender, EventArgs e);
        void ButtonUndo_Click(object sender, EventArgs e);
        void Cleanup();
        void MaximizeQfcFormViewer();
        void MinimizeQfcFormViewer();
        void SpnEmailPerLoad_ValueChanged(object sender, EventArgs e);
        void Viewer_Activate();
        int SpaceForEmail { get; }
        int ItemsPerIteration { get; }
        void LoadItems(IList<MailItem> listObjects);
        IQfcCollectionController Groups { get; }
        IntPtr FormHandle { get; }
    }
}