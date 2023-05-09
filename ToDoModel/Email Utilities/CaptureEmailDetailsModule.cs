using System.Collections.Generic;
using System.Globalization;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace ToDoModel
{

    public static class CaptureEmailDetailsModule
    {
        private const int NumberOfFields = 13;
        private readonly static Dictionary<string, string> dict_remap;
        private const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

        public static string[] CaptureEmailDetails(MailItem OlMail, string emailRootFolder, Dictionary<string, string> dictRemap = null)
        {
            string[] strAry;

            strAry = new string[14];

            // Const PR_SMTP_ADDRESS As String =
            // "http://schemas.microsoft.com/mapi/proptag/0x39FE001E"

            strAry[1] = GetTriage(OlMail);
            strAry[2] = GetEmailFolderPath(OlMail, emailRootFolder);
            strAry[3] = Strings.Format(OlMail.SentOn, @"yyyy-MM-dd\Th:mm:ss\+\0\0\:\0\0");

            var recipients = GetRecipients(OlMail);
            strAry[5] = recipients.recipientsTo;
            strAry[6] = recipients.recipientsCC;
            strAry[4] = GetSenderAddress(OlMail);
            strAry[7] = OlMail.Subject;
            strAry[8] = OlMail.Body;
            strAry[9] = Strings.Right(strAry[4], Strings.Len(strAry[4]) - Strings.InStr(strAry[4], "@"));
            strAry[10] = OlMail.ConversationID;
            strAry[11] = OlMail.EntryID;
            strAry[12] = GetAttachmentNames(OlMail);
            strAry[13] = GetActionTaken(OlMail);

            return strAry;

        }

        public static string GetActionTaken(MailItem OlMail)
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
                    int prop_tmp_int = Conversions.ToInteger(OlPA.GetProperty(PR_LAST_VERB_EXECUTED));
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

        private static string GetAttachmentNames(MailItem OlMail)
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
                if (Strings.Len(attachmentNames) > 2)
                {
                    attachmentNames = Strings.Right(attachmentNames, Strings.Len(attachmentNames) - 2);
                }
            }
            return attachmentNames;
        }

        // Private Function GetSenderAddress(OlMail As MailItem, PR_SMTP_ADDRESS As String) As String
        public static string GetSenderName(MailItem OlMail)
        {
            if (OlMail.Sent == false)
            {
                return "";
            }
            else if (OlMail.Sender.Type == "EX")
            {
                try
                {
                    var OlPA = OlMail.Sender.PropertyAccessor;
                    string senderAddress = Conversions.ToString(OlPA.GetProperty(PR_SMTP_ADDRESS));
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(senderAddress.Split('@')[0].Replace(".", " "));
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                return OlMail.Sender.Name;
            }

        }

        public static string GetSenderAddress(MailItem OlMail)
        {
            string senderAddress;

            if (OlMail.Sender.Type == "EX")
            {
                var OlPA = OlMail.Sender.PropertyAccessor;
                try
                {
                    senderAddress = Conversions.ToString(OlPA.GetProperty(PR_SMTP_ADDRESS));
                }
                catch
                {
                    try
                    {
                        senderAddress = OlMail.Sender.Name;
                    }
                    catch
                    {
                        senderAddress = "";
                    }
                }
            }
            else
            {
                senderAddress = OlMail.SenderEmailAddress;
            }
            return senderAddress;
        }

        private static string GetEmailFolderPath(MailItem OlMail, string emailRootFolder)
        {
            Folder OlParent = (Folder)OlMail.Parent;
            string folderPath = OlParent.FolderPath;
            int root_length = Strings.Len(emailRootFolder);
            if (Strings.Len(folderPath) > root_length)
            {
                folderPath = Strings.Right(folderPath, Strings.Len(folderPath) - root_length - 1);

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

        public static string GetTriage(MailItem OlMail)
        {
            var OlProperty = OlMail.UserProperties.Find("Triage");
            return OlProperty is null ? "" : (string)OlProperty;
        }

        public static (string recipientsTo, string recipientsCC) GetRecipients(MailItem OlMail)
        {

            string StrSMTPAddress;
            Recipients OlRecipients;
            string recipientsTo = "";
            string recipientsCC = "";

            OlRecipients = OlMail.Recipients;

            foreach (Recipient OlRecipient in OlRecipients)
            {
                StrSMTPAddress = ExtractRecipient(PR_SMTP_ADDRESS, OlRecipient);

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
            if (Strings.Len(recipientsCC) > 2)
                recipientsCC = Strings.Right(recipientsCC, Strings.Len(recipientsCC) - 2);
            if (Strings.Len(recipientsTo) > 2)
                recipientsTo = Strings.Right(recipientsTo, Strings.Len(recipientsTo) - 2);

            return (recipientsTo, recipientsCC);
        }

        private static string ExtractRecipient(string PR_SMTP_ADDRESS, Recipient OlRecipient)
        {
            var OlPA = OlRecipient.PropertyAccessor;
            string StrSMTPAddress;
            try
            {
                StrSMTPAddress = Conversions.ToString(OlPA.GetProperty(PR_SMTP_ADDRESS));
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