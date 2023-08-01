using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Linq;
using Microsoft.Office.Interop.Outlook;



namespace ToDoModel
{
    public class RecipientInfo
    {
        public RecipientInfo() { }
        
        public RecipientInfo(string name, string address, string html)
        {
            _name = name;
            _address = address;
            _html = html;
        }

        private string _name;
        private string _address;
        private string _html;

        public string Name { get => _name; set => _name = value; }
        public string Address { get => _address; set => _address = value; }
        public string Html { get => _html; set => _html = value; }
    }    

    public static class CaptureEmailDetailsModule
    {
        private const int NumberOfFields = 13;
        private readonly static Dictionary<string, string> dict_remap;
        private const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

        public static string[] Details(this MailItem OlMail, string emailRootFolder, Dictionary<string, string> dictRemap = null)
        {
            string[] strAry;

            strAry = new string[14];

            // Const PR_SMTP_ADDRESS As String =
            // "http://schemas.microsoft.com/mapi/proptag/0x39FE001E"

            strAry[1] = OlMail.GetTriage();
            strAry[2] = OlMail.GetEmailFolderPath(emailRootFolder);
            strAry[3] = OlMail.SentOn.ToString(@"yyyy-MM-dd\Th:mm:ss\+\0\0\:\0\0");

            var recipients = GetRecipients(OlMail);
            strAry[5] = recipients.recipientsTo;
            strAry[6] = recipients.recipientsCC;
            strAry[4] = GetSenderAddress(OlMail);
            strAry[7] = OlMail.Subject;
            strAry[8] = OlMail.Body;
            int idx = strAry[4].LastIndexOf("@");
            if (idx > -1) { strAry[9] = strAry[4].Substring(idx); }
            else { strAry[9] = strAry[4]; }
            strAry[10] = OlMail.ConversationID;
            strAry[11] = OlMail.EntryID;
            strAry[12] = GetAttachmentNames(OlMail);
            strAry[13] = GetActionTaken(OlMail);

            return strAry;

        }

        public static string GetActionTaken(this MailItem OlMail)
        {
            int lngLastVerbExec;
            const int Last_Verb_Reply_All = 103;
            const int Last_Verb_Reply_Sender = 102;
            const int Last_Verb_Reply_Forward = 104;
            const string PR_LAST_VERB_EXECUTED = "http://schemas.microsoft.com/mapi/proptag/0x10810003";
            string action;

            if (OlMail.IsMarkedAsTask == true)
            {
                action = "Task";
            }
            else
            {
                var OlPA = OlMail.PropertyAccessor;

                try
                {
                    int prop_tmp_int = (int)OlPA.GetProperty(PR_LAST_VERB_EXECUTED);
                    lngLastVerbExec = prop_tmp_int != 0 ? prop_tmp_int : 0;
                }
                catch
                {
                    lngLastVerbExec = 0;
                }

                switch (lngLastVerbExec)
                {
                    case Last_Verb_Reply_All:
                    case Last_Verb_Reply_Sender:
                    case Last_Verb_Reply_Forward:
                        {
                            action = "Acted";
                            break;
                        }

                    default:
                        {
                            action = "None";
                            break;
                        }
                }
            }

            return action;
        }

        private static string GetAttachmentNames(this MailItem OlMail)
        {
            int IntAttachment_Ct;
            string attachmentNames = "";

            IntAttachment_Ct = OlMail.Attachments.Count;
            if (IntAttachment_Ct > 0)
            {
                var OlAtmts = OlMail.Attachments;
                foreach (Attachment OlAtmt in OlAtmts)
                {
                    if (OlAtmt.Type != OlAttachmentType.olOLE)
                    {
                        attachmentNames = attachmentNames + "; " + OlAtmt.FileName;
                    }
                }
                if (attachmentNames.Length > 2)
                {
                    attachmentNames = attachmentNames.Substring(2);
                }
            }
            return attachmentNames;
        }

