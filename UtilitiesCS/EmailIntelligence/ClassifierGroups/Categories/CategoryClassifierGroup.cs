using SDILReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;
using UtilitiesCS.Extensions;
using AngleSharp.Css;
using UtilitiesCS.Extensions.Lazy;
using System.Threading;
using UtilitiesCS.HelperClasses;
using Microsoft.Graph.Communications.OnlineMeetings.GetAllRecordingsmeetingOrganizerUserIdMeetingOrganizerUserIdWithStartDateTimeWithEndDateTime;

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories
{
    public class CategoryClassifierGroup(IApplicationGlobals globals)
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal IApplicationGlobals Globals { get; private set; } = globals;

        internal readonly ClassifierGroupUtilities CgUtilities = new(globals);

        public async Task BuildClassifiersAsync()
        {            
            var miner = new EmailDataMiner(Globals);

            // Set up Progress Tracking            
            var (ppkg, sw) = await SetupProgressTracking();

            // Load the staging data            
            MinedMailInfo[] collection = await LoadStagingData(ppkg, sw);
            var allocation = Globals.TD.PrefixList.Count > 1 ? 
                (100 - ppkg.ProgressTrackerPane.Progress) / Globals.TD.PrefixList.Count: 
                100 - ppkg.ProgressTrackerPane.Progress;

            List<string> prefixList = ["Project"];
            foreach (var prefixLu in prefixList)
            {
                var prefix = Globals.TD.PrefixList.Find(x => x.Key == prefixLu);
                // Remove the existing Category Classifier
                Globals.AF.Manager.TryRemove(prefix.Key, out _);

                var childPpkg = await new ProgressPackage()
                    .InitializeAsync(ppkg.CancelSource, ppkg.Cancel, ppkg.ProgressTrackerPane.SpawnChild(allocation), ppkg.StopWatch)
                    .ConfigureAwait(false);

                // Get or Create the Classifier Group
                BayesianClassifierGroup classifierGroup = await LoadClassifierGroup(childPpkg, sw, collection, prefix);

                var childPpkg2 = await new ProgressPackage()
                    .InitializeAsync(childPpkg.CancelSource, childPpkg.Cancel, childPpkg.ProgressTrackerPane.SpawnChild(), childPpkg.StopWatch)
                    .ConfigureAwait(false);

                if (await BuildClassifiersAsync(classifierGroup, collection, childPpkg2, prefix))
                {
                    
                    // set the configuration of classifierGroup
                    if ((await Globals.AF.Manager.Configuration).TryGetValue(prefix.Key, out var loader))
                    {
                        classifierGroup.Config = loader.Config.DeepCopy() as NewSmartSerializableConfig;
                        classifierGroup.Serialize();

                        Globals.AF.Manager[prefix.Key] = classifierGroup.ToAsyncLazy();
                        //Globals.AF.Manager.Serialize();
                        MyBox.ShowDialog($"{prefix.Key} Classifier Built Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            Globals.AF.ProgressPane.Visible = false;
                        
        }

        private async Task<((CancellationTokenSource CancelSource, CancellationToken Cancel, 
            ProgressTrackerPane ProgressTrackerPane, SegmentStopWatch StopWatch), SegmentStopWatch)> SetupProgressTracking()
        {
            var ppkg = await ProgressPackage.CreateAsTuplePaneAsync(
                            progressTrackerPane: Globals.AF.ProgressTracker)
                            .ConfigureAwait(false);
            var sw = ppkg.StopWatch;
            Globals.AF.ProgressPane.Visible = true;
            return (ppkg, sw);
        }

        private async Task<BayesianClassifierGroup> LoadClassifierGroup((CancellationTokenSource CancelSource, CancellationToken Cancel, ProgressTrackerPane ProgressTrackerPane, SegmentStopWatch StopWatch) ppkg, SegmentStopWatch sw, MinedMailInfo[] collection, IPrefix prefix)
        {
            ppkg.ProgressTrackerPane.Report(0, $"Building {prefix.Key} Classifier -> Creating Classifier Group");
            var classifierGroup = await CgUtilities.GetOrCreateClassifierGroupAsync(collection, prefix.Key);
            sw.LogDuration("Get or Create Classifier Group and shared token base");
            sw.WriteToLog(clear: false);
            ppkg.ProgressTrackerPane.Report(20, $"Building {prefix.Key} Classifier -> Creating Classifier Group");
            return classifierGroup;
        }

        private async Task<BayesianClassifierGroup> LoadClassifierGroup(ProgressPackage ppkg, SegmentStopWatch sw, MinedMailInfo[] collection, IPrefix prefix)
        {
            ppkg.ProgressTrackerPane.Report(20, $"Building {prefix.Key} Classifier -> Creating Classifier Group");
            var classifierGroup = await CgUtilities.GetOrCreateClassifierGroupAsync(collection, prefix.Key);
            sw.LogDuration("Get or Create Classifier Group and shared token base");
            sw.WriteToLog(clear: false);
            return classifierGroup;
        }

        private async Task<MinedMailInfo[]> LoadStagingData((CancellationTokenSource CancelSource, CancellationToken Cancel, ProgressTrackerPane ProgressTrackerPane, SegmentStopWatch StopWatch) ppkg, SegmentStopWatch sw)
        {
            ppkg.ProgressTrackerPane.Report(0, "Building Category Classifiers -> Load Mined Mail Info");
            if (!Globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot)) { return default; }
            var folderPath = Path.Combine(folderRoot, "Bayesian");
            var collection = await EmailDataMiner.Load<MinedMailInfo[]>(folderPath);
            collection.ThrowIfNullOrEmpty();
            sw?.LogDuration("Load Staging");
            ppkg.ProgressTrackerPane.Report(10, "Building Category Classifiers -> Loaded Mined Mail Info");
            return collection;
        }

        public async Task<bool> BuildClassifiersAsync(BayesianClassifierGroup classifierGroup, MinedMailInfo[] collection, ProgressPackage ppkg, IPrefix prefix)
        {            
            var exploded = collection.Select(x => ExplodeMailsByCategory(x, prefix)).SelectMany(x => x).ToList();

            var groups = exploded.GroupBy(x => x.GroupingKey);
            
            var sw = ppkg.StopWatch;

            bool success = false;
            try
            {
                await AsyncMultiTasker.AsyncMultiTaskChunker(groups, async (group) =>
                {
                    await BuildClassifierAsync(group, classifierGroup, ppkg.Cancel);
                }, ppkg.ProgressTrackerPane, "Building Classifiers", ppkg.Cancel);
                sw.LogDuration("Build Classifiers");
                sw.WriteToLog(clear: false);
                success = true;
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message, e);
            }
            return success;
        }

        internal IEnumerable<MinedMailInfo> ExplodeMailsByCategory(MinedMailInfo m, IPrefix prefix)
        {
            if (m.Categories.IsNullOrEmpty()) { return new List<MinedMailInfo> { m }; }
            var categories = new FlagParser([.. m.Categories.Split(separator: ',', trim: true)])
                .Combined.AsListWithPrefix.Where(x => x.Contains(prefix.Value)).ToList();

            var exploded = categories.Select(x => 
            { 
                var deepcopy = m.DeepCopy();
                deepcopy.GroupingKey = x;
                return deepcopy;
            });
            return exploded;
        }

        public virtual async Task BuildClassifierAsync(
            IGrouping<string, MinedMailInfo> group,
            BayesianClassifierGroup classifierGroup,
            CancellationToken cancel)
        {
            var matchFrequency = group.Select(minedMail => minedMail.Tokens)
                                      .SelectMany(x => x)
                                      .GroupAndCount();

            var matchCorpus = new Corpus(matchFrequency);
            var matchEmailCount = group.Count();
            await classifierGroup.RebuildClassifier(
                group.Key, matchFrequency, matchEmailCount, cancel);
        }

    }
}
