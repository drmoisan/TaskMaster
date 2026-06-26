using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.EmailIntelligence
{
    public partial class SpamBayes
    {
        public async Task TestAsync(Selection selection)
        {
            if (ClassifierGroup is null)
            {
                return;
            }
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

        public async Task TestAsync(IItemInfo helper)
        {
            var probability = await CalculateProbabilityAsync(helper.Tokens);
            await TestActionAsync(helper, probability);
        }

        public async Task TestAsync(object item)
        {
            if (item is MailItem mailItem)
            {
                var tokens = await TokenizeAsync(mailItem);
                var probability = await CalculateProbabilityAsync(tokens);
                await TestActionAsync(mailItem, probability);
            }
            else
            {
                logger.Warn("Skipping SpamBayes for unknown item type");
            }
        }

        public async Task TrainAsync(Selection selection, bool isSpam)
        {
            if (ClassifierGroup is null)
            {
                return;
            }
            foreach (object item in selection)
            {
                if (item is MailItem mailItem)
                {
                    await TrainAsync(mailItem, isSpam);
                }
            }

            ClassifierGroup.Serialize();
        }

        public override async Task TrainAsync(string[] tokens, bool isSpam)
        {
            var spamOrHam = isSpam ? "Spam" : "Ham";
            await ClassifierGroup
                .Classifiers[spamOrHam]
                .TrainAsync(await tokens.GroupAndCountAsync(), 1, default);
        }

        public string[] TokenizeEmail(object email)
        {
            return email as MailItem is null
                ? []
                : new MailItemHelper(email as MailItem, Globals)
                    .LoadAll(Globals, Globals.Ol.Inbox, true)
                    .Tokens;
        }

        public async Task<string[]> TokenizeEmailAsync(object email)
        {
            return email as MailItem is null
                ? []
                : (
                    await MailItemHelper.FromMailItemAsync(
                        email as MailItem,
                        Globals,
                        default,
                        true
                    )
                ).Tokens;
        }

        public async Task TrainCallbackAsync(object item, bool isSpam)
        {
            MailItem mailItem = item as MailItem;
            await Task.Run(async () =>
            {
                if (isSpam)
                {
                    mailItem.SetUdf("Spam", 1.0, OlUserPropertyType.olPercent);
                    if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.JunkCertain.FolderPath)
                    {
                        await mailItem.TryMoveAsync(Globals.Ol.JunkCertain);
                    }
                }
                else
                {
                    mailItem.SetUdf("Spam", 0.0, OlUserPropertyType.olPercent);
                    if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.Inbox.FolderPath)
                    {
                        await mailItem.TryMoveAsync(Globals.Ol.Inbox);
                    }
                }
            });
        }
    }
}
