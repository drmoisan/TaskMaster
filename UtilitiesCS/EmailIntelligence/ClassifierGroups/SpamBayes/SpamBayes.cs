#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.NewSmartSerializable.Config;
using UtilitiesCS.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UtilitiesCS.EmailIntelligence
{
    public partial class SpamBayes : TristateEngine, IConditionalEngine<MailItemHelper>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region Constructors and Static Methods

        public SpamBayes(IApplicationGlobals globals)
            : base()
        {
            Globals = globals;
            //Init();
        }

        private SpamBayes()
            : base() { }

        public static async Task<SpamBayes?> CreateAsync(
            IApplicationGlobals globals,
            bool initialize = true,
            Enums.NotFoundEnum treatment = Enums.NotFoundEnum.Skip,
            CancellationToken token = default
        )
        {
            var sb = new SpamBayes();
            sb.Globals = globals;

            // Diagnosis-only sub-step attribution (issue #211, Phase 3.5): time each CreateAsync
            // sub-step with a local Stopwatch and emit one [spam-init] line per sub-step through the
            // existing log4net logger. Behavior-preserving: the validation results, early returns,
            // and the returned engine are unchanged; only timing lines are added. Stopwatch only;
            // no banned timing APIs.
            var probe = new SpamInitTimingProbe(s => logger.Debug(s));

            var validatePathsWatch = Stopwatch.StartNew();
            var pathsAreSet = sb.ValidatePathsSet();
            validatePathsWatch.Stop();
            probe.EmitStep("ValidatePathsSet", validatePathsWatch.Elapsed.TotalMilliseconds);
            if (!pathsAreSet)
            {
                return null;
            }

            var validateClassifierWatch = Stopwatch.StartNew();
            var classifierIsValid = await sb.ValidateSpamClassifierAsync(
                sb.HasValidSpamClassifierAsync,
                sb.SpamBayesMissingHandlerAsync,
                treatment,
                token
            );
            validateClassifierWatch.Stop();
            probe.EmitStep(
                "ValidateSpamClassifier",
                validateClassifierWatch.Elapsed.TotalMilliseconds
            );
            if (!classifierIsValid)
            {
                return null;
            }

            var initWatch = Stopwatch.StartNew();
            var engine = await Task.Run(sb.InitAsync, token);
            initWatch.Stop();
            probe.EmitStep("InitAsync(modelLoad)", initWatch.Elapsed.TotalMilliseconds);
            return engine;
        }

        public async Task<SpamBayes?> InitAsync()
        {
            Globals.ThrowIfNull();

            Globals.AF.Manager.TryGetValue("Spam", out var spamHamGroupTask);
            if (spamHamGroupTask is not null)
            {
                ClassifierGroup = await spamHamGroupTask;
                Tokenize = TokenizeEmail;
                TokenizeAsync = TokenizeEmailAsync;
                CalculateProbability = ClassifierGroup.Classifiers["Spam"].chi2_spamprob;
                CalculateProbabilityAsync = ClassifierGroup.Classifiers["Spam"].Chi2SpamProbAsync;
                CallbackAsync = TrainCallbackAsync;
                Threshhold = new TristateThreshhold(0.8, 0.2);
                return this;
            }
            else
            {
                return null;
            }
        }

        public static BayesianClassifierGroup CreateNewClassifier()
        {
            var group = new BayesianClassifierGroup
            {
                TotalEmailCount = 0,
                SharedTokenBase = new Corpus(),
                Name = GroupName,
            };
            foreach (var name in ClassNames)
            {
                group.Classifiers[name] = new BayesianClassifierShared(name, group);
            }
            return group;
        }

        public static async Task<BayesianClassifierGroup> CreateSpamClassifiersAsync(
            CancellationToken token = default
        )
        {
            return await Task.Run(CreateNewClassifier, token);
        }

        #endregion Constructors and Static Methods

        #region Classifier Validation

        internal bool ValidatePathsSet()
        {
            // Diagnosis-only per-folder attribution (issue #211, Phase 3.5): time each COM folder
            // resolution individually with a local Stopwatch and emit one [spam-init] line per
            // folder access through the existing log4net logger. Behavior-preserving: the
            // ThrowIfNull chain, the ArgumentNullException catch/logging, and the bool return
            // semantics (which folder causes false vs true) are unchanged; only timing lines are
            // added. Stopwatch only; no banned timing APIs.
            var probe = new SpamInitTimingProbe(s => logger.Debug(s));
            try
            {
                var junkCertainWatch = Stopwatch.StartNew();
                Globals.ThrowIfNull().Ol.ThrowIfNull().JunkCertain.ThrowIfNull();
                junkCertainWatch.Stop();
                probe.EmitStep(
                    "ValidatePathsSet.JunkCertain",
                    junkCertainWatch.Elapsed.TotalMilliseconds
                );

                var junkPotentialWatch = Stopwatch.StartNew();
                Globals.Ol.JunkPotential.ThrowIfNull();
                junkPotentialWatch.Stop();
                probe.EmitStep(
                    "ValidatePathsSet.JunkPotential",
                    junkPotentialWatch.Elapsed.TotalMilliseconds
                );

                var inboxWatch = Stopwatch.StartNew();
                Globals.Ol.Inbox.ThrowIfNull();
                inboxWatch.Stop();
                probe.EmitStep("ValidatePathsSet.Inbox", inboxWatch.Elapsed.TotalMilliseconds);
            }
            catch (ArgumentNullException e)
            {
                logger.Error(
                    $"Error initializing {nameof(SpamBayes)} in {nameof(ValidatePathsSet)}: {e.Message}"
                );
                return false;
            }
            return true;
        }

        internal async Task<bool> ValidateSpamClassifierAsync(
            Func<CancellationToken, Task<(bool, string)>> asyncValidator,
            Func<Enums.NotFoundEnum, string, CancellationToken, Task<bool>> asyncAction,
            Enums.NotFoundEnum treatment,
            CancellationToken cancel
        )
        {
            var (isValid, message) = await asyncValidator(cancel);
            return isValid ? true : await asyncAction(treatment, message, cancel);
        }

        public async Task<(bool, string)> HasValidSpamClassifierAsync(CancellationToken token)
        {
            try
            {
                Globals.ThrowIfNull().AF.ThrowIfNull().Manager.ThrowIfNull();
            }
            catch (ArgumentNullException e)
            {
                return (false, e.Message);
            }

            if (!Globals.AF.Manager.TryGetValue(GroupName, out var classifierGroupTask))
            {
                return (false, $"No classifier group named {GroupName} was found in manager.");
            }
            else
            {
                var classifierGroup = await classifierGroupTask;
                if (classifierGroup is null)
                {
                    return (false, $"No classifier group named {GroupName} was found in manager.");
                }
                else
                {
                    foreach (var name in ClassNames)
                    {
                        if (!classifierGroup.Classifiers.TryGetValue(name, out var classifier))
                        {
                            return (
                                false,
                                $"{GroupName} classifier group cannot find classifier named {name}."
                            );
                        }
                    }
                }
                return (true, "");
            }
        }

        public async Task<bool> SpamBayesMissingHandlerAsync(
            Enums.NotFoundEnum treatment,
            string message,
            CancellationToken cancel
        )
        {
            switch (treatment)
            {
                case Enums.NotFoundEnum.Skip:
                    logger.Warn($"{message} Skipping load");
                    return false;

                case Enums.NotFoundEnum.Create:
                    logger.Warn($"{message} Creating new classifier");
                    Globals.AF.Manager[GroupName] = (
                        await CreateSpamClassifiersAsync(cancel)
                    ).ToAsyncLazy();
                    return true;

                case Enums.NotFoundEnum.Throw:
                    logger.Error($"{message} Throwing exception");
                    throw new ArgumentNullException(message);

                case Enums.NotFoundEnum.Ask:
                    logger.Warn($"{message}. Asking user");
                    var result = MyBox.ShowDialog(
                        $"{message} Would you like to create a new classifier?",
                        $"Cannot Load {GroupName}",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );
                    if (result == DialogResult.Yes)
                    {
                        ClassifierGroup = await CreateSpamClassifiersAsync(cancel);
                        Globals.AF.Manager[GroupName] = ClassifierGroup.ToAsyncLazy();
                        if (
                            (await Globals.AF.Manager.Configuration)?.TryGetValue(
                                "Spam",
                                out var loader
                            )
                            ?? false && loader is not null
                        )
                        {
                            ClassifierGroup.Config = loader.Config;
                            ClassifierGroup.Serialize();
                            return true;
                        }
                        else
                        {
                            MyBox.ShowDialog(
                                "Could not create Spam classifier because configuration could not be found.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                default:
                    logger.Error($"Unknown value for variable {nameof(treatment)}: {treatment}");
                    throw new ArgumentOutOfRangeException(nameof(treatment), "Unknown treatment");
            }
        }

        #endregion Classifier Validation

        #region Public Properties

        public ISmartSerializableConfig Config => ClassifierGroup.Config;

        protected internal IApplicationGlobals Globals
        {
            get => _globals;
            protected set => _globals = value;
        }

        // Set by constructor/CreateAsync or a builder; not tracked as ctor init by the compiler.
        private IApplicationGlobals _globals = null!;

        public BayesianClassifierGroup ClassifierGroup
        {
            get => _classifierGroup;
            set => _classifierGroup = value;
        }

        // Assigned by InitAsync; null until activated (IsActivated reflects that at runtime).
        private BayesianClassifierGroup _classifierGroup = null!;

        public static readonly HashSet<string> ClassNames = ["Spam", "Ham"];
        public static readonly string GroupName = "Spam";

        public bool IsActivated => ClassifierGroup is not null;

        #endregion Public Properties

        #region Public Classifier Methods

        #endregion Public Classifier Methods

        //#region Activation and Configuration

        ////public async Task ToggleActivationAsync()
        ////{
        ////    var configurations = await Globals.AF.Manager.Configuration;
        ////    if (configurations.TryGetValue("Spam", out var loader))
        ////    {
        ////        loader.Config.ClassifierActivated = !loader.Config.ClassifierActivated;
        ////        SpamHamGroup = loader.Config.ClassifierActivated ? await Globals.AF.Manager["Spam"] : null;
        ////    }
        ////    else
        ////    {
        ////        MessageBox.Show("Could not find configuration for SpamBayes", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        ////    }
        ////}

        //public async Task ShowDiskDialog(bool local)
        //{
        //    if (local) { ClassifierGroup.Config.ActivateLocalDisk(); }
        //    else { ClassifierGroup.Config.ActivateNetDisk(); }
        //    await ChangeDiskCallback(local);
        //}

        //internal void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    //if (e.PropertyName == "ActiveDisk")
        //    if (e.PropertyName.Contains("ActiveDisk"))
        //    {
        //        IdleAsyncQueue.AddEntry(false, async () => await ChangeDiskCallback(ClassifierGroup.Config.ActiveDisk == INewSmartSerializableConfig.ActiveDiskEnum.Local));
        //        //await ChangeDiskCallback(SpamHamGroup.Config.ActiveDisk == INewSmartSerializableConfig.ActiveDiskEnum.Local);
        //    }
        //}

        //internal virtual async Task ChangeDiskCallback(bool local)
        //{
        //    var response = MessageBox.Show($"SpamBayes is now using {(local ? "local" : "network")} disk. Would you like to save the current classifier?",
        //                    "Save Configuration",
        //                    MessageBoxButtons.YesNo,
        //                    MessageBoxIcon.Question);
        //    if (response == DialogResult.Yes) { ClassifierGroup.Serialize(); }
        //    else
        //    {
        //        response = MessageBox.Show($"Would you like to reload the classifier from {(local ? "local" : "network")}", "Reload Classifier",
        //            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //        if (response == DialogResult.Yes)
        //        {
        //            await Globals.AF.Manager.ResetLoadManagerAsyncLazy();
        //            Globals.AF.Manager.TryGetValue("Spam", out var spamHamGroupTask);
        //            if (spamHamGroupTask is not null)
        //            {
        //                ClassifierGroup = await spamHamGroupTask;
        //                CalculateProbability = ClassifierGroup.Classifiers["Spam"].chi2_spamprob;
        //                CalculateProbabilityAsync = ClassifierGroup.Classifiers["Spam"].Chi2SpamProbAsync;
        //            }
        //        }
        //    }
        //}

        //public void ShowSaveInfo() => ConfigController.Show(Globals, ClassifierGroup.Config);

        //#endregion Activation and Configuration

        #region Not Implemented

        public override void Train(string[] tokens, bool isSpam)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented

        #region IConditionalEngine Implementation

        //public static async Task<ConditionalItemEngine<MailItemHelper>> CreateEngineAsync(IApplicationGlobals globals)
        //{
        //    var ce = new ConditionalItemEngine<MailItemHelper>();
        //    ce.AsyncCondition = (item) => Task.Run(() =>
        //        item is MailItem mailItem && mailItem.MessageClass == "IPM.Note" &&
        //        mailItem.UserProperties.Find("Spam") is null);
        //    ce.EngineInitializer = async (globals) => ce.Engine = await CreateAsync(globals);
        //    await ce.EngineInitializer(globals);
        //    ce.AsyncAction = (item) => ce.Engine is not null ? ((SpamBayes)ce.Engine).TestAsync(item) : null;
        //    ce.EngineName = "SpamBayes";
        //    ce.Message = $"{ce.EngineName} is null. Skipping actions";
        //    return ce;
        //}

        public static async Task<IConditionalEngine<MailItemHelper>?> CreateEngineAsync(
            IApplicationGlobals globals
        )
        {
            var sb = await CreateAsync(globals);
            return sb;
        }

        void IConditionalEngine<MailItemHelper>.Serialize()
        {
            this.ClassifierGroup.Serialize();
        }

        public object Engine => this;

        public Func<IApplicationGlobals, Task> EngineInitializer =>
            async (globals) => await Task.CompletedTask;

        public string EngineName => "Spam";

        public string Message => $"{nameof(SpamBayes)} is null. Skipping actions";

        public MailItemHelper TypedItem { get; set; } = null!;

        #endregion IConditionalEngine Implementation
    }
}
