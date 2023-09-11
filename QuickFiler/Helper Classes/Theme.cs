using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using BrightIdeasSoftware;

namespace QuickFiler.Helper_Classes
{
    public class Theme
    {
        public Theme() { }
        public Theme(string name,
                     QfcItemViewer itemViewer,
                     IQfcItemController parent,
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
            _itemViewer = itemViewer;
            _parent = parent;
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

        private Color _navBackColor;
        private Color _navForeColor;
        private IQfcItemController _parent;
        private QfcItemViewer _itemViewer;
        
        
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

        public void SetMailRead(bool async)
        {
            if (async) { _itemViewer.BeginInvoke(new System.Action(() => SetMailRead())); }
            else { _itemViewer.Invoke(new System.Action(() => SetMailRead())); }
        }

        public void SetMailRead()
        {
            _itemViewer.LblSender.SetTheme(backColor: _mailReadBackColor,
                                           forecolor: _mailReadForeColor);
            _itemViewer.lblSubject.SetTheme(backColor: _mailReadBackColor,
                                            forecolor: _mailReadForeColor);
        }

        public void SetMailUnread(bool async)
        {
            if (async) { _itemViewer.BeginInvoke(new System.Action(() => SetMailUnread())); }
            else { _itemViewer.Invoke(new System.Action(() => SetMailUnread())); }
        }
        
        private void SetMailUnread()
        {
            _itemViewer.LblSender.SetTheme(backColor: _mailUnreadBackColor,
                                            forecolor: _mailUnreadForeColor);
            _itemViewer.lblSubject.SetTheme(backColor: _mailUnreadBackColor,
                                            forecolor: _mailUnreadForeColor);
        }

        public void SetTheme(bool async)
        {
            if (async) { _itemViewer.BeginInvoke(new System.Action(() => SetTheme())); }
            else { _itemViewer.Invoke(new System.Action(() => SetTheme())); }
        }
        
        private void SetTheme()
        {
            // Active item navigation colors
            _itemViewer.LblItemNumber.SetTheme(backColor: _navBackColor,
                                        forecolor: _navForeColor);

            // General thematic colors
            foreach (TableLayoutPanel tlp in _parent.TableLayoutPanels)
            {
                tlp.SetTheme(backColor: TlpBackColor);
            }

            // Shortcut accelerator colors  
            foreach (IQfcTipsDetails tipsDetails in _parent.ListTipsDetails)
            {
                tipsDetails.LabelControl.SetTheme(backColor: TipsDetailsBackColor,
                                                    forecolor: TipsDetailsForeColor);
            }

            // Mail item colors
            if (_parent.Mail.UnRead == true) { SetMailUnread(); }
            else { SetMailRead(); }

            // Button colors
            foreach (Button btn in _parent.Buttons)
            {
                if (btn.DialogResult == DialogResult.OK) { btn.SetTheme(backColor: ButtonClickedColor); }
                else { btn.SetTheme(backColor: ButtonBackColor); }
            }

            // Colors for the folder search
            // TODO: Override the draw function because these colors do not work as expected
            _itemViewer.TxtboxSearch.BackColor = TxtboxSearchBackColor;
            _itemViewer.TxtboxSearch.ForeColor = TxtboxSearchForeColor;

            // Colors for email body
            _itemViewer.TxtboxBody.BackColor = TxtboxBodyBackColor;
            _itemViewer.TxtboxBody.ForeColor = TxtboxBodyForeColor;

            // TODO: Override the draw function because these colors do not work as expected
            _itemViewer.CboFolders.BackColor = CboFoldersBackColor;
            _itemViewer.CboFolders.ForeColor = CboFoldersForeColor;
            
            _itemViewer.TopicThread.BackColor = DefaultBackColor;
            _itemViewer.TopicThread.ForeColor = DefaultForeColor;
            
            var headerstyle = new HeaderFormatStyle();
            headerstyle.SetBackColor(DefaultBackColor);
            headerstyle.SetForeColor(DefaultForeColor);

            foreach (OLVColumn column in _itemViewer.TopicThread.Columns)
            {
                column.HeaderFormatStyle = headerstyle;    
            }

            if (_itemViewer.L0v2h2_Web.CoreWebView2 is not null)
            {
                _itemViewer.L0v2h2_Web.CoreWebView2.Profile.PreferredColorScheme = Web2ViewScheme;
                HtmlConverter(HtmlDark);
            }
            
            // Default colors   
            _itemViewer.BackColor = DefaultBackColor;
            _itemViewer.ForeColor = DefaultForeColor;
        }

    }
}


