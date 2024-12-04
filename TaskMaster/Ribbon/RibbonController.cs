using Office = Microsoft.Office.Core;
using Outlook = Microsoft.Office.Interop.Outlook;
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
using System;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using QuickFiler.Controllers;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.Extensions.Lazy;
using TaskMaster.Ribbon;


namespace TaskMaster
{

    public class RibbonController
    {
        private RibbonViewer _viewer;
        protected internal ApplicationGlobals Globals {get; set; }
        private bool blHook = true;
        private IFilerHomeController _quickFiler;
        private bool _quickFilerLoaded = false;

        public RibbonController() { }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                _quickFiler = new QuickFiler.Controllers.QfcHomeController(Globals, ReleaseQuickFiler);
                _quickFiler.Run();
            }
        }

        internal async Task LoadQuickFilerAsync()
        {
            if (!_quickFilerLoaded)
            {
                _quickFilerLoaded = true;
                _quickFiler = await QuickFiler.Controllers.QfcHomeController.LaunchAsync(Globals, ReleaseQuickFiler);
                if (_quickFiler is null)
                    _quickFilerLoaded = false;
            }
        }


        private void ReleaseQuickFiler()
        {
            _quickFiler = null;
            _quickFilerLoaded = false;
        }

        internal void ReviseProjectData()
        {
            var controller = new ToDoModel.Data_Model.Project.ProjectController(Globals.TD.ProjInfo);
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

        internal void HideHeadersNoChildren()
        {
            var dataTree = new TreeOfToDoItems([]);
            dataTree.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, Globals);
            dataTree.HideEmptyHeadersInView();
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

        #region SettingsMenu

        internal bool IsMoveEntireConversationActive() => Globals.QfSettings.MoveEntireConversation;
        internal void ToggleMoveEntireConversation() => Globals.InternalQfSettings.MoveEntireConversation = !Globals.InternalQfSettings.MoveEntireConversation;

        internal bool IsSaveAttachmentsActive() => Globals.QfSettings.SaveAttachments;
        internal void ToggleSaveAttachments() => Globals.InternalQfSettings.SaveAttachments = !Globals.InternalQfSettings.SaveAttachments;

        internal bool IsSavePicturesActive() => Globals.QfSettings.SavePictures;
        internal void ToggleSavePictures() => Globals.InternalQfSettings.SavePictures = !Globals.InternalQfSettings.SavePictures;

        internal bool IsSaveEmailCopyActive() => Globals.QfSettings.SaveEmailCopy;
        internal void ToggleSaveEmailCopy() => Globals.InternalQfSettings.SaveEmailCopy = !Globals.InternalQfSettings.SaveEmailCopy;

        #endregion SettingsMenu
                
        internal void SortEmail()
        {
            var sorter = new EfcHomeController(Globals, () => { });
            sorter.Run();
        }

        internal async Task SortEmailAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var sorter = await EfcHomeController.CreateAsync(Globals, () => { });
            sorter.Run();
        }

        internal async Task FindFolderAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var sorter = await EfcHomeController.LoadFinderAsync(Globals, () => { });
            sorter.Run();

        }

        

        #region Folder Classifier

        internal async Task ScrapeAndMineAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var miner = new UtilitiesCS.EmailIntelligence.Bayesian.EmailDataMiner(Globals);
            await miner.DeleteStagingFilesAsync();
            await miner.MineEmails();
        }

        #endregion Folder Classifier

        #region BayesianPerformance

        
        internal async Task GetConfusionDriversAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var tuner = new BayesianPerformanceMeasurement(Globals);
            await tuner.GetConfusionDriversAsync();
            //var serializer = new BayesianSerializationHelper(_globals);
            //var testScores = await serializer.DeserializeAsync<VerboseTestScores[]>("VerboseTestScores[]");
            //var ppkg = await new ProgressPackage().InitializeAsync(progressTrackerPane: _globals.AF.ProgressTracker);
            //_globals.AF.ProgressPane.Visible = true;
            //var errors = await tuner.DiagnosePoorPerformanceAsync(testScores, ppkg.ProgressTrackerPane);
        }
        internal async Task TryChartMetricsAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var tuner = new BayesianPerformanceMeasurement(Globals);
            await tuner.ShowSensitivityChartAsync(null);
        }

        internal async Task InvestigateErrorsAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());

            var performance = new BayesianPerformanceController(Globals);
            await performance.InvestigatePerformance();
        }

        internal void PopulateUdf()
        {
            FlagTasks.PopulateUdf(null, Globals);
        }

        internal void TryDeepCompareEmails()
        {
            var email1 = Globals.Ol.App.ActiveExplorer().Selection[1] as Outlook.MailItem;
            var email2 = Globals.Ol.App.ActiveExplorer().Selection[2] as Outlook.MailItem;
            Deep.DeepDifferences<MailItem>(email1, email2);
        }

        #endregion BayesianPerformance

        #region Spam Manager

        //private AsyncLazy<SpamBayes> _sb;
        //internal AsyncLazy<SpamBayes> SB 
        //{
        //    get 
        //    {
        //        if (SynchronizationContext.Current is null)
        //            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
        //        if (_sb is null) { ResetSb(); }
        //        return _sb; 
        //    }
        //}
        internal SpamBayes SB 
        {
            get 
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(
                        new WindowsFormsSynchronizationContext());
                return Globals.Engines.InboxEngines.TryGetValue("Spam", out var engine) ? engine as SpamBayes : null; 
            }
        }

        internal IAppItemEngines Engines => Globals.Engines;

        internal async Task ClearSpamManagerAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var response = MessageBox.Show("Are you sure you want to clear the Spam Manager? This cannot be undone", "Clear Spam Manager", MessageBoxButtons.YesNo);
            if (response == DialogResult.Yes)
            {
                if ((await Globals.AF.Manager.Configuration).TryGetValue(SpamBayes.GroupName, out var loader))
                {
                    var classifier = await SpamBayes.CreateSpamClassifiersAsync();
                    classifier.Config.CopyFrom(loader.Config, true);
                    classifier.Serialize();
                    Globals.AF.Manager[SpamBayes.GroupName] = classifier.ToAsyncLazy();
                    await Globals.Engines.RestartEngineAsync(SpamBayes.GroupName);
                }                
            }
        }
                
        //internal async Task TrainSpam()
        //{
        //    var sb = await SB;
        //    if (sb is not null) { await sb.TrainAsync(OlSelection, true); }
        //}

        //internal async Task TrainHam()
        //{
        //    var sb = await SB;
        //    if (sb is not null) { await sb.TrainAsync(OlSelection, false); }
        //}

        //internal async Task TestSpam()
        //{
        //    var sb = await SB;
        //    if (sb is not null) { await sb.TestAsync(OlSelection); }
            
        //}

        internal void TestSpamVerbose()
        {
            throw new NotImplementedException();
        }

        internal void SpamMetrics()
        {
            throw new NotImplementedException();
        }

        internal void SpamInvestigateErrors()
        {
            throw new NotImplementedException();
        }

        #endregion Spam Manager

        #region Triage

        private AsyncLazy<Triage> _triage;
        internal AsyncLazy<Triage> Triage
        {
            get
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

                return _triage;
            }
        }
        internal void ResetTriage()
        {
            _triage = new(async () => await UtilitiesCS.EmailIntelligence.Triage.CreateAsync(
                Globals, true, Enums.NotFoundEnum.Ask));
        }

        internal async Task TriageSelectionAsync()
        {
            var triage = await Triage;
            if (triage is null) { ResetTriage(); }
            else { await triage.TestAsync(OlSelection); }

            //if (SynchronizationContext.Current is null)
            //    SynchronizationContext.SetSynchronizationContext(
            //        new WindowsFormsSynchronizationContext());
            //var triage = new UtilitiesCS.EmailIntelligence.ClassifierGroups.Triage.Triage(_globals, _globals.AF.Manager);
            //await triage.ClassifyAsync(_globals.Ol.App.ActiveExplorer().Selection);
        }

        internal async Task TriageSetAAsync()
        {
            var triage = await Triage;
            if (triage is null) { ResetTriage(); }
            else { await triage.TrainAsync(OlSelection, "A"); }
            //if (SynchronizationContext.Current is null)
            //    SynchronizationContext.SetSynchronizationContext(
            //        new WindowsFormsSynchronizationContext());
            //var triage = new UtilitiesCS.EmailIntelligence.ClassifierGroups.Triage.Triage(_globals, _globals.AF.Manager);
            //await triage.TrainAsync(_globals.Ol.App.ActiveExplorer().Selection, "A");
        }

        internal async Task TriageSetBAsync()
        {
            var triage = await Triage;
            if (triage is null) { ResetTriage(); }
            else { await triage.TrainAsync(OlSelection, "B"); }
        }

        internal async Task TriageSetCAsync()
        {
            var triage = await Triage;
            if (triage is null) { ResetTriage(); }
            else { await triage.TrainAsync(OlSelection, "C"); }
        }


        internal async Task TriageSetPrecision() 
        {
            var triage = await Triage;
            if (triage is null) { ResetTriage(); }
            else 
            {
                var precision = InputBox.ShowDialog("Enter Precision", "Set Precision", $"{triage.ClassifierGroup.MinimumProbability}");
                if (double.TryParse(precision, out double result))
                {
                    triage.ClassifierGroup.MinimumProbability = result;
                    //_globals.AF.Manager.Serialize();
                    triage.ClassifierGroup.Serialize();
                }
            }
        }

        internal async Task ClearTriageAync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var triage = await new UtilitiesCS.EmailIntelligence.Triage(Globals).InitAsync();
            await triage.CreateNewTriageClassifierGroupAsync(default);
        }

        internal void TryDeleteTriageSpamFields()
        {
            foreach (var item in OlSelection)
            {
                if (item is MailItem mailItem)
                {
                    mailItem.DeleteUdf("Triage");
                    mailItem.DeleteUdf("Spam");                    
                }
            }
        }

        

        #endregion Triage

        internal async Task IntelligenceAsync()
        {
            var selection = Globals.Ol.App.ActiveExplorer().Selection;
            if (selection is not null &&  selection.Count > 0)
            {
                await selection
                 .Cast<object>()
                 .ToAsyncEnumerable()
                 .ForEachAwaitAsync(Globals.Events.ProcessMailItemAsync);
            }
        }

    }
}