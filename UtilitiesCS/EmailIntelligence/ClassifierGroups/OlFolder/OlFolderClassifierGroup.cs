#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net.Repository.Hierarchy;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;
using UtilitiesCS.Threading;

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder
{
    public class OlFolderClassifierGroup(IApplicationGlobals globals)
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private IApplicationGlobals _globals = globals;
        internal IApplicationGlobals Globals => _globals;

        internal readonly ClassifierGroupUtilities CgUtilities = new(globals);

        #region Folder predictor seam (LCPPN, flag-gated)

        private LcppnFolderPredictorConfig? _folderPredictorConfig;

        /// <summary>
        /// Configuration that controls the Folder predictor seam. When not explicitly set, it is
        /// resolved once from the persisted production default exposed by
        /// <see cref="IAppAutoFileObjects.UseLcppnPredictor"/> (reached through
        /// <c>Globals.AF</c>), which defaults to ON. Production callers therefore honor the persisted
        /// setting without hand-setting the flag; toggling the setting OFF restores the flat
        /// <c>Manager["Folder"]</c> path (AC13 parity). The property remains settable so tests can
        /// inject a config without a live settings store.
        /// </summary>
        public virtual LcppnFolderPredictorConfig FolderPredictorConfig
        {
            get => _folderPredictorConfig ??= ResolveFolderPredictorConfigFromSettings();
            set => _folderPredictorConfig = value;
        }

        /// <summary>
        /// Builds the default <see cref="LcppnFolderPredictorConfig"/> from the persisted production
        /// setting (<see cref="IAppAutoFileObjects.UseLcppnPredictor"/>). The bool flows across the
        /// globals boundary so this assembly never references the host's settings store directly.
        /// </summary>
        /// <returns>A config whose <c>UseLcppnPredictor</c> mirrors the persisted default.</returns>
        private LcppnFolderPredictorConfig ResolveFolderPredictorConfigFromSettings()
        {
            return new LcppnFolderPredictorConfig
            {
                UseLcppnPredictor = Globals.AF.UseLcppnPredictor,
            };
        }

        /// <summary>
        /// Builds an <see cref="LcppnFolderPredictor"/> from the mined corpus using
        /// <c>FolderInfo.RelativePath</c> as the leaf label. The shared flat manager registration
        /// is untouched by this method; the result is held by the Folder-only seam and returned by
        /// <see cref="GetFolderPredictorAsync"/> when the LCPPN flag is set.
        /// </summary>
        /// <param name="collection">The mined mail corpus; must not be null.</param>
        /// <returns>A populated LCPPN folder predictor.</returns>
        public virtual Task<LcppnFolderPredictor> BuildLcppnPredictorAsync(
            MinedMailInfo[] collection
        )
        {
            collection.ThrowIfNull();
            return Task.Run(() => LcppnFolderPredictor.Build(collection, FolderPredictorConfig));
        }

        /// <summary>
        /// Stores the built LCPPN predictor in the shared Folder-only holder on
        /// <c>Globals.AF.FolderPredictor</c>. Used by the registration site after a flag-on build
        /// and exposed as an internal seam so the holder can be set in isolation without running the
        /// full Outlook-backed build pipeline. Routing through the shared holder ensures fresh
        /// per-call <c>OlFolderClassifierGroup</c> instances resolve the same predictor.
        /// </summary>
        /// <param name="predictor">The predictor to hold; may be null to clear the holder.</param>
        internal void SetLcppnPredictor(LcppnFolderPredictor predictor)
        {
            Globals.AF.FolderPredictor = predictor;
        }

        /// <summary>
        /// Resolves the active Folder predictor as an <see cref="IFolderPredictor"/>. When
        /// <see cref="LcppnFolderPredictorConfig.UseLcppnPredictor"/> is true the held LCPPN
        /// predictor is returned; otherwise the unchanged flat <c>Manager["Folder"]</c>
        /// <see cref="BayesianClassifierGroup"/> is awaited and returned. This is the only Folder
        /// seam the callers route through; both predictors satisfy <see cref="IFolderPredictor"/>.
        /// </summary>
        /// <returns>The active Folder predictor typed as <see cref="IFolderPredictor"/>.</returns>
        public virtual async Task<IFolderPredictor> GetFolderPredictorAsync()
        {
            if (
                FolderPredictorConfig?.UseLcppnPredictor == true
                && Globals.AF.FolderPredictor is not null
            )
            {
                return Globals.AF.FolderPredictor;
            }

            return await Globals.AF.Manager["Folder"];
        }

        #endregion Folder predictor seam (LCPPN, flag-gated)

        #region Build Classifiers

        public virtual async Task<ConcurrentObservableCollection<MinedMailInfo>?> LoadStaging()
        {
            _mailInfoCollection = await Task.Run(() =>
            {
                if (Globals.FS.SpecialFolders.TryGetValue("PythonStaging", out var pythonStaging))
                {
                    return new ConcurrentObservableCollection<MinedMailInfo>(
                        Globals.FS.Filenames.EmailInfoStagingFile,
                        pythonStaging
                    );
                }
                else
                {
                    return null;
                }
            });

            return _mailInfoCollection;
        }

        protected ConcurrentObservableCollection<MinedMailInfo>? _mailInfoCollection;

        public virtual async Task<BayesianClassifierGroup> GetOrCreateClassifierGroupAsync(
            MinedMailInfo[] collection
        )
        {
            collection.ThrowIfNull();

            var group = await Task.Run(() =>
                CgUtilities.Deserialize<BayesianClassifierGroup>("StagingClassifierGroup")
            );
            if (group is null)
            {
                group = await CreateClassifierGroupAsync(collection);
                CgUtilities.SerializeAndSave(group, "StagingClassifierGroup");
            }
            return group;
        }

        public virtual async Task<BayesianClassifierGroup> CreateClassifierGroupAsync(
            MinedMailInfo[] collection
        )
        {
            return await Task.Run(() =>
            {
                var group = new BayesianClassifierGroup
                {
                    TotalEmailCount = collection.Count(),
                    SharedTokenBase = new Corpus(
                        collection.SelectMany(x => x.Tokens).GroupAndCount()
                    ),
                };
                return group;
            });
        }

        public virtual async Task BuildClassifierAsync(
            IGrouping<string, MinedMailInfo> group,
            BayesianClassifierGroup classifierGroup,
            CancellationToken cancel
        )
        {
            var matchFrequency = group
                .Select(minedMail => minedMail.Tokens)
                .SelectMany(x => x)
                .GroupAndCount();

            var matchCorpus = new Corpus(matchFrequency);
            var matchEmailCount = group.Count();
            await classifierGroup.RebuildClassifier(
                group.Key,
                matchFrequency,
                matchEmailCount,
                cancel
            );
        }

        public async Task<bool> BuildFolderClassifiersAsync(
            BayesianClassifierGroup classifierGroup,
            MinedMailInfo[] collection,
            ProgressPackage ppkg
        )
        {
            var groups = collection.GroupBy(x => x.FolderInfo!.RelativePath);
            var sw = ppkg.StopWatch!;

            bool success = false;
            try
            {
                await AsyncMultiTasker.AsyncMultiTaskChunker(
                    groups,
                    async (group) =>
                    {
                        await BuildClassifierAsync(group, classifierGroup, ppkg.Cancel);
                    },
                    ppkg.ProgressTrackerPane!,
                    "Building Classifiers",
                    ppkg.Cancel
                );
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

        public async Task BuildClassifiersAsync()
        {
            Globals.AF.Manager.TryRemove("Folder", out _);
            var miner = new EmailDataMiner(Globals);

            var ppkg = await ProgressPackage //.CreateAsTupleAsync(screen: Globals.Ol.GetExplorerScreen());
                .CreateAsTuplePaneAsync(progressTrackerPane: Globals.AF.ProgressTracker)
                .ConfigureAwait(false);
            var sw = ppkg.StopWatch!;
            Globals.AF.ProgressPane.Visible = true;
            ppkg.ProgressTrackerPane!.Report(
                0,
                "Building Folder Classifier -> Load Mined Mail Info"
            );

            if (!_globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                return;
            }

            var folderPath = Path.Combine(folderRoot, "Bayesian");
            var collection = await EmailDataMiner.Load<MinedMailInfo[]>(folderPath);
            collection.ThrowIfNullOrEmpty();
            sw.LogDuration("Load Staging");

            ppkg.ProgressTrackerPane!.Report(
                10,
                "Building Folder Classifier -> Getting Folder Paths"
            );

            var folderPaths = miner
                .QueryOlFolderInfo(miner.GetOlFolderTree())
                .Select(x => x.RelativePath)
                .ToList();
            sw.LogDuration("Get Folder Paths");

            ppkg.ProgressTrackerPane!.Report(
                20,
                "Building Folder Classifier -> Creating Classifier Group"
            );
            var classifierGroup = await GetOrCreateClassifierGroupAsync(collection);
            sw.LogDuration("Get or Create Classifier Group and shared token base");
            sw.WriteToLog(clear: false);
            ppkg.ProgressTrackerPane!.Report(
                30,
                "Building Folder Classifier -> Building Classifiers"
            );

            var childPpkg = await new ProgressPackage()
                .InitializeAsync(
                    ppkg.CancelSource,
                    ppkg.Cancel,
                    ppkg.ProgressTrackerPane!.SpawnChild(),
                    ppkg.StopWatch
                )
                .ConfigureAwait(false);

            if (await BuildFolderClassifiersAsync(classifierGroup, collection, childPpkg))
            {
                Globals.AF.ProgressPane.Visible = false;
                // set the configuration of classifierGroup
                if ((await Globals.AF.Manager.Configuration).TryGetValue("Folder", out var loader))
                {
                    classifierGroup.Config = (
                        loader.Config.DeepCopy() as NewSmartSerializableConfig
                    )!;
                    classifierGroup.Serialize();

                    Globals.AF.Manager["Folder"] = classifierGroup.ToAsyncLazy();
                    //Globals.AF.Manager.Serialize();

                    // Flag-gated LCPPN seam: when UseLcppnPredictor is set, also build and hold the
                    // hierarchy-aware predictor for GetFolderPredictorAsync to return. The flat
                    // Manager["Folder"] registration above is left unchanged in either case. The
                    // predictor is serialized to its OWN dedicated file (LcppnFolder.json), distinct
                    // from Folder.json, so it can be rehydrated on the next startup.
                    if (FolderPredictorConfig?.UseLcppnPredictor == true)
                    {
                        LcppnFolderPredictor predictor = await BuildLcppnPredictorAsync(collection);
                        predictor.Config = LcppnFolderPredictorStore.BuildConfig(folderRoot);
                        predictor.Serialize();
                        Globals.AF.FolderPredictor = predictor;
                    }

                    MyBox.ShowDialog(
                        "Folder Classifier Built Successfully",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
        }

        public async Task CreateSpamClassifiersAsync()
        {
            var temp = await Task.Run(() =>
            {
                var group = new BayesianClassifierGroup
                {
                    TotalEmailCount = 0,
                    SharedTokenBase = new Corpus(),
                };
                return group;
            });
            Globals.AF.Manager["Spam"] = temp.ToAsyncLazy();
            var configurations = await Globals.AF.Manager.Configuration;
            if (configurations.TryGetValue("Spam", out var loader))
            {
                temp.Config = loader.Config;
            }
            temp.Serialize();
            //Globals.AF.Manager.Serialize();
        }

        #endregion Build Classifiers
    }
}
