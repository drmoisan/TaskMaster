using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using TaskTree;
using TaskVisualization;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.OutlookExtensions;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster
{
    public partial class RibbonController
    {
        #region SettingsMenu

        internal bool IsMoveEntireConversationActive() => Globals.QfSettings.MoveEntireConversation;

        internal void ToggleMoveEntireConversation() =>
            Globals.InternalQfSettings.MoveEntireConversation = !Globals
                .InternalQfSettings
                .MoveEntireConversation;

        internal bool IsSaveAttachmentsActive() => Globals.QfSettings.SaveAttachments;

        internal void ToggleSaveAttachments() =>
            Globals.InternalQfSettings.SaveAttachments = !Globals
                .InternalQfSettings
                .SaveAttachments;

        internal bool IsSavePicturesActive() => Globals.QfSettings.SavePictures;

        internal void ToggleSavePictures() =>
            Globals.InternalQfSettings.SavePictures = !Globals.InternalQfSettings.SavePictures;

        internal bool IsSaveEmailCopyActive() => Globals.QfSettings.SaveEmailCopy;

        internal void ToggleSaveEmailCopy() =>
            Globals.InternalQfSettings.SaveEmailCopy = !Globals.InternalQfSettings.SaveEmailCopy;

        internal bool IsHighConfidenceModeActive() => Globals.QfSettings.HighConfidenceModeEnabled;

        internal void ToggleHighConfidenceMode() =>
            Globals.InternalQfSettings.HighConfidenceModeEnabled = !Globals
                .InternalQfSettings
                .HighConfidenceModeEnabled;

        internal void SetHighConfidenceModeForLaunch(bool enabled) =>
            Globals.InternalQfSettings.HighConfidenceModeEnabled = enabled;

        internal string GetHighConfidenceThresholdText() =>
            Math.Round(Globals.QfSettings.HighConfidenceThreshold * 100, 0)
                .ToString(CultureInfo.InvariantCulture);

        internal void SetHighConfidenceThresholdText(string text)
        {
            if (
                double.TryParse(
                    text,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double percent
                )
                && percent >= 0
                && percent <= 100
            )
            {
                Globals.InternalQfSettings.HighConfidenceThreshold = percent / 100.0;
            }
        }

        #endregion SettingsMenu

        #region Folder Classifier

        internal async Task ScrapeAndMineAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var miner = new EmailDataMiner(Globals);
            await miner.DeleteStagingFilesAsync();
            await miner.MineEmails();
        }

        internal async Task ContinueMiningAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var miner = new EmailDataMiner(Globals);
            await miner.MineEmails();
        }

        internal async Task BuildFolderClassifierAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var miner = new OlFolderClassifierGroup(Globals);
            await miner.BuildClassifiersAsync();
        }

        internal async Task BuildCategoryClassifierAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var miner = new CategoryClassifierGroup(Globals);
            await miner.BuildClassifiersAsync();
        }

        internal async Task BuildActionableClassifierAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var miner = new ActionableClassifierGroup(Globals);
            await miner.BuildClassifiersAsync(5);
        }

        #endregion Folder Classifier

        #region BayesianPerformance

        internal async Task GetConfusionDriversAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var tuner = new BayesianPerformanceMeasurement(Globals);
            await tuner.GetConfusionDriversAsync();
        }

        internal async Task TryChartMetricsAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var tuner = new BayesianPerformanceMeasurement(Globals);
            await tuner.ShowSensitivityChartAsync(null);
        }

        internal async Task InvestigateErrorsAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );

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

        internal SpamBayes SB
        {
            get
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(
                        new WindowsFormsSynchronizationContext()
                    );
                return Globals?.Engines?.InboxEngines?.TryGetValue("Spam", out var engine) ?? false
                    ? engine as SpamBayes
                    : null;
            }
        }

        internal IAppItemEngines Engines => Globals.Engines;

        internal async Task ClearSpamManagerAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var response = MessageBox.Show(
                "Are you sure you want to clear the Spam Manager? This cannot be undone",
                "Clear Spam Manager",
                MessageBoxButtons.YesNo
            );
            if (response == DialogResult.Yes)
            {
                if (
                    (await Globals.AF.Manager.Configuration).TryGetValue(
                        SpamBayes.GroupName,
                        out var loader
                    )
                )
                {
                    var classifier = await SpamBayes.CreateSpamClassifiersAsync();
                    classifier.Config.CopyFrom(loader.Config, true);
                    classifier.Serialize();
                    Globals.AF.Manager[SpamBayes.GroupName] = classifier.ToAsyncLazy();
                    await Globals.Engines.RestartEngineAsync(SpamBayes.GroupName);
                }
            }
        }

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

        private AsyncLazy<Triage> _triageAsync;
        internal AsyncLazy<Triage> TriageAsync
        {
            get
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(
                        new WindowsFormsSynchronizationContext()
                    );

                return _triageAsync;
            }
        }

        internal void ResetTriage()
        {
            _triageAsync = new(async () =>
                await UtilitiesCS.EmailIntelligence.Triage.CreateAsync(
                    Globals,
                    true,
                    Enums.NotFoundEnum.Ask
                )
            );
        }

        internal Triage Triage
        {
            get
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(
                        new WindowsFormsSynchronizationContext()
                    );
                return
                    Globals?.Engines?.InboxEngines?.TryGetValue("Triage", out var engine) ?? false
                    ? engine as Triage
                    : null;
            }
        }

        internal async Task TriageSelectionAsync()
        {
            var triage = await TriageAsync;
            if (triage is null)
            {
                ResetTriage();
            }
            else
            {
                await triage.TestAsync(OlSelection);
            }
        }

        internal async Task TriageSetAAsync()
        {
            var triage = await TriageAsync;
            if (triage is null)
            {
                ResetTriage();
            }
            else
            {
                await triage.TrainAsync(OlSelection, "A");
            }
        }

        internal async Task TriageSetBAsync()
        {
            var triage = await TriageAsync;
            if (triage is null)
            {
                ResetTriage();
            }
            else
            {
                await triage.TrainAsync(OlSelection, "B");
            }
        }

        internal async Task TriageSetCAsync()
        {
            var triage = await TriageAsync;
            if (triage is null)
            {
                ResetTriage();
            }
            else
            {
                await triage.TrainAsync(OlSelection, "C");
            }
        }

        internal async Task TriageSetPrecision()
        {
            var triage = await TriageAsync;
            if (triage is null)
            {
                ResetTriage();
            }
            else
            {
                var precision = InputBox.ShowDialog(
                    "Enter Precision",
                    "Set Precision",
                    $"{triage.ClassifierGroup.MinimumProbability}"
                );
                if (double.TryParse(precision, out double result))
                {
                    triage.ClassifierGroup.MinimumProbability = result;
                    triage.ClassifierGroup.Serialize();
                }
            }
        }

        internal async Task ResetTriageClassifierAync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            var triage = await new UtilitiesCS.EmailIntelligence.Triage(Globals).InitAsync();
            await triage.CreateNewTriageClassifierGroupAsync(default);
        }

        internal void TryDeleteTriageSpamFields()
        {
            foreach (var item in OlSelection)
            {
                if (item is MailItem mailItem)
                {
                    mailItem.DeleteUdf("AutoProcessed");
                    mailItem.DeleteUdf("Triage");
                    mailItem.DeleteUdf("Spam");
                }
            }
        }

        #endregion Triage

        internal async Task IntelligenceAsync()
        {
            var selection = Globals.Ol.App.ActiveExplorer().Selection;
            if (selection is not null && selection.Count > 0)
            {
                // ForEachAwaitAsync (System.Linq.Async) is obsolete (CS0618) per the framework's
                // migration guidance ("Use the language support for async foreach instead"), but
                // replacing it with `await foreach` here is a control-flow change to a production
                // async method, not an annotation-only edit. Suppressing narrowly preserves the
                // exact pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
                await selection
                    .Cast<object>()
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(Globals.Events.ProcessMailItemAsync);
#pragma warning restore CS0618
            }
        }
    }
}
