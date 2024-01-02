using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace UtilitiesCS //QuickFiler
{
    /// <summary>
    /// Class to cache information about a mail item.
    /// </summary>
    public class MailItemInfo: INotifyPropertyChanged 
    {
        #region Constructors, Initializers, and Destructors

        public MailItemInfo() { }

        public MailItemInfo(MailItem item)
        {
            _item = item;
        }

        public MailItemInfo(DataFrame df, long indexRow, string emailPrefixToStrip)
        {
            _entryId = (string)df["EntryID"][indexRow];
            _storeId = (string)df["Store"][indexRow];
            _senderName = (string)df["SenderName"][indexRow];
            _sender = new RecipientInfo() { Name = _senderName, Address = (string)df["SenderSmtpAddress"][indexRow] };
            _folder = (string)df["Folder Name"][indexRow];
            _emailPrefixToStrip = emailPrefixToStrip;
            DateTime.TryParse((string)df["SentOn"][indexRow], out _sentDate);
            _conversationIndex = (string)df["ConversationIndex"][indexRow];
        }

        public static MailItemInfo FromDf(DataFrame df, long indexRow, Outlook.NameSpace olNs, string emailPrefixToStrip, CancellationToken token = default)
        {
            var info = new MailItemInfo(df, indexRow, emailPrefixToStrip);
            info.ResolveMail(olNs, strict: true);
            info.LoadPriority(emailPrefixToStrip, token);
            return info;
        }

        public static async Task<MailItemInfo> FromDfAsync(DataFrame df, long indexRow, Outlook.NameSpace olNs, string emailPrefixToStrip, CancellationToken token, bool background)
        {
            token.ThrowIfCancellationRequested();

            //TaskScheduler priority = background ? PriorityScheduler.BelowNormal : PriorityScheduler.AboveNormal;

            var info = new MailItemInfo(df, indexRow, emailPrefixToStrip);
            await info.ResolveMailAsync(olNs, token, background);

            token.ThrowIfCancellationRequested();
            await Task.Factory.StartNew(
                () =>
                {
                    info.Subject = info.Item.Subject;
                    info.Body = CompressPlainText(info.Item.Body, emailPrefixToStrip);
                    info.Triage = info.Item.GetTriage();
                    info.SentOn = info.Item.SentOn.ToString("g");
                    info.Actionable = info.Item.GetActionTaken();
                    info.ConversationIndex = info.Item.ConversationIndex;
                    info.UnRead = info.Item.UnRead;
                    info.IsTaskFlagSet = (info.Item.FlagStatus == OlFlagStatus.olFlagMarked || info.Item.FlagStatus == OlFlagStatus.olFlagComplete);
                    info.LoadRecipients();
                },
                token);//,
                //TaskCreationOptions.None,
                //priority);

            return info;
        }

        public static async Task<MailItemInfo> FromMailItemAsync(
            MailItem item, 
            string emailPrefixToStrip, 
            CancellationToken token, 
            bool loadAll)
        {
            //TraceUtility.LogMethodCall(item, emailPrefixToStrip,token,loadAll);

            token.ThrowIfCancellationRequested();

            var info = new MailItemInfo(item);
            if (item is null) { throw new ArgumentNullException(); }
            info.EntryId = item.EntryID;
            info.SetSender(item.GetSenderInfo());
            info.Subject = item.Subject;
            info.Body = CompressPlainText(item.Body, emailPrefixToStrip);
            info.Triage = item.GetTriage();
            info.SentOn = item.SentOn.ToString("g");
            info.Actionable = item.GetActionTaken();
            info.Folder = ((Folder)item.Parent).Name;
            info.ConversationIndex = item.ConversationIndex;
            info.UnRead = item.UnRead;
            info.IsTaskFlagSet = (item.FlagStatus == OlFlagStatus.olFlagMarked || item.FlagStatus == OlFlagStatus.olFlagComplete);
            var recipientTask = Task.Factory.StartNew(() => info.LoadRecipients(),
                                                      token,
                                                      TaskCreationOptions.LongRunning,
                                                      TaskScheduler.Default);
            if (loadAll) { await recipientTask; }
                                              
            return info;
        }

        public MailItem ResolveMail(Outlook.NameSpace olNs, bool strict = false)
        {
            return Initializer.GetOrLoad(
                ref _item, 
                () => (MailItem)olNs.GetItemFromID(_entryId, _storeId), 
                strict, 
                _entryId, 
                _storeId);
        }

        public async Task<MailItem> ResolveMailAsync(Outlook.NameSpace olNs, CancellationToken token, bool background)
        {
            //TaskScheduler priority = background ? PriorityScheduler.BelowNormal : PriorityScheduler.AboveNormal;

            return await Task.Factory.StartNew(
                () => ResolveMail(olNs, strict: true),
                token);//,
                //TaskCreationOptions.None,
                //priority);
        }

        async public Task<bool> LoadAsync(Outlook.NameSpace olNs, bool darkMode = false)
        {
            _item = await Task.FromResult((MailItem)olNs.GetItemFromID(_entryId, _storeId));
            _sender.Html = EmailDetails.ConvertRecipientToHtml(_sender.Address, _sender.Name);
            _senderHtml = _sender.Html;
            LoadRecipients();
            _html = GetHtml();
            if (darkMode) { _html = ToggleDark(Enums.ToggleState.On); }
            _triage = _item.GetTriage();
            _sentOn = _sentDate.ToString("g");
            _actionable = _item.GetActionTaken();

            return true;
        }

        public void LoadPriority(string emailPrefixToStrip, CancellationToken token = default)
        {
            if (!_completedLoadingPriority && _loadNotStarted.CheckAndSetFirstCall)
            {
                if (_item is null) { throw new ArgumentNullException(); }
                _entryId = _item.EntryID;
                _sender = _item.GetSenderInfo();
                _senderName = _sender.Name;
                _senderHtml = _sender.Html;
                _subject = _item.Subject;
                _body = CompressPlainText(_item.Body, emailPrefixToStrip);
                _triage = _item.GetTriage();
                _sentOn = _item.SentOn.ToString("g");
                _actionable = _item.GetActionTaken();
                _folder = ((Folder)_item.Parent).Name;
                _conversationIndex = _item.ConversationIndex;
                _unread = _item.UnRead;
                _isTaskFlagSet = (_item.FlagStatus == OlFlagStatus.olFlagMarked);
                _token = token;
                // RecipientsTask = Task.Factory.StartNew(() => LoadRecipients(), token);
                _ = Task.Factory.StartNew(() => LoadRecipients(), token);
                _completedLoadingPriority = true;
            }
            else 
            { 
                Task.Delay(100).Wait(); 
            }
        }

        public void LoadAll(string emailPrefixToStrip)
        {
            if (_item is null) { throw new ArgumentNullException(); }
            _entryId = _item.EntryID;
            _sender = _item.GetSenderInfo();
            _senderName = _sender.Name;
            _senderHtml = _sender.Html;
            _subject = _item.Subject;
            _body = CompressPlainText(_item.Body, emailPrefixToStrip);
            _triage = _item.GetTriage();
            _sentOn = _item.SentOn.ToString("g");
            _actionable = _item.GetActionTaken();
            _folder = ((Folder)_item.Parent).Name;
            _conversationIndex = _item.ConversationIndex;
            _unread = _item.UnRead;
            _isTaskFlagSet = (_item.FlagStatus == OlFlagStatus.olFlagMarked);
            LoadRecipients();
        }

        public void LoadRecipients()
        {
            RecipientsLoaded = Enums.LoadState.Loading;
            _toRecipients = _item.GetToRecipients().GetInfo().ToArray();
            _toRecipientsName = string.Join("; ",_toRecipients.Select(t => t.Name));
            _toRecipientsHtml = string.Join("; ", _toRecipients.Select(t => t.Html));
            _ccRecipients = _item.GetCcRecipients().GetInfo().ToArray();
            _ccRecipientsName = string.Join("; ", _ccRecipients.Select(t => t.Name));
            _ccRecipientsHtml = string.Join("; ", _ccRecipients.Select(t => t.Html));
            RecipientsLoaded = Enums.LoadState.Loaded;
            if (_html is not null) { _html = GetHtml(); }
        }

        internal void SetSender(RecipientInfo sender)
        {
            _sender = sender;
            _senderName = sender.Name;
            _senderHtml = sender.Html;
        }
                
        #endregion

        #region Private variables and enums
        
        private string _storeId;
        private Enums.ToggleState _darkMode = Enums.ToggleState.Off;
        private ThreadSafeSingleShotGuard _recipientsStarted = new();
        private CancellationToken _token;
        private readonly ThreadSafeSingleShotGuard _loadNotStarted = new();
        private bool _completedLoadingPriority;

        #endregion

        #region Public Properties

        internal T RecipientsInitialized<T>(ref T variable, T defaultValue)
        {
            switch (RecipientsLoaded)
            {
                case Enums.LoadState.NotLoaded:
                    LoadRecipients();
                    variable = defaultValue;
                    break;
                case Enums.LoadState.Loading:
                    variable = defaultValue;
                    break;
                case Enums.LoadState.Loaded:
                    break;
            }
            
            return variable;
        }
        internal string Initialized(ref string variable)
        {
            if (variable is null) { LoadPriority(_emailPrefixToStrip); }
            return variable;
        }
        internal bool Initialized(ref bool? variable)
        {
            // check if one of the nullable variables is null which would indicate
            // the need to initialize
            if (variable is null) { LoadPriority(_emailPrefixToStrip); }
            return (bool)variable;
        }

        private string _actionable;
        public string Actionable { get => Initialized(ref _actionable); set => _actionable = value; }

        private string _body;
        public string Body { get => Initialized(ref _body); set => _body = value; }

        private string _conversationIndex;
        public string ConversationIndex { get => Initialized(ref _conversationIndex); set => _conversationIndex = value; }

        private string _emailPrefixToStrip = "";
        public string EmailPrefixToStrip { get => _emailPrefixToStrip; internal set => _emailPrefixToStrip = value; }

        private string _entryId;
        public string EntryId { get => Initialized(ref _entryId); set => _entryId = value; }

        private string _folder;
        public string Folder { get => Initialized(ref _folder); set => _folder = value; }

        private MailItem _item;
        public MailItem Item { get => _item; set => _item = value; }

        private IMailItemInfo.PlainTextOptionsEnum _plainTextOptions = IMailItemInfo.PlainTextOptionsEnum.StripAll;
        public IMailItemInfo.PlainTextOptionsEnum PlainTextOptions { get => _plainTextOptions; set => _plainTextOptions = value; }
        
        //private Task _recipientsTask;
        //internal Task RecipientsTask 
        //{ 
        //    get
        //    {
        //        if (_recipientsTask is null) { LoadPriority(_emailPrefixToStrip); }
        //        return _recipientsTask;
        //    }
        //    private set => _recipientsTask = value;  
        //}

        private string _sentOn;
        public string SentOn { get => Initialized(ref _sentOn); set => _sentOn = value; }

        private string _subject;
        public string Subject { get => Initialized(ref _subject); set => _subject = value; }
        
        private string _senderHtml;
        public string SenderHtml { get => Initialized(ref _senderHtml); set => _senderHtml = value; }

        private string _senderName;
        public string SenderName { get => Initialized(ref _senderName); set => _senderName = value; }
        
        private RecipientInfo _sender;
        public RecipientInfo Sender
        {
            get
            {
                if (_sender is null)
                    LoadPriority(_emailPrefixToStrip);
                return _sender;
            }
            set => _sender = value;
        }

        private Enums.LoadState _recipientsLoaded = Enums.LoadState.NotLoaded;
        public Enums.LoadState RecipientsLoaded 
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _recipientsLoaded;
            [MethodImpl(MethodImplOptions.Synchronized)]
            private set => _recipientsLoaded = value; 
        }

        private string _ccRecipientsHtml;
        public string CcRecipientsHtml
        {
            get => RecipientsInitialized(ref _ccRecipientsHtml, RecipientsLoaded.ToString());
            set { _ccRecipientsHtml = value; NotifyPropertyChanged(); }
        }

        private string _ccRecipientsName;
        public string CcRecipientsName
        {
            get => RecipientsInitialized(ref _ccRecipientsName, RecipientsLoaded.ToString());
            set { _ccRecipientsName = value; NotifyPropertyChanged(); }
        }

        private RecipientInfo[] _ccRecipients;
        public RecipientInfo[] CcRecipients => RecipientsInitialized(ref _ccRecipients, default);

        private string _toRecipientsHtml;
        public string ToRecipientsHtml 
        {
            get => RecipientsInitialized(ref _toRecipientsHtml, RecipientsLoaded.ToString());
            set { _toRecipientsHtml = value; NotifyPropertyChanged(); }
        }

        private string _toRecipientsName;
        public string ToRecipientsName 
        {
            get => RecipientsInitialized(ref _toRecipientsName, RecipientsLoaded.ToString());
            set { _toRecipientsName = value; NotifyPropertyChanged(); }
        }

        private RecipientInfo[] _toRecipients;
        public RecipientInfo[] ToRecipients => RecipientsInitialized(ref _toRecipients, default); 
        
        private string _triage;
        public string Triage { get => Initialized(ref _triage); set => _triage = value; }

        private string _html = null;
        public string Html { get => _html ?? GetHtml(); private set => _html = value; }

        private DateTime _sentDate;
        public DateTime SentDate
        {
            get
            {
                if (_sentDate == default)
                {
                    if (_item is not null) { _sentDate = _item.SentOn; }
                }
                return _sentDate;
            }
            set => _sentDate = value;
        }

        private AttachmentInfo[] _attachments;
        public AttachmentInfo[] Attachments
        {
            get => Initializer.GetOrLoad(ref _attachments, LoadAttachmentsInfo);
            set => _attachments = value;
        }
        internal AttachmentInfo[] LoadAttachmentsInfo()
        {
            return Item.Attachments
                       .Cast<Attachment>()
                       .Select(x => new AttachmentInfo(x, _sentDate, _folder, _emailPrefixToStrip))
                       .ToArray();
        }

        public string GetHeaders()
        {
            return (string)Item.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x007D001F/");
        }

        public string[] Tokens 
        { 
            get => Initializer.GetOrLoad(ref _tokens, LoadTokens); 
            private set => _tokens = value; 
        }
        private string[] _tokens;
        public string[] LoadTokens() 
        {
            _tokens = Tokenizer.tokenize(this).ToArray();
            return _tokens;
        }
        public async Task<IEnumerable<string>> TokenizeAsync()
        {
            _tokens = await Task.Run(() => Tokenizer.tokenize(this).ToArray());
            return _tokens;
        }

        [JsonIgnore]
        public EmailTokenizer Tokenizer { get => _tokenizer ??= new EmailTokenizer(); }
        private EmailTokenizer _tokenizer;
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
            return CompressPlainText(text ?? "", IMailItemInfo.PlainTextOptionsEnum.StripAll, emailPrefixToStrip ?? "");
        }

        internal static string CompressPlainText(string text, IMailItemInfo.PlainTextOptionsEnum options, string emailPrefixToStrip)
        {
            if (options.HasFlag(IMailItemInfo.PlainTextOptionsEnum.StripWarning) && emailPrefixToStrip != "")
                text = text.Replace(emailPrefixToStrip, "");

            if (options.HasFlag(IMailItemInfo.PlainTextOptionsEnum.StripLinks))
            {
                var replacementText = "";
                if (options.HasFlag(IMailItemInfo.PlainTextOptionsEnum.ShowStripped))
                    replacementText = "<link>";
                text = Regex.Replace(text, @"<https://[^>]+>", replacementText); //Strip links
            }

            if (options.HasFlag(IMailItemInfo.PlainTextOptionsEnum.StripReplyHeader) || 
                options.HasFlag(IMailItemInfo.PlainTextOptionsEnum.StripReplyBody))
            {
                var replacementText = "";
                if (options.HasFlag(IMailItemInfo.PlainTextOptionsEnum.ShowStripped | IMailItemInfo.PlainTextOptionsEnum.StripReplyHeader) &&
                    !options.HasFlag(IMailItemInfo.PlainTextOptionsEnum.StripReplyBody))
                    replacementText = "<EOM> Chain: $3";
                else if (!options.HasFlag(IMailItemInfo.PlainTextOptionsEnum.StripReplyHeader))
                    replacementText += "$1";
                else if (!options.HasFlag(IMailItemInfo.PlainTextOptionsEnum.StripReplyBody))
                    replacementText += "$3";

                text = Regex.Replace(text, @"(From:([^\n]*\n){1,4}Subject: {0,1}[rR][eE]:.*)(.|\n|\r)*\z", replacementText); //Strip reply footer
            }

            if (options.HasFlag(IMailItemInfo.PlainTextOptionsEnum.StripFormatting))
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
        
        private bool? _unread;
        public bool UnRead
        {
            get => (bool)Initializer.GetOrLoad(ref _unread, loader: () => _item.UnRead, strict: false, dependencies: _item)!;
            set => Initializer.SetAndSave(ref _unread, value, (x) => _item.UnRead = x ?? false, () => _item.Save(), null, false);
        }

        private bool? _isTaskFlagSet;
                
        public bool IsTaskFlagSet { get => Initialized(ref _isTaskFlagSet); set => _isTaskFlagSet = value; }

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
            return revisedBody;
        }

        #endregion

    }
}
