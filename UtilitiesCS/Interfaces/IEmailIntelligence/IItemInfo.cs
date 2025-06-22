using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS;

public interface IItemInfo: IEquatable<ItemInfo>
{
    [Flags]
    public enum PlainTextOptionsEnum
    {
        Original = 0,
        ShowStripped = 1,
        StripWarning = 2,
        StripLinks = 4,
        StripFormatting = 8,
        StripReplyHeader = 16,
        StripReplyBody = 32,
        StripAllSilently = 62,
        StripAll = 63
    }

    string Actionable { get; set; }
    IAttachment[] AttachmentsInfo { get; }
    string Body { get; set; }
    string Categories { get; set; }
    string ConversationID { get; set; }
    string EmailPrefixToStrip { get; }
    string EntryId { get; set; }
    string StoreId { get; set; }
    string FolderName { get; set; }
    IFolderWrapper FolderInfo { get; set; }
    string Html { get; }
    string HTMLBody { get; }
    int InternetCodepage { get; }
    bool IsTaskFlagSet { get; set; }
    PlainTextOptionsEnum PlainTextOptions { get; set; }
    int Size { get; set; }
    IRecipientInfo Sender { get; set; }
    IRecipientInfo[] CcRecipients { get; }
    IRecipientInfo[] ToRecipients { get; }
    DateTime SentDate { get; set; }
    string SentOn { get; set; }

    string Subject { get; set; }
    string[] Tokens { get; }
    string Triage { get; set; }
    bool UnRead { get; set; }
    
    [JsonIgnore]
    SegmentStopWatch Sw { get; set; }
}