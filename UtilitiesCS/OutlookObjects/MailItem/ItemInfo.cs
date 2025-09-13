using Microsoft.Office.Interop.Outlook;
using System;
using System.Drawing;
using System.Linq;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.EmailIntelligence
{
    [Serializable]
    public class ItemInfo : IItemInfo
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ItemInfo() { }

        public ItemInfo(IItemInfo itemInfo)
        {
            Actionable = itemInfo.Actionable;
            AttachmentsInfo = itemInfo.AttachmentsInfo;
            Body = itemInfo.Body;
            Categories = itemInfo.Categories;
            ConversationID = itemInfo.ConversationID;
            EmailPrefixToStrip = itemInfo.EmailPrefixToStrip;
            EntryId = itemInfo.EntryId;
            StoreId = itemInfo.StoreId;
            FolderName = itemInfo.FolderName;
            FolderInfo = itemInfo.FolderInfo;
            Html = itemInfo.Html;
            HTMLBody = itemInfo.HTMLBody;
            InternetCodepage = itemInfo.InternetCodepage;
            IsTaskFlagSet = itemInfo.IsTaskFlagSet;
            PlainTextOptions = itemInfo.PlainTextOptions;
            Size = itemInfo.Size;
            Sender = itemInfo.Sender;
            CcRecipients = itemInfo.CcRecipients;
            ToRecipients = itemInfo.ToRecipients;
            SentDate = itemInfo.SentDate;
            SentOn = itemInfo.SentOn;
            Subject = itemInfo.Subject;
            Tokens = itemInfo.Tokens;
            Triage = itemInfo.Triage;
            UnRead = itemInfo.UnRead;
        }

        public string Actionable { get; set; }
        public IAttachment[] AttachmentsInfo { get; set; }
        public string Body { get; set; }
        public string Categories { get; set; }
        public string ConversationID { get; set; }
        public string EmailPrefixToStrip { get; set; }
        public string EntryId { get; set; }
        public string StoreId { get; set; }
        public int Size { get; set; }
        public string FolderName { get; set; }
        public IFolderWrapper FolderInfo { get; set; }
        public string Html { get; set; }
        public string HTMLBody { get; set; }
        public int InternetCodepage { get; set; }
        public bool IsTaskFlagSet { get; set; }
        public SegmentStopWatch Sw { get; set; }

        public IItemInfo.PlainTextOptionsEnum PlainTextOptions { get; set; }
        public IRecipientInfo Sender { get; set; }
        public IRecipientInfo[] CcRecipients { get; set; }
        public IRecipientInfo[] ToRecipients { get; set; }

        public DateTime SentDate { get; set; }
        public string SentOn { get; set; }
        public string Subject { get; set; }
        public string[] Tokens { get; set; }
        public string Triage { get; set; }
        public bool UnRead { get; set; }

        public bool Equals(IItemInfo other)
        {
            if (other is null) { return false; }
            else if (ReferenceEquals(this, other)) { return true; }
            else
            {
                //if (Size != other.Size) return false;
                if (SentDate != other.SentDate) return false;
                if (Subject != other.Subject) return false;
                if (Body != other.Body) return false;
                if (!Sender.Equals(other.Sender)) return false;
                if (!RecipientsEquivalent(CcRecipients, other.CcRecipients)) return false;
                if (!RecipientsEquivalent(ToRecipients, other.ToRecipients)) return false;
                return true;
            }
        }

        public override int GetHashCode()
        {
            // Use a simple hash code based on EntryId, StoreId, and Subject            
            //return Size.GetHashCode() + SentDate.GetHashCode() * 31 +
            return    (Subject ?? "").GetHashCode() * 31 * 31 +
                (Body ?? "").GetHashCode() * 31 * 31 * 31 +
                Sender.GetHashCode() * 31 * 31 * 31 * 31 +
                GetRecipientsHashCode(CcRecipients) * 31 *31*31*31*31 +
                GetRecipientsHashCode(ToRecipients) * 31 * 31 * 31 * 31 * 31 * 31;
        }

        internal bool RecipientsEquivalent(IRecipientInfo[] source, IRecipientInfo[] other)
        {
            if (source == null && other == null) return true;
            if (source == null || other == null) return false;
            if (source.Length != other.Length) return false;
            if (source.Intersect(other).Count() != other.Length) return false;
            return true;
        }

        internal int GetRecipientsHashCode(IRecipientInfo[] recipients)
        {
            if (recipients == null || recipients.Length == 0) return 0;
            int hash = 0;
            int i = 0;
            foreach (var recipient in recipients)
            {
                hash += (int)Math.Pow(recipient.GetHashCode(), i++);
            }
            return hash;
        }
    }
}
