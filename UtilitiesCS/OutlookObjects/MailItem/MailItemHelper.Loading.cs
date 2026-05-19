using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.HelperClasses;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public partial class MailItemHelper
    {
        public static MailItemHelper FromDf(
            DataFrame df,
            long indexRow,
            IApplicationGlobals appGlobals,
            CancellationToken token = default
        )
        {
            var info = new MailItemHelper(df, indexRow, appGlobals.Ol.EmailPrefixToStrip);
            info.ResolveMail(appGlobals.Ol.NamespaceMAPI, strict: true);
            info.InitLazyFields(appGlobals);
            info.LoadPriorityForce();
            info.FolderInfo.OlRoot = ResolveFolderRoot(
                appGlobals,
                info.FolderInfo.OlFolder.FolderPath
            );
            return info;
        }

        public static async Task<MailItemHelper> FromDfAsync(
            DataFrame df,
            long indexRow,
            IApplicationGlobals appGlobals,
            CancellationToken token,
            bool background,
            bool resolveOnly
        )
        {
            token.ThrowIfCancellationRequested();

            var info = new MailItemHelper(df, indexRow, appGlobals.Ol.EmailPrefixToStrip);
            await info.ResolveMailAsync(appGlobals.Ol.NamespaceMAPI, token, background);
            info.InitLazyFields(appGlobals);

            if (!resolveOnly)
            {
                await info.FromDfAfterResolved();
            }

            return info;
        }

        public async Task<MailItemHelper> FromDfAfterResolved()
        {
            _token.ThrowIfCancellationRequested();
            var projectionStopwatch = Stopwatch.StartNew();
            LogMailItemTiming(
                "FromDfAfterResolved post-snapshot projection start | post-snapshot projection"
            );
            await Task.Run(LoadPriorityForce, _token);

            FolderInfo.OlRoot = ResolveFolderRoot(Globals, FolderInfo.OlFolder.FolderPath);

            _token.ThrowIfCancellationRequested();
            await Task.Run(
                () =>
                {
                    LoadRecipientsForce();
                    if (Html is not null) { }
                },
                _token
            );

            LogMailItemTiming(
                "FromDfAfterResolved post-snapshot projection complete | post-snapshot projection",
                $"elapsedMs={projectionStopwatch.ElapsedMilliseconds}"
            );

            return this;
        }

        public static async Task<MailItemHelper> FromDfAsync(
            DataFrame df,
            long indexRow,
            IApplicationGlobals appGlobals,
            CancellationToken token,
            bool background
        )
        {
            token.ThrowIfCancellationRequested();

            var info = new MailItemHelper(df, indexRow, appGlobals.Ol.EmailPrefixToStrip);
            await info.ResolveMailAsync(appGlobals.Ol.NamespaceMAPI, token, background);

            token.ThrowIfCancellationRequested();
            info.InitLazyFields(appGlobals);

            info.FolderInfo.OlRoot = ResolveFolderRoot(
                appGlobals,
                info.FolderInfo.OlFolder.FolderPath
            );

            token.ThrowIfCancellationRequested();
            await Task.Run(
                () =>
                {
                    info.LoadRecipientsForce();
                    if (info.Html is not null) { }
                },
                token
            );

            return info;
        }

        internal static Folder ResolveFolderRoot(IApplicationGlobals appGlobals, string folderPath)
        {
            if (folderPath.Contains(appGlobals.Ol.ArchiveRootPath))
            {
                return appGlobals.Ol.ArchiveRoot;
            }

            return appGlobals.Ol.Inbox;
        }

        public static Task<MailItemHelper> FromMailItemAsync(
            MailItem item,
            IApplicationGlobals appGlobals,
            CancellationToken token,
            bool loadAll
        )
        {
            token.ThrowIfCancellationRequested();
            item.ThrowIfNull();

            var materializationStopwatch = Stopwatch.StartNew();
            LogMailItemTiming(
                "[MailItem timing] FromMailItemAsync COM-backed materialization start | COM-backed materialization"
            );

            var info = new MailItemHelper(item, appGlobals);
            info.Sw = new SegmentStopWatch().Start();
            var materializedProjectionSnapshot = TryProjectMailItemMembers(item);
            LogMailItemTiming(
                "FromMailItemAsync tokenization dependency preparation start | tokenization dependency preparation",
                $"subject={materializedProjectionSnapshot.Subject}; entryId={materializedProjectionSnapshot.EntryId}"
            );
            info.MaterializeTokenizationDependencies();
            LogMailItemTiming(
                "FromMailItemAsync tokenization dependency preparation complete | tokenization dependency preparation",
                $"subject={materializedProjectionSnapshot.Subject}; entryId={materializedProjectionSnapshot.EntryId}; elapsedMs={materializationStopwatch.ElapsedMilliseconds}"
            );

            LogMailItemTiming(
                "FromMailItemAsync COM-backed materialization complete | COM-backed materialization",
                $"subject={materializedProjectionSnapshot.Subject}; entryId={materializedProjectionSnapshot.EntryId}; elapsedMs={materializationStopwatch.ElapsedMilliseconds}"
            );

            token.ThrowIfCancellationRequested();
            return Task.FromResult(info);
        }

        public MailItem ResolveMail(Outlook.NameSpace olNs, bool strict = false)
        {
            return Initializer.GetOrLoad(
                ref _item,
                () => (MailItem)olNs.GetItemFromID(EntryId, StoreId),
                strict,
                _entryId,
                _storeId
            );
        }

        public async Task<MailItem> ResolveMailAsync(
            Outlook.NameSpace olNs,
            CancellationToken token,
            bool background
        )
        {
            return await Task.Run(() => ResolveMail(olNs, strict: true), token);
        }

        public void LoadPriorityForce()
        {
            Item.ThrowIfNull();
            _ = new object[]
            {
                EntryId,
                Sender,
                SenderName,
                SenderHtml,
                Subject,
                Body,
                Categories,
                Triage,
                SentOn,
                Actionable,
                FolderInfo,
                FolderName,
                Globals,
                ConversationID,
            };
        }

        internal void MaterializeTokenizationDependencies()
        {
            _ = new object[]
            {
                InternetCodepage,
                Subject,
                Body,
                HTMLBody,
                Sender,
                ToRecipients,
                CcRecipients,
                AttachmentsInfo,
            };
        }

        public MailItemHelper LoadAll(
            IApplicationGlobals globals,
            Folder olRoot,
            bool loadTokens = false
        )
        {
            if (Item is null)
            {
                throw new ArgumentNullException();
            }
            InitLazyFields(globals);

            LoadPriorityForce();
            FolderInfo.OlRoot = olRoot;
            LoadRecipientsForce();
            if (Html is not null) { }
            if (loadTokens)
            {
                _ = Tokens;
            }
            return this;
        }

        public void LoadRecipientsForce()
        {
            _ = new string[]
            {
                ToRecipientsName,
                ToRecipientsHtml,
                CcRecipientsName,
                CcRecipientsHtml,
            };
            Sw?.LogDuration("LoadRecipientsForce");
        }

        public void LoadRecipients()
        {
            var recipients = Item.Recipients.Cast<Recipient>().ToArray();
            Sw?.LogDuration("Recipients -> Cast to array");
            ToRecipients = recipients
                .Where(x => x.Type == (int)OlMailRecipientType.olTo)
                .Select(x => x.GetInfo())
                .ToArray();

            ToRecipientsName = string.Join("; ", ToRecipients.Select(t => t.Name));
            ToRecipientsHtml = string.Join("; ", ToRecipients.Select(t => t.Html));
            CcRecipients = recipients
                .Where(x => x.Type == (int)OlMailRecipientType.olCC)
                .Select(x => x.GetInfo())
                .ToArray();

            CcRecipientsName = string.Join("; ", CcRecipients.Select(t => t.Name));
            CcRecipientsHtml = string.Join("; ", CcRecipients.Select(t => t.Html));

            Sw?.LogDuration("LoadRecipients");
        }

        internal void SetSender(IRecipientInfo sender)
        {
            _sender = sender.ToLazy();
            _senderName = sender.Name.ToLazy();
            _senderHtml = sender.Html.ToLazy();
        }
    }
}
