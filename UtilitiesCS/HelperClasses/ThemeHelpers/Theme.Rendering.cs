using System.Windows.Forms;
using BrightIdeasSoftware;

namespace UtilitiesCS
{
    public partial class Theme
    {
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
            // why (issue #254): the read-state probe MailRead() reads MailItem.UnRead on a
            // COM object that can be stale/moved/deleted in the High-Confidence view, which
            // throws a COMException. That throw previously aborted this renderer before the
            // sender/subject labels were recolored, leaving them at the prior theme's colors
            // after a dark/light toggle. Evaluate the probe defensively at this UI boundary so
            // a probe fault cannot skip re-theming; default to unread coloring (still within
            // the current theme family) when the read state cannot be determined. The catch is
            // deliberately narrow to COMException — unrelated exceptions must still propagate.
            bool isRead;
            try
            {
                isRead = MailRead();
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                isRead = false;
            }

            if (!isRead)
            {
                SetMailUnread();
            }
            else
            {
                SetMailRead();
            }

            // Button colors
            foreach (Button btn in _buttons)
            {
                if (btn.DialogResult == DialogResult.OK)
                {
                    btn.BackColor = ButtonClickedColor;
                }
                else
                {
                    btn.BackColor = ButtonBackColor;
                }
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

            // #351: the folder control is the WebView2 breadcrumb; dark/light switching uses the
            // existing WebView2 mechanism (PreferredColorScheme, pattern below for the body pane)
            // plus a themeChange bridge message that swaps the page's CSS custom properties.
            if (_breadcrumbWebView2?.CoreWebView2 is not null)
            {
                _breadcrumbWebView2.CoreWebView2.Profile.PreferredColorScheme = Web2ViewScheme;
            }
            _breadcrumbThemeNotifier?.Invoke(
                Web2ViewScheme == Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Dark
                    ? "dark"
                    : "light"
            );

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
    }
}
