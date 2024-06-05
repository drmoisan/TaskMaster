using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.EmailIntelligence.Bayesian;
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
            Tokenize = TokenizeEmail;
            TokenizeAsync = TokenizeEmailAsync;
            CalculateProbability = Globals.AF.Manager["Spam"].Classifiers["Spam"].chi2_spamprob;
            CalculateProbabilityAsync = Globals.AF.Manager["Spam"].Classifiers["Spam"].Chi2SpamProbAsync;
            CallbackAsync = TrainCallbackAsync;
            Threshhold = new TristateThreshhold(0.9, 0.1);
        }

        public IApplicationGlobals Globals { get => _globals; set => _globals = value; }
        private IApplicationGlobals _globals;

        public static async Task CreateNewSpamManagerAsync(ScDictionary<string, BayesianClassifierGroup> manager)
        {
            manager["Spam"] = await CreateSpamClassifiersAsync();
            manager.Serialize();
        }        
        
        internal static async Task<BayesianClassifierGroup> CreateSpamClassifiersAsync(CancellationToken token = default)
        {
            return await Task.Run(() =>
            {
                var group = new BayesianClassifierGroup
                {
                    TotalEmailCount = 0,
                    SharedTokenBase = new Corpus()
                };
                return group;
            }, token);
            
        }

        #endregion Constructors and Static Methods

        public async Task TestAsync(Selection selection)
        {
            foreach (object item in selection)
            {
                if (item is MailItem mailItem)
                {
                    var tokens = await TokenizeAsync(mailItem);
                    var probability = await CalculateProbabilityAsync(tokens);                    
                    await TestAction(mailItem, probability);
                }
            }
        }

        public async Task TestAsync(object item)
        {
            if (item is MailItem mailItem)
            {
                var tokens = await TokenizeAsync(mailItem);
                var probability = await CalculateProbabilityAsync(tokens);
                await TestAction(mailItem, probability);
            }   
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
            var classifierName = isSpam ? "Spam" : "Ham";
            Globals.AF.Manager["Spam"].Classifiers[classifierName].Train(await tokens.GroupAndCountAsync(), 1);
        }

        public string[] TokenizeEmail(object email)
        {
            return email as MailItem is null ? [] : new MailItemHelper(email as MailItem).LoadAll(Globals, Globals.Ol.EmailRoot, true).Tokens;
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

        public async Task TestAction(object item, double probability)
        {
            await Task.Run(() => 
            {
                MailItem mailItem = item as MailItem;
                mailItem.SetUdf("Spam", probability, OlUserPropertyType.olPercent);
                var isSpam = GetTristate(probability);
                if (isSpam == true)
                {
                    if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.JunkCertain.FolderPath)
                        mailItem.Move(Globals.Ol.JunkCertain);
                }
                else if (isSpam == false)
                {
                    if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.Inbox.FolderPath)
                        mailItem.Move(Globals.Ol.Inbox);
                }
                else
                {
                    if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.JunkPossible.FolderPath)
                        mailItem.Move(Globals.Ol.JunkPossible);
                }
            });
            
        }

        #region Not Implemented

        public override void Train(string[] tokens, bool isSpam) { throw new NotImplementedException(); }

        #endregion Not Implemented

        //var probability = await CalculateProbabilityAsync(tokens);
    }

}
