using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler
{
    internal static class QfcThemeHelper
    {
        public static void SetTheme(this TableLayoutPanel tlp, Color backColor)
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
        }

        public static void SetTheme(this Control control, Color backColor, Color forecolor)
        {
            control.BackColor = backColor;
            control.ForeColor = forecolor;
        }

        public static Dictionary<string, Theme> SetupThemes(
            IQfcItemController controller,
            ItemViewer viewer,
            Action<Enums.ToggleState> htmlConverter,
            UtilitiesCS.Threading.IUiDispatcher uiDispatcher
        )
        {
            if (controller is null)
            {
                throw new ArgumentNullException(nameof(controller));
            }
            if (viewer is null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }

            return SetupThemes(
                BuildProductionControlSet(controller, viewer, htmlConverter, uiDispatcher)
            );
        }

        internal static QfcThemeControlSet BuildProductionControlSet(
            IQfcItemController controller,
            ItemViewer viewer,
            Action<Enums.ToggleState> htmlConverter,
            UtilitiesCS.Threading.IUiDispatcher uiDispatcher
        )
        {
            if (controller is null)
            {
                throw new ArgumentNullException(nameof(controller));
            }
            if (viewer is null)
            {
                throw new ArgumentNullException(nameof(viewer));
            }

            return new QfcThemeControlSet(
                viewer.LblItemNumber,
                viewer.LblSender,
                viewer.LblSubject,
                controller.TableLayoutPanels,
                controller.Buttons,
                viewer.MenuItems,
                viewer.MoveOptionsStrip,
                controller.ListTipsDetails,
                controller.ListTipsExpanded,
                viewer.TxtboxSearch,
                viewer.TxtboxBody,
                viewer.L0vhBreadcrumb_WebView2,
                theme => viewer.BreadcrumbCoordinator?.SetTheme(theme),
                viewer.TopicThread,
                viewer.L0v2h2_WebView2,
                viewer,
                () => !controller.Mail.UnRead,
                htmlConverter,
                uiDispatcher
            );
        }

        internal static Dictionary<string, Theme> SetupThemes(QfcThemeControlSet controlSet)
        {
            if (controlSet is null)
            {
                throw new ArgumentNullException(nameof(controlSet));
            }

            return new Dictionary<string, Theme>
            {
                {
                    "LightNormal",
                    CreateTheme(
                        controlSet,
                        "LightNormal",
                        CoreWebView2PreferredColorScheme.Light,
                        Enums.ToggleState.Off,
                        SystemColors.HotTrack,
                        SystemColors.Control,
                        SystemColors.Control,
                        Color.Black,
                        Color.White,
                        // issue #269: CreateTheme positional order is (mailReadForeColor,
                        // mailReadBackColor, mailUnreadForeColor, mailUnreadBackColor). In Light
                        // themes the Sender/Subject labels are dark text on a light background;
                        // unread uses blue text as the accent (not a blue/black background).
                        SystemColors.ControlText,
                        SystemColors.Control,
                        Color.MediumBlue,
                        SystemColors.Control,
                        Color.Black,
                        Color.White,
                        SystemColors.Control,
                        SystemColors.ControlDark,
                        Color.LightSkyBlue,
                        SystemColors.Window,
                        SystemColors.WindowText,
                        SystemColors.Control,
                        SystemColors.ControlText,
                        SystemColors.Window,
                        SystemColors.WindowText,
                        SystemColors.Control,
                        SystemColors.ControlText
                    )
                },
                {
                    "LightActive",
                    CreateTheme(
                        controlSet,
                        "LightActive",
                        CoreWebView2PreferredColorScheme.Light,
                        Enums.ToggleState.Off,
                        Color.Green,
                        SystemColors.Control,
                        Color.LightCyan,
                        Color.Black,
                        Color.White,
                        // issue #269: CreateTheme positional order is (mailReadForeColor,
                        // mailReadBackColor, mailUnreadForeColor, mailUnreadBackColor). In Light
                        // themes the Sender/Subject labels are dark text on a light background;
                        // unread uses blue text as the accent (not a blue/light-cyan background).
                        SystemColors.ControlText,
                        Color.LightCyan,
                        Color.MediumBlue,
                        Color.LightCyan,
                        Color.Black,
                        Color.White,
                        Color.LightCyan,
                        Color.DarkCyan,
                        Color.LightSkyBlue,
                        SystemColors.Window,
                        SystemColors.WindowText,
                        Color.LightCyan,
                        SystemColors.ControlText,
                        SystemColors.Window,
                        SystemColors.WindowText,
                        Color.LightCyan,
                        SystemColors.ControlText
                    )
                },
                {
                    "DarkNormal",
                    CreateTheme(
                        controlSet,
                        "DarkNormal",
                        CoreWebView2PreferredColorScheme.Dark,
                        Enums.ToggleState.On,
                        Color.FromArgb(64, 64, 64),
                        SystemColors.Control,
                        Color.Black,
                        Color.LightSkyBlue,
                        SystemColors.ActiveCaptionText,
                        Color.WhiteSmoke,
                        Color.Black,
                        Color.Goldenrod,
                        Color.Black,
                        Color.LightSkyBlue,
                        SystemColors.ActiveCaptionText,
                        Color.DimGray,
                        Color.DarkGray,
                        Color.LightSkyBlue,
                        Color.FromArgb(30, 30, 30),
                        Color.WhiteSmoke,
                        Color.Black,
                        Color.WhiteSmoke,
                        Color.DimGray,
                        Color.WhiteSmoke,
                        Color.Black,
                        Color.WhiteSmoke
                    )
                },
                {
                    "DarkActive",
                    CreateTheme(
                        controlSet,
                        "DarkActive",
                        CoreWebView2PreferredColorScheme.Dark,
                        Enums.ToggleState.On,
                        SystemColors.HotTrack,
                        SystemColors.Control,
                        Color.FromArgb(64, 64, 64),
                        Color.LightSkyBlue,
                        SystemColors.ActiveCaptionText,
                        Color.WhiteSmoke,
                        Color.FromArgb(64, 64, 64),
                        Color.Goldenrod,
                        Color.FromArgb(64, 64, 64),
                        Color.LightSkyBlue,
                        SystemColors.ActiveCaptionText,
                        Color.DimGray,
                        Color.DarkGray,
                        Color.LightSkyBlue,
                        Color.FromArgb(30, 30, 30),
                        Color.WhiteSmoke,
                        Color.FromArgb(64, 64, 64),
                        Color.WhiteSmoke,
                        Color.DimGray,
                        Color.WhiteSmoke,
                        Color.FromArgb(64, 64, 64),
                        Color.WhiteSmoke
                    )
                },
            };
        }

        public static Dictionary<string, Theme> SetupFormThemes(
            IList<Control> panels,
            IList<Control> buttons
        )
        {
            var lightNormal = new Dictionary<string, ThemeControlGroup>
            {
                {
                    "Default2Color",
                    new ThemeControlGroup(
                        controls: panels,
                        back: SystemColors.Control,
                        fore: SystemColors.ControlText
                    )
                },
                {
                    "Buttons",
                    new ThemeControlGroup(
                        controls: buttons,
                        backMain: SystemColors.Control,
                        foreMain: SystemColors.ControlText,
                        backAlt: SystemColors.Control,
                        foreAlt: SystemColors.ControlText,
                        hover: Color.LightCyan,
                        isAltHover: (x) => false
                    )
                },
            };
            var darkNormal = new Dictionary<string, ThemeControlGroup>
            {
                {
                    "Default2Color",
                    new ThemeControlGroup(
                        controls: panels,
                        back: Color.Black,
                        fore: Color.WhiteSmoke
                    )
                },
                {
                    "Buttons",
                    new ThemeControlGroup(
                        controls: buttons,
                        backMain: Color.DimGray,
                        foreMain: Color.WhiteSmoke,
                        backAlt: Color.DimGray,
                        foreAlt: Color.WhiteSmoke,
                        hover: Color.DarkGray,
                        isAltHover: (x) => false
                    )
                },
            };
            return new Dictionary<string, Theme>
            {
                { "LightNormal", new Theme("LightNormal", lightNormal) },
                { "DarkNormal", new Theme("DarkNormal", darkNormal) },
            };
        }

        private static Theme CreateTheme(
            QfcThemeControlSet controlSet,
            string name,
            CoreWebView2PreferredColorScheme web2ViewScheme,
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
            Color defaultForeColor
        )
        {
            return new Theme(
                name: name,
                lblItemNumber: controlSet.LblItemNumber,
                lblSender: controlSet.LblSender,
                lblSubject: controlSet.LblSubject,
                tableLayoutPanels: controlSet.TableLayoutPanels,
                buttons: controlSet.Buttons,
                menuItems: controlSet.MenuItems,
                menuStrip: controlSet.MenuStrip,
                tipsDetailsLabels: controlSet.TipsDetailsLabels,
                tipsExpanded: controlSet.TipsExpanded,
                textboxSearch: controlSet.TextboxSearch,
                textboxBody: controlSet.TextboxBody,
                breadcrumbWebView2: controlSet.BreadcrumbWebView2,
                breadcrumbThemeNotifier: controlSet.BreadcrumbThemeNotifier,
                topicThread: controlSet.TopicThread,
                webView2: controlSet.WebView2,
                viewer: controlSet.Viewer,
                mailRead: controlSet.MailRead,
                web2ViewScheme: web2ViewScheme,
                htmlConverter: controlSet.HtmlConverter,
                htmlDark: htmlDark,
                navBackgColor: navBackgColor,
                navForeColor: navForeColor,
                tlpBackColor: tlpBackColor,
                tipsForeColor: tipsForeColor,
                tipsBackColor: tipsBackColor,
                mailReadBackColor: mailReadBackColor,
                mailReadForeColor: mailReadForeColor,
                mailUnreadBackColor: mailUnreadBackColor,
                mailUnreadForeColor: mailUnreadForeColor,
                tipsDetailsBackColor: tipsDetailsBackColor,
                tipsDetailsForeColor: tipsDetailsForeColor,
                buttonBackColor: buttonBackColor,
                buttonMouseOverColor: buttonMouseOverColor,
                buttonClickedColor: buttonClickedColor,
                txtboxSearchBackColor: txtboxSearchBackColor,
                txtboxSearchForeColor: txtboxSearchForeColor,
                txtboxBodyBackColor: txtboxBodyBackColor,
                txtboxBodyForeColor: txtboxBodyForeColor,
                cboFoldersBackColor: cboFoldersBackColor,
                cboFoldersForeColor: cboFoldersForeColor,
                defaultBackColor: defaultBackColor,
                defaultForeColor: defaultForeColor,
                uiDispatcher: controlSet.UiDispatcher
            );
        }
    }
}
