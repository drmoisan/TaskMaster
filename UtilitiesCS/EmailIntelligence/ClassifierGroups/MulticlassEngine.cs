using Microsoft.Office.Interop.Outlook;
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

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups
{
    /// <summary>
    /// Abstract class representing a multiclass engine for email classification.
    /// </summary>
    public abstract class MulticlassEngine<T> : IConditionalEngine<MailItemHelper> where T : MulticlassEngine<T>, new()
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MulticlassEngine"/> class.
        /// </summary>
        protected MulticlassEngine() {  }

        /// <summary>
        /// Initializes a new instance of the <see cref="MulticlassEngine"/> class with the specified globals.
        /// </summary>
        /// <param name="globals">The application globals.</param>
        public MulticlassEngine(IApplicationGlobals globals)
        {
            Globals = globals;
            CgUtilities = new(Globals);
        }

        /// <summary>
        /// Asynchronously initializes the engine with the specified group name.
        /// </summary>
        /// <typeparam name="T">The type of the multiclass engine.</typeparam>
        /// <param name="groupName">The name of the group.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the initialized engine.</returns>        
        public async Task<T> InitAsync(string groupName) 
        {
            Globals.ThrowIfNull();

            Globals.AF.Manager.TryGetValue(groupName, out var classifierTask);
            if (classifierTask is not null)
            {
                ClassifierGroup = await classifierTask;
                EngineName = groupName;
                AsyncCondition = (item) => Task.Run(() => Condition(item));
                // not sure if this is a logical error that will throw a runtime exception
                T value = (T)this;
                return value;
            }
            else { return default; }
        }

        /// <summary>
        /// Asynchronously creates and initializes a new instance of the specified multiclass engine type.
        /// </summary>
        /// <typeparam name="T">The type of the multiclass engine.</typeparam>
        /// <param name="globals">The application globals.</param>
        /// <param name="categoryGroup">The category group name.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created engine.</returns>        
        public static async Task<T> CreateEngineAsync(
            IApplicationGlobals globals,
            string categoryGroup,
            CancellationToken token = default)
        {
            var cg = new T
            {
                Globals = globals
            };

            return await Task.Run(() => cg.InitAsync(categoryGroup), token);
        }

        #endregion ctor

        /// <summary>
        /// Gets or sets the application globals.
        /// </summary>
        internal IApplicationGlobals Globals { get; private set; }

        /// <summary>
        /// Gets the classifier group utilities.
        /// </summary>
        internal ClassifierGroupUtilities CgUtilities;

        #region Build Category Classifier

        /// <summary>
        /// Asynchronously builds the classifiers.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task BuildClassifiersAsync(int minimumCountPerToken = 0)
        {
            // Ensure engine was initialized with correct variables
            EngineName.ThrowIfNullOrEmpty();
            Globals.ThrowIfNull();

            var miner = new EmailDataMiner(Globals);
            
            // Set up Progress Tracking            
            var (ppkg, sw) = await SetupProgressTracking();

            // Load the staging data            
            MinedMailInfo[] collection = await LoadStagingData(ppkg, sw);
            var allocation = (100 - ppkg.ProgressTrackerPane.Progress);
            
            // Remove the existing Category Classifier
            Globals.AF.Manager.TryRemove(EngineName, out _);

            var childPpkg = await new ProgressPackage()
                .InitializeAsync(ppkg.CancelSource, ppkg.Cancel, ppkg.ProgressTrackerPane.SpawnChild(allocation), ppkg.StopWatch)
                .ConfigureAwait(false);

            // Get or Create the Classifier Group
            BayesianClassifierGroup classifierGroup = await LoadClassifierGroup(childPpkg, sw, collection, EngineName, minimumCountPerToken);

            var childPpkg2 = await new ProgressPackage()
                .InitializeAsync(childPpkg.CancelSource, childPpkg.Cancel, childPpkg.ProgressTrackerPane.SpawnChild(), childPpkg.StopWatch)
                .ConfigureAwait(false);

            if (await BuildClassifiersAsync(classifierGroup, collection, childPpkg2, EngineName, minimumCountPerToken))
            {

                // set the configuration of classifierGroup
                if ((await Globals.AF.Manager.Configuration).TryGetValue(EngineName, out var loader))
                {
                    classifierGroup.Config = loader.Config.DeepCopy() as NewSmartSerializableConfig;
                    classifierGroup.Serialize();

                    Globals.AF.Manager[EngineName] = classifierGroup.ToAsyncLazy();
                    //Globals.AF.Manager.Serialize();
                    MyBox.ShowDialog($"{EngineName} Classifier Built Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            Globals.AF.ProgressPane.Visible = false;

        }

        /// <summary>
        /// Asynchronously builds the classifiers for the specified classifier group and collection.
        /// </summary>
        /// <param name="classifierGroup">The parent collection which will hold the <seealso cref="BayesianClassifierShared"/> to be created and added.</param>
        /// <param name="collection">The collection of <seealso cref="MinedMailInfo"/> from which to build the classifier.</param>
        /// <param name="ppkg">The <seealso cref="ProgressPackage"/> to track progress of the operation.</param>
        /// <param name="groupName">The name of the grouping and classifier.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the classifiers were built successfully.</returns>
        public abstract Task<bool> BuildClassifiersAsync(BayesianClassifierGroup classifierGroup, MinedMailInfo[] collection, ProgressPackage ppkg, string groupName, int minimumCountPerToken = 0);


        /// <summary>
        /// Asynchronously builds a classifier for a specific group of mined email information.
        /// </summary>
        /// <param name="group">The group of mined email information for which to build the classifier.</param>
        /// <param name="classifierGroup">The classifier group to rebuild the classifier in.</param>
        /// <param name="cancel">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>
        public virtual async Task BuildClassifierAsync(
            IGrouping<string, MinedMailInfo> group,
            BayesianClassifierGroup classifierGroup,
            CancellationToken cancel,
            int minimumCountPerToken = 0)
        {
            var matchFrequency = group.Select(minedMail => minedMail.Tokens)
                                      .SelectMany(x => x)
                                      .GroupAndCount()
                                      .Where(kvp => kvp.Value > minimumCountPerToken)
                                      .ToDictionary();

            var matchCorpus = new Corpus(matchFrequency);
            var matchEmailCount = group.Count();
            await classifierGroup.RebuildClassifier(
                group.Key, matchFrequency, matchEmailCount, cancel);
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

        private async Task<BayesianClassifierGroup> LoadClassifierGroup((CancellationTokenSource CancelSource, CancellationToken Cancel, ProgressTrackerPane ProgressTrackerPane, SegmentStopWatch StopWatch) ppkg, SegmentStopWatch sw, MinedMailInfo[] collection, string groupName)
        {
            ppkg.ProgressTrackerPane.Report(0, $"Building {groupName} Classifier -> Creating Classifier Group");
            var classifierGroup = await CgUtilities.GetOrCreateClassifierGroupAsync(collection, groupName);
            sw.LogDuration("Get or Create Classifier Group and shared token base");
            sw.WriteToLog(clear: false);
            ppkg.ProgressTrackerPane.Report(20, $"Building {groupName} Classifier -> Creating Classifier Group");
            return classifierGroup;
        }

        private async Task<BayesianClassifierGroup> LoadClassifierGroup(ProgressPackage ppkg, SegmentStopWatch sw, MinedMailInfo[] collection, string groupName, int minimumCountPerToken = 0)
        {
            ppkg.ProgressTrackerPane.Report(20, $"Building {groupName} Classifier -> Creating Classifier Group");
            var classifierGroup = await CgUtilities.GetOrCreateClassifierGroupAsync(collection, groupName, minimumCountPerToken);
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

        #endregion Build Category Classifier

        #region Public Properties


        /// <summary>
        /// Gets or sets the classifier group.
        /// </summary>
        public BayesianClassifierGroup ClassifierGroup { get; set; }

        /// <summary>
        /// Gets a <seealso cref="bool"/> value indicating whether the engine is activated.
        /// </summary>
        public bool IsActivated => ClassifierGroup is not null;

        /// <summary>
        /// Gets or sets the probability threshold for classification. Potential classes below this 
        /// probability will be excluded from the results
        /// </summary>
        public double ProbabilityThreshold { get; set; } = 0.8;

        /// <summary>
        /// Tests the classifier asynchronously with the specified mail item helper.
        /// </summary>
        /// <param name="helper">The mail item helper.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task TestAsync(MailItemHelper helper);

        #endregion Public Properties


        #region IConditionalEngine Implementation

        /// <summary>
        /// Gets the configuration of the classifier group.
        /// </summary>
        public ISmartSerializableConfig Config => ClassifierGroup.Config;

        /// <summary>
        /// Serializes the classifier group.
        /// </summary>
        void IConditionalEngine<MailItemHelper>.Serialize()
        {
            this.ClassifierGroup.Serialize();
        }

        /// <summary>
        /// Delegate that represents the asynchronous action that runs when a condition is met
        /// </summary>
        public Func<MailItemHelper, Task> AsyncAction { get; internal set; }

        /// <summary>
        /// Delegate representing the asynchronous condition that must be true to run the engine.
        /// </summary>
        public Func<object, Task<bool>> AsyncCondition { get; internal set; }

        /// <summary>
        /// Determines whether the specified item meets the condition and logs details about the result
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item meets the condition; otherwise, <c>false</c>.</returns>
        internal bool Condition(object item)
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

        /// <summary>
        /// Returns this class as the engine object in compliance with the <seealso cref="IConditionalEngine{}"/> interface
        /// </summary>
        public object Engine => this;

        /// <summary>
        /// Delegate from the <seealso cref="IConditionalEngine{}"/> interface representing the 
        /// asynchronous action that initializes the engine
        /// </summary>
        public Func<IApplicationGlobals, Task> EngineInitializer { get; internal set; } = async(globals) => await Task.CompletedTask;

        /// <summary>
        /// Property from the <seealso cref="IConditionalEngine{}"/> interface that represents the name of the engine
        /// </summary>
        public string EngineName { get; internal set; }

        /// <summary>
        /// Property from the <seealso cref="IConditionalEngine{}"/> interface that represents the message that is 
        /// delivered if the engine is null
        /// </summary>
        public string Message { get; internal set; } = $"{nameof(MulticlassEngine<T>)} is null. Skipping actions";

        /// <summary>
        /// Gets or sets the typed mail item helper.
        /// </summary>
        public MailItemHelper TypedItem { get; set; }


        #endregion IConditionalEngine Implementation


    }
}
