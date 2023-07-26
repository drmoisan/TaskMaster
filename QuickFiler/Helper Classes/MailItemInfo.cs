using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToDoModel;

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

        private MailItem _item;
        private string _sender; 
        private string _recipient;
        private string _subject;
        private string _body;
        private string _triage;
        private string _actionable;
        private string _sentOn;
        private string _folder;
        private bool _unread;
        
        public MailItem Item { get => _item; set => _item = value; }
        public string Sender { get => Initialized(ref _sender); set => _sender = value; }
        public string Recipient { get => Initialized(ref _recipient); set => _recipient = value; }
        public string Subject { get => Initialized(ref _subject); set => _subject = value; }
        public string Body { get => Initialized(ref _body); set => _body = value; }
        public string Triage { get => Initialized(ref _triage); set => _triage = value; }
        public string Actionable { get => Initialized(ref _actionable); set => _actionable = value; }
        public string SentOn { get => Initialized(ref _sentOn); set => _sentOn = value; }
        public string Folder { get => Initialized(ref _folder); set => _folder = value; }
        public bool UnRead { get => Initialized(ref _unread); set => _unread = value; }
        
        public DateTime SentDate { get => _item.SentOn; }

        internal string Initialized(ref string variable)
        {
            if (variable is null) { ExtractBasics(); }
            return variable;
        }

        internal bool Initialized(ref bool variable)
        {
            // check if one of the nullable variables is null which would indicate
            // the need to initialize
            if (_sender is null) { ExtractBasics(); }
            return variable;
        }

        public bool ExtractBasics()
        {
            if (_item is null) { throw new ArgumentNullException(); }
            _sender = _item.GetSenderName();
            _subject = _item.Subject;
            _body = CompressPlainText(_item.Body);
            _triage = _item.GetTriage();
            _sentOn = _item.SentOn.ToString("g");
            _actionable = _item.GetActionTaken();
            _folder = ((Folder)_item.Parent).Name;
            return true;            
        }

        internal string CompressPlainText(string text)
        {
            //text = text.Replace(System.Environment.NewLine, " ");
            text = text.Replace(Properties.Resources.Email_Prefix_To_Strip, "");
            text = Regex.Replace(text, @"<https://[^>]+>", " <link> "); //Strip links
            text = Regex.Replace(text, @"[\s]", " ");
            text = Regex.Replace(text, @"[ ]{2,}", " ");
            text = text.Trim();
            text += " <EOM>";
            return text;
        }

        internal string EmailHeader
        {
            get => //@"<div class=""WordSection1"">
@"
<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>From:<span style='mso-tab-count:1'> </span></span></b><span
style='color:black'>" + this.Sender + @"<o:p></o:p></span></p>

<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>Sent:<span style='mso-tab-count:1'> </span></span></b><span
style='color:black'>" + this.SentOn + @"<o:p></o:p></span></p>

<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>To:<span style='mso-tab-count:1'> </span></span></b><span
style='color:black'>" + this.Recipient + @"<o:p></o:p></span></p>

<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>Subject:<span style='mso-tab-count:1'></span></span></b><span
style='color:black'>" + this.Subject + @"<o:p></o:p></span></p>

<p class=MsoNormal><o:p>&nbsp;</o:p></p>";
        }
        
        public string GetHTML()
        {
            string body = _item.HTMLBody;
            var rx = new Regex(@"(<body[\S\s]*?>)", RegexOptions.Multiline);
            string revisedBody = rx.Replace(body, "$1" + EmailHeader);
            //string revisedBody = body.Replace(@"<div class=""WordSection1"">", EmailHeader);
            return revisedBody;
        }
    }
}
