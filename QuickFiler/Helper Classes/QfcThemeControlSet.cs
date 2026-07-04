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
            ComboBox comboFolders,
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
            ComboFolders = comboFolders ?? throw new ArgumentNullException(nameof(comboFolders));
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

        internal ComboBox ComboFolders { get; }

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
