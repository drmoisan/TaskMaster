using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.ReusableTypeClasses;
using System.Data;
using System.Reflection;
using System;
using System.Text.RegularExpressions;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS
{
    
    public static class EmailDetails
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int _numberOfFields = 13;
        
        private const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

        #region Public Methods and Extensions
        
        public static string ConvertRecipientToHtml(string name, string address)
        {
              return $"{name} &lt;<a href=\"mailto:{address}\">{address}</a>&gt;";
        }

        public static string[] Details(this MailItem OlMail, string emailRootFolder, IScoDictionary<string, string> dictRemap = null)
        {
            string[] strAry;

            strAry = new string[14];

            // Const PR_SMTP_ADDRESS As String =
            // "http://schemas.microsoft.com/mapi/proptag/0x39FE001E"

            strAry[1] = OlMail.GetTriage();
            strAry[2] = OlMail.GetEmailFolderPath(emailRootFolder, dictRemap);
            strAry[3] = OlMail.SentOn.ToString(@"yyyy-MM-dd\Th:mm:ss\+\0\0\:\0\0");

            var recipients = GetRecipients(OlMail);
            strAry[5] = recipients.recipientsTo;
            strAry[6] = recipients.recipientsCC;
            strAry[4] = GetSenderAddress(OlMail);
            strAry[7] = OlMail.Subject;
            strAry[8] = OlMail.Body;
            int idx = strAry[4]?.LastIndexOf("@") ?? -1;
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

        public static string GetSenderName(this MailItem olMail)
        {
            AddressEntry sender = olMail.Sender;
            string senderName = "";

            if (sender?.AddressEntryUserType == OlAddressEntryUserType.olExchangeUserAddressEntry || sender?.AddressEntryUserType == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry)
            {
                ExchangeUser exchUser = sender.GetExchangeUser();
                if (exchUser != null)
                {
                    senderName = $"{exchUser.FirstName} {exchUser.LastName}";
                }
                else
                {
                    senderName = sender.Name;
                }
            }
            else
            {
                senderName = olMail.SenderName;
            }
            return senderName;
            
        }

        public static string GetSenderAddress(this MailItem olMail)
        {
            AddressEntry sender = olMail.Sender;
            string senderAddress = "";

            if (sender?.AddressEntryUserType == OlAddressEntryUserType.olExchangeUserAddressEntry || sender?.AddressEntryUserType == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry)
            {
                ExchangeUser exchUser = sender.GetExchangeUser();
                if (exchUser != null)
                {
                    senderAddress = exchUser.PrimarySmtpAddress;
                }
                else
                {
                    senderAddress = sender.Address;
                }
            }
            else
            {
                senderAddress = olMail.SenderEmailAddress;
            }

            return senderAddress;

            //string senderAddress;

            //if (olMail.Sender.Type == "EX")
            //{
            //    var OlPA = olMail.Sender.PropertyAccessor;
            //    try
            //    {
            //        senderAddress =(string)OlPA.GetProperty(PR_SMTP_ADDRESS);
            //    }
            //    catch
            //    {
            //        try
            //        {
            //            senderAddress = olMail.Sender.Name;
            //        }
            //        catch
            //        {
            //            senderAddress = "";
            //        }
            //    }
            //}
            //else
            //{
            //    senderAddress = olMail.SenderEmailAddress;
            //}
            //return senderAddress;
        }

        public static IRecipientInfo GetSenderInfo(this MailItem olMail)
        {
            var name = olMail.GetSenderName();
            var address = olMail.GetSenderAddress();
            var html = ConvertRecipientToHtml(name, address);
            return new RecipientInfo(name, address, html);
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

        public static IEnumerable<RecipientInfo> GetInfo(this IEnumerable<Recipient> recipients)
        {
            return recipients.Select(x => x.GetInfo());
        }

        public static RecipientInfo GetInfo(this Recipient recipient, SegmentStopWatch sw = null)
        {
            (var name, var address) = GetRecipientInfo(recipient);
            sw?.LogDuration("GetRecipientInfo");
            //string name = GetRecipientName(recipient);
            //string address = GetRecipientAddress(recipient);
            string html = ConvertRecipientToHtml(name, address);
            sw?.LogDuration("ConvertRecipientToHtml");
            var ri = new RecipientInfo(name, address, html);
            sw?.LogDuration("New RecipientInfo");
            return ri;
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

        #endregion

        #region Private Helper Methods

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
        
        private static string GetEmailFolderPath(this MailItem OlMail, string emailRootFolder, IScoDictionary<string, string> dictRemap)
        {
            Folder OlParent = (Folder)OlMail.Parent;
            string folderPath = OlParent.FolderPath;
            int root_length = emailRootFolder.Length + 1;
            if (folderPath.Length > root_length)
            {
                folderPath = folderPath.Substring(root_length);

                // If folder has been remapped, put the target folder
                if (dictRemap is not null)
                {
                    if (dictRemap.ContainsKey(folderPath))
                    {
                        folderPath = dictRemap[folderPath];
                    }
                }
            }
            return folderPath;
        }

        private static string GetRecipientAddress(Recipient olRecipient)
        {
            string smtpAddress;

            if (olRecipient.AddressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangeUserAddressEntry || olRecipient.AddressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry)
            {
                ExchangeUser exchUser = olRecipient.AddressEntry.GetExchangeUser();
                if (exchUser != null)
                {
                    smtpAddress = exchUser.PrimarySmtpAddress;
                }
                else
                {
                    smtpAddress = olRecipient.Address;
                }
            }
            else
            {
                smtpAddress = olRecipient.Address;
            }
            return smtpAddress;
            //var OlPA = OlRecipient.PropertyAccessor;
            //string StrSMTPAddress;
            //try
            //{
            //    StrSMTPAddress = (string)OlPA.GetProperty(PR_SMTP_ADDRESS);
            //}
            //catch
            //{
            //    try
            //    {
            //        StrSMTPAddress = OlRecipient.Address;
            //    }
            //    catch
            //    {
            //        try
            //        {
            //            StrSMTPAddress = OlRecipient.Name;
            //        }
            //        catch
            //        {
            //            StrSMTPAddress = "";
            //        }
            //    }
            //}
            //return StrSMTPAddress;
        }
        
        internal static (string FirstName, string LastName, string DomainName) ExtractNameFromAddress(string address) 
        {
            var rx = new Regex(@"^(.+)@([^@]+)$");
            var match = rx.Match(address);
            if (!(match.Success && match.Groups.Count == 3))            
                return (null,null,null);
            string domain = match.Groups[2].Value;
            string mailbox = match.Groups[1].Value;
            rx = new Regex(@"(?:^|\.)(?=[^""]|""?|)""?((?(1)[^""]*|[^\.""]*))""?(?=\.|$|@)");
            var nameParts = rx.Matches(mailbox).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            //var nameParts = mailbox.Split('.');
            switch (nameParts.Length)
            {
                case 1:
                    return (nameParts[0], null, domain);
                case 2:
                    return (nameParts[0], nameParts[1], domain);
                default:
                    if (nameParts.Length - nameParts.Count(p => p.Length == 1) >= 2)
                        nameParts = nameParts.Where(p => p.Length > 1).ToArray();
                    return (nameParts[0], nameParts[1], domain);
            }
        }
        
        private static (string Name, string Address) GetRecipientInfo(Recipient recipient)
        {
            string name, address;
            if (recipient.AddressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangeUserAddressEntry || recipient.AddressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry)
            {
                ExchangeUser exchUser = recipient.AddressEntry.GetExchangeUser();
                if (exchUser != null)
                {
                    var firstNameExch = exchUser.FirstName;
                    address = exchUser.PrimarySmtpAddress;
                    var rx = new Regex(@"^(.+)@([^@]+)$");
                    name = $"{exchUser.FirstName} {exchUser.LastName}";
                }
                else
                {
                    name = recipient.Name;
                    address = recipient.Address;
                }
            }
            else
            {
                name = recipient.Name;
                address = recipient.Address;
            }
            
            return (name, address);
        }
        
        private static string GetRecipientName(Recipient olRecipient)
        {
            string recipientName;
            if (olRecipient.AddressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangeUserAddressEntry || olRecipient.AddressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry)
            {
                ExchangeUser exchUser = olRecipient.AddressEntry.GetExchangeUser();
                if (exchUser != null)
                {
                    recipientName = $"{exchUser.FirstName} {exchUser.LastName}";
                }
                else
                {
                    recipientName = olRecipient.Name;
                }
            }
            else { recipientName = olRecipient.Name;}
            return recipientName;
        }
        
        private static string GetRecipientHtml(Recipient olRecipient)
        {
            return ConvertRecipientToHtml(
                GetRecipientName(olRecipient), 
                GetRecipientAddress(olRecipient));
        }
                

        #endregion
    }
}