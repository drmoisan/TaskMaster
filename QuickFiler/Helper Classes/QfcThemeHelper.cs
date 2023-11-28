using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;
using System.ComponentModel;

namespace QuickFiler
{
    internal static class QfcThemeHelper
    {
        public static void SetTheme(this TableLayoutPanel tlp, 
                                    Color backColor)
        {
            tlp.BackColor = backColor;
        }
        
        public static void SetTheme(this Label lbl, Color backColor, Color forecolor)
        {
            lbl.BackColor = backColor;
            lbl.ForeColor = forecolor;
        }

        public static void SetTheme(this Button btn, Color backColor)
        {
            btn.BackColor = backColor;
            //btn.ForeColor = forecolor;
        }

        public static void SetTheme(this Control control, Color backColor, Color forecolor)
        {
            control.BackColor = backColor;
            control.ForeColor = forecolor;
        }

        public static Dictionary<string, Theme> SetupThemes(IQfcItemController controller,
                                                           ItemViewer viewer,
                                                           Action<Enums.ToggleState> htmlConverter)
        {
            var themes = new Dictionary<string, Theme>
            {
                {
                    "LightNormal",
                    new Theme(name: "LightNormal",
                              lblItemNumber: viewer.LblItemNumber,
                              lblSender: viewer.LblSender,
                              lblSubject: viewer.LblSubject,
                              tableLayoutPanels: controller.TableLayoutPanels,
                              buttons: controller.Buttons,
                              menuItems: viewer.MenuItems,
                              menuStrip: viewer.MoveOptionsStrip,
                              tipsDetailsLabels: controller.ListTipsDetails,
                              tipsExpanded: controller.ListTipsExpanded,
                              textboxSearch: viewer.TxtboxSearch,
                              textboxBody: viewer.TxtboxBody,
                              comboFolders: viewer.CboFolders,
                              topicThread: viewer.TopicThread,
                              webView2: viewer.L0v2h2_WebView2,
                              viewer: (Control)viewer,
                              mailRead: new Func<bool>(()=>!controller.Mail.UnRead),
                              web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Light,
                              htmlConverter: htmlConverter,
                              htmlDark: Enums.ToggleState.Off,
                              navBackgColor: SystemColors.HotTrack,
                              navForeColor: SystemColors.Control,
                              tlpBackColor: SystemColors.Control,
                              tipsForeColor: Color.Black,
                              tipsBackColor: Color.White,
                              mailReadBackColor: SystemColors.Control,
                              mailReadForeColor: SystemColors.ControlText,
                              mailUnreadBackColor: SystemColors.Control,
                              mailUnreadForeColor: Color.MediumBlue,
                              tipsDetailsBackColor: Color.Black,
                              tipsDetailsForeColor: Color.White,
                              buttonBackColor: SystemColors.Control,
                              buttonMouseOverColor: SystemColors.ControlDark,
                              buttonClickedColor: Color.LightSkyBlue,
                              txtboxSearchBackColor: SystemColors.Window,
                              txtboxSearchForeColor: SystemColors.WindowText,
                              txtboxBodyBackColor: SystemColors.Control,
                              txtboxBodyForeColor: SystemColors.ControlText,
                              cboFoldersBackColor: SystemColors.Window,
                              cboFoldersForeColor: SystemColors.WindowText,
                              defaultBackColor: SystemColors.Control,
                              defaultForeColor: SystemColors.ControlText)
                    },
                {
                    "LightActive",
                    new Theme(name: "LightActive",
                              lblItemNumber: viewer.LblItemNumber,
                              lblSender: viewer.LblSender,
                              lblSubject: viewer.LblSubject,
                              tableLayoutPanels: controller.TableLayoutPanels,
                              buttons: controller.Buttons,
                              menuItems: viewer.MenuItems,
                              menuStrip: viewer.MoveOptionsStrip,
                              tipsDetailsLabels: controller.ListTipsDetails,
                              tipsExpanded: controller.ListTipsExpanded,
                              textboxSearch: viewer.TxtboxSearch,
                              textboxBody: viewer.TxtboxBody,
                              comboFolders: viewer.CboFolders,
                              topicThread: viewer.TopicThread,
                              webView2: viewer.L0v2h2_WebView2,
                              viewer: (Control)viewer,
                              mailRead: new Func<bool>(()=>!controller.Mail.UnRead),
                              web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Light,
                              htmlConverter: htmlConverter,
                              htmlDark: Enums.ToggleState.Off,
                              navBackgColor: Color.Green,
                              navForeColor: SystemColors.Control,
                              tlpBackColor: Color.LightCyan,
                              tipsForeColor: Color.Black,
                              tipsBackColor: Color.White,
                              mailReadBackColor: Color.LightCyan,
                              mailReadForeColor: SystemColors.ControlText,
                              mailUnreadBackColor: Color.LightCyan,
                              mailUnreadForeColor: Color.MediumBlue,
                              tipsDetailsBackColor: Color.Black,
                              tipsDetailsForeColor: Color.White,
                              buttonBackColor: Color.LightCyan,
                              buttonMouseOverColor: Color.DarkCyan,
                              buttonClickedColor: Color.LightSkyBlue,
                              txtboxSearchBackColor: SystemColors.Window,
                              txtboxSearchForeColor: SystemColors.WindowText,
                              txtboxBodyBackColor: Color.LightCyan,
                              txtboxBodyForeColor: SystemColors.ControlText,
                              cboFoldersBackColor: SystemColors.Window,
                              cboFoldersForeColor: SystemColors.WindowText,
                              defaultBackColor: Color.LightCyan,
                              defaultForeColor: SystemColors.ControlText)
                    },
                {
                    "DarkNormal",
                    new Theme(name: "DarkNormal",
                              lblItemNumber: viewer.LblItemNumber,
                              lblSender: viewer.LblSender,
                              lblSubject: viewer.LblSubject,
                              tableLayoutPanels: controller.TableLayoutPanels,
                              buttons: controller.Buttons,
                              menuItems: viewer.MenuItems,
                              menuStrip: viewer.MoveOptionsStrip,
                              tipsDetailsLabels: controller.ListTipsDetails,
                              tipsExpanded: controller.ListTipsExpanded,
                              textboxSearch: viewer.TxtboxSearch,
                              textboxBody: viewer.TxtboxBody,
                              comboFolders: viewer.CboFolders,
                              topicThread: viewer.TopicThread,
                              webView2: viewer.L0v2h2_WebView2,
                              viewer: (Control)viewer,
                              mailRead: new Func<bool>(()=>!controller.Mail.UnRead),
                              web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Dark,
                              htmlConverter: htmlConverter,
                              htmlDark: Enums.ToggleState.On,
                              navBackgColor: Color.FromArgb(64,64,64),
                              navForeColor: SystemColors.Control,
                              tlpBackColor: Color.Black,
                              tipsForeColor: Color.LightSkyBlue,
                              tipsBackColor: SystemColors.ActiveCaptionText,
                              mailReadForeColor: Color.WhiteSmoke,
                              mailReadBackColor: Color.Black,
                              mailUnreadForeColor: Color.Goldenrod,
                              mailUnreadBackColor: Color.Black,
                              tipsDetailsBackColor: Color.LightSkyBlue,
                              tipsDetailsForeColor: SystemColors.ActiveCaptionText,
                              buttonBackColor: Color.DimGray,
                              buttonMouseOverColor: Color.DarkGray,
                              buttonClickedColor: Color.LightSkyBlue,
                              txtboxSearchBackColor: Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30))))),
                              txtboxSearchForeColor: Color.WhiteSmoke,
                              txtboxBodyBackColor: Color.Black,
                              txtboxBodyForeColor: Color.WhiteSmoke,
                              cboFoldersBackColor: Color.DimGray,
                              cboFoldersForeColor: Color.WhiteSmoke,
                              defaultBackColor: Color.Black,
                              defaultForeColor: Color.WhiteSmoke)
                },
                {
                    "DarkActive",
                    new Theme(name: "DarkActive",
                              lblItemNumber: viewer.LblItemNumber,
                              lblSender: viewer.LblSender,
                              lblSubject: viewer.LblSubject,
                              tableLayoutPanels: controller.TableLayoutPanels,
                              buttons: controller.Buttons,
                              menuItems: viewer.MenuItems,
                              menuStrip: viewer.MoveOptionsStrip,
                              tipsDetailsLabels: controller.ListTipsDetails,
                              tipsExpanded: controller.ListTipsExpanded,
                              textboxSearch: viewer.TxtboxSearch,
                              textboxBody: viewer.TxtboxBody,
                              comboFolders: viewer.CboFolders,
                              topicThread: viewer.TopicThread,
                              webView2: viewer.L0v2h2_WebView2,
                              viewer: (Control)viewer,
                              mailRead: new Func<bool>(()=>!controller.Mail.UnRead),
                              web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Dark,
                              htmlConverter: htmlConverter,
                              htmlDark: Enums.ToggleState.On,
                              navBackgColor: SystemColors.HotTrack,
                              navForeColor: SystemColors.Control,
                              tlpBackColor: Color.FromArgb(64,64,64),
                              tipsForeColor: Color.LightSkyBlue,
                              tipsBackColor: SystemColors.ActiveCaptionText,
                              mailReadForeColor: Color.WhiteSmoke,
                              mailReadBackColor: Color.FromArgb(64,64,64),
                              mailUnreadForeColor: Color.Goldenrod,
                              mailUnreadBackColor: Color.FromArgb(64,64,64),
                              tipsDetailsBackColor: Color.LightSkyBlue,
                              tipsDetailsForeColor: SystemColors.ActiveCaptionText,
                              buttonBackColor: Color.DimGray,
                              buttonMouseOverColor: Color.DarkGray,
                              buttonClickedColor: Color.LightSkyBlue,
                              txtboxSearchBackColor: Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30))))),
                              txtboxSearchForeColor: Color.WhiteSmoke,
                              txtboxBodyBackColor: Color.FromArgb(64,64,64),
                              txtboxBodyForeColor: Color.WhiteSmoke,
                              cboFoldersBackColor: Color.DimGray,
                              cboFoldersForeColor: Color.WhiteSmoke,
                              defaultBackColor: Color.FromArgb(64,64,64),
                              defaultForeColor: Color.WhiteSmoke)
                }
            };
            return themes;
        }

        public static Dictionary<string, Theme> SetupFormThemes(IList<Control> panels, IList<Control> buttons) 
        {
            var darkDarkGrey = Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30))))); //30,30,30
            var lightNormal = new Dictionary<string, ThemeControlGroup>
            {
                //{ "Nav", new ThemeControlGroup(controls: nav, back: SystemColors.HotTrack, fore: SystemColors.Control) },
                //{ "Tips", new ThemeControlGroup(controls: tips, back: SystemColors.ControlText, fore: SystemColors.Control) },
                //{ "highlighted", new ThemeControlGroup(controls: highlighted, back: SystemColors.Window, fore: SystemColors.ControlText) },
                { "Default2Color", new ThemeControlGroup(controls: panels, back: SystemColors.Control, fore: SystemColors.ControlText) },
                { "Buttons", new ThemeControlGroup(controls: buttons, backMain: SystemColors.Control, foreMain: SystemColors.ControlText, backAlt: SystemColors.Control, foreAlt: SystemColors.ControlText, hover: Color.LightCyan, isAltHover: (x) => false) },
                //{ "CheckBoxes", new ThemeControlGroup(controls: checkboxes, backMain: SystemColors.Control, foreMain: SystemColors.ControlText, backAlt: SystemColors.Control, foreAlt: SystemColors.ControlText, hover: Color.LightCyan, isAltHover: (x) => ((CheckBox)x).Checked ) },
            };
            var darkNormal = new Dictionary<string, ThemeControlGroup>
            {
                //{ "Nav", new ThemeControlGroup(controls: nav, back: Color.FromArgb(64,64,64), fore: SystemColors.Control) },
                //{ "Tips", new ThemeControlGroup(controls: tips, back: Color.LightSkyBlue, fore: SystemColors.ControlText) },
                //{ "highlighted", new ThemeControlGroup(controls: highlighted, back: darkDarkGrey, fore: Color.WhiteSmoke) },
                { "Default2Color", new ThemeControlGroup(controls: panels, back: Color.Black, fore: Color.WhiteSmoke) },
                { "Buttons", new ThemeControlGroup(controls: buttons, backMain: Color.DimGray, foreMain: Color.WhiteSmoke, backAlt: Color.DimGray, foreAlt: Color.WhiteSmoke, hover: Color.DarkGray, isAltHover: (x) => false ) },
                //{ "CheckBoxes", new ThemeControlGroup(controls: checkboxes, backMain: Color.Black, foreMain: Color.WhiteSmoke, backAlt: Color.Black, foreAlt: Color.WhiteSmoke, hover: Color.DarkGray, isAltHover: (x) => ((CheckBox)x).Checked ) },
            };
            var themes = new Dictionary<string, Theme>
            {
                { "LightNormal", new Theme("LightNormal", lightNormal) },
                //{ "LightActive", new Theme("LightActive", lightActive) },
                { "DarkNormal", new Theme("DarkNormal", darkNormal) },
                //{ "DarkActive", new Theme("DarkActive", darkActive) }
            };
            return themes;
        }
    }
}
