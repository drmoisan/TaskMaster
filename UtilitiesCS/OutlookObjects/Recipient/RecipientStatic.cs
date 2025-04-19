using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Extensions;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS
{
    public static class RecipientStatic
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int _numberOfFields = 13;

        private const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

        public static Outlook.AddressList GetGlobalAddressList(this Outlook.Store store, Outlook.Application olApp)
        {
            string PR_EMSMDB_SECTION_UID =
                @"http://schemas.microsoft.com/mapi/proptag/0x3D150102";
            if (store == null)
            {
                throw new ArgumentNullException();
            }
            Outlook.PropertyAccessor oPAStore = store.PropertyAccessor;
            string storeUID = oPAStore.BinaryToString(
                oPAStore.GetProperty(PR_EMSMDB_SECTION_UID));
            foreach (Outlook.AddressList addrList
                in olApp.Session.AddressLists)
            {
                Outlook.PropertyAccessor oPAAddrList =
                    addrList.PropertyAccessor;
                string addrListUID = oPAAddrList.BinaryToString(
                    oPAAddrList.GetProperty(PR_EMSMDB_SECTION_UID));
                // Return addrList if match on storeUID
                // and type is olExchangeGlobalAddressList.
                if (addrListUID == storeUID && addrList.AddressListType ==
                    Outlook.OlAddressListType.olExchangeGlobalAddressList)
                {
                    return addrList;
                }
            }
            return null;
        }

        public static string ConvertRecipientToHtml(string name, string address)
        {
            return $"{name} &lt;<a href=\"mailto:{address}\">{address}</a>&gt;";
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
            if (senderAddress.IsNullOrEmpty())
            {
                var olPA = sender.PropertyAccessor;
                try
                {
                    senderAddress = olPA.GetProperty(PR_SMTP_ADDRESS) as string;
                    if (senderAddress.IsNullOrEmpty())
                        throw new InvalidOperationException("Sender address is null or empty");
                }
                catch
                {
                    try
                    {
                        senderAddress = olMail.SenderName;
                        if (senderAddress.IsNullOrEmpty() || senderAddress.StartsWith("/o=ExchangeLabs"))
                            throw new InvalidOperationException("Sender address and name are null or empty");
                    }
                    catch
                    {
                        senderAddress = "";
                    }
                }
            }

            return senderAddress;
        }

        public static IRecipientInfo GetSenderInfo(this MailItem olMail)
        {
            olMail.ThrowIfNull();
            if (olMail.Sender is null)
            {
                return new RecipientInfo("", "", "");
            }
            else
            {
                var name = olMail.GetSenderName();
                var address = olMail.GetSenderAddress();
                var html = ConvertRecipientToHtml(name, address);
                return new RecipientInfo(name, address, html);
            }
        }

        public static IRecipientInfo GetSenderInfo(this MailItem olMail, Outlook.NameSpace ns)
        {
            olMail.ThrowIfNull();
            if (olMail.Sender is null)
            {
                return new RecipientInfo("", "", "");
            }
            else
            {                
                var recipient = olMail.Sender.ToResolvedRecipient(ns)?.GetInfo();
                if (recipient is not null) { return recipient; }
                else
                {
                    var name = olMail.GetSenderName();
                    var address = olMail.GetSenderAddress();
                    var html = ConvertRecipientToHtml(name, address);
                    return new RecipientInfo(name, address, html);
                }
            }
        }

        public static (string recipientsTo, string recipientsCC) GetRecipients(this MailItem olMail, Outlook.NameSpace ns)
        {
            var olRecipients = olMail.Recipients;
            if (olRecipients is null) { return ("", ""); }

            List<string> recipientsTo = [];
            List<string> recipientsCC = [];

            foreach (Recipient olRecipient in olRecipients)
            {
                var resolved = olRecipient.ToResolvedRecipient(ns);
                var smtpAddress = GetRecipientAddress(resolved);
                if (resolved.Type == (int)OlMailRecipientType.olTo)
                {
                    recipientsTo.Add(smtpAddress);
                }
                else if (resolved.Type == (int)OlMailRecipientType.olCC)
                {
                    recipientsCC.Add(smtpAddress);
                }
            }

            return (string.Join("; ", recipientsTo), string.Join("; ", recipientsCC));
        }



        public static (string recipientsTo, string recipientsCC) GetRecipients(this MailItem olMail)
        {
            var olRecipients = olMail.Recipients;
            if (olRecipients is null) { return ("", ""); }

            List<string> recipientsTo = [];
            List<string> recipientsCC = [];

            foreach (Recipient olRecipient in olRecipients)
            {
                
                var smtpAddress = GetRecipientAddress(olRecipient);
                if (olRecipient.Type == (int)OlMailRecipientType.olTo)
                {
                    recipientsTo.Add(smtpAddress);
                }
                else if (olRecipient.Type == (int)OlMailRecipientType.olCC)
                {
                    recipientsCC.Add(smtpAddress);
                }
            }

            return (string.Join("; ", recipientsTo), string.Join("; ", recipientsCC));
        }

        public static IEnumerable<RecipientInfo> GetInfo(this IEnumerable<Recipient> recipients, StoresWrapper storesWrapper)
        {
            foreach (var recipient in recipients)
            {
                var name = GetRecipientName(recipient);
                var address = GetRecipientAddress(recipient);
                var html = ConvertRecipientToHtml(name, address);
                yield return new RecipientInfo(name, address, html);
            }
        }

        public static Recipient ToResolvedRecipient(this AddressEntry addressEntry, Outlook.NameSpace ns)
        {
            var resolvedRecipient = ns.CreateRecipient(addressEntry.Name);
            if (resolvedRecipient.Resolve())
            {
                return resolvedRecipient;
            }
            else
            {
                return default;
            }
        }

        public static Recipient ToResolvedRecipient(this Recipient recipient, Outlook.NameSpace ns)
        {
            var resolvedRecipient = ns.CreateRecipient(recipient.Name);
            if (resolvedRecipient.Resolve())
            {
                return resolvedRecipient;
            }
            else
            {
                recipient.Resolve();
                return recipient;
            }
        }

        public static IEnumerable<RecipientInfo> GetInfo(this IEnumerable<Recipient> recipients)
        {            
            return recipients.Select(x => x.GetInfo());
        }

        public static RecipientInfo GetInfo(this Recipient recipient, SegmentStopWatch sw = null)
        {
            (var name, var address) = GetRecipientInfo(recipient);
            sw?.LogDuration("GetRecipientInfo");
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
            if (smtpAddress.IsNullOrEmpty())
            {
                var olPA = olRecipient.PropertyAccessor;
                try
                {
                    smtpAddress = (string)olPA.GetProperty(PR_SMTP_ADDRESS);
                    if (smtpAddress.IsNullOrEmpty())
                        throw new InvalidOperationException("SMTP address is null or empty");
                }
                catch
                {
                    try
                    {
                        smtpAddress = olRecipient.Name;
                        if (smtpAddress.IsNullOrEmpty() || smtpAddress.StartsWith("/o=ExchangeLabs"))
                            throw new InvalidOperationException("SMTP address and name are null, empty, or malformed");
                    }
                    catch (System.Exception)
                    {
                        smtpAddress = "";
                    }
                }
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
                return (null, null, null);
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

        internal static (string Name, string Address) GetRecipientInfo(Recipient recipient)
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
            else { recipientName = olRecipient.Name; }
            return recipientName;
        }

        private static string GetRecipientHtml(Recipient olRecipient)
        {
            return ConvertRecipientToHtml(
                GetRecipientName(olRecipient),
                GetRecipientAddress(olRecipient));
        }


    }
}
