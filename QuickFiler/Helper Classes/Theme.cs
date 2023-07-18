using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;

namespace QuickFiler.Helper_Classes
{
    public class Theme
    {
        public Theme() { }
        public Theme(string name,
                     QfcItemViewer itemViewer,
                     IQfcItemController parent,
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

        private QfcItemViewer _itemViewer;
        private IQfcItemController _parent;

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

        public void SetMailRead()
        {
            _itemViewer.Invoke(new System.Action(() =>
            {
                _itemViewer.LblSender.SetTheme(backColor: _mailReadBackColor,
                                               forecolor: _mailReadForeColor);
            _itemViewer.lblSubject.SetTheme(backColor: _mailReadBackColor,
                                            forecolor: _mailReadForeColor);
            }));
        }

        public void SetMailUnread()
        {
            _itemViewer.Invoke(new System.Action(() =>
            {
                _itemViewer.LblSender.SetTheme(backColor: _mailUnreadBackColor,
                                               forecolor: _mailUnreadForeColor);
                _itemViewer.lblSubject.SetTheme(backColor: _mailUnreadBackColor,
                                                forecolor: _mailUnreadForeColor);
            }));
        }

        public void SetTheme()
        {
            _itemViewer.Invoke(new System.Action(() =>
            {
                _itemViewer.LblPos.SetTheme(backColor: _navBackColor,
                                            forecolor: _navForeColor);

                foreach (TableLayoutPanel tlp in _parent.TableLayoutPanels)
                {
                    tlp.SetTheme(backColor: TlpBackColor);
                }

                foreach (IQfcTipsDetails tipsDetails in _parent.ListTipsDetails)
                {
                    tipsDetails.LabelControl.SetTheme(backColor: TipsDetailsBackColor,
                                                      forecolor: TipsDetailsForeColor);
                }

                if (_parent.Mail.UnRead == true) { SetMailUnread(); }
                else { SetMailRead(); }

                foreach (Button btn in _parent.Buttons)
                {
                    btn.SetTheme(backColor: ButtonBackColor);
                }

                _itemViewer.TxtboxSearch.BackColor = TxtboxSearchBackColor;
                _itemViewer.TxtboxSearch.ForeColor = TxtboxSearchForeColor;

                _itemViewer.TxtboxBody.BackColor = TxtboxBodyBackColor;
                _itemViewer.TxtboxBody.ForeColor = TxtboxBodyForeColor;
                _itemViewer.CboFolders.BackColor = CboFoldersBackColor;
                _itemViewer.CboFolders.ForeColor = CboFoldersForeColor;
                _itemViewer.BackColor = DefaultBackColor;
                _itemViewer.ForeColor = DefaultForeColor;

            }));
        }

    }
}


