using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UtilitiesCS.Extensions;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    
    public static class EmailDetails
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int _numberOfFields = 13;
        
        private const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

        #region Public Methods and Extensions
        
        public static string[] Details(this MailItem OlMail, string emailRootFolder, IScoDictionary<string, string> dictRemap = null)
        {
            string[] strAry;

            strAry = new string[14];

            // Const PR_SMTP_ADDRESS As String =
            // "http://schemas.microsoft.com/mapi/proptag/0x39FE001E"

            strAry[1] = OlMail.GetTriage();
            strAry[2] = OlMail.GetEmailFolderPath(emailRootFolder, dictRemap);
            strAry[3] = OlMail.SentOn.ToString(@"yyyy-MM-dd\Th:mm:ss\+\0\0\:\0\0");

            var recipients = OlMail.GetRecipients();
            strAry[5] = recipients.recipientsTo;
            strAry[6] = recipients.recipientsCC;
            strAry[4] = OlMail.GetSenderAddress();
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

        public static string[] Details(this MailItemHelper helper, IScoDictionary<string, string> dictRemap = null)
        {
            string[] strAry;

            strAry = new string[14];

            // Const PR_SMTP_ADDRESS As String =
            // "http://schemas.microsoft.com/mapi/proptag/0x39FE001E"

            strAry[1] = helper.Triage;
            if (dictRemap is not null && dictRemap.TryGetValue(helper.FolderInfo.RelativePath, out string folderPath))
            {
                strAry[2] = folderPath;
            }
            else
            {
                strAry[2] = helper.FolderInfo.RelativePath;
            }
            strAry[3] = helper.SentOn; //OlMail.SentOn.ToString(@"yyyy-MM-dd\Th:mm:ss\+\0\0\:\0\0");

            //var recipients = OlMail.GetRecipients();
            strAry[5] = string.Join("; ", helper.ToRecipients.Select(x => x.Address));
            strAry[6] = string.Join("; ", helper.CcRecipients.Select(x => x.Address));
            strAry[4] = helper.Sender.Address;
            strAry[7] = helper.Subject;
            strAry[8] = helper.Body;
            int idx = strAry[4]?.LastIndexOf("@") ?? -1;
            if (idx > -1) { strAry[9] = strAry[4].Substring(idx); }
            else { strAry[9] = strAry[4]; }
            strAry[10] = helper.ConversationID;
            strAry[11] = helper.EntryId;            
            strAry[12] = string.Join("; ", helper.AttachmentsInfo.Select(x => x.FileName));
            try
            {
                strAry[13] = GetActionTaken(helper.Item);
            }
            catch (System.Exception)
            {
                strAry[13] = "None";
            }

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

        //public static string GetSenderName(this MailItem olMail)
        //{
        //    AddressEntry sender = olMail.Sender;
        //    string senderName = "";

        //    if (sender?.AddressEntryUserType == OlAddressEntryUserType.olExchangeUserAddressEntry || sender?.AddressEntryUserType == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry)
        //    {
        //        ExchangeUser exchUser = sender.GetExchangeUser();
        //        if (exchUser != null)
        //        {
        //            senderName = $"{exchUser.FirstName} {exchUser.LastName}";
        //        }
        //        else
        //        {
        //            senderName = sender.Name;
        //        }
        //    }
        //    else
        //    {
        //        senderName = olMail.SenderName;
        //    }
        //    return senderName;
            
        //}

        //public static string GetSenderAddress(this MailItem olMail)
        //{
        //    AddressEntry sender = olMail.Sender;
        //    string senderAddress = "";

        //    if (sender?.AddressEntryUserType == OlAddressEntryUserType.olExchangeUserAddressEntry || sender?.AddressEntryUserType == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry)
        //    {
        //        ExchangeUser exchUser = sender.GetExchangeUser();
        //        if (exchUser != null)
        //        {
        //            senderAddress = exchUser.PrimarySmtpAddress;
        //        }
        //        else
        //        {
        //            senderAddress = sender.Address;
        //        }
        //    }
        //    else
        //    {
        //        senderAddress = olMail.SenderEmailAddress;
        //    }
        //    if (senderAddress.IsNullOrEmpty())
        //    {
        //        var olPA = sender.PropertyAccessor;
        //        try
        //        {
        //            senderAddress = olPA.GetProperty(PR_SMTP_ADDRESS) as string;
        //            if (senderAddress.IsNullOrEmpty())
        //                throw new InvalidOperationException("Sender address is null or empty");
        //        }
        //        catch
        //        {
        //            try
        //            {
        //                senderAddress = olMail.SenderName;
        //                if (senderAddress.IsNullOrEmpty() || senderAddress.StartsWith("/o=ExchangeLabs"))
        //                    throw new InvalidOperationException("Sender address and name are null or empty");
        //            }
        //            catch
        //            {
        //                senderAddress = "";
        //            }
        //        }
        //    }

        //    return senderAddress;
        //}

        //public static IRecipientInfo GetSenderInfo(this MailItem olMail)
        //{
        //    olMail.ThrowIfNull();
        //    if (olMail.Sender is null)
        //    {
        //        return new RecipientInfo("", "", "");
        //    }
        //    else
        //    {
        //        var name = olMail.GetSenderName();
        //        var address = olMail.GetSenderAddress();
        //        var html = ConvertRecipientToHtml(name, address);
        //        return new RecipientInfo(name, address, html);
        //    }                
        //}

        public static string GetTriage(this MailItem olMail)
        {
            olMail.ThrowIfNull();
            var olProperty = olMail.UserProperties.Find("Triage", true);
            return olProperty is null ? "" : (string)olProperty.Value;
        }

        public static string GetTriage(this MeetingItem olMeeting)
        {
            olMeeting.ThrowIfNull();
            var olProperty = olMeeting.UserProperties.Find("Triage", true);
            return olProperty is null ? "" : (string)olProperty.Value;
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
                        

        #endregion
    }
}