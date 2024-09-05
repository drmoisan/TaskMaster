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
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.EmailIntelligence
{
    public class SpamBayes : TristateEngine
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Static Methods

        public SpamBayes(IApplicationGlobals globals) : base()
        {
            Globals = globals;
            Init();
            //Tokenize = TokenizeEmail;
            //TokenizeAsync = TokenizeEmailAsync;
            //CalculateProbability = Globals.AF.Manager["Spam"].Classifiers["Spam"].chi2_spamprob;
            //CalculateProbabilityAsync = Globals.AF.Manager["Spam"].Classifiers["Spam"].Chi2SpamProbAsync;
            //CallbackAsync = TrainCallbackAsync;
            //Threshhold = new TristateThreshhold(0.9, 0.1);
        }

        private SpamBayes() : base() { }

        public SpamBayes Init() 
        {
            Globals.ThrowIfNull();
            
            SpamHamGroup = Globals.AF.Manager["Spam"];
            Tokenize = TokenizeEmail;
            TokenizeAsync = TokenizeEmailAsync;
            CalculateProbability = SpamHamGroup.Classifiers["Spam"].chi2_spamprob;
            CalculateProbabilityAsync = Globals.AF.Manager["Spam"].Classifiers["Spam"].Chi2SpamProbAsync;
            CallbackAsync = TrainCallbackAsync;
            Threshhold = new TristateThreshhold(0.9, 0.1);
            
            return this; 
        }

        

        internal async Task<bool> ValidateSpamManagerAsync(
            Func<CancellationToken, Task<(bool,string)>> asyncValidator,
            Func<Enums.NotFoundEnum, string, CancellationToken, Task<bool>> asyncAction, 
            Enums.NotFoundEnum treatment, 
            CancellationToken cancel)
        {
            var (isValid, message) = await asyncValidator(cancel);
            return isValid ? true : await asyncAction(treatment, message, cancel);
        }

        public virtual bool HasValidSpamManager(out string message)
        {
            try
            {
                Globals.ThrowIfNull().AF.ThrowIfNull().Manager.ThrowIfNull();
            }
            catch (ArgumentNullException e)
            {
                message = e.Message;
                return false;
            }

            if (!Globals.AF.Manager.TryGetValue(GroupName, out var classifierGroup))
            {
                message = $"No classifier group named {GroupName} was found in manager.";
                return false;
            }
            else
            {
                foreach (var name in ClassNames)
                {
                    if (!classifierGroup.Classifiers.TryGetValue(name, out var classifier))
                    {
                        message = $"{GroupName} classifier group cannot find classifier named {name}.";
                        return false;
                    }
                }
                message = "";
                return true;
            }
            //else if (!classifierGroup.Classifiers.TryGetValue("Spam", out var spamClassifier))
            //{
            //    message = "SpamBayes classifier group cannot find a Spam classifier.";
            //    return false;
            //}
            //else
            //{
            //    message = "";
            //    return true;
            //}
        }

        public async Task<(bool, string)> HasValidSpamManagerAsync(CancellationToken token)
        {
            string message = "";
            return await Task.Run(() => HasValidSpamManager(out message), token) ? (true, message) : (false, message);
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
                    Globals.AF.Manager[GroupName] = await CreateSpamClassifiersAsync(cancel);
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
                        Globals.AF.Manager[GroupName] = await CreateSpamClassifiersAsync(cancel);
                        Globals.AF.Manager.Serialize();
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

        public static async Task<SpamBayes> CreateAsync(
            IApplicationGlobals globals,
            bool initialize = true,
            Enums.NotFoundEnum treatment = Enums.NotFoundEnum.Skip, 
            CancellationToken token = default)
        {
            var sb = new SpamBayes();
            sb.Globals = globals;
            if (!await sb.ValidateSpamManagerAsync(sb.HasValidSpamManagerAsync, sb.SpamBayesMissingHandlerAsync,
                treatment, token)) { return null;  }
            
            return await Task.Run(sb.Init, token);
             
        }

        public static BayesianClassifierGroup CreateSpamClassifier()
        {
            var group = new BayesianClassifierGroup
            {
                TotalEmailCount = 0,
                SharedTokenBase = new Corpus()
            };
            //group.Classifiers["Spam"] = new BayesianClassifierShared("Spam", group);
            //group.Classifiers["Ham"] = new BayesianClassifierShared("Ham", group);
            foreach (var name in ClassNames)
            {
                group.Classifiers[name] = new BayesianClassifierShared(name, group);
            }
            return group;
        }

        public static async Task<BayesianClassifierGroup> CreateSpamClassifiersAsync(CancellationToken token = default)
        {
            return await Task.Run(CreateSpamClassifier, token);            
        }

        #endregion Constructors and Static Methods

        #region public Properties

        protected internal IApplicationGlobals Globals { get => _globals; protected set => _globals = value; }
        private IApplicationGlobals _globals;
        
        public BayesianClassifierGroup SpamHamGroup { get => _spamHamGroup; set => _spamHamGroup = value; }
        private BayesianClassifierGroup _spamHamGroup;

        public static readonly HashSet<string> ClassNames = ["Spam", "Ham"];
        public static readonly string GroupName = "Spam";

        #endregion public Properties

        public async Task TestAsync(Selection selection)
        {
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
            foreach (object item in selection)
            {
                if (item is MailItem mailItem)
                {
                    await TrainAsync(mailItem, isSpam);
                }
            }
            Globals.AF.Manager.Serialize();
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

        #region Not Implemented

        public override void Train(string[] tokens, bool isSpam) { throw new NotImplementedException(); }

        #endregion Not Implemented

        //var probability = await CalculateProbabilityAsync(tokens);
    }

}
