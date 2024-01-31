using System;

namespace UtilitiesCS.EmailIntelligence
{
    [Serializable]
    public class ItemInfo : IItemInfo
    {
        public ItemInfo() { }
        public ItemInfo(IItemInfo itemInfo)
        {
            Actionable = itemInfo.Actionable;
            AttachmentsInfo = itemInfo.AttachmentsInfo;
            Body = itemInfo.Body;
            ConversationIndex = itemInfo.ConversationIndex;
            EmailPrefixToStrip = itemInfo.EmailPrefixToStrip;
            EntryId = itemInfo.EntryId;
            StoreId = itemInfo.StoreId;
            FolderName = itemInfo.FolderName;
            Html = itemInfo.Html;
            IsTaskFlagSet = itemInfo.IsTaskFlagSet;
            PlainTextOptions = itemInfo.PlainTextOptions;
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
        public IAttachmentInfo[] AttachmentsInfo { get; set; }
        public string Body { get; set; }
        public string ConversationIndex { get; set; }
        public string EmailPrefixToStrip { get; set; }
        public string EntryId { get; set; }
        public string StoreId { get; set; }
        public string FolderName { get; set; }
        public string Html { get; set; }
        public bool IsTaskFlagSet { get; set; }
        
        public IItemInfo.PlainTextOptionsEnum PlainTextOptions { get; set; }
        public RecipientInfo Sender { get; set; }
        public RecipientInfo[] CcRecipients { get; set; }
        public RecipientInfo[] ToRecipients { get; set; }

        public DateTime SentDate { get; set; }
        public string SentOn { get; set; }
        public string Subject { get; set; }
        public string[] Tokens { get; set; }
        public string Triage { get; set; }
        public bool UnRead { get; set; }
    }
}
