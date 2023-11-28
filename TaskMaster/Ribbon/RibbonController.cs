using System.Collections.Generic;
using System.Diagnostics;
using Office = Microsoft.Office.Core;
using Outlook = Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.VisualBasic;
using TaskTree;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using QuickFiler.Interfaces;
using System.Windows.Forms;
using QuickFiler;
using Microsoft.Office.Interop.Outlook;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing.Imaging;
using stdole;
using System;

namespace TaskMaster
{

    public class RibbonController
    {
        private RibbonViewer _viewer;
        private ApplicationGlobals _globals;
        private bool blHook = true;
        private IFilerHomeController _quickFiler;
        private bool _quickFilerLoaded = false;

        public RibbonController() { }

        internal void SetGlobals(ApplicationGlobals AppGlobals)
        {
            _globals = AppGlobals;
        }

        internal void SetViewer(RibbonViewer Viewer)
        {
            _viewer = Viewer;
        }

        internal void RefreshIDList()
        {
            // _globals.TD.IDList_Refresh()
            _globals.TD.IDList.RefreshIDList(_globals.Ol.App);
            MessageBox.Show("ID Refresh Complete");
        }

        internal void SplitToDoID()
        {
            ToDoEvents.Refresh_ToDoID_Splits(_globals.Ol.App);
        }

        internal void LoadTaskTree()
        {
            var taskTreeViewer = new TaskTreeForm();
            var dataModel = new TreeOfToDoItems([]);
            dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, _globals);
            var taskTreeController = new TaskTreeController(_globals, taskTreeViewer, dataModel);
            taskTreeViewer.Show();
        }

        internal void LoadQuickFiler()
        {
            bool loaded = false;
            if (_quickFiler is not null)
                loaded = _quickFiler.Loaded;
            if (loaded == false)
            {
                _quickFiler = new QuickFiler.Controllers.QfcHomeController(_globals, ReleaseQuickFiler);
                _quickFiler.Run();
            }
        }

        internal async Task LoadQuickFilerAsync()
        {
            if (!_quickFilerLoaded)
            {
                _quickFilerLoaded = true;
                _quickFiler = await QuickFiler.Controllers.QfcHomeController.LaunchAsync(_globals, ReleaseQuickFiler);
                if (_quickFiler is null)
                    _quickFilerLoaded = false;
            }
        }

        
        private void ReleaseQuickFiler()
        {
            _quickFiler = null;
            _quickFilerLoaded = false;
        }

        internal void ReviseProjectInfo()
        {
            _globals.TD.ProjInfo.SetIdUpdateAction(_globals.TD.IDList.SubstituteIdRoot);
            var _projInfoView = new ProjectInfoWindow(_globals.TD.ProjInfo);
            _projInfoView.Show();
        }

        internal void CompressIDs()
        {
            _globals.TD.IDList.CompressToDoIDs(_globals);
            MessageBox.Show("ID Compression Complete");
        }

        internal void BtnMigrateIDs_Click()
        {
            // Globals.ThisAddIn.MigrateToDoIDs()
            ToDoEvents.MigrateToDoIDs(_globals.Ol.App);
        }

        internal string GetHookButtonText(Office.IRibbonControl _)
        {
            if (blHook)
            {
                return "Unhook Events";
            }
            else
            {
                return "Hook Events";
            }
        }

        internal void ToggleEventsHook(Office.IRibbonUI Ribbon)
        {
            if (blHook == true)
            {
                _globals.Events.Unhook();
                blHook = false;
                Ribbon.InvalidateControl("BtnHookToggle");
                MessageBox.Show("Events Disconnected");
            }
            else
            {
                _globals.Events.Hook();
                blHook = true;
                Ribbon.InvalidateControl("BtnHookToggle");
                MessageBox.Show("Hooked Events");
            }
        }

        internal void ToggleDarkMode() => _globals.Ol.DarkMode = !_globals.Ol.DarkMode;
        internal bool IsDarkModeActive() => _globals.Ol.DarkMode;

