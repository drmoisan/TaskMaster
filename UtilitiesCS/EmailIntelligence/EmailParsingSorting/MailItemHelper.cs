using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.Extensions;
using UtilitiesCS.HelperClasses;
using Fizzler;

namespace UtilitiesCS //QuickFiler
{
    /// <summary>
    /// Class to cache information about a mail item.
    /// </summary>
    public class MailItemHelper : INotifyPropertyChanged, IItemInfo
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors, Initializers, and Destructors

        public MailItemHelper() 
        {
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
            _sender = new(() => _item.GetSenderInfo(), true);
            _senderHtml = new(() => Sender?.Html ?? "", true);
            _senderName = new(() => Sender?.Name ?? "", true);
            _actionable = new(() => _item.GetActionTaken(), true);
            _body = new(() => CompressPlainText(_item.Body, EmailPrefixToStrip), true);
            _conversationID = new(() => _item.ConversationID, true);
            _emailPrefixToStrip = new(() => Globals.Ol.EmailPrefixToStrip, true);
            _storeId = new(() => ((Folder)_item.Parent).StoreID, true);
            _folderName = new(() => ((Folder)_item.Parent).Name, true);
            _folderInfo = new(() => new OlFolderWrapper((Folder)Item.Parent, ResolveFolderRoot(globals, ((Folder)Item.Parent).FolderPath)));
            _htmlBody = new(() => _item.HTMLBody, true);
            _html = new(() => GetHtml(HTMLBody), true);
            _isTaskFlagSet = new(() => _item.FlagStatus == OlFlagStatus.olFlagMarked);
            _olRecipients = new(() => _item.Recipients?.Cast<Recipient>().ToArray(), true);
            _ccRecipients = new(() => OlRecipients?.Where(x => x.Type == (int)OlMailRecipientType.olCC).Select(x => x.GetInfo()).ToArray(), true);
            _toRecipients = new(() => OlRecipients?.Where(x => x.Type == (int)OlMailRecipientType.olTo).Select(x => x.GetInfo()).ToArray(), true);
            _toRecipientsName = new(() => string.Join("; ", ToRecipients?.Select(t => t.Name) ?? [""]), true);
            _toRecipientsHtml = new(() => string.Join("; ", ToRecipients?.Select(t => t.Html) ?? [""]), true);
            _ccRecipientsName = new(() => string.Join("; ", CcRecipients?.Select(t => t.Name) ?? [""]), true);
            _ccRecipientsHtml = new(() => string.Join("; ", CcRecipients?.Select(t => t.Html) ?? [""]), true);
            _sentDate = new(() => _item.SentOn, true);
            _sentOn = new(() => this.SentDate.ToString("g"), true);
            _subject = new(() => _item.Subject, true);
            _tokens = new(() => Tokenizer.Tokenize(this).ToArray(), true);
            _triage = new(() => _item.GetTriage(), true);
            _unread = new(() => _item.UnRead, true);
            _attachmentsHelper = new(() => _item.Attachments
                                                .Cast<Attachment>()
                                                .Select(x => new AttachmentHelper(x, SentDate, FolderName))
                                                .ToArray(), true);
            _attachmentsInfo = new(() => AttachmentsHelper?.Select(x => x.AttachmentInfo)?.ToArray());
            _internetCodepage = new(() => _item.InternetCodepage, true);
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

        public static MailItemHelper FromDf(DataFrame df, long indexRow, IApplicationGlobals appGlobals, CancellationToken token = default)
        {
            var info = new MailItemHelper(df, indexRow, appGlobals.Ol.EmailPrefixToStrip);
            info.ResolveMail(appGlobals.Ol.NamespaceMAPI, strict: true);
            info.InitLazyFields(appGlobals);
            //info.LoadPriority(appGlobals, token);
            info.LoadPriorityForce();
            info.FolderInfo.OlRoot = ResolveFolderRoot(appGlobals, info.FolderInfo.OlFolder.FolderPath);
            return info;
        }

        public static async Task<MailItemHelper> FromDfAsync(DataFrame df, long indexRow, IApplicationGlobals appGlobals, CancellationToken token, bool background, bool resolveOnly)
        {
            token.ThrowIfCancellationRequested();

            var info = new MailItemHelper(df, indexRow, appGlobals.Ol.EmailPrefixToStrip);
            await info.ResolveMailAsync(appGlobals.Ol.NamespaceMAPI, token, background);
            info.InitLazyFields(appGlobals);

            if (!resolveOnly) { await info.FromDfAfterResolved(); }

            return info;
        }

        public async Task<MailItemHelper> FromDfAfterResolved()
        {
            _token.ThrowIfCancellationRequested();
            //await Task.Run(() => LoadPriorityItems(Globals, _token), _token);
            await Task.Run(LoadPriorityForce, _token);

            FolderInfo.OlRoot = ResolveFolderRoot(Globals, FolderInfo.OlFolder.FolderPath);

            _token.ThrowIfCancellationRequested();
            await Task.Run(() =>
            {
                LoadRecipientsForce();
                if (Html is not null) { } // Force Html to evaluate //{ _html = GetHtml().ToLazy(); }
            }, _token);

            return this;
        }

        public static async Task<MailItemHelper> FromDfAsync(DataFrame df, long indexRow, IApplicationGlobals appGlobals, CancellationToken token, bool background)
        {
            token.ThrowIfCancellationRequested();

            var info = new MailItemHelper(df, indexRow, appGlobals.Ol.EmailPrefixToStrip);
            await info.ResolveMailAsync(appGlobals.Ol.NamespaceMAPI, token, background);

            token.ThrowIfCancellationRequested();
            //await Task.Run(() => info.LoadPriorityItems(appGlobals, token), token);
            info.InitLazyFields(appGlobals);


            info.FolderInfo.OlRoot = ResolveFolderRoot(appGlobals, info.FolderInfo.OlFolder.FolderPath);

            token.ThrowIfCancellationRequested();
            await Task.Run(() => 
            { 
                info.LoadRecipientsForce();
                if (info.Html is not null) { }// force eval //info._html = info.GetHtml().ToLazy(); }
            }, token);

            return info;
        }

        internal static Folder ResolveFolderRoot(IApplicationGlobals appGlobals, string folderPath)
        {
            if (folderPath.Contains(appGlobals.Ol.ArchiveRootPath))
            {
                return appGlobals.Ol.ArchiveRoot;
            }
            else
            {
                return appGlobals.Ol.EmailRoot;
            }
        }
        
        public static async Task<MailItemHelper> FromMailItemAsync(
            MailItem item,
            IApplicationGlobals appGlobals,
            CancellationToken token,
            bool loadAll)
        {
            //TraceUtility.LogMethodCall(item, emailPrefixToStrip,token,loadAll);
            
            token.ThrowIfCancellationRequested();
            item.ThrowIfNull();

            return await Task.Run(() =>
            {
                var info = new MailItemHelper(item, appGlobals);
                info.Sw = new SegmentStopWatch().Start();
                return info;
            }, token);
        }

        public MailItem ResolveMail(Outlook.NameSpace olNs, bool strict = false)
        {
            return Initializer.GetOrLoad(
                ref _item,
                () => (MailItem)olNs.GetItemFromID(EntryId, StoreId),
                strict,
                _entryId,
                _storeId);
        }

        public async Task<MailItem> ResolveMailAsync(Outlook.NameSpace olNs, CancellationToken token, bool background)
        {
            //TaskScheduler priority = background ? PriorityScheduler.BelowNormal : PriorityScheduler.AboveNormal;

            return await Task.Run(
                () => ResolveMail(olNs, strict: true),
                token);//,
                       //TaskCreationOptions.None,
                       //priority);
        }

        public void LoadPriorityForce()
        {
            Item.ThrowIfNull();
            _ = new object[] { EntryId, Sender, SenderName, SenderHtml, Subject, Body, Categories, 
                Triage, SentOn, Actionable, FolderInfo, FolderName, Globals, ConversationID };            
        }

        public MailItemHelper LoadAll(IApplicationGlobals globals, Folder olRoot, bool loadTokens = false)
        {
            if (Item is null) { throw new ArgumentNullException(); }
            InitLazyFields(globals);

            LoadPriorityForce();
            FolderInfo.OlRoot = olRoot;
            LoadRecipientsForce();
            if (Html is not null) { }//{ _html = GetHtml().ToLazy(); }
            //if (loadTokens) { LoadTokens(); }
            if (loadTokens) { _ = Tokens; }
            return this;
        }

        public void LoadRecipientsForce()
        {
            _ = new string[] { ToRecipientsName, ToRecipientsHtml, CcRecipientsName, CcRecipientsHtml };
            Sw?.LogDuration("LoadRecipientsForce");
        }

        public void LoadRecipients()
        {
            var recipients = Item.Recipients.Cast<Recipient>().ToArray();
            Sw?.LogDuration("Recipients -> Cast to array");
            ToRecipients = recipients.Where(x => x.Type == (int)OlMailRecipientType.olTo).Select(x => x.GetInfo()).ToArray();

            
            ToRecipientsName = string.Join("; ", ToRecipients.Select(t => t.Name));
            ToRecipientsHtml = string.Join("; ", ToRecipients.Select(t => t.Html));
            CcRecipients = recipients.Where(x => x.Type == (int)OlMailRecipientType.olCC).Select(x => x.GetInfo()).ToArray();
            
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

        #endregion

        #region Private variables and enums

        private Enums.ToggleState _darkMode = Enums.ToggleState.Off;
        private ThreadSafeSingleShotGuard _recipientsStarted = new();
        private CancellationToken _token;
        private readonly ThreadSafeSingleShotGuard _loadNotStarted = new();
        //private bool _completedLoadingPriority;
        public SegmentStopWatch Sw { get; set; }

        #endregion

        #region Public Properties

        private Lazy<string> _actionable;
        public string Actionable { get => _actionable?.Value; set => _actionable = value.ToLazy(); }

        private Lazy<string> _body;
        public string Body { get => _body?.Value; set => _body = value.ToLazy(); }

        private Lazy<string> _categories;
        public string Categories { get => _categories?.Value; set => _categories = value.ToLazy(); }

        private Lazy<string> _conversationID;
        public string ConversationID { get => _conversationID?.Value; set => _conversationID = value.ToLazy(); }

        private Lazy<string> _emailPrefixToStrip;
        public string EmailPrefixToStrip { get => _emailPrefixToStrip?.Value; internal set => _emailPrefixToStrip = value.ToLazy(); }

        //private string _entryId;
        //public string EntryId { get => PriorityInitialized(ref _entryId); set => _entryId = value; }
        
        //private Lazy<T> _entryId = new(() => { return default; }, true);
        private Lazy<string> _entryId; 
        public string EntryId { get => _entryId.Value; set => _entryId = value.ToLazy(); }

        private Lazy<IApplicationGlobals> _globals;
        [JsonIgnore]
        internal IApplicationGlobals Globals { get => _globals?.Value; set => _globals = value.ToLazy(); }
        
        private Lazy<string> _storeId;
        public string StoreId { get => _storeId.Value; set => _storeId = value.ToLazy(); }

        private Lazy<IFolderInfo> _folderInfo;
        public IFolderInfo FolderInfo { get => _folderInfo?.Value; set => _folderInfo = value.ToLazy(); }

        private Lazy<string> _folderName;
        public string FolderName { get => _folderName?.Value; set => _folderName = value.ToLazy(); }
        
        private MailItem _item;
        public virtual MailItem Item { get => _item; set => _item = value; }

        private IItemInfo.PlainTextOptionsEnum _plainTextOptions = IItemInfo.PlainTextOptionsEnum.StripAll;
        public virtual IItemInfo.PlainTextOptionsEnum PlainTextOptions { get => _plainTextOptions; set => _plainTextOptions = value; }

        private Lazy<string> _sentOn;
        public virtual string SentOn { get => _sentOn?.Value; set => _sentOn = value.ToLazy(); }

        private Lazy<string> _subject;
        public virtual string Subject { get => _subject?.Value; set => _subject = value.ToLazy(); }

        private Lazy<string> _senderHtml;
        public virtual string SenderHtml { get => _senderHtml?.Value; set => _senderHtml = value.ToLazy(); }

        private Lazy<string> _senderName;
        public virtual string SenderName { get => _senderName?.Value; set => _senderName = value.ToLazy(); }

        private Lazy<IRecipientInfo> _sender;
        public virtual IRecipientInfo Sender { get => _sender.Value; set => _sender = value.ToLazy(); }

        private LazyTry<Recipient[]> _olRecipients;
        internal virtual Recipient[] OlRecipients { get => _olRecipients.Value; set => _olRecipients = value.ToLazyTry(); }

        private Lazy<string> _ccRecipientsHtml;
        public virtual string CcRecipientsHtml
        {
            get => _ccRecipientsHtml.Value;
            set { _ccRecipientsHtml = value.ToLazy(); NotifyPropertyChanged(); }
        }

        private Lazy<string> _ccRecipientsName;
        public virtual string CcRecipientsName
        {
            get => _ccRecipientsName.Value;
            set { _ccRecipientsName = value.ToLazy(); NotifyPropertyChanged(); }
        }

        private Lazy<IRecipientInfo[]> _ccRecipients;
        public virtual IRecipientInfo[] CcRecipients
        {
            get => _ccRecipients.Value;
            protected set => _ccRecipients = value.ToLazy();
        }
        
        private Lazy<string> _toRecipientsHtml;
        public virtual string ToRecipientsHtml
        {
            get => _toRecipientsHtml.Value;
            set { _toRecipientsHtml = value.ToLazy(); NotifyPropertyChanged(); }
        }

        private Lazy<string> _toRecipientsName;
        public virtual string ToRecipientsName
        {
            get => _toRecipientsName.Value;
            set { _toRecipientsName = value.ToLazy(); NotifyPropertyChanged(); }
        }

        private Lazy<IRecipientInfo[]> _toRecipients;
        public virtual IRecipientInfo[] ToRecipients 
        { 
            get => _toRecipients.Value; 
            protected set => _toRecipients = value.ToLazy(); 
        }

        private Lazy<string> _triage;
        public virtual string Triage { get => _triage.Value; set => _triage = value.ToLazy(); }

        private Lazy<string> _html = null;
        public virtual string Html { get => _html.Value; private set => _html = value.ToLazy(); }

        private Lazy<string> _htmlBody;
        public virtual string HTMLBody { get => _htmlBody.Value; protected set => _htmlBody = value.ToLazy(); }

        private Lazy<DateTime> _sentDate;
        public virtual DateTime SentDate { get => _sentDate.Value; set => _sentDate = value.ToLazyValue(); }
        
        private Lazy<AttachmentHelper[]> _attachmentsHelper;
        public virtual AttachmentHelper[] AttachmentsHelper { get => _attachmentsHelper.Value; protected set => _attachmentsHelper = value.ToLazy(); }
        //{
        //    get => Initializer.GetOrLoad(ref _attachments, LoadAttachmentsInfo);
        //    private set => _attachments = value;
        //}

        internal AttachmentHelper[] LoadAttachmentsInfo()
        {
            var attachments = Item.Attachments
                                  .Cast<Attachment>()
                                  .Select(x => new AttachmentHelper(x, SentDate, FolderName, EmailPrefixToStrip))
                                  .ToArray();
            AttachmentsInfo = attachments.Select(x => x.AttachmentInfo).ToArray();
            return attachments;
        }

        private Lazy<IAttachment[]> _attachmentsInfo; 
        public IAttachment[] AttachmentsInfo { get => _attachmentsInfo?.Value; protected set => _attachmentsInfo = value.ToLazy(); }
        
        public string GetHeadersExtendedMapi()
        {
            return (string)Item.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x007D001F/");
        }

        public string[] Tokens { get => _tokens.Value; protected set => _tokens = value.ToLazy(); }
        private Lazy<string[]> _tokens;

        public async Task<IEnumerable<string>> TokenizeAsync()
        {
            Tokens = await Task.Run(() => Tokenizer.Tokenize(this).ToArray());
            Sw?.LogDuration("TokenizeAsync");
            return Tokens;
        }

        [JsonIgnore]
        public IEmailTokenizer Tokenizer { get => _tokenizer ??= new EmailTokenizer(); }
        private IEmailTokenizer _tokenizer;

        private Lazy<bool> _unread;
        public bool UnRead 
        { 
            get => _unread.Value;
            set 
            { 
                _unread = value.ToLazyValue(); 
                Item.UnRead = value;
                Item.Save();
            }
        }
        
        public int InternetCodepage
        {
            get => _internetCodepage.Value;
            set => _internetCodepage = value.ToLazyValue();
        }
        private Lazy<int> _internetCodepage;
        private int LoadInternetCodepage()
        {
            return _item.ThrowIfNull().InternetCodepage;
        }

        private Lazy<bool> _isTaskFlagSet;
        public bool IsTaskFlagSet { get => _isTaskFlagSet.Value; set => _isTaskFlagSet = value.ToLazyValue(); }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        #region HTML and Plain Text Methods

        internal static string CompressPlainText(string text, string emailPrefixToStrip)
        {
            return CompressPlainText(text ?? "", IItemInfo.PlainTextOptionsEnum.StripAll, emailPrefixToStrip ?? "");
        }

        internal static string CompressPlainText(string text, IItemInfo.PlainTextOptionsEnum options, string emailPrefixToStrip)
        {
            if (options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripWarning) && emailPrefixToStrip != "")
                text = text.Replace(emailPrefixToStrip, "");

            if (options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripLinks))
            {
                var replacementText = "";
                if (options.HasFlag(IItemInfo.PlainTextOptionsEnum.ShowStripped))
                    replacementText = "<link>";
                text = Regex.Replace(text, @"<https://[^>]+>", replacementText); //Strip links
            }

            if (options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripReplyHeader) ||
                options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripReplyBody))
            {
                var replacementText = "";
                if (options.HasFlag(IItemInfo.PlainTextOptionsEnum.ShowStripped | IItemInfo.PlainTextOptionsEnum.StripReplyHeader) &&
                    !options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripReplyBody))
                    replacementText = "<EOM> Chain: $3";
                else if (!options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripReplyHeader))
                    replacementText += "$1";
                else if (!options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripReplyBody))
                    replacementText += "$3";

                text = Regex.Replace(text, @"(From:([^\n]*\n){1,4}Subject: {0,1}[rR][eE]:.*)(.|\n|\r)*\z", replacementText); //Strip reply footer
            }

            if (options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripFormatting))
                text = Regex.Replace(text, @"[\s]", " ");
            text = Regex.Replace(text, @"[ ]{2,}", " ");
            text = text.Trim();
            text += " <EOM>";
            return text;
        }

        //text = Regex.Replace(text, @"From:([^\n]*\n){1,4}Subject: {0,1}[rR][eE]:(.|\n|\r)*\z", "");

        internal string EmailHeader2
        {
            get => //@"<div class=""WordSection1"">
@"
<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>From:<span style='mso-tab-count:1'> </span></span></b><span
style='color:black'>" + this.SenderName + @"<o:p></o:p></span></p>

<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>Sent:<span style='mso-tab-count:1'> </span></span></b><span
style='color:black'>" + this.SentOn + @"<o:p></o:p></span></p>

<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>To:<span style='mso-tab-count:1'> </span></span></b><span
style='color:black'>" + this.ToRecipientsName + @"<o:p></o:p></span></p>

<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>Subject:<span style='mso-tab-count:1'></span></span></b><span
style='color:black'>" + this.Subject + @"<o:p></o:p></span></p>

<p class=MsoNormal><o:p>&nbsp;</o:p></p>";
        }

