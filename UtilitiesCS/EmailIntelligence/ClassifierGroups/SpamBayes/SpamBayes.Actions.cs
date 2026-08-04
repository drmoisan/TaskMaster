#nullable enable
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.EmailIntelligence
{
    public partial class SpamBayes
    {
        internal void MoveSpamOrHam(object item, double probability)
        {
            var isSpam = GetTristate(probability);
            if (item is MailItemHelper helper && helper.Item is not null)
            {
                helper.Item.SetUdf("Spam", probability, OlUserPropertyType.olPercent);
                MoveSpamOrHam(helper, isSpam);
            }
            else if (item is MailItem mailItem)
            {
                mailItem.SetUdf("Spam", probability, OlUserPropertyType.olPercent);
                MoveSpamOrHam(mailItem, isSpam);
            }
        }

        internal void MoveSpamOrHam(MailItemHelper helper, bool? isSpam)
        {
            lock (helper.Item)
            {
                Folder? destination = GetDestinationFolder(helper.Item, isSpam);
                if (destination is not null)
                {
                    var moved = helper.Item.Move(destination);
                    if (moved is not null)
                    {
                        helper.Item = moved;
                    }
                }
            }
        }

        internal void MoveSpamOrHam(MailItem mailItem, bool? isSpam)
        {
            Folder? destination = GetDestinationFolder(mailItem, isSpam);
            if (destination is not null)
                mailItem.Move(destination);
        }

        internal Folder? GetDestinationFolder(MailItem mailItem, bool? isSpam)
        {
            if (mailItem is null)
            {
                return null;
            }
            if (isSpam == true)
            {
                var junkCertain = Globals?.Ol?.JunkCertain;
                return junkCertain ?? mailItem.Parent as Folder;
            }
            else if (isSpam == false)
            {
                //if (((mailItem.Parent as Folder)?.FolderPath ?? "") != Globals.Ol.Inbox.FolderPath)
                //    return Globals.Ol.Inbox;
                return null;
            }
            else
            {
                if (
                    ((mailItem.Parent as Folder)?.FolderPath ?? "")
                    != (Globals?.Ol?.JunkPotential?.FolderPath ?? "junk_potential_not_set")
                )
                    return Globals?.Ol?.JunkPotential;
            }
            return null;
        }

        public async Task TestActionAsync(object item, double probability)
        {
            await Task.Run(() => MoveSpamOrHam(item, probability));
        }

        //public async Task TestActionAsync(object item, double probability)
        //{
        //    await Task.Run(async () =>
        //    {
        //        var mailItem = item as MailItem;
        //        if (mailItem is not null)
        //        {
        //            mailItem.SetUdf("Spam", probability, OlUserPropertyType.olPercent);
        //            var isSpam = GetTristate(probability);
        //            if (isSpam == true)
        //            {
        //                if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.JunkCertain.FolderPath)
        //                    await mailItem.TryMoveAsync(Globals.Ol.JunkCertain, 3);
        //                //mailItem.Move(Globals.Ol.JunkCertain);
        //            }
        //            else if (isSpam == false)
        //            {
        //                if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.Inbox.FolderPath)
        //                    await mailItem.TryMoveAsync(Globals.Ol.Inbox, 3);
        //                //mailItem.Move(Globals.Ol.Inbox);
        //            }
        //            else
        //            {
        //                if (((Folder)mailItem.Parent).FolderPath != Globals.Ol.JunkPossible.FolderPath)
        //                    await mailItem.TryMoveAsync(Globals.Ol.JunkPossible, 3);
        //                //mailItem.Move(Globals.Ol.JunkPossible);
        //            }
        //        }

        //    });

        //}
    }
}