        internal void HideHeadersNoChildren()
        {
            var dataTree = new TreeOfToDoItems([]);
            dataTree.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, _globals);
            dataTree.HideEmptyHeadersInView();
        }

        internal void FlagAsTask()
        {
            var taskFlagger = new FlagTasks(_globals);
            taskFlagger.Run();
        }

        internal void UndoSort()
        {
            ToDoModel.SortEmail.Undo(_globals.AF.MovedMails,_globals.Ol.App);
        }

        #region SettingsMenu
        
        internal bool IsMoveEntireConversationActive() => _globals.QfSettings.MoveEntireConversation;
        internal void ToggleMoveEntireConversation() => _globals.InternalQfSettings.MoveEntireConversation = !_globals.InternalQfSettings.MoveEntireConversation;
        
        internal bool IsSaveAttachmentsActive() => _globals.QfSettings.SaveAttachments;
        internal void ToggleSaveAttachments() => _globals.InternalQfSettings.SaveAttachments = !_globals.InternalQfSettings.SaveAttachments;
        
        internal bool IsSavePicturesActive() => _globals.QfSettings.SavePictures;
        internal void ToggleSavePictures() => _globals.InternalQfSettings.SavePictures = !_globals.InternalQfSettings.SavePictures;
        
        internal bool IsSaveEmailCopyActive() => _globals.QfSettings.SaveEmailCopy;
        internal void ToggleSaveEmailCopy() => _globals.InternalQfSettings.SaveEmailCopy = !_globals.InternalQfSettings.SaveEmailCopy;

        #endregion SettingsMenu

        #region Try specific methods
        internal void RunTry()
        {
            
        }

        internal void TryGetConversationDataframe()
        {
            var Mail = _globals.Ol.App.ActiveExplorer().Selection[1];
            Outlook.Conversation conv = (Outlook.Conversation)Mail.GetConversation();
            Microsoft.Data.Analysis.DataFrame df = conv.GetDataFrame();
            Debug.WriteLine(df.PrettyText());
            df.Display();
        }
        internal void TryGetConversationOutlookTable()
        {
            var Mail = _globals.Ol.App.ActiveExplorer().Selection[1];
            Outlook.Conversation conv = (Outlook.Conversation)Mail.GetConversation();
            var table = conv.GetTable(WithFolder: true, WithStore: true);
            table.EnumerateTable();
        }
        internal void TryGetMailItemInfo()
        {
            var Mail = _globals.Ol.App.ActiveExplorer().Selection[1];
            var conversation = (Outlook.Conversation)Mail.GetConversation();
            var df = conversation.GetDataFrame();
            df.PrettyPrint();
            var mInfo = new MailItemInfo(df, 0, _globals.Ol.EmailPrefixToStrip);
        }
        internal void TryGetQfcDataModel()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var dc = new QuickFiler.Controllers.QfcDatamodel(_globals, token);
        }
        internal void TryGetTableInView()
        {
            Outlook.Table table = _globals.Ol.App.ActiveExplorer().GetTableInView();
        }
        internal void TryRebuildProjInfo()
        {
            _globals.TD.ProjInfo.Rebuild(_globals.Ol.App);
        }
        internal void TryRecipientGetInfo()
        {
            var Mail = (Outlook.MailItem)_globals.Ol.App.ActiveExplorer().Selection[1];
            var recipients = Mail.Recipients.Cast<Recipient>();
            var info = recipients.GetInfo();
        }
        internal void TrySubstituteIdRoot()
        {
            _globals.TD.IDList.SubstituteIdRoot("9710", "2501");
        }
        internal void TryGetImage()
        {
            var ae = _globals.Ol.App.ActiveExplorer();
            //var image = ae.CommandBars.GetImageMso("ReplyAll", 38, 38);
            var image3 = ae.CommandBars.GetImageMso("Forward", 38, 38);
            //var image5 = ae.CommandBars.GetImageMso("Reply", 100, 100);

            //System.Drawing.Image image2 = GetImage(image);
            //image2.Save(@"C:\Temp\ReplyAll.png", ImageFormat.Png);

            System.Drawing.Image image4 = GetImage(image3);
            image4.Save(@"C:\Temp\Forward.png", ImageFormat.Png);

            //System.Drawing.Image image6 = GetImage(image5);
            //image6.Save(@"C:\Temp\Reply.png", ImageFormat.Png);


        }
        internal System.Drawing.Image GetImage(IPictureDisp disp)
        {
            return System.Drawing.Image.FromHbitmap((IntPtr)disp.Handle, (IntPtr)disp.hPal);
        }
        internal void TryLegacySortMailToExistingRun2()
        {
            var mail = _globals.Ol.App.ActiveExplorer().Selection[1] as MailItem;
            var items = new List<Outlook.MailItem> { mail };
            ToDoModel.SortEmail.Run2(items, false, "_ Active Projects\\Countertop Beta", false, false, false, _globals, null, null);
        }

        #endregion

        internal void SortEmail()
        {
            var sorter = new EfcHomeController(_globals, () => { });
            sorter.Run();
        }

    }
}