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
            _txtboxSearchBackColor = txtboxSearchBackColor;
            _txtboxSearchForeColor = txtboxSearchForeColor;
            _txtboxBodyBackColor = txtboxBodyBackColor;
            _txtboxBodyForeColor = txtboxBodyForeColor;
            _cboFoldersBackColor = cboFoldersBackColor;
            _cboFoldersForeColor = cboFoldersForeColor;
            _defaultBackColor = defaultBackColor;
            _defaultForeColor = defaultForeColor;
        }

        private string _name;
        private Color _navBackColor;
        private Color _navForeColor;
        private Color _tlpBackColor;
        private Color _tipsForeColor;
        private Color _tipsBackColor;
        private Color _mailReadForeColor;
        private Color _mailReadBackColor;
        private Color _mailUnreadForeColor;
        private Color _mailUnreadBackColor;
        private Color _tipsDetailsBackColor;
        private Color _tipsDetailsForeColor;
        private Color _buttonBackColor;
        private Color _txtboxSearchBackColor;
        private Color _txtboxSearchForeColor;
        private Color _txtboxBodyBackColor;
        private Color _txtboxBodyForeColor;
        private Color _cboFoldersBackColor;
        private Color _cboFoldersForeColor;
        private Color _defaultBackColor;
        private Color _defaultForeColor;
        private Action<Enums.ToggleState> _htmlConverter;
        private Enums.ToggleState _htmlDark;
        private QfcItemViewer _itemViewer;
        private IQfcItemController _parent;
        private Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme _web2ViewScheme;

        public string Name { get => _name; set => _name = value; }
        public Color TlpBackColor { get => _tlpBackColor; set => _tlpBackColor = value; }
        public Color TipsForeColor { get => _tipsForeColor; set => _tipsForeColor = value; }
        public Color TipsBackColor { get => _tipsBackColor; set => _tipsBackColor = value; }
        public Color MailReadForeColor { get => _mailReadForeColor; set => _mailReadForeColor = value; }
        public Color MailReadBackColor { get => _mailReadBackColor; set => _mailReadBackColor = value; }
        public Color MailUnreadForeColor { get => _mailUnreadForeColor; set => _mailUnreadForeColor = value; }
        public Color MailUnreadBackColor { get => _mailUnreadBackColor; set => _mailUnreadBackColor = value; }
        public Color TipsDetailsForeColor { get => _tipsDetailsForeColor; set => _tipsDetailsForeColor = value; }
        public Color TipsDetailsBackColor { get => _tipsDetailsBackColor; set => _tipsDetailsBackColor = value; }
        public Color ButtonBackColor { get => _buttonBackColor; set => _buttonBackColor = value; }
        public Color TxtboxSearchBackColor { get => _txtboxSearchBackColor; set => _txtboxSearchBackColor = value; }
        public Color TxtboxSearchForeColor { get => _txtboxSearchForeColor; set => _txtboxSearchForeColor = value; }
        public Color TxtboxBodyBackColor { get => _txtboxBodyBackColor; set => _txtboxBodyBackColor = value; }
        public Color TxtboxBodyForeColor { get => _txtboxBodyForeColor; set => _txtboxBodyForeColor = value; }
        public Color CboFoldersBackColor { get => _cboFoldersBackColor; set => _cboFoldersBackColor = value; }
        public Color CboFoldersForeColor { get => _cboFoldersForeColor; set => _cboFoldersForeColor = value; }
        public Color DefaultBackColor { get => _defaultBackColor; set => _defaultBackColor = value; }
        public Color DefaultForeColor { get => _defaultForeColor; set => _defaultForeColor = value; }
        public Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme Web2ViewScheme { get => _web2ViewScheme; set => _web2ViewScheme = value; }
        public Action<Enums.ToggleState> HtmlConverter { get => _htmlConverter; set => _htmlConverter = value; }
        public Enums.ToggleState HtmlDark { get => _htmlDark; set => _htmlDark = value; }

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
            _itemViewer.LblPos.SetTheme(backColor: _navBackColor,
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
                btn.SetTheme(backColor: ButtonBackColor);
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


