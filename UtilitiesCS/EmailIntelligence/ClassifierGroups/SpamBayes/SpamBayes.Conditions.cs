#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.Extensions;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.EmailIntelligence
{
    public partial class SpamBayes
    {
        public Func<MailItemHelper, Task> AsyncAction =>
            // Preserves the pre-existing null-Task return when Engine is unset; null! keeps the
            // non-null delegate return type without changing behavior.
            (item) => Engine is not null ? ((SpamBayes)Engine).TestAsync(item) : null!;

        public Func<object, Task<bool>> AsyncCondition =>
            (item) => Task.Run(() => ConditionLog(item));

        private bool Condition(object item)
        {
            if (item is not MailItem mailItem)
            {
                return false;
            }
            if (mailItem.MessageClass != "IPM.Note")
            {
                return false;
            }
            if (mailItem.UserProperties.Find("Spam") is not null)
            {
                var autoCodeProp = mailItem.UserProperties.Find("AutoProcessed");
                if (autoCodeProp is not null)
                {
                    autoCodeProp.Value = true;
                    mailItem.Save();
                }
                return false;
            }

            return true;
        }

        private bool ConditionLog(object item)
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

            var spamProp = olItem.UserProperties.Find("Spam");
            if (spamProp is not null)
            {
                var autoCodeProp = olItem.UserProperties.Find("AutoProcessed");
                if (autoCodeProp is not null)
                {
                    autoCodeProp.Value = true;
                    olItem.Save();
                }
                else
                {
                    autoCodeProp = olItem.UserProperties.Add(
                        "AutoProcessed",
                        OlUserPropertyType.olYesNo,
                        true
                    );
                    autoCodeProp.Value = true;
                    olItem.Save();
                }
                logger.Debug(
                    $"Skipping: Has Spam property with value of {spamProp.Value} -> {GetOlItemString(olItem)}"
                );
                return false;
            }

            return true;
        }

        private string GetOlItemString(OutlookItem olItem)
        {
            var type = olItem.TryGet().OlItemType(out var typeVal)
                ? $"{typeVal}"
                : $"{olItem.InnerObject!.GetType()}";
            var created = olItem.TryGet().CreationTime(out var result)
                ? $" created on {result:g}"
                : "";
            var subject = olItem.Try().Subject;
            subject = subject.IsNullOrEmpty() ? "" : $" with subject {subject}";
            var sender = olItem.Try().SenderName;
            sender = sender.IsNullOrEmpty() ? "" : $" from {sender}";
            return $"{type}{created}{sender}{subject}";
        }
    }
}
