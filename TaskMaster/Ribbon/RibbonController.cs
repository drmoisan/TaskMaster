using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using TaskMaster.Ribbon;
using TaskTree;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.EmailIntelligence.OlFolderTools.FilterOlFolders;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.OutlookObjects.Store;
using Office = Microsoft.Office.Core;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster
{
    [ExcludeFromCodeCoverage]
    public partial class RibbonController
    {
        private RibbonViewer _viewer;
        protected internal ApplicationGlobals Globals { get; set; }
        private bool blHook = true;
        private IFilerHomeController _quickFiler;
        private bool _quickFilerLoaded = false;

        public RibbonController() { }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        internal void SetGlobals(ApplicationGlobals globals)
        {
            Globals = globals;
            Try = new(globals);
            //ResetSb();
            ResetTriage();
        }

        //internal void ResetSb()
        //{
        //    _sb = new(async () => await SpamBayes.CreateAsync(Globals, true, Enums.NotFoundEnum.Ask));
        //}

        internal void SetViewer(RibbonViewer Viewer)
        {
            _viewer = Viewer;
        }

        internal Selection OlSelection => Globals.Ol.App.ActiveExplorer().Selection;

        internal TryFunctionalityInConstruction Try { get; set; }

        protected internal virtual IOutlookFolderTreeService FolderTreeService =>
            Globals.Ol.FolderTreeService;

        internal void RefreshIDList()
        {
            // _globals.TD.IDList_Refresh()
            Globals.TD.IDList.RefreshIDList(Globals.Ol.App);
            MessageBox.Show("ID Refresh Complete");
        }

        internal async Task SplitToDoIdAsync()
        {
            await ToDoEvents.RefreshToDoIdSplitsAsync(Globals.Ol.App);
        }

        internal void LoadTaskTree()
        {
            var taskTreeViewer = new TaskTreeForm();
            var dataModel = new TreeOfToDoItems([]);
            dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, Globals);
            var taskTreeController = new TaskTreeController(Globals, taskTreeViewer, dataModel);
            taskTreeViewer.Show();
        }

        internal void LoadQuickFiler()
        {
            bool loaded = false;
            if (_quickFiler is not null)
                loaded = _quickFiler.Loaded;
            if (loaded == false)
            {
                _quickFiler = new QuickFiler.Controllers.QfcHomeController(
                    Globals,
                    ReleaseQuickFiler
                ).Init();
                _quickFiler.Run();
            }
        }

        internal async Task LoadQuickFilerAsync()
        {
            if (!_quickFilerLoaded)
            {
                SetHighConfidenceModeForLaunch(false);
                _quickFilerLoaded = true;
                _quickFiler = await QuickFiler.Controllers.QfcHomeController.LaunchAsync(
                    Globals,
                    ReleaseQuickFiler
                );
                if (_quickFiler is null)
                    _quickFilerLoaded = false;
            }
        }

        /// <summary>
        /// Launches Quick Filer with high-confidence mode active. Mirrors
        /// <see cref="LoadQuickFilerAsync"/> but first enables high-confidence mode so the loaded
        /// session filters below-threshold suggestions. Uses the same <c>_quickFilerLoaded</c>
        /// guard so the standard launch path is unaffected.
        /// </summary>
        internal async Task LoadQuickFilerHighConfidenceAsync()
        {
            if (!_quickFilerLoaded)
            {
                _quickFilerLoaded = true;
                SetHighConfidenceModeForLaunch(true);
                _quickFiler = await QuickFiler.Controllers.QfcHomeController.LaunchAsync(
                    Globals,
                    ReleaseQuickFiler
                );
                if (_quickFiler is null)
                    _quickFilerLoaded = false;
            }
        }

        private void ReleaseQuickFiler()
        {
            _quickFiler = null;
            _quickFilerLoaded = false;
            SetHighConfidenceModeForLaunch(false);
        }

        internal void ReviseProjectData()
        {
            var controller = new ToDoModel.Data_Model.Project.ProjectController(
                Globals.TD.ProjInfo,
                Globals.TD.ProgramInfo
            );
            controller.Run();
        }

        internal void CompressIDs()
        {
            Globals.TD.IDList.CompressToDoIDs(Globals);
            MessageBox.Show("ID Compression Complete");
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
                Globals.Events.Unhook();
                blHook = false;
                Ribbon.InvalidateControl("BtnHookToggle");
                MessageBox.Show("Events Disconnected");
            }
            else
            {
                Globals.Events.Hook();
                blHook = true;
                Ribbon.InvalidateControl("BtnHookToggle");
                MessageBox.Show("Hooked Events");
            }
        }

        internal void ToggleDarkMode() => Globals.Ol.DarkMode = !Globals.Ol.DarkMode;

        internal bool IsDarkModeActive() => Globals.Ol.DarkMode;

        internal async Task HideHeadersNoChildrenAsync()
        {
            var dataTree = new TreeOfToDoItems([]);
            await Task.Run(() =>
                dataTree.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, Globals)
            );
            await Task.Run(dataTree.HideEmptyHeadersInView);
        }

        internal async Task ShowHeadersNoChildrenAsync()
        {
            var dataTree = new TreeOfToDoItems([]);
            await Task.Run(() =>
                dataTree.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadNotComplete, Globals)
            );
            await Task.Run(dataTree.ShowEmptyHeadersInView);
        }

        internal void FlagAsTask()
        {
            var taskFlagger = new FlagTasks(Globals);
            taskFlagger.Run();
        }

        internal async Task UndoSortAsync()
        {
            await UtilitiesCS.SortEmail.UndoAsync(Globals.AF.MovedMails, Globals);
        }

        internal void SortEmail()
        {
            var sorter = new EfcHomeController(Globals, () => { });
            sorter.Run();
        }

        internal async Task SortEmailAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var sorter = await EfcHomeController.CreateAsync(Globals, () => { });
            sorter.Run();
        }

        internal async Task FindFolderAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var sorter = await EfcHomeController.LoadFinderAsync(Globals, () => { });
            sorter.Run();
        }

        internal void FolderStoresSettings()
        {
            var wrapper = new StoreWrapperController(Globals);
            wrapper.Launch();
        }
    }
}
