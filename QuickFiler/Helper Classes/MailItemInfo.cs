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
using ToDoModel;
using UtilitiesCS;

namespace QuickFiler
{
    /// <summary>
    /// Class to cache information about a mail item.
    /// </summary>
    public class MailItemInfo
    {
        public MailItemInfo() { }

        public MailItemInfo(MailItem item)
        {
            _item = item;
        }        

        public MailItemInfo(DataFrame df, long indexRow)
        {
            _entryId = (string)df["EntryID"][indexRow];
            _storeId = (string)df["Store"][indexRow];
            _senderName = (string)df["SenderName"][indexRow];
            _sender = new RecipientInfo() { Name = _senderName, Address = (string)df["SenderSmtpAddress"][indexRow] }; 
            _folder = (string)df["Folder Name"][indexRow];
            _sentDate = DateTime.Parse((string)df["SentOn"][indexRow]);
            _conversationIndex = (string)df["ConversationIndex"][indexRow];
        }

        public static async Task<MailItemInfo> FromMailItemAsync(MailItem item)
        {
            var info = new MailItemInfo(item);
            if (item is null) { throw new ArgumentNullException(); }
            info.EntryId = item.EntryID;
            info.SetSender(item.GetSenderInfo());
            info.Subject = item.Subject;
            info.Body = CompressPlainText(item.Body);
            info.Triage = item.GetTriage();
            info.SentOn = item.SentOn.ToString("g");
            info.Actionable = item.GetActionTaken();
            info.Folder = ((Folder)item.Parent).Name;
            info.ConversationIndex = item.ConversationIndex;
            info.UnRead = item.UnRead;
            info.IsTaskFlagSet = (item.FlagStatus == OlFlagStatus.olFlagMarked || item.FlagStatus == OlFlagStatus.olFlagComplete);
            await Task.Factory.StartNew(() => info.LoadRecipients(),
                                              default,
                                              TaskCreationOptions.None,
                                              PriorityScheduler.BelowNormal);
            return info;
        }

        private string _storeId;
        private RecipientInfo _sender;
        private RecipientInfo _toRecipients;
        private RecipientInfo _ccRecipients;
        private Enums.ToggleState _darkMode = Enums.ToggleState.Off;

        [Flags] 
        public enum PlainTextOptionsEnum
        {
            Original = 0,
            ShowStripped = 1,
            StripWarning = 2,
            StripLinks = 4,
            StripFormatting = 8,
            StripReplyHeader = 16,
            StripReplyBody = 32,
            StripAllSilently = 62,
            StripAll = 63
        }

        
        
        #region Public Properties

        private string _actionable;
        public string Actionable { get => Initialized(ref _actionable); set => _actionable = value; }        
        
        private string _body;
        public string Body { get => Initialized(ref _body); set => _body = value; }
        
        private string _ccRecipientsHtml;
        public string CcRecipientsHtml { get => Initialized(ref _ccRecipientsHtml); set => _ccRecipientsHtml = value; }
        
        private string _ccRecipientsName;
        public string CcRecipientsName { get => Initialized(ref _ccRecipientsName); set => _ccRecipientsName = value;  }
        
        private string _conversationIndex;
        public string ConversationIndex { get => Initialized(ref _conversationIndex); set => _conversationIndex = value; }
        
        private string _entryId;
        public string EntryId { get => Initialized(ref _entryId); set => _entryId = value; }
        
        private string _folder;
        public string Folder { get => Initialized(ref _folder); set => _folder = value; }
        
        private MailItem _item;
        public MailItem Item { get => _item; set => _item = value; }
                
        private PlainTextOptionsEnum _plainTextOptions = PlainTextOptionsEnum.StripAll;
        public PlainTextOptionsEnum PlainTextOptions { get => _plainTextOptions; set => _plainTextOptions = value; }

        private string _senderHtml;
        public string SenderHtml { get => Initialized(ref _senderHtml); set => _senderHtml = value; }
        
        private string _senderName; 
        public string SenderName { get => Initialized(ref _senderName); set => _senderName = value; }
        
        private string _sentOn;
        public string SentOn { get => Initialized(ref _sentOn); set => _sentOn = value; }
        
        private string _subject;
        public string Subject { get => Initialized(ref _subject); set => _subject = value; }
        
        private string _toRecipientsHtml;
        public string ToRecipientsHtml { get => Initialized(ref _toRecipientsHtml); set => _toRecipientsHtml = value; }
        
        private string _toRecipientsName;
        public string ToRecipientsName { get => Initialized(ref _toRecipientsName); set => _toRecipientsName = value; }
        
        private string _triage;
        public string Triage { get => Initialized(ref _triage); set => _triage = value; }
        
        private string _html;
        public string Html { get => _html ?? GetHTML(); private set => _html = value; }

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

        #endregion

        #region Initialization Methods

        internal string Initialized(ref string variable)
        {
            if (variable is null) { LoadPriority(); }
            return variable;
        }

        internal bool Initialized(ref bool? variable)
        {
            // check if one of the nullable variables is null which would indicate
            // the need to initialize
            if (variable is null) { LoadPriority(); }
            return (bool)variable;
        }