        // Private Function GetSenderAddress(OlMail As MailItem, PR_SMTP_ADDRESS As String) As String
        public static string GetSenderName(this MailItem olMail)
        {
            if (olMail.Sent == false)
            {
                return "";
            }
            else if (olMail.Sender.Type == "EX")
            {
                try
                {
                    var OlPA = olMail.Sender.PropertyAccessor;
                    string senderAddress = (string)OlPA.GetProperty(PR_SMTP_ADDRESS);
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(senderAddress.Split('@')[0].Replace(".", " "));
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                return olMail.Sender.Name;
            }

        }

        public static string GetSenderAddress(this MailItem olMail)
        {
            string senderAddress;

            if (olMail.Sender.Type == "EX")
            {
                var OlPA = olMail.Sender.PropertyAccessor;
                try
                {
                    senderAddress =(string)OlPA.GetProperty(PR_SMTP_ADDRESS);
                }
                catch
                {
                    try
                    {
                        senderAddress = olMail.Sender.Name;
                    }
                    catch
                    {
                        senderAddress = "";
                    }
                }
            }
            else
            {
                senderAddress = olMail.SenderEmailAddress;
            }
            return senderAddress;
        }

        public static RecipientInfo GetSenderInfo(this MailItem olMail)
        {
            var name = olMail.GetSenderName();
            var address = olMail.GetSenderAddress();
            var html = ConvertRecipientToHtml(name, address);
            return new RecipientInfo(name, address, html);
        }

        private static string GetEmailFolderPath(this MailItem OlMail, string emailRootFolder)
        {
            Folder OlParent = (Folder)OlMail.Parent;
            string folderPath = OlParent.FolderPath;
            int root_length = emailRootFolder.Length;
            if (folderPath.Length > root_length)
            {
                folderPath = folderPath.Substring(root_length);

                // If folder has been remapped, put the target folder
                if (dict_remap is not null)
                {
                    if (dict_remap.ContainsKey(folderPath))
                    {
                        folderPath = dict_remap[folderPath];
                    }
                }
            }
            return folderPath;
        }

        public static string GetTriage(this MailItem OlMail)
        {
            var OlProperty = OlMail.UserProperties.Find("Triage", true);
            return OlProperty is null ? "" : (string)OlProperty.Value;
        }

        public static (string recipientsTo, string recipientsCC) GetRecipients(this MailItem OlMail)
        {

            string StrSMTPAddress;
            Recipients OlRecipients;
            string recipientsTo = "";
            string recipientsCC = "";

            OlRecipients = OlMail.Recipients;

            foreach (Recipient OlRecipient in OlRecipients)
            {
                StrSMTPAddress = GetRecipientAddress(OlRecipient);

                if (OlRecipient.Type == (int)OlMailRecipientType.olTo)
                {
                    recipientsTo = recipientsTo + "; " + StrSMTPAddress;
                }
                else if (OlRecipient.Type == (int)OlMailRecipientType.olCC)
                {
                    recipientsCC = recipientsCC + "; " + StrSMTPAddress;
                }
            }

            // Trim off extra semicolon if any values were set
            if (recipientsCC.Length > 2)
                recipientsCC = recipientsCC.Substring(2);
            if (recipientsTo.Length > 2)
                recipientsTo = recipientsTo.Substring(2);

            return (recipientsTo, recipientsCC);
        }

        public static RecipientInfo GetInfo(this IEnumerable<Recipient> recipients)
        {
            var recipientTuples = recipients.Select(GetRecipientInfo);
            return new RecipientInfo(
                string.Join("; ", recipientTuples.Select(t => t.Name)),
                string.Join("; ", recipientTuples.Select(t => t.Address)),
                string.Join("; ", recipientTuples.Select(t => t.Html)));
        }

        public static string GetToRecipientsInHtml(MailItem olMail)
        {
            return string.Join("; ", GetToRecipients(olMail).Select(GetRecipientHtml));
        }
        
        public static IEnumerable<Recipient> GetToRecipients(this MailItem olMail)
        {
            return olMail.Recipients.Cast<Recipient>().Where(r => r.Type == (int)OlMailRecipientType.olTo);
        }
        
        public static IEnumerable<Recipient> GetCcRecipients(this MailItem olMail)
        {
            return olMail.Recipients.Cast<Recipient>().Where(r => r.Type == (int)OlMailRecipientType.olCC);
        }

        private static string GetRecipientHtml(Recipient olRecipient)
        {
            return ConvertRecipientToHtml(
                GetRecipientName(olRecipient), 
                GetRecipientAddress(olRecipient));
        }

        private static RecipientInfo GetRecipientInfo(Recipient olRecipient)
        {
            string name = GetRecipientName(olRecipient);
            string address = GetRecipientAddress(olRecipient);
            string html = ConvertRecipientToHtml(name, address);
            return new RecipientInfo(name, address, html);
        }

        public static string ConvertRecipientToHtml(string name, string address)
        {
              return $"{name} &lt;<a href=\"mailto:{address}\">{address}</a>&gt;";
        }
        
        

        private static string GetRecipientName(Recipient olRecipient)
        {
            return olRecipient.Name;
        }
        
        private static string GetRecipientAddress(Recipient OlRecipient)
        {
            var OlPA = OlRecipient.PropertyAccessor;
            string StrSMTPAddress;
            try
            {
                StrSMTPAddress = (string)OlPA.GetProperty(PR_SMTP_ADDRESS);
            }
            catch
            {
                try
                {
                    StrSMTPAddress = OlRecipient.Address;
                }
                catch
                {
                    try
                    {
                        StrSMTPAddress = OlRecipient.Name;
                    }
                    catch
                    {
                        StrSMTPAddress = "";
                    }
                }
            }
            return StrSMTPAddress;
        }
    }
}