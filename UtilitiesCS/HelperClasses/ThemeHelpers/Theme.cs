using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using BrightIdeasSoftware;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    public class Theme
    {
        #region Constructors and Initializers

        public Theme(string name,
                     Label lblItemNumber,
                     Label lblSender,
                     Label lblSubject,
                     IList<TableLayoutPanel> tableLayoutPanels,
                     IList<Button> buttons,
                     IList<System.ComponentModel.Component> menuItems,
                     MenuStrip menuStrip,
                     IList<IQfcTipsDetails> tipsDetailsLabels,
                     IList<IQfcTipsDetails> tipsExpanded,
                     TextBox textboxSearch,
                     TextBox textboxBody,
                     ComboBox comboFolders,
                     FastObjectListView topicThread,
                     Microsoft.Web.WebView2.WinForms.WebView2 webView2,
                     Control viewer,
                     Func<bool> mailRead,
                     Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme web2ViewScheme,
                     Action<Enums.ToggleState> htmlConverter,
                     Enums.ToggleState htmlDark,
                     Color navBackgColor,
                     Color navForeColor,
                     Color tlpBackColor,
                     Color tipsForeColor,
                     Color tipsBackColor,
                     Color mailReadForeColor,
                     Color mailReadBackColor,
                     Color mailUnreadForeColor,
                     Color mailUnreadBackColor,
                     Color tipsDetailsBackColor,
                     Color tipsDetailsForeColor,
                     Color buttonBackColor,
                     Color buttonMouseOverColor,
                     Color buttonClickedColor,
                     Color txtboxSearchBackColor,
                     Color txtboxSearchForeColor,
                     Color txtboxBodyBackColor,
                     Color txtboxBodyForeColor,
                     Color cboFoldersBackColor,
                     Color cboFoldersForeColor,
                     Color defaultBackColor,
                     Color defaultForeColor)
        {
            _name = name;
            _lblItemNumber = lblItemNumber;
            _lblSender = lblSender;
            _lblSubject = lblSubject;
            _tableLayoutPanels = tableLayoutPanels;
            _buttons = buttons;
            _menuItems = menuItems;
            _menuStrip = menuStrip;
            _tipsDetailsLabels = tipsDetailsLabels;
            _tipsExpanded = tipsExpanded;
            _textboxSearch = textboxSearch;
            _textboxBody = textboxBody;
            _comboFolders = comboFolders;
            _topicThread = topicThread;
            _webView2 = webView2;
            _viewer = viewer;
            MailRead = mailRead;
            _web2ViewScheme = web2ViewScheme;
            _htmlConverter = htmlConverter;
            _htmlDark = htmlDark;
            _navBackColor = navBackgColor;
            _navForeColor = navForeColor;
            _tlpBackColor = tlpBackColor;
            _tipsForeColor = tipsForeColor;
            _tipsBackColor = tipsBackColor;
            _mailReadForeColor = mailReadForeColor;
            _mailReadBackColor = mailReadBackColor;
            _mailUnreadForeColor = mailUnreadForeColor;
            _mailUnreadBackColor = mailUnreadBackColor;
            _tipsDetailsBackColor = tipsDetailsBackColor;
            _tipsDetailsForeColor = tipsDetailsForeColor;
            _buttonBackColor = buttonBackColor;
            _buttonMouseOverColor = buttonMouseOverColor;
            _buttonClickedColor = buttonClickedColor;
            _txtboxSearchBackColor = txtboxSearchBackColor;
            _txtboxSearchForeColor = txtboxSearchForeColor;
            _txtboxBodyBackColor = txtboxBodyBackColor;
            _txtboxBodyForeColor = txtboxBodyForeColor;
            _cboFoldersBackColor = cboFoldersBackColor;
            _cboFoldersForeColor = cboFoldersForeColor;
            _defaultBackColor = defaultBackColor;
            _defaultForeColor = defaultForeColor;
        }

        private Label _lblItemNumber;
        private Label _lblSender;
        private Label _lblSubject;
        private IList<TableLayoutPanel> _tableLayoutPanels;
        private IList<Button> _buttons;
        private IList<System.ComponentModel.Component> _menuItems;
        private MenuStrip _menuStrip;
        private IList<IQfcTipsDetails> _tipsDetailsLabels;
        IList<IQfcTipsDetails> _tipsExpanded;
        private TextBox _textboxSearch;
        private TextBox _textboxBody;
        private ComboBox _comboFolders;
        private FastObjectListView _topicThread;
        private Microsoft.Web.WebView2.WinForms.WebView2 _webView2;
        private Control _viewer;
        private Func<bool> MailRead;

        public Theme() { }

        public Theme(string name,
                     Dictionary<string, ThemeControlGroup> controlGroups)
        {
            if (controlGroups is null) { throw new ArgumentNullException(nameof(controlGroups));}
            _name = name;
            _controlGroups = controlGroups;
        }

        
        #endregion
        

        #region Public Properties

        private Color _navBackColor;
        public Color NavBackColor { get => _navBackColor; set => _navBackColor = value; }
        
        private Color _navForeColor;
        public Color NavForeColor { get => _navForeColor; set => _navForeColor = value; }
                        
        private Action<Enums.ToggleState> _htmlConverter;
        public Action<Enums.ToggleState> HtmlConverter { get => _htmlConverter; set => _htmlConverter = value; }
        
        private Color _buttonBackColor;
        public Color ButtonBackColor { get => _buttonBackColor; set => _buttonBackColor = value; }

        private Color _buttonMouseOverColor;
        public Color ButtonMouseOverColor { get => _buttonMouseOverColor; set => _buttonMouseOverColor = value; }

        private Color _buttonClickedColor;
        public Color ButtonClickedColor { get => _buttonClickedColor; set => _buttonClickedColor = value; }

        private Color _cboFoldersBackColor;
        public Color CboFoldersBackColor { get => _cboFoldersBackColor; set => _cboFoldersBackColor = value; }
        
        private Color _cboFoldersForeColor;
        public Color CboFoldersForeColor { get => _cboFoldersForeColor; set => _cboFoldersForeColor = value; }
        
        private Color _defaultBackColor;
        public Color DefaultBackColor { get => _defaultBackColor; set => _defaultBackColor = value; }
        
        private Color _defaultForeColor;
        public Color DefaultForeColor { get => _defaultForeColor; set => _defaultForeColor = value; }        
        
        private Color _mailReadBackColor;
        public Color MailReadBackColor { get => _mailReadBackColor; set => _mailReadBackColor = value; }
        
        private Color _mailReadForeColor;
        public Color MailReadForeColor { get => _mailReadForeColor; set => _mailReadForeColor = value; }
        
        private Color _mailUnreadBackColor;
        public Color MailUnreadBackColor { get => _mailUnreadBackColor; set => _mailUnreadBackColor = value; }
        
        private Color _mailUnreadForeColor;
        public Color MailUnreadForeColor { get => _mailUnreadForeColor; set => _mailUnreadForeColor = value; }
        
        private Color _tipsBackColor;
        public Color TipsBackColor { get => _tipsBackColor; set => _tipsBackColor = value; }
        
        private Color _tipsDetailsBackColor;
        public Color TipsDetailsBackColor { get => _tipsDetailsBackColor; set => _tipsDetailsBackColor = value; }
        
        private Color _tipsDetailsForeColor;
        public Color TipsDetailsForeColor { get => _tipsDetailsForeColor; set => _tipsDetailsForeColor = value; }
        
        private Color _tipsForeColor;
        public Color TipsForeColor { get => _tipsForeColor; set => _tipsForeColor = value; }
        
        private Color _tlpBackColor;
        public Color TlpBackColor { get => _tlpBackColor; set => _tlpBackColor = value; }
        
        private Color _txtboxBodyBackColor;
        public Color TxtboxBodyBackColor { get => _txtboxBodyBackColor; set => _txtboxBodyBackColor = value; }
        
        private Color _txtboxBodyForeColor;
        public Color TxtboxBodyForeColor { get => _txtboxBodyForeColor; set => _txtboxBodyForeColor = value; }
        
        private Color _txtboxSearchBackColor;
        public Color TxtboxSearchBackColor { get => _txtboxSearchBackColor; set => _txtboxSearchBackColor = value; }
        
        private Color _txtboxSearchForeColor;
        public Color TxtboxSearchForeColor { get => _txtboxSearchForeColor; set => _txtboxSearchForeColor = value; }
        
        private Enums.ToggleState _htmlDark;
        public Enums.ToggleState HtmlDark { get => _htmlDark; set => _htmlDark = value; }
        
        private Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme _web2ViewScheme;
        public Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme Web2ViewScheme { get => _web2ViewScheme; set => _web2ViewScheme = value; }
       
        private string _name;
        public string Name { get => _name; set => _name = value; }

        private Dictionary<string, ThemeControlGroup> _controlGroups;
        public Dictionary<string, ThemeControlGroup> ControlGroups { get => _controlGroups; set => _controlGroups = value; }

        #endregion

        #region Public Methods

        public void SetMailRead(bool async)
        {
            if (_lblSender is null) {  throw new System.InvalidOperationException(
                $"Variable {nameof(_lblSender)} is null");}
            if (async) { _lblSender.BeginInvoke(new System.Action(() => SetMailRead())); }
            else { _lblSender.Invoke(new System.Action(() => SetMailRead())); }
        }

        public void SetMailRead()
        {
            if (_lblSender is null)
            {
                throw new System.InvalidOperationException(
                $"Variable {nameof(_lblSender)} is null");
            }
            if (_lblSubject is null)
            {
                throw new System.InvalidOperationException(
                $"Variable {nameof(_lblSubject)} is null");
            }
            _lblSender.BackColor = _mailReadBackColor;
            _lblSender.ForeColor = _mailReadForeColor;
            _lblSubject.BackColor = _mailReadBackColor;
            _lblSubject.ForeColor = _mailReadForeColor;
        }

        public void SetMailUnread(bool async)
        {
            if (_lblSender is null)
            {
                throw new System.InvalidOperationException(
                $"Variable {nameof(_lblSender)} is null");
            }
            if (async) { _lblSender.BeginInvoke(new System.Action(() => SetMailUnread())); }
            else { _lblSender.Invoke(new System.Action(() => SetMailUnread())); }
        }
        
        private void SetMailUnread()
        {
            if (_lblSender is null)
            {
                throw new System.InvalidOperationException(
                $"Variable {nameof(_lblSender)} is null");
            }
            if (_lblSubject is null)
            {
                throw new System.InvalidOperationException(
                $"Variable {nameof(_lblSubject)} is null");
            }
            _lblSender.BackColor = _mailUnreadBackColor;
            _lblSender.ForeColor = _mailUnreadForeColor;
            _lblSubject.BackColor = _mailUnreadBackColor;
            _lblSubject.ForeColor = _mailUnreadForeColor;
        }

        public void SetQfcTheme(bool async)
        {
            if (async) { UiThread.Dispatcher.InvokeAsync(() => SetQfcTheme()); }
            else if (_lblItemNumber.InvokeRequired) 
            { 
                _lblItemNumber.Invoke(() => SetQfcTheme());
            }
            else { SetQfcTheme(); }
            //UiThread.Dispatcher.Invoke(() => SetQfcTheme()); 
            
            //if (async) { _lblSender.BeginInvoke(new System.Action(() => SetQfcTheme())); }
            //else { _lblSender.Invoke(new System.Action(() => SetQfcTheme())); }
        }
        
        public async Task SetQfcThemeAsync()
        {
            await UiThread.Dispatcher.InvokeAsync(()=> SetQfcTheme());
        }

        private void SetQfcTheme()
        {
            // Active item navigation colors
            _lblItemNumber.BackColor = _navBackColor;
            _lblItemNumber.ForeColor = _navForeColor;

            // General thematic colors
            foreach (TableLayoutPanel tlp in _tableLayoutPanels)
            {
                tlp.BackColor = TlpBackColor;
            }

            // Shortcut accelerator colors  
            foreach (var tipsDetails in _tipsDetailsLabels)
            {
                tipsDetails.LabelControl.BackColor = TipsDetailsBackColor;
                tipsDetails.LabelControl.ForeColor = TipsDetailsForeColor;
            }

            foreach (var tipsDetails in _tipsExpanded)
            {
                tipsDetails.LabelControl.BackColor = TipsDetailsBackColor;
                tipsDetails.LabelControl.ForeColor = TipsDetailsForeColor;
            }

            // Mail item colors
            if (!MailRead()) { SetMailUnread(); }
            else { SetMailRead(); }

            // Button colors
            foreach (Button btn in _buttons)
            {
                if (btn.DialogResult == DialogResult.OK) { btn.BackColor = ButtonClickedColor; }
                else { btn.BackColor = ButtonBackColor; }
            }

            foreach (System.ComponentModel.Component menuItem in _menuItems)
            {
                if (menuItem is ToolStripMenuItem)
                {
                    var item = menuItem as ToolStripMenuItem;
                    item.BackColor = ButtonBackColor;
                    //item.ForeColor = ButtonForeColor;
                }
            }

            _menuStrip.BackColor = DefaultBackColor;
            
            _menuStrip.ForeColor = DefaultForeColor;
            // Colors for the folder search
            // TODO: Override the draw function because these colors do not work as expected
            _textboxSearch.BackColor = TxtboxSearchBackColor;
            _textboxSearch.ForeColor = TxtboxSearchForeColor;

            // Colors for email body
            _textboxBody.BackColor = TxtboxBodyBackColor;
            _textboxBody.ForeColor = TxtboxBodyForeColor;

            // TODO: Override the draw function because these colors do not work as expected
            _comboFolders.BackColor = CboFoldersBackColor;
            _comboFolders.ForeColor = CboFoldersForeColor;

            _topicThread.BackColor = DefaultBackColor;
            _topicThread.ForeColor = DefaultForeColor;

            var headerstyle = new HeaderFormatStyle();
            headerstyle.SetBackColor(DefaultBackColor);
            headerstyle.SetForeColor(DefaultForeColor);

            foreach (OLVColumn column in _topicThread.Columns)
            {
                column.HeaderFormatStyle = headerstyle;
            }

            if (_webView2.CoreWebView2 is not null)
            {
                _webView2.CoreWebView2.Profile.PreferredColorScheme = Web2ViewScheme;
                HtmlConverter(HtmlDark);
            }

            // Default colors   
            _viewer.BackColor = DefaultBackColor;
            _viewer.ForeColor = DefaultForeColor;
        }

        public void SetTheme()
        {
            ControlGroups.ForEach(controlGroup => controlGroup.Value.ApplyTheme());
        }

        public void SetTheme(bool async)
        {
            ControlGroups.ForEach(controlGroup => controlGroup.Value.ApplyTheme(async));
        }

        #endregion

    }
}


