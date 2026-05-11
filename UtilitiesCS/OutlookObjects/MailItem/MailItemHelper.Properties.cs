using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;

namespace UtilitiesCS
{
    public partial class MailItemHelper
    {
        #region Public Properties

        private Lazy<string> _actionable;
        public string Actionable
        {
            get => _actionable?.Value ?? string.Empty;
            set => _actionable = value.ToLazy();
        }

        private Lazy<string> _body;
        public string Body
        {
            get => _body?.Value ?? string.Empty;
            set => _body = value.ToLazy();
        }

        private Lazy<string> _categories;
        public string Categories
        {
            get => _categories?.Value ?? string.Empty;
            set => _categories = value.ToLazy();
        }

        private Lazy<string> _conversationID;
        public string ConversationID
        {
            get => _conversationID?.Value ?? string.Empty;
            set => _conversationID = value.ToLazy();
        }

        private Lazy<string> _emailPrefixToStrip;
        public string EmailPrefixToStrip
        {
            get => _emailPrefixToStrip?.Value ?? string.Empty;
            internal set => _emailPrefixToStrip = value.ToLazy();
        }

        private Lazy<string> _entryId;
        public string EntryId
        {
            get => _entryId?.Value ?? string.Empty;
            set => _entryId = value.ToLazy();
        }

        private Lazy<IApplicationGlobals> _globals;

        [JsonIgnore]
        internal IApplicationGlobals Globals
        {
            get => _globals?.Value;
            set => _globals = value.ToLazy();
        }

        private Lazy<string> _storeId;
        public string StoreId
        {
            get => _storeId?.Value ?? string.Empty;
            set => _storeId = value.ToLazy();
        }

        private Lazy<IFolderWrapper> _folderInfo;
        public IFolderWrapper FolderInfo
        {
            get => _folderInfo?.Value;
            set => _folderInfo = value.ToLazy();
        }

        private Lazy<string> _folderName;
        public string FolderName
        {
            get => _folderName?.Value ?? string.Empty;
            set => _folderName = value.ToLazy();
        }

        private MailItem _item;
        public virtual MailItem Item
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _item;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set => _item = value;
        }

        private IItemInfo.PlainTextOptionsEnum _plainTextOptions = IItemInfo
            .PlainTextOptionsEnum
            .StripAll;
        public virtual IItemInfo.PlainTextOptionsEnum PlainTextOptions
        {
            get => _plainTextOptions;
            set => _plainTextOptions = value;
        }

        private Lazy<string> _sentOn;
        public virtual string SentOn
        {
            get => _sentOn?.Value ?? string.Empty;
            set => _sentOn = value.ToLazy();
        }

        private Lazy<string> _subject;
        public virtual string Subject
        {
            get => _subject?.Value ?? string.Empty;
            set => _subject = value.ToLazy();
        }

        private Lazy<string> _senderHtml;
        public virtual string SenderHtml
        {
            get => _senderHtml?.Value ?? string.Empty;
            set => _senderHtml = value.ToLazy();
        }

        private Lazy<string> _senderName;
        public virtual string SenderName
        {
            get => _senderName?.Value ?? string.Empty;
            set => _senderName = value.ToLazy();
        }

        private Lazy<IRecipientInfo> _sender;
        public virtual IRecipientInfo Sender
        {
            get => _sender?.Value;
            set => _sender = value.ToLazy();
        }

        private Lazy<int> _size;
        public virtual int Size
        {
            get => _size?.Value ?? 0;
            set => _size = value.ToLazyValue();
        }

        private LazyTry<Recipient[]> _olRecipients;
        internal virtual Recipient[] OlRecipients
        {
            get => _olRecipients?.Value ?? Array.Empty<Recipient>();
            set => _olRecipients = value.ToLazyTry();
        }

        private Lazy<string> _ccRecipientsHtml;
        public virtual string CcRecipientsHtml
        {
            get => _ccRecipientsHtml?.Value ?? string.Empty;
            set
            {
                _ccRecipientsHtml = value.ToLazy();
                NotifyPropertyChanged();
            }
        }

        private Lazy<string> _ccRecipientsName;
        public virtual string CcRecipientsName
        {
            get => _ccRecipientsName?.Value ?? string.Empty;
            set
            {
                _ccRecipientsName = value.ToLazy();
                NotifyPropertyChanged();
            }
        }

