using Microsoft.Office.Interop.Outlook;
using System;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;

namespace QuickFiler
{
    public interface IMailItemInfo
    {
        string Actionable { get; set; }
        string Body { get; set; }
        string CcRecipientsHtml { get; set; }
        string CcRecipientsName { get; set; }
        string ConversationIndex { get; set; }
        string EntryId { get; set; }
        string Folder { get; set; }
        string Html { get; }
        bool IsTaskFlagSet { get; set; }
        MailItem Item { get; set; }
        MailItemInfo.PlainTextOptionsEnum PlainTextOptions { get; set; }
        string SenderHtml { get; set; }
        string SenderName { get; set; }
        DateTime SentDate { get; set; }
        string SentOn { get; set; }
        string Subject { get; set; }
        string ToRecipientsHtml { get; set; }
        string ToRecipientsName { get; set; }
        string Triage { get; set; }
        bool UnRead { get; set; }

        Task<bool> LoadAsync(NameSpace olNs, bool darkMode = false);
        bool LoadPriority();
        void LoadRecipients();
        Task<MailItem> ResolveMail(NameSpace olNs, CancellationToken token);
        string ToggleDark();
        string ToggleDark(Enums.ToggleState desiredState);
    }
}