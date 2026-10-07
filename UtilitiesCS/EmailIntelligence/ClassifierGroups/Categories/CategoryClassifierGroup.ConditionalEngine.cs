#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories
{
    public partial class CategoryClassifierGroup
    {
        #region IConditionalEngine Implementation

        public ISmartSerializableConfig Config => ClassifierGroup.Config;

        //public static async Task<IConditionalEngine<MailItemHelper>> CreateEngineAsync(IApplicationGlobals globals)
        //{
        //    var sb = await CreateAsync(globals);
        //    return sb;
        //}

        void IConditionalEngine<MailItemHelper>.Serialize()
        {
            this.ClassifierGroup.Serialize();
        }

        public Func<MailItemHelper, Task> AsyncAction =>
            (item) =>
                (Engine is not null && CategorySetter is not null)
                    ? ((CategoryClassifierGroup)Engine).TestAsync(item)
                    // Preserves the pre-existing null-Task return; null! keeps the non-null delegate type.
                    : null!;

        //public Func<MailItemHelper, Task> AsyncAction { get; set; }

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
            //if (mailItem.UserProperties.Find("Spam") is not null) { return false; }
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

            //var spamProp = olItem.UserProperties.Find("Spam");
            //if (spamProp is not null)
            //{
            //    logger.Debug($"Skipping: Has Spam property with value of {spamProp.Value} -> {GetOlItemString(olItem)}");
            //    return false;
            //}

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

        public object Engine => this;

        public Func<IApplicationGlobals, Task> EngineInitializer =>
            async (globals) => await Task.CompletedTask;

        public string EngineName { get; internal set; } = null!;

        public string Message => $"{nameof(CategoryClassifierGroup)} is null. Skipping actions";

        public MailItemHelper TypedItem { get; set; } = null!;

        #endregion IConditionalEngine Implementation
    }
}
