using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Fizzler;
using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS //QuickFiler
{
    /// <summary>
    /// Class to cache information about a mail item.
    /// </summary>
    public partial class MailItemHelper : INotifyPropertyChanged, IItemInfo
    {
        internal readonly struct MailItemProjection
        {
            public MailItemProjection(string subject, string entryId)
            {
                Subject = subject ?? string.Empty;
                EntryId = entryId ?? string.Empty;
            }

            public string Subject { get; }
            public string EntryId { get; }
        }

        internal static MailItemProjection TryProjectMailItemMembers(object source)
        {
            if (source is null)
            {
                return new MailItemProjection(string.Empty, string.Empty);
            }

            var type = source.GetType();
            var subject = type.GetProperty("Subject")?.GetValue(source) as string;
            var entryId = type.GetProperty("EntryID")?.GetValue(source) as string;
            return new MailItemProjection(subject ?? string.Empty, entryId ?? string.Empty);
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private static string DescribeSynchronizationContext(SynchronizationContext syncContext)
        {
            return syncContext?.GetType().FullName ?? "null";
        }

        private static string BuildMailItemTimingContext()
        {
            return $"threadId={Thread.CurrentThread.ManagedThreadId}; syncContext={DescribeSynchronizationContext(SynchronizationContext.Current)}";
        }

        private static void LogMailItemTiming(string phase, string details = null)
        {
            var detailSegment = string.IsNullOrWhiteSpace(details) ? string.Empty : $" | {details}";
            var phaseLabel = phase.StartsWith("[MailItem timing]", StringComparison.Ordinal)
                ? phase
                : $"[MailItem timing] {phase}";
            logger.Debug($"{phaseLabel} | {BuildMailItemTimingContext()}{detailSegment}");
        }

        #region Constructors, Initializers, and Destructors

        public MailItemHelper()
        {
            InitializeSafeDefaults();
            _attachmentsInfo = new(() => AttachmentsHelper.Select(x => x.AttachmentInfo).ToArray());
        }

        public MailItemHelper(MailItem item, IApplicationGlobals globals)
        {
            _item = item;
            InitLazyFields(globals);
        }

        internal void InitLazyFields(IApplicationGlobals globals)
        {
            _globals = globals.ToLazy();
            _entryId = new(() => _item.EntryID, true);
            _categories = new(() => _item.Categories, true);
            _sender = new(() => _item.GetSenderInfo(), true);
            _senderHtml = new(() => Sender?.Html ?? "", true);
            _senderName = new(() => Sender?.Name ?? "", true);
            _actionable = new(() => _item.GetActionTaken(), true);
            _body = new(() => CompressPlainText(_item.Body, EmailPrefixToStrip), true);
            _conversationID = new(() => _item.ConversationID, true);
            _emailPrefixToStrip = new(() => Globals.Ol.EmailPrefixToStrip, true);
            _storeId = new(() => ((Folder)_item.Parent).StoreID, true);
            _folderName = new(() => ((Folder)_item.Parent).Name, true);
            _folderInfo = new(() =>
                new FolderWrapper(
                    (Folder)Item.Parent,
                    ResolveFolderRoot(globals, ((Folder)Item.Parent).FolderPath)
                )
            );
            _htmlBody = new(() => _item.HTMLBody, true);
            _html = new(() => GetHtml(HTMLBody), true);
            _isTaskFlagSet = new(() => _item.FlagStatus == OlFlagStatus.olFlagMarked);
            _olRecipients = new(() => _item.Recipients?.Cast<Recipient>().ToArray(), true);
            _ccRecipients = new(
                () =>
                    OlRecipients
                        ?.Where(x => x.Type == (int)OlMailRecipientType.olCC)
                        .Select(x => x.GetInfo())
                        .ToArray(),
                true
            );
            _toRecipients = new(
                () =>
                    OlRecipients
                        ?.Where(x => x.Type == (int)OlMailRecipientType.olTo)
                        .Select(x => x.GetInfo())
                        .ToArray(),
                true
            );
            _toRecipientsName = new(
                () => string.Join("; ", ToRecipients?.Select(t => t.Name) ?? [""]),
                true
            );
            _toRecipientsHtml = new(
                () => string.Join("; ", ToRecipients?.Select(t => t.Html) ?? [""]),
                true
            );
            _ccRecipientsName = new(
                () => string.Join("; ", CcRecipients?.Select(t => t.Name) ?? [""]),
                true
            );
            _ccRecipientsHtml = new(
                () => string.Join("; ", CcRecipients?.Select(t => t.Html) ?? [""]),
                true
            );
            _sentDate = new(() => _item.SentOn, true);
            _sentOn = new(() => this.SentDate.ToString("g"), true);
            _size = new(() => _item.Size, true);
            _subject = new(() => _item.Subject, true);
            _tokens = new(() => Tokenizer.Tokenize(this).ToArray(), true);
            _triage = new(() => _item.GetTriage(), true);
            _unread = new(() => _item.UnRead, true);
            _attachmentsHelper = new(
                () =>
                    _item
                        .Attachments.Cast<Attachment>()
                        .Select(x => new AttachmentHelper(x, SentDate, FolderName))
                        .ToArray(),
                true
            );
            _attachmentsInfo = new(() =>
                AttachmentsHelper?.Select(x => x.AttachmentInfo)?.ToArray()
            );
            _internetCodepage = new(() => _item.InternetCodepage, true);
        }

        private void InitializeSafeDefaults()
        {
            _actionable = string.Empty.ToLazy();
            _body = string.Empty.ToLazy();
            _categories = string.Empty.ToLazy();
            _conversationID = string.Empty.ToLazy();
            _emailPrefixToStrip = string.Empty.ToLazy();
            _entryId = string.Empty.ToLazy();
            _globals = new Lazy<IApplicationGlobals>(() => null, true);
            _storeId = string.Empty.ToLazy();
            _folderInfo = new Lazy<IFolderWrapper>(() => null, true);
            _folderName = string.Empty.ToLazy();
            _sentOn = string.Empty.ToLazy();
            _subject = string.Empty.ToLazy();
            _senderHtml = string.Empty.ToLazy();
            _senderName = string.Empty.ToLazy();
            _sender = new Lazy<IRecipientInfo>(() => null, true);
            _size = 0.ToLazyValue();
            _olRecipients = Array.Empty<Recipient>().ToLazyTry();
            _ccRecipientsHtml = string.Empty.ToLazy();
            _ccRecipientsName = string.Empty.ToLazy();
            _ccRecipients = Array.Empty<IRecipientInfo>().ToLazy();
            _toRecipientsHtml = string.Empty.ToLazy();
            _toRecipientsName = string.Empty.ToLazy();
            _toRecipients = Array.Empty<IRecipientInfo>().ToLazy();
            _triage = string.Empty.ToLazy();
            _html = string.Empty.ToLazy();
            _htmlBody = string.Empty.ToLazy();
            _sentDate = DateTime.MinValue.ToLazyValue();
            _attachmentsHelper = Array.Empty<AttachmentHelper>().ToLazy();
            _attachmentsInfo = Array.Empty<IAttachment>().ToLazy();
            _tokens = Array.Empty<string>().ToLazy();
            _unread = false.ToLazyValue();
            _internetCodepage = 0.ToLazyValue();
            _isTaskFlagSet = false.ToLazyValue();
        }

        public MailItemHelper(DataFrame df, long indexRow, string emailPrefixToStrip)
        {
            EntryId = (string)df["EntryID"][indexRow];
            StoreId = (string)df["Store"][indexRow];
        }

        protected MailItemHelper(IItemInfo itemInfo)
        {
            _actionable = itemInfo.Actionable.ToLazy();
            _body = itemInfo.Body.ToLazy();
            _conversationID = itemInfo.ConversationID.ToLazy();
            _emailPrefixToStrip = itemInfo.EmailPrefixToStrip.ToLazy();
            _entryId = itemInfo.EntryId.ToLazy();
            _storeId = itemInfo.StoreId.ToLazy();
            FolderName = itemInfo.FolderName;
            FolderInfo = itemInfo.FolderInfo;
            _html = itemInfo.Html.ToLazy();
            _isTaskFlagSet = itemInfo.IsTaskFlagSet.ToLazyValue();
            _plainTextOptions = itemInfo.PlainTextOptions;
            _sender = itemInfo.Sender.ToLazy();
            _ccRecipients = itemInfo.CcRecipients.ToLazy();
            _toRecipients = itemInfo.ToRecipients.ToLazy();
            _sentDate = itemInfo.SentDate.ToLazyValue();
            _sentOn = itemInfo.SentOn.ToLazy();
            _subject = itemInfo.Subject.ToLazy();
            _tokens = itemInfo.Tokens.ToLazy();
            _triage = itemInfo.Triage.ToLazy();
            _unread = itemInfo.UnRead.ToLazyValue();
            _attachmentsInfo = itemInfo.AttachmentsInfo.ToLazy();
        }

        #endregion

        #region Private variables and enums

        private Enums.ToggleState _darkMode = Enums.ToggleState.Off;
        private ThreadSafeSingleShotGuard _recipientsStarted = new();
        private CancellationToken _token;
        private readonly ThreadSafeSingleShotGuard _loadNotStarted = new();

        //private bool _completedLoadingPriority;
        public SegmentStopWatch Sw { get; set; }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
    }
}
