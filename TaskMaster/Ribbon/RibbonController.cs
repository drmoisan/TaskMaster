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
using UtilitiesVB;
using QuickFiler.Interfaces;
using System.Windows.Forms;

namespace TaskMaster
{

    public class RibbonController
    {
        private RibbonViewer _viewer;
        private IApplicationGlobals _globals;
        private bool blHook = true;
        private QuickFiler.Legacy.QfcLauncher _quickfileLegacy;
        private QuickFiler.Interfaces.IQfcHomeController _quickFiler;

        public RibbonController()
        {
        }

        internal void SetGlobals(IApplicationGlobals AppGlobals)
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
            Interaction.MsgBox("ID Refresh Complete");
        }

        internal void SplitToDoID()
        {
            ToDoEvents.Refresh_ToDoID_Splits(_globals.Ol.App);
        }

        internal void LoadTaskTree()
        {
            var taskTreeViewer = new TaskTreeForm();
            var dataModel = new TreeOfToDoItems(new List<TreeNode<ToDoItem>>());
            dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, _globals.Ol.App);
            var taskTreeController = new TaskTreeController(_globals, taskTreeViewer, dataModel);
            taskTreeViewer.Show();
        }

        internal void LoadQuickFilerOld()
        {
            bool loaded = false;
            if (_quickfileLegacy is not null)
                loaded = _quickfileLegacy.Loaded;
            if (loaded == false)
            {
                _quickfileLegacy = new QuickFiler.Legacy.QfcLauncher(_globals, ReleaseQuickFilerLegacy);
                _quickfileLegacy.Run();
            }
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

        private void ReleaseQuickFilerLegacy()
        {
            _quickfileLegacy = null;
        }

        private void ReleaseQuickFiler()
        {
            _quickFiler = null;
        }

        internal void ReviseProjectInfo()
        {
            var _projInfoView = new ProjectInfoWindow(Globals.ThisAddIn.ProjInfo);
            _projInfoView.Show();
        }

        internal void CompressIDs()
        {
            _globals.TD.IDList.CompressToDoIDs(_globals.Ol.App);
            MessageBox.Show("ID Compression Complete");
        }

        internal void BtnMigrateIDs_Click()
        {
            // Globals.ThisAddIn.MigrateToDoIDs()
            ToDoEvents.MigrateToDoIDs(_globals.Ol.App);
        }

        internal string GetHookButtonText(Office.IRibbonControl control)
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
                Globals.ThisAddIn.Events_Unhook();
                blHook = false;
                Ribbon.InvalidateControl("BtnHookToggle");
                Interaction.MsgBox("Events Disconnected");
            }
            else
            {
                Globals.ThisAddIn.Events_Hook();
                blHook = true;
                Ribbon.InvalidateControl("BtnHookToggle");
                Interaction.MsgBox("Hooked Events");
            }
        }

        internal void HideHeadersNoChildren()
        {
            var dataTree = new TreeOfToDoItems(new List<TreeNode<ToDoItem>>());
            dataTree.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, Globals.ThisAddIn.Application);
            dataTree.HideEmptyHeadersInView();
        }

        internal void FlagAsTask()
        {
            var taskFlagger = new FlagTasks(_globals);
            taskFlagger.Run();
        }

        internal void Runtest()
        {
            //Outlook.Table table = _globals.Ol.App.ActiveExplorer().GetTableInView();

            //var dc = new QuickFiler.Controllers.QfcDatamodel(_globals.Ol.App.ActiveExplorer(), _globals.Ol.App);

            //_globals.TD.ProjInfo.Rebuild(_globals.Ol.App);

            _globals.TD.IDList.SubstituteIdRoot("9710", "2501");

            //var Mail = _globals.Ol.App.ActiveExplorer().Selection[1];
            //Outlook.Conversation conv = (Outlook.Conversation)Mail.GetConversation();
            //Microsoft.Data.Analysis.DataFrame df = conv.GetDataFrame();
            //Debug.WriteLine(df.PrettyText());
            //df.Display();

            // Dim table As Outlook.Table = conv.GetTable(WithFolder:=True, WithStore:=True)
            // table.EnumerateTable()
        }

    }
}