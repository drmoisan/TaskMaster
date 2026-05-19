using System;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.EmailIntelligence;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public partial class MailItemHelper
    {
        #region Serialization Conversion Methods

        public ItemInfo ToSerializableObject()
        {
            return new ItemInfo(this);
        }

        public ItemInfo ToMatchableObject()
        {
            var info = new ItemInfo();
            info.Size = Size;
            info.SentDate = SentDate;
            info.Subject = Subject;
            info.Body = Body;
            info.Sender = Sender;
            info.CcRecipients = CcRecipients;
            info.ToRecipients = ToRecipients;
            info.EntryId = EntryId;
            info.StoreId = StoreId;
            return info;
        }

        public static MailItemHelper FromSerializableObject(
            ItemInfo itemInfo,
            Outlook.NameSpace olNs
        )
        {
            var helper = new MailItemHelper(itemInfo);
            try
            {
                helper.ResolveMail(olNs, strict: true);
                helper.AttachmentsHelper = helper
                    .Item.Attachments.Cast<Attachment>()
                    .Select(x => new AttachmentHelper(
                        x,
                        helper.SentDate,
                        helper.FolderName,
                        helper.EmailPrefixToStrip
                    ))
                    .ToArray();
            }
            catch (System.Exception e)
            {
                var msg =
                    $"Error in {nameof(MailItemHelper)}.{nameof(FromSerializableObject)}\n"
                    + $"{nameof(ItemInfo)} sent on {itemInfo.SentOn} from {itemInfo.Sender} in folder "
                    + $"{itemInfo.FolderName}. See exception message: \n{e.Message}";
                logger.Error(msg, e);
            }
            return helper;
        }

        #endregion Serialization Conversion Methods

        #region IEquatable<ItemInfo> Implementation

        public bool Equals(IItemInfo other)
        {
            if (other is null)
            {
                return false;
            }
            else if (ReferenceEquals(this, other))
            {
                return true;
            }
            else
            {
                if (Size != other.Size)
                    return false;
                if (SentDate != other.SentDate)
                    return false;
                if (Subject != other.Subject)
                    return false;
                if (Body != other.Body)
                    return false;
                if (Sender != other.Sender)
                    return false;
                if (!RecipientsEquivalent(CcRecipients, other.CcRecipients))
                    return false;
                if (!RecipientsEquivalent(ToRecipients, other.ToRecipients))
                    return false;
                return true;
            }
        }

        internal bool RecipientsEquivalent(IRecipientInfo[] source, IRecipientInfo[] other)
        {
            if (source == null && other == null)
                return true;
            if (source == null || other == null)
                return false;
            if (source.Length != other.Length)
                return false;

            var matched = new bool[other.Length];
            foreach (var sourceRecipient in source)
            {
                var foundMatch = false;
                for (var i = 0; i < other.Length; i++)
                {
                    if (matched[i])
                    {
                        continue;
                    }

                    if (RecipientInfosMatch(sourceRecipient, other[i]))
                    {
                        matched[i] = true;
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool RecipientInfosMatch(IRecipientInfo left, IRecipientInfo right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            if (left.Equals(right) || right.Equals(left))
            {
                return true;
            }

            return string.Equals(
                    left.Name ?? string.Empty,
                    right.Name ?? string.Empty,
                    StringComparison.Ordinal
                )
                && string.Equals(
                    left.Address ?? string.Empty,
                    right.Address ?? string.Empty,
                    StringComparison.Ordinal
                )
                && string.Equals(
                    left.Html ?? string.Empty,
                    right.Html ?? string.Empty,
                    StringComparison.Ordinal
                );
        }

        #endregion IEquatable<ItemInfo> Implementation
    }
}
