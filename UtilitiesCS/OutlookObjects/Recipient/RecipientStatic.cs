using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        private static string FirstNonEmptyValue(params Func<string>[] valueFactories)
        {
            foreach (var valueFactory in valueFactories)
            {
                var value = NormalizeRecipientValue(valueFactory(), rejectLegacyExchangeDn: false);
                if (!value.IsNullOrEmpty())
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static string FirstValidAddressValue(params Func<string>[] valueFactories)
        {
            foreach (var valueFactory in valueFactories)
            {
                var value = NormalizeRecipientValue(valueFactory(), rejectLegacyExchangeDn: true);
                if (!value.IsNullOrEmpty())
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static string NormalizeRecipientValue(string value, bool rejectLegacyExchangeDn)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (
                rejectLegacyExchangeDn
                && value.StartsWith("/o=ExchangeLabs", StringComparison.OrdinalIgnoreCase)
            )
            {
                return null;
            }

            return value;
        }

        private static T TryGetComValue<T>(Func<T> valueFactory, string context)
            where T : class
        {
            try
            {
                return valueFactory();
            }
            catch (COMException ex)
            {
                logger.Warn($"Failed to read {context}. {ex.Message}");
                return null;
            }
        }

        private static string TryGetPropertyValueAsString(
            Func<PropertyAccessor> propertyAccessorFactory,
            string propertyName,
            string context
        )
        {
            var propertyAccessor = TryGetComValue(
                propertyAccessorFactory,
                $"{context} property accessor"
            );
            if (propertyAccessor is null)
            {
                return null;
            }

            try
            {
                return propertyAccessor.GetProperty(propertyName) as string;
            }
            catch (COMException ex)
            {
                logger.Warn($"Failed to read {context} property {propertyName}. {ex.Message}");
                return null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        // Outlook can report an Exchange-flavored address entry type even when
        // GetExchangeUser or its directory-backed properties are unreadable.
        private static ExchangeUser TryGetExchangeUser(AddressEntry addressEntry, string context)
        {
            if (addressEntry is null)
            {
                return null;
            }

            return TryGetComValue(() => addressEntry.GetExchangeUser(), $"{context} exchange user");
        }

        private static string TryGetExchangeDisplayName(AddressEntry addressEntry, string context)
        {
            var exchangeUser = TryGetExchangeUser(addressEntry, context);
            if (exchangeUser is null)
            {
                return null;
            }

            var firstName = TryGetComValue(() => exchangeUser.FirstName, $"{context} first name");
            var lastName = TryGetComValue(() => exchangeUser.LastName, $"{context} last name");
            var nameParts = new[] { firstName, lastName }.Where(part =>
                !string.IsNullOrWhiteSpace(part)
            );

            return string.Join(" ", nameParts);
        }

        private static string TryGetExchangePrimarySmtpAddress(
            AddressEntry addressEntry,
            string context
        )
        {
            var exchangeUser = TryGetExchangeUser(addressEntry, context);
            if (exchangeUser is null)
            {
                return null;
            }

            return TryGetComValue(
                () => exchangeUser.PrimarySmtpAddress,
                $"{context} primary SMTP address"
            );
        }

        private static AddressEntry TryGetRecipientAddressEntry(Recipient recipient)
        {
            if (recipient is null)
            {
                return null;
            }

            try
            {
                if (!recipient.Resolved)
                {
                    recipient.Resolve();
                }

                return recipient.AddressEntry;
            }
            catch (COMException ex)
            {
                logger.Warn($"Failed to resolve recipient address entry. {ex.Message}");
                return null;
            }
        }

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
            var sender = TryGetComValue(() => olMail.Sender, "mail sender");

            return FirstNonEmptyValue(
                () => TryGetExchangeDisplayName(sender, "mail sender"),
                () => TryGetComValue(() => olMail.SenderName, "mail sender name"),
                () => TryGetComValue(() => sender?.Name, "mail sender display name")
            );
        }

        public static string GetSenderName(this MeetingItem olMeeting)
        {
            return olMeeting.SenderName;
        }

        public static string GetSenderAddress(this MeetingItem olMeeting)
        {
            return olMeeting.SenderEmailAddress;
        }

        public static string GetSenderAddress(this MailItem olMail)
        {
            var sender = TryGetComValue(() => olMail.Sender, "mail sender");

            return FirstValidAddressValue(
                () => TryGetExchangePrimarySmtpAddress(sender, "mail sender"),
                () => TryGetComValue(() => olMail.SenderEmailAddress, "mail sender email address"),
                () => TryGetComValue(() => sender?.Address, "mail sender address"),
                () =>
                    TryGetPropertyValueAsString(
                        () => sender?.PropertyAccessor,
                        PR_SMTP_ADDRESS,
                        "mail sender"
                    ),
                () => TryGetComValue(() => olMail.SenderName, "mail sender fallback name"),
                () => TryGetComValue(() => sender?.Name, "mail sender fallback display name")
            );
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
            var addressEntry = TryGetRecipientAddressEntry(olRecipient);

            return FirstValidAddressValue(
                () => TryGetExchangePrimarySmtpAddress(addressEntry, "recipient"),
                () => TryGetComValue(() => olRecipient?.Address, "recipient address"),
                () =>
                    TryGetPropertyValueAsString(
                        () => olRecipient?.PropertyAccessor,
                        PR_SMTP_ADDRESS,
                        "recipient"
                    ),
                () => TryGetComValue(() => olRecipient?.Name, "recipient fallback name")
            );
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
            if (recipient is null)
            {
                return (null, null);
            }

            return (GetRecipientName(recipient), GetRecipientAddress(recipient));
        }

        internal static (string Name, string Address) GetExchangeSenderInfo(AddressEntry sender)
        {
            if (sender is null)
            {
                return (null, null);
            }

            return (
                TryGetExchangeDisplayName(sender, "sender"),
                TryGetExchangePrimarySmtpAddress(sender, "sender")
            );
        }

        private static string GetRecipientName(Recipient olRecipient)
        {
            var addressEntry = TryGetRecipientAddressEntry(olRecipient);

            return FirstNonEmptyValue(
                () => TryGetExchangeDisplayName(addressEntry, "recipient"),
                () => TryGetComValue(() => olRecipient?.Name, "recipient name")
            );
        }

        private static string GetRecipientHtml(Recipient olRecipient)
        {
            return ConvertRecipientToHtml(
                GetRecipientName(olRecipient),
                GetRecipientAddress(olRecipient)
            );
        }

        internal static bool TryGetExchangeRecipientType(
            Outlook.Recipient recipient,
            out Outlook.OlAddressEntryUserType userType
        )
        {
            userType = default;

            if (recipient is null)
            {
                return false;
            }

            try
            {
                if (!recipient.Resolved && !recipient.Resolve())
                {
                    return false;
                }

                var addressEntry = recipient.AddressEntry;
                if (addressEntry is null)
                {
                    return false;
                }

                userType = addressEntry.AddressEntryUserType;
                return true;
            }
            catch (COMException)
            {
                return false;
            }
        }

        internal static bool TryGetExchangeAddressEntryType(
            Outlook.AddressEntry addressEntry,
            out Outlook.OlAddressEntryUserType userType
        )
        {
            userType = default;

            if (addressEntry is null)
            {
                return false;
            }

            try
            {
                userType = addressEntry.AddressEntryUserType;
                return true;
            }
            catch (COMException)
            {
                return false;
            }
        }
    }
}