        private Lazy<IRecipientInfo[]> _ccRecipients;
        public virtual IRecipientInfo[] CcRecipients
        {
            get => _ccRecipients?.Value ?? Array.Empty<IRecipientInfo>();
            protected set => _ccRecipients = value.ToLazy();
        }

        private Lazy<string> _toRecipientsHtml;
        public virtual string ToRecipientsHtml
        {
            get => _toRecipientsHtml?.Value ?? string.Empty;
            set
            {
                _toRecipientsHtml = value.ToLazy();
                NotifyPropertyChanged();
            }
        }

        private Lazy<string> _toRecipientsName;
        public virtual string ToRecipientsName
        {
            get => _toRecipientsName?.Value ?? string.Empty;
            set
            {
                _toRecipientsName = value.ToLazy();
                NotifyPropertyChanged();
            }
        }

        private Lazy<IRecipientInfo[]> _toRecipients;
        public virtual IRecipientInfo[] ToRecipients
        {
            get => _toRecipients?.Value ?? Array.Empty<IRecipientInfo>();
            protected set => _toRecipients = value.ToLazy();
        }

        private Lazy<string> _triage;
        public virtual string Triage
        {
            get => _triage?.Value ?? string.Empty;
            set => _triage = value.ToLazy();
        }

        private Lazy<string> _html = null;
        public virtual string Html
        {
            get => _html?.Value ?? string.Empty;
            private set => _html = value.ToLazy();
        }

        private Lazy<string> _htmlBody;
        public virtual string HTMLBody
        {
            get => _htmlBody?.Value ?? string.Empty;
            protected set => _htmlBody = value.ToLazy();
        }

        private Lazy<DateTime> _sentDate;
        public virtual DateTime SentDate
        {
            get => _sentDate?.Value ?? default;
            set => _sentDate = value.ToLazyValue();
        }

        private Lazy<AttachmentHelper[]> _attachmentsHelper;
        public virtual AttachmentHelper[] AttachmentsHelper
        {
            get => _attachmentsHelper?.Value ?? Array.Empty<AttachmentHelper>();
            protected set => _attachmentsHelper = value.ToLazy();
        }

        internal AttachmentHelper[] LoadAttachmentsInfo()
        {
            var attachments = Item
                .Attachments.Cast<Attachment>()
                .Select(x => new AttachmentHelper(x, SentDate, FolderName, EmailPrefixToStrip))
                .ToArray();
            AttachmentsInfo = attachments.Select(x => x.AttachmentInfo).ToArray();
            return attachments;
        }

        private Lazy<IAttachment[]> _attachmentsInfo;
        public IAttachment[] AttachmentsInfo
        {
            get => _attachmentsInfo?.Value;
            protected set => _attachmentsInfo = value.ToLazy();
        }

        public string GetHeadersExtendedMapi()
        {
            return (string)
                Item.PropertyAccessor.GetProperty(
                    "http://schemas.microsoft.com/mapi/proptag/0x007D001F/"
                );
        }

        public string[] Tokens
        {
            get => _tokens?.Value ?? Array.Empty<string>();
            protected set => _tokens = value.ToLazy();
        }
        private Lazy<string[]> _tokens;

        public async Task<IEnumerable<string>> TokenizeAsync()
        {
            MaterializeTokenizationDependencies();
            Tokens = await Task.Run(() => Tokenizer.Tokenize(this).ToArray());
            Sw?.LogDuration("TokenizeAsync");
            return Tokens;
        }

        [JsonIgnore]
        public IEmailTokenizer Tokenizer
        {
            get => _tokenizer ??= new EmailTokenizer();
        }
        private IEmailTokenizer _tokenizer;

        private Lazy<bool> _unread;
        public bool UnRead
        {
            get => _unread?.Value ?? false;
            set
            {
                _unread = value.ToLazyValue();
                Item.UnRead = value;
                Item.Save();
            }
        }

        public int InternetCodepage
        {
            get => _internetCodepage?.Value ?? 0;
            set => _internetCodepage = value.ToLazyValue();
        }
        private Lazy<int> _internetCodepage;

        private int LoadInternetCodepage()
        {
            return _item.ThrowIfNull().InternetCodepage;
        }

        private Lazy<bool> _isTaskFlagSet;
        public bool IsTaskFlagSet
        {
            get => _isTaskFlagSet?.Value ?? false;
            set => _isTaskFlagSet = value.ToLazyValue();
        }

        #endregion
    }
}
