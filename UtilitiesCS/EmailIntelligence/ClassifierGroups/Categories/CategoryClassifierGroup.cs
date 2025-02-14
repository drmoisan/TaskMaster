using AngleSharp.Css;
using Microsoft.Graph.Communications.OnlineMeetings.GetAllRecordingsmeetingOrganizerUserIdMeetingOrganizerUserIdWithStartDateTimeWithEndDateTime;
using Microsoft.Graph.Drives.Item.Items.Item.GetActivitiesByInterval;
using Microsoft.Office.Interop.Outlook;
using SDILReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories
{
    public class CategoryClassifierGroup: IConditionalEngine<MailItemHelper>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ctor
        
        private CategoryClassifierGroup() { }

        public CategoryClassifierGroup(IApplicationGlobals globals)
        {
            Globals = globals;
            CgUtilities = new(Globals);
        }

        public async Task<CategoryClassifierGroup> InitAsync(string groupName)
        {
            Globals.ThrowIfNull();
                                    
            Globals.AF.Manager.TryGetValue(groupName, out var classifierTask);
            if (classifierTask is not null)
            {
                ClassifierGroup = await classifierTask;
                EngineName = groupName;
                return this;
            }
            else { return null; }
            
        }

        public static async Task<CategoryClassifierGroup> CreateEngineAsync(
            IApplicationGlobals globals,
            string categoryGroup,
            CancellationToken token = default)
        {
            var cg = new CategoryClassifierGroup();
            cg.Globals = globals;

            return await Task.Run(() => cg.InitAsync(categoryGroup), token);

        }

        #endregion ctor

        internal IApplicationGlobals Globals { get; private set; } 

        internal ClassifierGroupUtilities CgUtilities;

        #region Build Category Classifier

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

            List<string> prefixList = ["Context","Project"];
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
            var exploded = collection.Where(x => !x.Categories.IsNullOrEmpty()).Select(x => ExplodeMailsByCategory(x, prefix)).SelectMany(x => x).ToList();

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
        #endregion Build Category Classifier

        #region Public Properties

        
        public BayesianClassifierGroup ClassifierGroup { get; set; }

        public bool IsActivated => ClassifierGroup is not null;

        public double ProbabilityThreshold { get; set; } = 0.8;
        
        public Func<IEnumerable<string>, MailItemHelper, Task> CategorySetter { get; set; }

        public async Task TestAsync(MailItemHelper helper)
        {
            var results = await GetMatchingCategoriesAsync(helper);
            if (results.Count() > 0) { await CategorySetter(results, helper); }            
        }

        public async Task<string[]> GetMatchingCategoriesAsync(MailItemHelper helper)
        {
            var results = await ClassifierGroup.ClassifyAsync(helper.Tokens, default);
            var filtered = results
                .Where(x => x.Probability > ProbabilityThreshold).Select(x => x.Class)
                .ToArray();
            return filtered;
        }

        public string[] GetMatchingCategories(MailItemHelper helper)
        {
            var results = ClassifierGroup.Classify(helper.Tokens);
            var filtered = results
                .Where(x => x.Probability > ProbabilityThreshold).Select(x => x.Class)
                .ToArray();
            return filtered;
        }

        #endregion Public Properties


        #region IConditionalEngine Implementation

        public ISmartSerializableConfig Config => ClassifierGroup.Config;

        //public static async Task<IConditionalEngine<MailItemHelper>> CreateEngineAsync(IApplicationGlobals globals)
        //{
        //    var sb = await CreateAsync(globals);
        //    return sb;
        //}

        void IConditionalEngine<MailItemHelper>.Serialize()
        {
            this.ClassifierGroup.Serialize();
        }

        public Func<MailItemHelper, Task> AsyncAction => (item) => 
            (Engine is not null && CategorySetter is not null) ? 
            ((CategoryClassifierGroup)Engine).TestAsync(item) : null;
        //public Func<MailItemHelper, Task> AsyncAction { get; set; }
       
        public Func<object, Task<bool>> AsyncCondition => (item) => Task.Run(() => ConditionLog(item));

        private bool Condition(object item)
        {
            if (item is not MailItem mailItem) { return false; }
            if (mailItem.MessageClass != "IPM.Note") { return false; }
            //if (mailItem.UserProperties.Find("Spam") is not null) { return false; }
            return true;
        }

        private bool ConditionLog(object item)
        {
            var olItem = new OutlookItem(item);
            if (olItem.TryGet().OlItemType(out var result) && result != OlItemType.olMailItem)
            {
                logger.Debug($"Skipping: Not MailItem -> {GetOlItemString(olItem)}");
                return false;
            }

            if (olItem.Try().MessageClass != "IPM.Note")
            {
                logger.Debug($"Skipping: Message class -> {GetOlItemString(olItem)}");
                return false;
            }

            //var spamProp = olItem.UserProperties.Find("Spam");
            //if (spamProp is not null)
            //{
            //    logger.Debug($"Skipping: Has Spam property with value of {spamProp.Value} -> {GetOlItemString(olItem)}");
            //    return false;
            //}

            return true;
        }

        private string GetOlItemString(OutlookItem olItem)
        {
            var type = olItem.TryGet().OlItemType(out var typeVal) ? $"{typeVal}" : $"{olItem.InnerObject.GetType()}";
            var created = olItem.TryGet().CreationTime(out var result) ? $" created on {result:g}" : "";
            var subject = olItem.Try().Subject;
            subject = subject.IsNullOrEmpty() ? "" : $" with subject {subject}";
            var sender = olItem.Try().SenderName;
            sender = sender.IsNullOrEmpty() ? "" : $" from {sender}";
            return $"{type}{created}{sender}{subject}";
        }

        public object Engine => this;

        public Func<IApplicationGlobals, Task> EngineInitializer => async (globals) => await Task.CompletedTask;

        public string EngineName { get; internal set; }

        public string Message => $"{nameof(CategoryClassifierGroup)} is null. Skipping actions";

        public MailItemHelper TypedItem { get; set; }


        #endregion IConditionalEngine Implementation

    }
}
