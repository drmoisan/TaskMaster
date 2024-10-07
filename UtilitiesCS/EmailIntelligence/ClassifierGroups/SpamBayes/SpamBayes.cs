using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.NewSmartSerializable.Config;

namespace UtilitiesCS.EmailIntelligence
{
    public class SpamBayes : TristateEngine, IConditionalEngine<MailItemHelper>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Static Methods

        public SpamBayes(IApplicationGlobals globals) : base()
        {
            Globals = globals;
            //Init();
        }

        private SpamBayes() : base() { }

        public static async Task<SpamBayes> CreateAsync(
            IApplicationGlobals globals,
            bool initialize = true,
            Enums.NotFoundEnum treatment = Enums.NotFoundEnum.Skip,
            CancellationToken token = default)
        {
            var sb = new SpamBayes();
            sb.Globals = globals;
            
            if (!await sb.ValidateSpamClassifierAsync(
                sb.HasValidSpamClassifierAsync, 
                sb.SpamBayesMissingHandlerAsync,
                treatment, 
                token)) 
            { 
                return null; 
            }

            return await Task.Run(sb.InitAsync, token);

        }

        public async Task<SpamBayes> InitAsync() 
        {
            Globals.ThrowIfNull();

            Globals.AF.Manager.TryGetValue("Spam", out var spamHamGroupTask);
            if (spamHamGroupTask is not null)
            {
                SpamHamGroup = await spamHamGroupTask;
                SpamHamGroup.Config.PropertyChanged += Config_PropertyChanged;
                Tokenize = TokenizeEmail;
                TokenizeAsync = TokenizeEmailAsync;
                CalculateProbability = SpamHamGroup.Classifiers["Spam"].chi2_spamprob;
                CalculateProbabilityAsync = SpamHamGroup.Classifiers["Spam"].Chi2SpamProbAsync;
                CallbackAsync = TrainCallbackAsync;
                Threshhold = new TristateThreshhold(0.8, 0.2);
                return this; 
            }
            else
            {
                return null;
            }
        }
                
