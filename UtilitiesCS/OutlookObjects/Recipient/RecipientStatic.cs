using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.Extensions;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.OutlookObjects.Store;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static class RecipientStatic
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private const int _numberOfFields = 13;

        private const string PR_SMTP_ADDRESS =
            "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

        public static Outlook.AddressList GetGlobalAddressList(
            this Outlook.Store store,
            Outlook.Application olApp
        )
        {
            string PR_EMSMDB_SECTION_UID = @"http://schemas.microsoft.com/mapi/proptag/0x3D150102";
            if (store == null)
            {
                throw new ArgumentNullException();
            }
            Outlook.PropertyAccessor oPAStore = store.PropertyAccessor;
            string storeUID = oPAStore.BinaryToString(oPAStore.GetProperty(PR_EMSMDB_SECTION_UID));
            foreach (Outlook.AddressList addrList in olApp.Session.AddressLists)
            {
                Outlook.PropertyAccessor oPAAddrList = addrList.PropertyAccessor;
                string addrListUID = oPAAddrList.BinaryToString(
                    oPAAddrList.GetProperty(PR_EMSMDB_SECTION_UID)
                );
                // Return addrList if match on storeUID
                // and type is olExchangeGlobalAddressList.
                if (
                    addrListUID == storeUID
                    && addrList.AddressListType
                        == Outlook.OlAddressListType.olExchangeGlobalAddressList
                )
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
            if (sender is null)
            {
                return olMail.SenderName ?? string.Empty;
            }

            // Prefer the Exchange directory name (first + last) over the mail-item SenderName
            // field, which can be stale or formatted differently from the directory entry.
            try
            {
                if (
                    sender.AddressEntryUserType == OlAddressEntryUserType.olExchangeUserAddressEntry
                    || sender.AddressEntryUserType
                        == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry
                )
                {
                    ExchangeUser exchUser = sender.GetExchangeUser();
                    if (exchUser != null)
                    {
                        return $"{exchUser.FirstName} {exchUser.LastName}";
                    }
                }
            }
            catch (System.Exception ex)
            {
                logger.Warn(
                    "GetSenderName (MailItem): Exchange directory lookup failed; falling back to display name.",
                    ex
                );
            }

            // AddressEntry.Name is more current than the stored SenderName field.
            return !sender.Name.IsNullOrEmpty() ? sender.Name : olMail.SenderName ?? string.Empty;
        }

        public static string GetSenderName(this MeetingItem olMeeting)
        {
            return olMeeting.SenderName ?? string.Empty;
        }

        public static string GetSenderAddress(this MeetingItem olMeeting)
        {
            var address = olMeeting.SenderEmailAddress ?? string.Empty;

            // Exchange DN addresses (X500 format) cannot be resolved to a usable SMTP address
            // without a session reference; fall back to the display name in those cases.
            if (address.IsNullOrEmpty() || address.StartsWith("/o=ExchangeLabs"))
            {
                var name = olMeeting.SenderName ?? string.Empty;
                return name.StartsWith("/o=ExchangeLabs") ? string.Empty : name;
            }

            return address;
        }

        public static string GetSenderAddress(this MailItem olMail)
        {
            AddressEntry sender = olMail.Sender;
            if (sender is null)
            {
                return olMail.SenderEmailAddress ?? string.Empty;
            }

            // Prefer the Exchange directory primary SMTP over the mail-item field, which
            // may contain a stale or X500 Exchange DN address.
            string address = null;
            try
            {
                if (
                    sender.AddressEntryUserType == OlAddressEntryUserType.olExchangeUserAddressEntry
                    || sender.AddressEntryUserType
                        == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry
                )
                {
                    ExchangeUser exchUser = sender.GetExchangeUser();
                    if (exchUser != null)
                    {
                        address = exchUser.PrimarySmtpAddress;
                    }
                }
            }
            catch (System.Exception ex)
            {
                logger.Warn(
                    "GetSenderAddress (MailItem): Exchange directory lookup failed; falling back.",
                    ex
                );
            }

            // Fall back to the mail-item address fields when Exchange lookup yields nothing.
            if (address.IsNullOrEmpty())
            {
                address = !olMail.SenderEmailAddress.IsNullOrEmpty()
                    ? olMail.SenderEmailAddress
                    : sender.Address;
            }

            // Last resort: query the MAPI property accessor, then use SenderName if still empty.
            if (address.IsNullOrEmpty())
            {
                var olPA = sender.PropertyAccessor;
                try
                {
                    address = (string)olPA.GetProperty(PR_SMTP_ADDRESS);
                    if (address.IsNullOrEmpty())
                        throw new InvalidOperationException("SMTP address is null or empty");
                }
                catch
                {
                    try
                    {
                        address = olMail.SenderName;
                        if (address.IsNullOrEmpty() || address.StartsWith("/o=ExchangeLabs"))
                            throw new InvalidOperationException(
                                "Sender address and name are null, empty, or malformed"
                            );
                    }
                    catch (System.Exception)
                    {
                        address = string.Empty;
                    }
                }
            }

            return address ?? string.Empty;
        }

        public static IRecipientInfo GetSenderInfo(this MeetingItem olMeeting)
        {
            olMeeting.ThrowIfNull();
            if (olMeeting.SenderName.IsNullOrEmpty())
            {
                return new RecipientInfo("", "", "");
            }
            else
            {
                var name = olMeeting.GetSenderName();
                var address = olMeeting.GetSenderAddress();
                var html = ConvertRecipientToHtml(name, address);
                return new RecipientInfo(name, address, html);
            }
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
                var pa = olMail.Sender.PropertyAccessor;
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
                if (recipient is not null)
                {
                    return recipient;
                }
                else
                {
                    var name = olMail.GetSenderName();
                    var address = olMail.GetSenderAddress();
                    var html = ConvertRecipientToHtml(name, address);
                    return new RecipientInfo(name, address, html);
                }
            }
        }

        public static (string recipientsTo, string recipientsCC) GetRecipients(
            this MailItem olMail,
            Outlook.NameSpace ns
        )
        {
            var olRecipients = olMail.Recipients;
            if (olRecipients is null)
            {
                return ("", "");
            }

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

        public static (string recipientsTo, string recipientsCC) GetRecipients(
            this MeetingItem olMeeting,
            Outlook.NameSpace ns
        )
        {
            var olRecipients = olMeeting.Recipients;
            if (olRecipients is null)
            {
                return ("", "");
            }

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
            if (olRecipients is null)
            {
                return ("", "");
            }

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

        public static (string recipientsTo, string recipientsCC) GetRecipients(
            this MeetingItem olMeeting
        )
        {
            var olRecipients = olMeeting.Recipients;
            if (olRecipients is null)
            {
                return ("", "");
            }

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

        public static IEnumerable<RecipientInfo> GetInfo(
            this IEnumerable<Recipient> recipients,
            StoresWrapper storesWrapper
        )
        {
            foreach (var recipient in recipients)
            {
                var name = GetRecipientName(recipient);
                var address = GetRecipientAddress(recipient);
                var html = ConvertRecipientToHtml(name, address);
                yield return new RecipientInfo(name, address, html);
            }
        }

        public static Recipient ToResolvedRecipient(
            this AddressEntry addressEntry,
            Outlook.NameSpace ns
        )
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
            return olMail
                .Recipients.Cast<Recipient>()
                .Where(r => r.Type == (int)OlMailRecipientType.olTo);
        }

        public static IEnumerable<Recipient> GetToRecipients(this MeetingItem olMeeting)
        {
            return olMeeting
                .Recipients.Cast<Recipient>()
                .Where(r => r.Type == (int)OlMailRecipientType.olTo);
        }

        public static IEnumerable<Recipient> GetCcRecipients(this MailItem olMail)
        {
            return olMail
                .Recipients.Cast<Recipient>()
                .Where(r => r.Type == (int)OlMailRecipientType.olCC);
        }

        public static IEnumerable<Recipient> GetCcRecipients(this MeetingItem olMeeting)
        {
            return olMeeting
                .Recipients.Cast<Recipient>()
                .Where(r => r.Type == (int)OlMailRecipientType.olCC);
        }

        private static string GetRecipientAddress(Recipient olRecipient)
        {
            string smtpAddress = string.Empty;

            try
            {
                var addressEntry = olRecipient.AddressEntry;
                if (IsExchangeAddressEntry(addressEntry))
                {
                    ExchangeUser exchUser = addressEntry.GetExchangeUser();
                    if (exchUser != null)
                    {
                        smtpAddress = exchUser.PrimarySmtpAddress;
                    }
                }
                else
                {
                    smtpAddress = olRecipient.Address;
                }
            }
            catch (System.Exception ex)
            {
                logger.Warn(
                    "GetRecipientAddress: Exchange directory lookup failed; falling back to recipient data.",
                    ex
                );
            }

            return smtpAddress.IsNullOrEmpty()
                ? GetRecipientFallbackAddress(olRecipient)
                : smtpAddress;
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

        internal static (
            string FirstName,
            string LastName,
            string DomainName
        ) ExtractNameFromAddress(string address)
        {
            var rx = new Regex(@"^(.+)@([^@]+)$");
            var match = rx.Match(address);
            if (!(match.Success && match.Groups.Count == 3))
                return (null, null, null);
            string domain = match.Groups[2].Value;
            string mailbox = match.Groups[1].Value;
            rx = new Regex(@"(?:^|\.)(?=[^""]|""?|)""?((?(1)[^""]*|[^\.""]*))""?(?=\.|$|@)");
            var nameParts = rx.Matches(mailbox)
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToArray();
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
            // Delegate to the robust helpers so Exchange directory lookup and PropertyAccessor
            // fallbacks are applied consistently — matching the GetInfo / GetRecipients paths.
            return (GetRecipientName(recipient), GetRecipientAddress(recipient));
        }

        private static string GetRecipientName(Recipient olRecipient)
        {
            try
            {
                var addressEntry = olRecipient.AddressEntry;
                if (IsExchangeAddressEntry(addressEntry))
                {
                    ExchangeUser exchUser = addressEntry.GetExchangeUser();
                    var exchangeDisplayName = GetExchangeUserDisplayName(exchUser);
                    if (!exchangeDisplayName.IsNullOrEmpty())
                    {
                        return exchangeDisplayName;
                    }
                }
            }
            catch (System.Exception ex)
            {
                logger.Warn(
                    "GetRecipientName: Exchange directory lookup failed; falling back to recipient data.",
                    ex
                );
            }

            return GetRecipientFallbackName(olRecipient);
        }

        private static string GetRecipientHtml(Recipient olRecipient)
        {
            return ConvertRecipientToHtml(
                GetRecipientName(olRecipient),
                GetRecipientAddress(olRecipient)
            );
        }

        private static bool IsExchangeAddressEntry(AddressEntry addressEntry)
        {
            return addressEntry is not null
                && (
                    addressEntry.AddressEntryUserType
                        == OlAddressEntryUserType.olExchangeUserAddressEntry
                    || addressEntry.AddressEntryUserType
                        == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry
                );
        }

        private static string GetExchangeUserDisplayName(ExchangeUser exchUser)
        {
            if (exchUser is null)
            {
                return string.Empty;
            }

            return $"{exchUser.FirstName} {exchUser.LastName}".Trim();
        }

        private static string GetRecipientFallbackName(Recipient olRecipient)
        {
            try
            {
                if (!olRecipient.Name.IsNullOrEmpty())
                {
                    return olRecipient.Name;
                }
            }
            catch (System.Exception ex)
            {
                logger.Warn(
                    "GetRecipientName: recipient display-name lookup failed; falling back to address data.",
                    ex
                );
            }

            return GetRecipientFallbackAddress(olRecipient);
        }

        private static string GetRecipientFallbackAddress(Recipient olRecipient)
        {
            string smtpAddress = string.Empty;

            try
            {
                smtpAddress = olRecipient.Address;
            }
            catch (System.Exception ex)
            {
                logger.Warn(
                    "GetRecipientAddress: recipient address lookup failed; falling back to property accessor.",
                    ex
                );
            }

            if (smtpAddress.IsNullOrEmpty())
            {
                try
                {
                    smtpAddress = (string)olRecipient.PropertyAccessor.GetProperty(PR_SMTP_ADDRESS);
                    if (smtpAddress.IsNullOrEmpty())
                    {
                        throw new InvalidOperationException("SMTP address is null or empty");
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Warn(
                        "GetRecipientAddress: property accessor lookup failed; falling back to recipient name.",
                        ex
                    );
                    smtpAddress = string.Empty;
                }
            }

            if (smtpAddress.IsNullOrEmpty())
            {
                try
                {
                    smtpAddress = olRecipient.Name;
                    if (smtpAddress.IsNullOrEmpty() || smtpAddress.StartsWith("/o=ExchangeLabs"))
                    {
                        throw new InvalidOperationException(
                            "SMTP address and name are null, empty, or malformed"
                        );
                    }
                }
                catch (System.Exception)
                {
                    smtpAddress = string.Empty;
                }
            }

            return smtpAddress;
        }
    }
}