        internal void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter, System.Action objectSaver)
        {
            variable = value;
            if (objectSetter is null) { throw new ArgumentNullException($"Method {nameof(SetAndSave)} failed because {nameof(objectSetter)} was passed as null"); }
            objectSetter(value);
            if (objectSaver is not null) { objectSaver(); }   
        }

        internal T GetOrLoad<T>(ref T value, Func<T> loader)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T))) { value = loader(); }
            return value;
        }

        internal T GetOrLoad<T>(ref T value, Func<T> loader, params object[] dependencies)
        {
            if (dependencies is null) { throw new ArgumentNullException($"Method {nameof(GetOrLoad)} failed the dependency check because {nameof(dependencies)} was passed as a null array"); }
            if (dependencies.Any(x => x is null))
            {
                var errors = dependencies.FindIndices(x => x is null).Select(x => x.ToString()).ToArray().SentenceJoin();
                throw new ArgumentNullException($"Method {nameof(GetOrLoad)} failed the dependency check because {nameof(dependencies)} contains a null value at position {errors}");
            }
            return GetOrLoad(ref value, loader);
        }

        public bool LoadPriority()
        {
            if (_item is null) { throw new ArgumentNullException(); }
            _entryId = _item.EntryID;
            _sender = _item.GetSenderInfo();
            _senderName = _sender.Name;
            _senderHtml = _sender.Html;
            _subject = _item.Subject;
            _body = CompressPlainText(_item.Body);
            _triage = _item.GetTriage();
            _sentOn = _item.SentOn.ToString("g");
            _actionable = _item.GetActionTaken();
            _folder = ((Folder)_item.Parent).Name;
            _conversationIndex = _item.ConversationIndex;
            _unread = _item.UnRead;
            _isTaskFlagSet = (_item.FlagStatus == OlFlagStatus.olFlagMarked);
            _ = Task.Factory.StartNew(() => LoadRecipients(), 
                                      default, 
                                      TaskCreationOptions.None, 
                                      PriorityScheduler.BelowNormal);
            return true;            
        }

        async public Task<bool> LoadAsync(Outlook.NameSpace olNs, bool darkMode=false)
        {
            _item = await Task.FromResult((MailItem)olNs.GetItemFromID(_entryId, _storeId));
            _sender.Html = EmailDetails.ConvertRecipientToHtml(_sender.Address, _sender.Name);
            _senderHtml = _sender.Html;
            LoadRecipients();
            _html = GetHTML();
            if (darkMode) { _html = ToggleDark(Enums.ToggleState.On); }
            _triage = _item.GetTriage();
            _sentOn = _sentDate.ToString("g");
            _actionable = _item.GetActionTaken();
            
            return true;
        }
        
        public void LoadRecipients()
        {
            _toRecipients = _item.GetToRecipients().GetInfo();
            _toRecipientsName = _toRecipients.Name;
            _toRecipientsHtml = _toRecipients.Html;
            _ccRecipients = _item.GetCcRecipients().GetInfo();
            _ccRecipientsName = _ccRecipients.Name;
            _ccRecipientsHtml = _ccRecipients.Html;
        }

        internal void SetSender(RecipientInfo sender)
        {
            _sender = sender;
            _senderName = sender.Name;
            _senderHtml = sender.Html;
        }

        #endregion

        #region HTML and Plain Text Methods

        internal static string CompressPlainText(string text) 
        { 
            return CompressPlainText(text, PlainTextOptionsEnum.StripAll); 
        }


        internal static string CompressPlainText(string text, PlainTextOptionsEnum options)
        {
            if (options.HasFlag(PlainTextOptionsEnum.StripWarning)) 
                text = text.Replace(Properties.Resources.Email_Prefix_To_Strip, "");
            
            if (options.HasFlag(PlainTextOptionsEnum.StripLinks))
            {
                var replacementText = "";
                if (options.HasFlag(PlainTextOptionsEnum.ShowStripped))
                    replacementText = "<link>";
                text = Regex.Replace(text, @"<https://[^>]+>", replacementText); //Strip links
            }
            
            if (options.HasFlag(PlainTextOptionsEnum.StripReplyHeader) || options.HasFlag(PlainTextOptionsEnum.StripReplyBody))
            {
                var replacementText = "";
                if (options.HasFlag(PlainTextOptionsEnum.ShowStripped | PlainTextOptionsEnum.StripReplyHeader) && 
                    !options.HasFlag(PlainTextOptionsEnum.StripReplyBody)) 
                    replacementText = "<EOM> Chain: $3";
                else if (!options.HasFlag(PlainTextOptionsEnum.StripReplyHeader))
                    replacementText += "$1";
                else if (!options.HasFlag(PlainTextOptionsEnum.StripReplyBody))
                    replacementText += "$3";

                text = Regex.Replace(text, @"(From:([^\n]*\n){1,4}Subject: {0,1}[rR][eE]:.*)(.|\n|\r)*\z", replacementText); //Strip reply footer
            }

            if (options.HasFlag(PlainTextOptionsEnum.StripFormatting))
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
            get => (bool)GetOrLoad(ref _unread, loader: () => _item.UnRead, dependencies: _item)!;
            set => SetAndSave(ref _unread, value, (x)=>_item.UnRead = x ?? false, ()=>_item.Save()); 
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
            if ((desiredState == Enums.ToggleState.On)&&_darkMode== Enums.ToggleState.Off) 
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
                
        internal string GetHTML()
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
