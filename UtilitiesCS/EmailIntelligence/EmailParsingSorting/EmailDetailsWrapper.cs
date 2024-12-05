using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.EmailIntelligence.EmailParsingSorting
{
    public class EmailDetailsWrapper() : IEmailDetailsWrapper
    {
        public string[] Details(MailItem olMail, string emailRootFolder, IScoDictionary<string, string> dictRemap = null) => olMail.Details(emailRootFolder, dictRemap);
        public string GetActionTaken(MailItem OlMail) => OlMail.GetActionTaken();
        public string GetSenderName(MailItem olMail) => olMail.GetSenderName();
        public string GetSenderAddress(MailItem olMail) => olMail.GetSenderAddress();
        public IRecipientInfo GetSenderInfo(MailItem olMail) => olMail.GetSenderInfo();
        public string GetTriage(MailItem OlMail) => OlMail.GetTriage();
        public (string recipientsTo, string recipientsCC) GetRecipients(MailItem OlMail) => OlMail.GetRecipients();
        public IEnumerable<RecipientInfo> GetInfo(IEnumerable<Recipient> recipients) => recipients.GetInfo();
        public RecipientInfo GetInfo(Recipient recipient, SegmentStopWatch sw = null) => recipient.GetInfo(sw);
        public IEnumerable<Recipient> GetToRecipients(MailItem olMail) => olMail.GetToRecipients();
        public IEnumerable<Recipient> GetCcRecipients(MailItem olMail) => olMail.GetCcRecipients();
    }
}
