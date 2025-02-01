using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFiler.Controllers
{
    public interface IQfcFormController: IFilerFormController
    {
        string ActiveTheme { get; set; }
        bool DarkMode { get; set; }        
        IQfcFormViewer FormViewer { get; }
        IQfcCollectionController Groups { get; }
        int ItemsPerIteration { get; set; }
        int SpaceForEmail { get; }
        CancellationToken Token { get; }
        CancellationTokenSource TokenSource { get; }

        
        void ButtonSkip_Click(object sender, EventArgs e);
        void ButtonUndo_Click();
        void ButtonUndo_Click(object sender, EventArgs e);
        void CaptureItemSettings();        
        void LoadItems(IList<MailItem> listObjects);
        void LoadItems(TableLayoutPanel tlp, List<QfcItemGroup> itemGroups);
        Task LoadItemsAsync(IList<MailItem> listObjects);
        int LoadItemsPerIteration();        
        void RegisterFormEventHandlers();
        void RemoveTemplatesAndSetupTlp();
        void SetupLightDark();
        Task SkipGroupAsync();
        void SpnEmailPerLoad_ValueChanged(object sender, EventArgs e);        
        void UnregisterFormEventHandlers();
        void Viewer_Activate();
    }
}