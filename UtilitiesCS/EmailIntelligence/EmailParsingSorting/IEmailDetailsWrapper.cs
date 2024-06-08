using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.EmailIntelligence.EmailParsingSorting
{
    public interface IEmailDetailsWrapper
    {
        string[] Details(MailItem olMail, string emailRootFolder, IScoDictionary<string, string> dictRemap = null);
        string GetActionTaken(MailItem OlMail);
        IEnumerable<Recipient> GetCcRecipients(MailItem olMail);
        IEnumerable<RecipientInfo> GetInfo(IEnumerable<Recipient> recipients);
        RecipientInfo GetInfo(Recipient recipient, SegmentStopWatch sw = null);
        (string recipientsTo, string recipientsCC) GetRecipients(MailItem OlMail);
        string GetSenderAddress(MailItem olMail);
        IRecipientInfo GetSenderInfo(MailItem olMail);
        string GetSenderName(MailItem olMail);
        IEnumerable<Recipient> GetToRecipients(MailItem olMail);
        string GetTriage(MailItem OlMail);
    }
}