#nullable enable
        private string? _emailHeader = null;
        internal string EmailHeader
        {
            get
            {
                if (_emailHeader is null)
                {
                    _emailHeader = @"
    <div>
		<div style=""font-family:Calibri,serif;border-right:none;border-bottom:1pt solid rgb(225,225,225);border-left:none;border-top:none;padding:3pt 0in 0in"">
			<p class=""MsoNormal"">
				<b>From:</b>" + this.SenderHtml + @"<br>
				<b>Sent:</b>" + this.SentOn + @"<br>
				<b>To:</b>" + this.ToRecipientsHtml + @"<br>
				<b>Cc:</b>" + this.CcRecipientsHtml + @"<br>
				<b>Subject:</b>" + this.Subject + @"
			</p>
		</div>
	</div>
";
                }
                return _emailHeader;
            }
        }       

#nullable disable

        internal string DarkModeHeader
        {
            get => @"
<style>
body { filter: invert(100%) }
* { backdrop-filter: invert(20%) }
img {
    -webkit-filter: invert(100%) !important;
    -moz-filter: invert(100%) !important;
    -o-filter: invert(100%) !important;
    -ms-filter: invert(100%) !important;
}
</style>
";
        }

        public string ToggleDark()
        {
            if (_darkMode == Enums.ToggleState.On)
            { return ToggleDark(Enums.ToggleState.Off); }
            else { return ToggleDark(Enums.ToggleState.On); }
        }

        public string ToggleDark(Enums.ToggleState desiredState)
        {
            if ((desiredState == Enums.ToggleState.On) && _darkMode == Enums.ToggleState.Off)
            {
                _darkMode = Enums.ToggleState.On;
                var regex = new Regex(@"(</head>)", RegexOptions.Multiline);
                Html = regex.Replace(Html, DarkModeHeader + "$1");
            }
            else if ((desiredState == Enums.ToggleState.Off) && _darkMode == Enums.ToggleState.On)
            {
                _darkMode = Enums.ToggleState.Off;
                var regex = new Regex(Regex.Escape(DarkModeHeader), RegexOptions.Multiline);
                Html = regex.Replace(Html, "");
            }
            return Html;
        }

        internal string GetHtml()
        {
            string body = _item.HTMLBody;
            var regex = new Regex(@"(<body[\S\s]*?>)", RegexOptions.Multiline);
            string revisedBody = regex.Replace(body, "$1" + EmailHeader);
            //string revisedBody = body.Replace(@"<div class=""WordSection1"">", EmailHeader);
            Sw?.LogDuration("GetHtml");
            return revisedBody;
        }

        internal string GetHtml(string htmlBody)
        {
            string body = _item.HTMLBody;
            var regex = new Regex(@"(<body[\S\s]*?>)", RegexOptions.Multiline);
            string revisedBody = regex.Replace(body, "$1" + EmailHeader);
            //string revisedBody = body.Replace(@"<div class=""WordSection1"">", EmailHeader);
            //Sw?.LogDuration("GetHtml");
            return revisedBody;
        }

        #endregion

        #region Serialization Conversion Methods

        public ItemInfo ToSerializableObject() 
        {
            return new ItemInfo(this);
        }

        public static MailItemHelper FromSerializableObject(ItemInfo itemInfo, Outlook.NameSpace olNs)
        {   
            var helper = new MailItemHelper(itemInfo);
            try
            {
                helper.ResolveMail(olNs, strict: true);
                helper.AttachmentsHelper = helper
                    .Item.Attachments
                    .Cast<Attachment>()
                    .Select(x => new AttachmentHelper(
                        x, helper.SentDate, helper.FolderName, helper.EmailPrefixToStrip))
                    .ToArray();
            }
            catch (System.Exception e)
            {
                var msg = $"Error in {nameof(MailItemHelper)}.{nameof(FromSerializableObject)}\n" +
                    $"{nameof(ItemInfo)} sent on {itemInfo.SentOn} from {itemInfo.Sender} in folder " +
                    $"{itemInfo.FolderName}. See exception message: \n{e.Message}";
                logger.Error(msg,e);
            }
            return helper;
        }

        #endregion Serialization Conversion Methods
    }
}
