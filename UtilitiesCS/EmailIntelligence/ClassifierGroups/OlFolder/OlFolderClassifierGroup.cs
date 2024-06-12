using log4net.Repository.Hierarchy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using System.IO;

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder
{
    public class OlFolderClassifierGroup(IApplicationGlobals globals)
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IApplicationGlobals _globals = globals;
        internal IApplicationGlobals Globals => _globals;

        internal readonly ClassifierGroupUtilities CgUtilities = new(globals);

        #region Build Classifiers

        public virtual async Task<ScoCollection<MinedMailInfo>> LoadStaging()
        {
            _mailInfoCollection = await Task.Run(
                () => new ScoCollection<MinedMailInfo>(
                    Globals.FS.Filenames.EmailInfoStagingFile,
                    Globals.FS.FldrPythonStaging));

            return _mailInfoCollection;
        }

        protected ScoCollection<MinedMailInfo> _mailInfoCollection;

        public virtual async Task<BayesianClassifierGroup> GetOrCreateClassifierGroupAsync(MinedMailInfo[] collection)
        {
            collection.ThrowIfNull();

            var group = await Task.Run(() => CgUtilities.Deserialize<BayesianClassifierGroup>("StagingClassifierGroup"));
            if (group is null)
            {
                group = await CreateClassifierGroupAsync(collection);
                CgUtilities.SerializeAndSave(group, "StagingClassifierGroup");
            }
            return group;
        }

        public virtual async Task<BayesianClassifierGroup> CreateClassifierGroupAsync(
            MinedMailInfo[] collection)
        {
            return await Task.Run(() =>
            {
                var group = new BayesianClassifierGroup
                {
                    TotalEmailCount = collection.Count(),
                    SharedTokenBase = new Corpus(
                        collection.SelectMany(x => x.Tokens).GroupAndCount())
                };
                return group;
            });
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

        public async Task<bool> BuildFolderClassifiersAsync(BayesianClassifierGroup classifierGroup, MinedMailInfo[] collection, ProgressPackage ppkg)
        {
            var groups = collection.GroupBy(x => x.FolderInfo.RelativePath);
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

        public async Task BuildFolderClassifiersAsync()
        {
            Globals.AF.Manager.Clear();
            var miner = new EmailDataMiner(Globals);

            var ppkg = await ProgressPackage //.CreateAsTupleAsync(screen: Globals.Ol.GetExplorerScreen());
                .CreateAsTuplePaneAsync(progressTrackerPane: Globals.AF.ProgressTracker).ConfigureAwait(false);
            var sw = ppkg.StopWatch;
            Globals.AF.ProgressPane.Visible = true;
            ppkg.ProgressTrackerPane.Report(0, "Building Folder Classifier -> Load Mined Mail Info");

            var folderPath = Path.Combine(_globals.FS.FldrAppData, "Bayesian");
            var collection = await EmailDataMiner.Load<MinedMailInfo[]>(folderPath);
            collection.ThrowIfNullOrEmpty();
            sw.LogDuration("Load Staging");

            ppkg.ProgressTrackerPane.Report(10, "Building Folder Classifier -> Getting Folder Paths");

            var folderPaths = miner.QueryOlFolderInfo(miner.GetOlFolderTree()).Select(x => x.RelativePath).ToList();
            sw.LogDuration("Get Folder Paths");

            ppkg.ProgressTrackerPane.Report(20, "Building Folder Classifier -> Creating Classifier Group");
            var classifierGroup = await GetOrCreateClassifierGroupAsync(collection);
            sw.LogDuration("Get or Create Classifier Group and shared token base");
            sw.WriteToLog(clear: false);
            ppkg.ProgressTrackerPane.Report(30, "Building Folder Classifier -> Building Classifiers");

            var childPpkg = await new ProgressPackage()
                .InitializeAsync(ppkg.CancelSource, ppkg.Cancel, ppkg.ProgressTrackerPane.SpawnChild(), ppkg.StopWatch)
                .ConfigureAwait(false);

            if (await BuildFolderClassifiersAsync(classifierGroup, collection, childPpkg))
            {
                Globals.AF.ProgressPane.Visible = false;
                Globals.AF.Manager["Folder"] = classifierGroup;
                Globals.AF.Manager.Serialize();
                MessageBox.Show("Folder Classifier Built Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public async Task CreateSpamClassifiersAsync()
        {
            var temp = await Task.Run(() =>
            {
                var group = new BayesianClassifierGroup
                {
                    TotalEmailCount = 0,
                    SharedTokenBase = new Corpus()
                };
                return group;
            });
            Globals.AF.Manager["Spam"] = temp;
            Globals.AF.Manager.Serialize();
        }
                        
        #endregion Build Classifiers

    }
}
