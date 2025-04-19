using Microsoft.Office.Interop.Outlook;
using System;
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
        public string FolderName { get; set; }
        public IFolderWrapper FolderInfo { get; set; }
        public string Html { get; set; }
        public string HTMLBody { get; set; }
        public int InternetCodepage { get; set; }
        public bool IsTaskFlagSet { get; set; }
        public SegmentStopWatch Sw { get; set;}
        
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
    }
}