        public static BayesianClassifierGroup CreateNewSpamClassifier()
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
            return group;
        }

        public static async Task<BayesianClassifierGroup> CreateSpamClassifiersAsync(CancellationToken token = default)
        {
            return await Task.Run(CreateNewSpamClassifier, token);            
        }

        #endregion Constructors and Static Methods

        #region Classifier Validation

        internal async Task<bool> ValidateSpamClassifierAsync(
            Func<CancellationToken, Task<(bool, string)>> asyncValidator,
            Func<Enums.NotFoundEnum, string, CancellationToken, Task<bool>> asyncAction,
            Enums.NotFoundEnum treatment,
            CancellationToken cancel)
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
                if (classifierGroup is null) { return (false, $"No classifier group named {GroupName} was found in manager."); }
                else
                {
                    foreach (var name in ClassNames)
                    {
                        if (!classifierGroup.Classifiers.TryGetValue(name, out var classifier))
                        {
                            return (false, $"{GroupName} classifier group cannot find classifier named {name}.");
                        }
                    }
                }
                return (true, "");
            }
        }

        public async Task<bool> SpamBayesMissingHandlerAsync(Enums.NotFoundEnum treatment, string message, CancellationToken cancel)
        {
            switch (treatment)
            {
                case Enums.NotFoundEnum.Skip:
                    logger.Warn($"{message} Skipping load");
                    return false;

                case Enums.NotFoundEnum.Create:
                    logger.Warn($"{message} Creating new classifier");
                    Globals.AF.Manager[GroupName] = (await CreateSpamClassifiersAsync(cancel)).ToAsyncLazy();
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
                        SpamHamGroup = await CreateSpamClassifiersAsync(cancel);
                        Globals.AF.Manager[GroupName] = SpamHamGroup.ToAsyncLazy();
                        if ((await Globals.AF.Manager.Configuration)?.TryGetValue("Spam", out var loader) ?? false && loader is not null)
                        {
                            SpamHamGroup.Config = loader.Config;
                            SpamHamGroup.Serialize();
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Could not create Spam classifier because configuration could not be found.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        protected internal IApplicationGlobals Globals { get => _globals; protected set => _globals = value; }
        private IApplicationGlobals _globals;
        
        public BayesianClassifierGroup SpamHamGroup { get => _spamHamGroup; set => _spamHamGroup = value; }
        private BayesianClassifierGroup _spamHamGroup;

        public static readonly HashSet<string> ClassNames = ["Spam", "Ham"];
        public static readonly string GroupName = "Spam";
        
        public bool IsActivated => SpamHamGroup is not null;
                
        #endregion Public Properties

        #region Public Classifier Methods

        public async Task TestAsync(Selection selection)
        {
            if (SpamHamGroup is null) { return; }
            foreach (object item in selection)
            {
                if (item is MailItem mailItem)
                {
                    var tokens = await TokenizeAsync(mailItem);
                    var probability = await CalculateProbabilityAsync(tokens);                    
                    await TestActionAsync(mailItem, probability);
                }
            }
        }

        public async Task TestAsync(MailItemHelper helper)
        {
            var probability = await CalculateProbabilityAsync(helper.Tokens);
            await TestActionAsync(helper.Item, probability);
        }

        public async Task TestAsync(object item)
        {
            if (item is MailItem mailItem)
            {
                var tokens = await TokenizeAsync(mailItem);
                var probability = await CalculateProbabilityAsync(tokens);
                await TestActionAsync(mailItem, probability);
            }
            else { logger.Warn("Skipping SpamBayes for unknown item type");  }
        }

        public async Task TrainAsync(Selection selection, bool isSpam)
        {
            if (SpamHamGroup is null) { return; }
            foreach (object item in selection)
            {
                if (item is MailItem mailItem)
                {
                    await TrainAsync(mailItem, isSpam);
                }
            }
            
            SpamHamGroup.Serialize();
        }

        public override async Task TrainAsync(string[] tokens, bool isSpam)
        {
            var spamOrHam = isSpam ? "Spam" : "Ham";
            await SpamHamGroup.Classifiers[spamOrHam].TrainAsync(await tokens.GroupAndCountAsync(), 1, default);
        }

        public string[] TokenizeEmail(object email)
        {
            return email as MailItem is null ? [] : new MailItemHelper(email as MailItem, Globals).LoadAll(Globals, Globals.Ol.EmailRoot, true).Tokens;
        }
        
        public async Task<string[]> TokenizeEmailAsync(object email) 
        { 
            return email as MailItem is null ? [] : (await MailItemHelper.FromMailItemAsync(email as MailItem, Globals, default, true)).Tokens; 
        }

        public async Task TrainCallbackAsync(object item, bool isSpam)
        {
            MailItem mailItem = item as MailItem;
            await Task.Run(() =>
            {
                if (isSpam)
                {
                    mailItem.SetUdf("Spam", 1.0, OlUserPropertyType.olPercent);
                    if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.JunkCertain.FolderPath)
                    {
                        mailItem.Move(Globals.Ol.JunkCertain);
                    }
                }
                else
                {
                    mailItem.SetUdf("Spam", 0.0, OlUserPropertyType.olPercent);
                    if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.Inbox.FolderPath)
                    {
                        mailItem.Move(Globals.Ol.Inbox);
                    }
                }
            });
            
        }

        public async Task TestActionAsync(object item, double probability)
        {
            await Task.Run(async () => 
            {
                var mailItem = item as MailItem;
                if (mailItem is not null)
                {
                    mailItem.SetUdf("Spam", probability, OlUserPropertyType.olPercent);
                    var isSpam = GetTristate(probability);
                    if (isSpam == true)
                    {
                        if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.JunkCertain.FolderPath)
                            await mailItem.TryMoveAsync(Globals.Ol.JunkCertain, 3);
                        //mailItem.Move(Globals.Ol.JunkCertain);
                    }
                    else if (isSpam == false)
                    {
                        if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.Inbox.FolderPath)
                            await mailItem.TryMoveAsync(Globals.Ol.Inbox, 3);
                        //mailItem.Move(Globals.Ol.Inbox);
                    }
                    else
                    {
                        if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.JunkPossible.FolderPath)
                            await mailItem.TryMoveAsync(Globals.Ol.JunkPossible, 3);
                        //mailItem.Move(Globals.Ol.JunkPossible);
                    }
                }
                
            });
            
        }

        #endregion Public Classifier Methods

        #region Activation and Configuration

        public async Task ToggleActivationAsync()
        {
            var configurations = await Globals.AF.Manager.Configuration;
            if (configurations.TryGetValue("Spam", out var loader))
            {
                loader.Activated = !loader.Activated;
                SpamHamGroup = loader.Activated ? await Globals.AF.Manager["Spam"] : null;
            }
            else
            {
                MessageBox.Show("Could not find configuration for SpamBayes", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        

        public async Task ShowDiskDialog(bool local)
        {
            if (local) { SpamHamGroup.Config.ActivateLocalDisk(); }
            else { SpamHamGroup.Config.ActivateNetDisk(); }
            await ChangeDiskCallback(local);
        }

        internal async void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveDisk") 
            { 
                await ChangeDiskCallback(SpamHamGroup.Config.ActiveDisk == INewSmartSerializableConfig.ActiveDiskEnum.Local);
            }
        }

        internal virtual async Task ChangeDiskCallback(bool local)
        {
            var response = MessageBox.Show($"SpamBayes is now using {(local ? "local" : "network")} disk. Would you like to save the current classifier?",
                            "Save Configuration",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
            if (response == DialogResult.Yes) { SpamHamGroup.Serialize(); }
            else
            {
                response = MessageBox.Show($"Would you like to reload the classifier from {(local ? "local" : "network")}", "Reload Classifier",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (response == DialogResult.Yes)
                {
                    await Globals.AF.Manager.ResetLoadManagerAsyncLazy();
                    Globals.AF.Manager.TryGetValue("Spam", out var spamHamGroupTask);
                    if (spamHamGroupTask is not null)
                    {
                        SpamHamGroup = await spamHamGroupTask;
                        CalculateProbability = SpamHamGroup.Classifiers["Spam"].chi2_spamprob;
                        CalculateProbabilityAsync = SpamHamGroup.Classifiers["Spam"].Chi2SpamProbAsync;
                    }
                }
            }
        }

        public void ShowSaveInfo() => ConfigController.Show(Globals, SpamHamGroup.Config);

        #endregion Activation and Configuration

        #region Not Implemented

        public override void Train(string[] tokens, bool isSpam) { throw new NotImplementedException(); }

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

        public static async Task<IConditionalEngine<MailItemHelper>> CreateEngineAsync(IApplicationGlobals globals)
        {
            var sb = await CreateAsync(globals);
            return sb;
        }

        public Func<MailItemHelper, Task> AsyncAction => (item) => Engine is not null ? ((SpamBayes)Engine).TestAsync(item) : null;

        public Func<object, Task<bool>> AsyncCondition => (item) => Task.Run(() =>
                item is MailItem mailItem && mailItem.MessageClass == "IPM.Note" &&
                mailItem.UserProperties.Find("Spam") is null);

        public object Engine => this;

        public Func<IApplicationGlobals, Task> EngineInitializer => async (globals) => await Task.CompletedTask;

        public string EngineName => "SpamBayes";

        public string Message => $"{nameof(SpamBayes)} is null. Skipping actions";

        public MailItemHelper TypedItem { get; set; }

        #endregion IConditionalEngine Implementation

    }

}
