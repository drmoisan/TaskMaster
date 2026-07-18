using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.Web.WebView2.WinForms;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace QuickFiler
{
    internal sealed class QfcThemeControlSet
    {
        internal QfcThemeControlSet(
            Label lblItemNumber,
            Label lblSender,
            Label lblSubject,
            IList<TableLayoutPanel> tableLayoutPanels,
            IList<Button> buttons,
            IList<Component> menuItems,
            MenuStrip menuStrip,
            IList<IQfcTipsDetails> tipsDetailsLabels,
            IList<IQfcTipsDetails> tipsExpanded,
            TextBox textboxSearch,
            TextBox textboxBody,
            WebView2 breadcrumbWebView2,
            Action<string> breadcrumbThemeNotifier,
            FastObjectListView topicThread,
            WebView2 webView2,
            Control viewer,
            Func<bool> mailRead,
            Action<Enums.ToggleState> htmlConverter,
            IUiDispatcher uiDispatcher
        )
        {
            LblItemNumber = lblItemNumber ?? throw new ArgumentNullException(nameof(lblItemNumber));
            LblSender = lblSender ?? throw new ArgumentNullException(nameof(lblSender));
            LblSubject = lblSubject ?? throw new ArgumentNullException(nameof(lblSubject));
            TableLayoutPanels = RequireCollection(tableLayoutPanels, nameof(tableLayoutPanels));
            Buttons = RequireCollection(buttons, nameof(buttons));
            MenuItems = RequireCollection(menuItems, nameof(menuItems));
            MenuStrip = menuStrip ?? throw new ArgumentNullException(nameof(menuStrip));
            TipsDetailsLabels = RequireCollection(tipsDetailsLabels, nameof(tipsDetailsLabels));
            TipsExpanded = RequireCollection(tipsExpanded, nameof(tipsExpanded));
            TextboxSearch = textboxSearch ?? throw new ArgumentNullException(nameof(textboxSearch));
            TextboxBody = textboxBody ?? throw new ArgumentNullException(nameof(textboxBody));
            BreadcrumbWebView2 =
                breadcrumbWebView2 ?? throw new ArgumentNullException(nameof(breadcrumbWebView2));
            BreadcrumbThemeNotifier =
                breadcrumbThemeNotifier
                ?? throw new ArgumentNullException(nameof(breadcrumbThemeNotifier));
            TopicThread = topicThread ?? throw new ArgumentNullException(nameof(topicThread));
            WebView2 = webView2 ?? throw new ArgumentNullException(nameof(webView2));
            Viewer = viewer ?? throw new ArgumentNullException(nameof(viewer));
            MailRead = mailRead ?? throw new ArgumentNullException(nameof(mailRead));
            HtmlConverter = htmlConverter ?? throw new ArgumentNullException(nameof(htmlConverter));
            UiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));
        }

        internal Label LblItemNumber { get; }

        internal Label LblSender { get; }

        internal Label LblSubject { get; }

        internal IList<TableLayoutPanel> TableLayoutPanels { get; }

        internal IList<Button> Buttons { get; }

        internal IList<Component> MenuItems { get; }

        internal MenuStrip MenuStrip { get; }

        internal IList<IQfcTipsDetails> TipsDetailsLabels { get; }

        internal IList<IQfcTipsDetails> TipsExpanded { get; }

        internal TextBox TextboxSearch { get; }

        internal TextBox TextboxBody { get; }

        // #351: the folder control is the WebView2 breadcrumb; the notifier posts the themeChange
        // bridge message through the viewer's coordinator.
        internal WebView2 BreadcrumbWebView2 { get; }

        internal Action<string> BreadcrumbThemeNotifier { get; }

        internal FastObjectListView TopicThread { get; }

        internal WebView2 WebView2 { get; }

        internal Control Viewer { get; }

        internal Func<bool> MailRead { get; }

        internal Action<Enums.ToggleState> HtmlConverter { get; }

        internal IUiDispatcher UiDispatcher { get; }

        private static IList<T> RequireCollection<T>(IList<T> value, string parameterName)
        {
            if (value is null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }
    }
}
