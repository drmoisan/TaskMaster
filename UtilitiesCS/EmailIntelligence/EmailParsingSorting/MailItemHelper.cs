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
            _globals = globals;
            _entryId = new(() => _item.EntryID, true);
            _sender = new(() => _item.GetSenderInfo(), true);

            _actionable = new(() => _item.GetActionTaken(), true);
            _body = new(() => _item.Body, true);
            _conversationID = new(() => _item.ConversationID, true);
            _emailPrefixToStrip = new(() => _globals.Ol.EmailPrefixToStrip, true);
            _storeId = new(() => ((Folder)_item.Parent).StoreID, true);
            _folderName = new(() => ((Folder)_item.Parent).Name, true);
            _folderInfo = new(() => new OlFolderInfo((Folder)Item.Parent, ResolveFolderRoot(globals, ((Folder)Item.Parent).FolderPath)));
            _html = new(() => GetHtml(_item.HTMLBody), true);
            _isTaskFlagSet = new(() => _item.FlagStatus == OlFlagStatus.olFlagMarked);
            _sender = new(() => _item.GetSenderInfo(), true);
            _olRecipients = new(() => _item.Recipients.Cast<Recipient>().ToArray(), true);
            _ccRecipients = new(() => OlRecipients.Where(x => x.Type == (int)OlMailRecipientType.olCC).Select(x => x.GetInfo()).ToArray(), true);
            _toRecipients = new(() => OlRecipients.Where(x => x.Type == (int)OlMailRecipientType.olTo).Select(x => x.GetInfo()).ToArray(), true);
            _toRecipientsName = new(() => string.Join("; ", ToRecipients.Select(t => t.Name)), true);
            _toRecipientsHtml = new(() => string.Join("; ", ToRecipients.Select(t => t.Html)), true);
            _ccRecipientsName = new(() => string.Join("; ", CcRecipients.Select(t => t.Name)), true);
            _ccRecipientsHtml = new(() => string.Join("; ", CcRecipients.Select(t => t.Html)), true);
            _sentDate = new(() => _item.SentOn, true);
            _sentOn = itemInfo.SentOn;
            _subject = itemInfo.Subject;
            _tokens = itemInfo.Tokens.ToLazy();
            _triage = itemInfo.Triage.ToLazy();
            _unread = itemInfo.UnRead;

            _attachmentsHelper = new(() => _item.Attachments
                                                .Cast<Attachment>()
                                                .Select(x => new AttachmentHelper(x, SentDate, FolderName, EmailPrefixToStrip))
                                                .ToArray(), true);
            _attachmentsInfo = new(() => AttachmentsHelper?.Select(x => x.AttachmentInfo)?.ToArray());
            

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
            _sentOn = itemInfo.SentOn;
            _subject = itemInfo.Subject;
            _tokens = itemInfo.Tokens.ToLazy();
            _triage = itemInfo.Triage.ToLazy();
            _unread = itemInfo.UnRead;
            
            _attachmentsInfo = itemInfo.AttachmentsInfo.ToLazy();
        }

        public static MailItemHelper FromDf(DataFrame df, long indexRow, IApplicationGlobals appGlobals, CancellationToken token = default)
        {
            var info = new MailItemHelper(df, indexRow, appGlobals.Ol.EmailPrefixToStrip);
            info.ResolveMail(appGlobals.Ol.NamespaceMAPI, strict: true);
            info.LoadPriority(appGlobals, token);
            info.FolderInfo.OlRoot = ResolveFolderRoot(appGlobals, info.FolderInfo.OlFolder.FolderPath);
            return info;
        }

        public static async Task<MailItemHelper> FromDfAsync(DataFrame df, long indexRow, IApplicationGlobals appGlobals, CancellationToken token, bool background, bool resolveOnly)
        {
            token.ThrowIfCancellationRequested();

            var info = new MailItemHelper(df, indexRow, appGlobals.Ol.EmailPrefixToStrip);
            await info.ResolveMailAsync(appGlobals.Ol.NamespaceMAPI, token, background);

            if (!resolveOnly) { await info.FromDfAfterResolved(); }

            return info;
        }

        public async Task<MailItemHelper> FromDfAfterResolved()
        {
            _token.ThrowIfCancellationRequested();
            await Task.Run(() => LoadPriorityItems(Globals, _token), _token);

            FolderInfo.OlRoot = ResolveFolderRoot(Globals, FolderInfo.OlFolder.FolderPath);

            _token.ThrowIfCancellationRequested();
            await Task.Run(() =>
            {
                LoadRecipients();
                if (_html is not null) { _html = GetHtml().ToLazy(); }
            }, _token);

            return this;
        }

        public static async Task<MailItemHelper> FromDfAsync(DataFrame df, long indexRow, IApplicationGlobals appGlobals, CancellationToken token, bool background)
        {
            token.ThrowIfCancellationRequested();

            var info = new MailItemHelper(df, indexRow, appGlobals.Ol.EmailPrefixToStrip);
            await info.ResolveMailAsync(appGlobals.Ol.NamespaceMAPI, token, background);

            token.ThrowIfCancellationRequested();
            await Task.Run(() => info.LoadPriorityItems(appGlobals, token), token);

            info.FolderInfo.OlRoot = ResolveFolderRoot(appGlobals, info.FolderInfo.OlFolder.FolderPath);

            token.ThrowIfCancellationRequested();
            await Task.Run(() => 
            { 
                info.LoadRecipients();
                if (info._html is not null) { info._html = info.GetHtml().ToLazy(); }
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

            var info = new MailItemHelper(item, appGlobals);
            info.Sw = new SegmentStopWatch().Start();
            

            await Task.Run(() => info.LoadPriorityItems(appGlobals, token), token);
            
            info.FolderInfo.OlRoot = ResolveFolderRoot(appGlobals, info.FolderInfo.OlFolder.FolderPath);

            var recipientTask = Task.Run(() => 
            { 
                info.LoadRecipients();
                if (info._html is not null) { info._html = info.GetHtml().ToLazy(); }
            }, token);
            if (loadAll) { await recipientTask; }
            
            return info;
        }

        public MailItem ResolveMail(Outlook.NameSpace olNs, bool strict = false)
        {
            return Initializer.GetOrLoad(
                ref _item,
                () => (MailItem)olNs.GetItemFromID(EntryId, _storeId),
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

        internal void LoadPriorityItems(IApplicationGlobals globals, CancellationToken token = default) 
        {
            if (Item is null) { throw new ArgumentNullException(); }
            EntryId = Item.EntryID;
            Sender = Item.GetSenderInfo();
            SenderName = Sender.Name;
            SenderHtml = Sender.Html;
            Subject = Item.Subject;
            Body = CompressPlainText(Item.Body, globals.Ol.EmailPrefixToStrip);
            Categories = Item.Categories;
            Triage = Item.GetTriage();
            SentOn = Item.SentOn.ToString("g");
            Actionable = Item.GetActionTaken();
            FolderInfo = new OlFolderInfo(
                (Outlook.Folder)Item.Parent, ResolveFolderRoot(globals, 
                ((Outlook.Folder)Item.Parent).FolderPath));
            FolderName = ((Folder)Item.Parent).Name;
            Globals = globals;
            ConversationID = Item.ConversationID;
            try
            {
                UnRead = Item.UnRead;
            }
            catch (System.Exception)
            {

            }
            
            IsTaskFlagSet = (Item.FlagStatus == OlFlagStatus.olFlagMarked);
            _token = token;
            Sw?.LogDuration("LoadPriorityItems");
        }

        public MailItemHelper LoadPriority(IApplicationGlobals globals, CancellationToken token = default)
        {
            if (!_completedLoadingPriority && _loadNotStarted.CheckAndSetFirstCall)
            {
                LoadPriorityItems(globals, token);
                _ = Task.Run(() => 
                { 
                    LoadRecipients();
                    if (_html is not null) { _html = GetHtml().ToLazy(); }
                }, token);
                _completedLoadingPriority = true;
                return this;
            }
            else
            {
                Task.Delay(100).Wait();
                return this;
            }
        }

        public MailItemHelper LoadAll(IApplicationGlobals globals, Folder olRoot, bool loadTokens = false)
        {
            if (Item is null) { throw new ArgumentNullException(); }
            LoadPriorityItems(globals, default);
            FolderInfo.OlRoot = olRoot;
            LoadRecipients();
            if (_html is not null) { _html = GetHtml().ToLazy(); }
            if (loadTokens) { LoadTokens(); }
            return this;
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

        internal void SetSender(RecipientInfo sender)
        {
            _sender = sender.ToLazy();
            _senderName = sender.Name;
            _senderHtml = sender.Html;
        }

        #endregion

        #region Private variables and enums

        private Enums.ToggleState _darkMode = Enums.ToggleState.Off;
        private ThreadSafeSingleShotGuard _recipientsStarted = new();
        private CancellationToken _token;
        private readonly ThreadSafeSingleShotGuard _loadNotStarted = new();
        private bool _completedLoadingPriority;
        public SegmentStopWatch Sw { get; set; }

        #endregion

        #region Public Properties

        private Lazy<string> _actionable;
        public string Actionable { get => _actionable.Value; set => _actionable = value.ToLazy(); }

        private Lazy<string> _body;
        public string Body { get => _body.Value; set => _body = value.ToLazy(); }

        private Lazy<string> _categories;
        public string Categories { get => _categories.Value; set => _categories = value.ToLazy(); }

        private Lazy<string> _conversationID;
        public string ConversationID { get => _conversationID.Value; set => _conversationID = value.ToLazy(); }

        private Lazy<string> _emailPrefixToStrip;
        public string EmailPrefixToStrip { get => _emailPrefixToStrip.Value; internal set => _emailPrefixToStrip = value.ToLazy(); }

        //private string _entryId;
        //public string EntryId { get => PriorityInitialized(ref _entryId); set => _entryId = value; }
        
        //private Lazy<T> _entryId = new(() => { return default; }, true);
        private Lazy<string> _entryId; 
        public string EntryId { get => _entryId.Value; set => _entryId = value.ToLazy(); }

        private IApplicationGlobals _globals;
        [JsonIgnore]
        internal IApplicationGlobals Globals 
        { 
            get => PriorityInitialized(ref _globals); 
            private set => _globals = value; 
        }

        private Lazy<string> _storeId;
        public string StoreId { get => _storeId.Value; set => _storeId = value.ToLazy(); }

        private Lazy<IFolderInfo> _folderInfo;
        public IFolderInfo FolderInfo { get => _folderInfo.Value; set => _folderInfo = value.ToLazy(); }

        private Lazy<string> _folderName;
        public string FolderName { get => _folderName.Value; set => _folderName = value.ToLazy(); }
        
        //private OlFolderInfo _folderInfo;

        private MailItem _item;
        public MailItem Item { get => _item; set => _item = value; }

        private IItemInfo.PlainTextOptionsEnum _plainTextOptions = IItemInfo.PlainTextOptionsEnum.StripAll;
        public IItemInfo.PlainTextOptionsEnum PlainTextOptions { get => _plainTextOptions; set => _plainTextOptions = value; }

        private string _sentOn;
        public string SentOn { get => PriorityInitialized(ref _sentOn); set => _sentOn = value; }

        private string _subject;
        public string Subject { get => PriorityInitialized(ref _subject); set => _subject = value; }

        private string _senderHtml;
        public string SenderHtml { get => PriorityInitialized(ref _senderHtml); set => _senderHtml = value; }

        private string _senderName;
        public string SenderName { get => PriorityInitialized(ref _senderName); set => _senderName = value; }

        private Lazy<RecipientInfo> _sender;
        public RecipientInfo Sender { get => _sender.Value; set => _sender = value.ToLazy(); }

        private Lazy<Recipient[]> _olRecipients;
        internal Recipient[] OlRecipients { get => _olRecipients.Value; set => _olRecipients = value.ToLazy(); }

        private Lazy<string> _ccRecipientsHtml;
        public string CcRecipientsHtml
        {
            get => _ccRecipientsHtml.Value;
            set { _ccRecipientsHtml = value.ToLazy(); NotifyPropertyChanged(); }
        }

        private Lazy<string> _ccRecipientsName;
        public string CcRecipientsName
        {
            get => _ccRecipientsName.Value;
            set { _ccRecipientsName = value.ToLazy(); NotifyPropertyChanged(); }
        }

        private Lazy<RecipientInfo[]> _ccRecipients;
        public RecipientInfo[] CcRecipients
        {
            get => _ccRecipients.Value;
            protected set => _ccRecipients = value.ToLazy();
        }
        
        private Lazy<string> _toRecipientsHtml;
        public string ToRecipientsHtml
        {
            get => _toRecipientsHtml.Value;
            set { _toRecipientsHtml = value.ToLazy(); NotifyPropertyChanged(); }
        }

        private Lazy<string> _toRecipientsName;
        public string ToRecipientsName
        {
            get => _toRecipientsName.Value;
            set { _toRecipientsName = value.ToLazy(); NotifyPropertyChanged(); }
        }

        private Lazy<RecipientInfo[]> _toRecipients;
        public RecipientInfo[] ToRecipients 
        { 
            get => _toRecipients.Value; 
            protected set => _toRecipients = value.ToLazy(); 
        }

        private Lazy<string> _triage;
        public string Triage { get => _triage.Value; set => _triage = value.ToLazy(); }

        private Lazy<string> _html = null;
        public string Html { get => _html.Value; private set => _html = value.ToLazy(); }

        private Lazy<string> _htmlBody;
        public string HTMLBody { get => _htmlBody.Value; protected set => _htmlBody = value.ToLazy(); }

        private Lazy<DateTime> _sentDate;
        public DateTime SentDate { get => _sentDate.Value; set => _sentDate = value.ToLazyValue(); }
        
        private Lazy<AttachmentHelper[]> _attachmentsHelper;
        public AttachmentHelper[] AttachmentsHelper { get => _attachmentsHelper.Value; protected set => _attachmentsHelper = value.ToLazy(); }
        //{
        //    get => Initializer.GetOrLoad(ref _attachments, LoadAttachmentsInfo);
        //    private set => _attachments = value;
        //}

        internal AttachmentHelper[] LoadAttachmentsInfo()
        {
            var attachments = Item.Attachments
                                  .Cast<Attachment>()
                                  .Select(x => new AttachmentHelper(x, SentDate, FolderName, _emailPrefixToStrip))
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
        //{
        //    get => Initializer.GetOrLoad(ref _tokens, LoadTokens);
        //    private set => _tokens = value;
        //}
        private Lazy<string[]> _tokens;
        
        public string[] LoadTokens()
        {
            _tokens = Tokenizer.tokenize(this).ToArray().ToLazy();
            return _tokens.Value;
        }
        public async Task<IEnumerable<string>> TokenizeAsync()
        {
            _tokens = await Task.Run(() => Tokenizer.tokenize(this).ToArray().ToLazy());
            Sw?.LogDuration("TokenizeAsync");
            return _tokens.Value;
        }

        [JsonIgnore]
        public EmailTokenizer Tokenizer { get => _tokenizer ??= new EmailTokenizer(); }
        private EmailTokenizer _tokenizer;

        private bool? _unread;
        public bool UnRead
        {
            get => (bool)Initializer.GetOrLoad(ref _unread, loader: () => _item.UnRead, strict: false, dependencies: _item)!;
            set => Initializer.SetAndSave(
                ref _unread, value, 
                (x) => _item.UnRead = x ?? false, 
                () => _item.Save(), null, false);
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

        #region Helper Methods

        internal T PriorityInitialized<T>(ref T variable)
        {
            if (variable is null) { LoadPriority(Globals); }
            return variable;
        }
        internal bool Initialized(ref bool? variable)
        {
            // check if one of the nullable variables is null which would indicate
            // the need to initialize
            if (variable is null) { LoadPriority(Globals); }
            return (bool)variable;
        }
        internal int Initialized(ref int? variable, Func<int> loader)
        {
            variable ??= loader();
            return (int)variable;
        }
        
        #endregion Helper Methods

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
