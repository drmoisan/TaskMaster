using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.EmailIntelligence
{
    public class Triage
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ctor

        public Triage(
            IApplicationGlobals globals,
            CancellationToken token = default)
        {
            //Manager = manager;
            Globals = globals;
            Token = token;
        }

        private Triage() { }

        public async Task<Triage> InitAsync()
        {
            TokenizeAsync = new EmailTokenizer().TokenizeAsync;
            CallbackAsync = (item, value) => Task.Run(() => ((MailItem)item).SetUdf("Triage", value));
            
            ClassifierGroup = await Globals.AF.Manager[GroupName];
            return this;
        }

        public static async Task<Triage> CreateAsync(
            IApplicationGlobals globals,
            bool initialize = true,
            Enums.NotFoundEnum treatment = Enums.NotFoundEnum.Skip,
            CancellationToken token = default)
        {
            var triage = new Triage();
            triage.Globals = globals;

            if (!await triage.ValidateTriageManagerAsync(
                triage.HasValidTriageManagerAsync,
                triage.TriageMissingHandlerAsync,
                treatment,
                token)) { return null; }

            return await Task.Run(triage.InitAsync, token);

        }

        public static async Task<ConditionalItemEngine<MailItemHelper>> CreateEngineAsync(IApplicationGlobals globals)
        {
            var ce = new ConditionalItemEngine<MailItemHelper>();

            ce.AsyncCondition = (item) => Task.Run(() =>
                item is MailItem mailItem && mailItem.MessageClass == "IPM.Note" &&
                mailItem.UserProperties.Find("Triage") is null);

            ce.EngineInitializer = async (globals) => ce.Engine = await Triage.CreateAsync(globals);
            await ce.EngineInitializer(globals);
            ce.AsyncAction = (item) => ce.Engine is not null ? ((Triage)ce.Engine).TestAsync(item) : null;
            ce.EngineName = "Triage";
            ce.Message = $"{ce.EngineName} is null. Skipping actions";
            
            return ce;
        }

        #endregion ctor

        public static readonly HashSet<string> ClassNames = ["A", "B", "C"];
        public static readonly string UnknownClassMarker = "U";
        internal static readonly string GroupName = "Triage";

        internal async Task<bool> ValidateTriageManagerAsync(
            Func<CancellationToken, Task<(bool, string)>> asyncValidator,
            Func<Enums.NotFoundEnum, string, CancellationToken, Task<bool>> asyncAction,
            Enums.NotFoundEnum treatment,
            CancellationToken cancel)
        {
            var (isValid, message) = await asyncValidator(cancel);
            return isValid ? true : await asyncAction(treatment, message, cancel);
        }
               
        public async Task<(bool, string)> HasValidTriageManagerAsync(CancellationToken token)
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
                foreach (var name in ClassNames)
                {
                    if (!classifierGroup.Classifiers.TryGetValue(name, out var classifier))
                    {
                        return (false, $"{GroupName} classifier group cannot find classifier named {name}.");
                    }
                }
                return (true, "");
            }
        }

        public async Task<bool> TriageMissingHandlerAsync(Enums.NotFoundEnum treatment, string message, CancellationToken cancel)
        {
            switch (treatment)
            {
                case Enums.NotFoundEnum.Skip:
                    logger.Warn($"{message} Skipping load");
                    return false;

                case Enums.NotFoundEnum.Create:
                    logger.Warn($"{message} Creating new classifier");
                    ClassifierGroup = await CreateTriageClassifiersAsync(ClassNames, cancel);
                    if ((await Globals.AF.Manager.Configuration).TryGetValue($"Config{GroupName}", out var loader))
                    {
                        ClassifierGroup.Config = loader.Config;
                        ClassifierGroup.Serialize();
                    }
                    Globals.AF.Manager[GroupName] = ClassifierGroup.ToAsyncLazy();
                    return true;

                case Enums.NotFoundEnum.Throw:
                    logger.Error($"{message} Throwing exception");
                    throw new ArgumentNullException(message);

                case Enums.NotFoundEnum.Ask:
                    logger.Warn($"{message}. Asking user");
                    var result = MessageBox.Show(
                        $"{message} Would you like to create a new classifier?",
                        $"Cannot Load {GroupName}",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        ClassifierGroup = await CreateTriageClassifiersAsync(ClassNames, cancel);
                        if ((await Globals.AF.Manager.Configuration).TryGetValue($"Config{GroupName}", out loader))
                        {
                            ClassifierGroup.Config = loader.Config;
                            ClassifierGroup.Serialize();
                        }
                        Globals.AF.Manager[GroupName] = ClassifierGroup.ToAsyncLazy();
                        return true;
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
                
        public static BayesianClassifierGroup CreateClassifier()
        {
            var group = new BayesianClassifierGroup
            {
                TotalEmailCount = 0,
                SharedTokenBase = new Corpus()
            };
            foreach (var name in ClassNames)
            {
                group.Classifiers[name] = new BayesianClassifierShared(name, group);
            }
            group.MinimumProbability = 0.9;
            return group;
        }

        public static async Task<BayesianClassifierGroup> CreateTriageClassifiersAsync(HashSet<string> classNames, CancellationToken token = default)
        {
            return await Task.Run(CreateClassifier, token);
        }


        #region Properties

        public BayesianClassifierGroup ClassifierGroup { get => _classifierGroup; set => _classifierGroup = value; }
        private BayesianClassifierGroup _classifierGroup;

        //internal ScDictionary<string, BayesianClassifierGroup> Manager { get; }
        internal CancellationToken Token { get; }

        protected internal IApplicationGlobals Globals { get; protected set; }

        /// <summary>
        /// Async Delegate Function that extracts an array of string tokens from an object
        /// </summary>
        public Func<object, IApplicationGlobals, CancellationToken, Task<string[]>> TokenizeAsync { get => _tokenizeAsync; set => _tokenizeAsync = value; }
        private Func<object, IApplicationGlobals, CancellationToken, Task<string[]>> _tokenizeAsync;

        public Func<object, string, Task> CallbackAsync { get => _callbackAsync; set => _callbackAsync = value; }
        private Func<object, string, Task> _callbackAsync;


        #endregion Properties

        public async Task CreateNewTriageClassifierGroupAsync(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                ClassifierGroup = new BayesianClassifierGroup();
                if ((await Globals.AF.Manager.Configuration).TryGetValue($"Config{GroupName}", out var loader)) 
                { 
                    ClassifierGroup.Config = loader.Config;
                    ClassifierGroup.Serialize();
                }
                Globals.AF.Manager[GroupName] = ClassifierGroup.ToAsyncLazy();
                
            }, token);
        }

        public async Task ClassifyAsync(Selection selection, CancellationToken token = default)
        {
            await selection.Cast<object>()
                .Where(x => x is MailItem)
                .ToAsyncEnumerable()
                .ForEachAwaitWithCancellationAsync(async (item, token) =>
                {
                    var predictions = await ClassifierGroup.ClassifyAsync(item, token);
                    var mostLikely = predictions.FirstOrDefault().Class;
                    if (CallbackAsync is not null) { await CallbackAsync(item, mostLikely); }
                }, token);
        }

        public async Task TrainAsync(Selection selection, string triageId, CancellationToken token = default)
        {
            await selection.Cast<object>()
                           .Where(x => x is MailItem)
                           .Cast<MailItem>()
                           .ToAsyncEnumerable()
                           .ForEachAwaitWithCancellationAsync((item, token) => TrainAsync(item, triageId), token);

            ClassifierGroup.Serialize();
        }

        public async Task TrainAsync(object item, string triageId, CancellationToken cancel = default)
        {
            TokenizeAsync.ThrowIfNull($"{nameof(TokenizeAsync)} delegate function cannot be null to Train classifier");
            item.ThrowIfNull($"{nameof(item)} cannot be null to Train classifier");
            var tokens = await TokenizeAsync(item, Globals, Token);
            await TrainAsync(tokens, triageId);
            if (CallbackAsync is not null) { await CallbackAsync(item, triageId); }
        }

        public async Task TrainAsync(string[] tokens, string triageId, CancellationToken cancel = default)
        {
            var classifierName = triageId;
            //Manager["Triage"].Classifiers[classifierName].Train(await tokens.GroupAndCountAsync(), 1);
            await Task.Run(() => ClassifierGroup.AddOrUpdateClassifier(classifierName, tokens, 1), cancel);
        }

        public async Task TestAsync(Selection selection, CancellationToken token = default)
        {
            if (selection is null) { return; }
            
            await selection
                .Cast<object>()
                .ToAsyncEnumerable()
                .Where(x => x is MailItem)
                .Cast<MailItem>()
                .SelectAwaitWithCancellation(async (item, token) =>
                {
                    var h = await MailItemHelper.FromMailItemAsync(item, Globals, token, false);
                    _ = h.Tokens;
                    return h;
                }).ForEachAwaitWithCancellationAsync(TestAsync, token);
        }

        public async Task TestAsync(MailItemHelper helper, CancellationToken token = default)
        {
            var predictions = await ClassifierGroup.ClassifyAsync(helper.Tokens, token);
            var predictedClass = predictions.Count() == 0 ? UnknownClassMarker : predictions.First().Class;
            await TestActionAsync(helper, predictedClass, token);
        }

        public async Task TestAsync(MailItem mailItem, CancellationToken cancel = default)
        {
            TokenizeAsync.ThrowIfNull($"{nameof(TokenizeAsync)} delegate function cannot " +
                $"be null to Predict {GroupName} from a {nameof(MailItem)}");

            var tokens = await TokenizeAsync(mailItem, Globals, cancel);
            var predictions = await ClassifierGroup.ClassifyAsync(tokens, cancel);
            var predictedClass = predictions.Count() == 0 ? UnknownClassMarker : predictions.First().Class;
            await TestActionAsync(mailItem, predictedClass, cancel);
        }

        public async Task TestActionAsync(MailItemHelper helper, string predictedClass, CancellationToken token = default)
        {
            await Task.Run(() =>
            {
                helper.Triage = predictedClass;
                helper.Item.SetUdf("Triage", predictedClass);
            });
        }

        public async Task TestActionAsync(MailItem mailItem, string predictedClass, CancellationToken token = default)
        {
            await Task.Run(() => mailItem.SetUdf("Triage", predictedClass), token);
        }
    }
}