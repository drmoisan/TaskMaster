using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.HelperClasses.ThemeHelpers
{
    /// <summary>
    /// Regression coverage for issue #254: toggling dark/light must re-theme the mail
    /// sender/subject labels on every QuickFiler item, even when the injected read-state
    /// probe (<c>MailRead</c>) throws on a stale/moved Outlook <c>MailItem</c>. The tests
    /// reuse the handle-less big-constructor doubles pattern proven in
    /// <see cref="Theme_DispatcherTests"/> so the private synchronous
    /// <c>SetQfcTheme()</c> renderer runs on the test thread with no live Outlook, no COM,
    /// no dispatcher, and no temp files. The three cases give full branch coverage of the
    /// changed read-state block: probe returns true, probe returns false, probe throws.
    /// </summary>
    [TestClass]
    public class Theme_MailLabelThemingTests
    {
        private static readonly Color PreviousThemeSentinel = Color.Magenta;
        private static readonly Color UnreadBack = Color.Navy;
        private static readonly Color UnreadFore = Color.White;
        private static readonly Color ReadBack = Color.Maroon;
        private static readonly Color ReadFore = Color.Gainsboro;

        /// <summary>
        /// Builds a fully handle-less <see cref="Theme"/> via the big constructor. The two
        /// mail labels are returned via out parameters and pre-set to a distinct
        /// "previous-theme" sentinel color so a stale label is observable.
        /// </summary>
        private static Theme BuildTheme(
            Func<bool> mailRead,
            out Label lblSender,
            out Label lblSubject
        )
        {
            lblSender = new Label
            {
                BackColor = PreviousThemeSentinel,
                ForeColor = PreviousThemeSentinel,
            };
            lblSubject = new Label
            {
                BackColor = PreviousThemeSentinel,
                ForeColor = PreviousThemeSentinel,
            };

            return new Theme(
                name: "LightNormal",
                lblItemNumber: new Label(),
                lblSender: lblSender,
                lblSubject: lblSubject,
                tableLayoutPanels: new List<TableLayoutPanel>(),
                buttons: new List<Button>(),
                menuItems: new List<System.ComponentModel.Component>(),
                menuStrip: new MenuStrip(),
                tipsDetailsLabels: new List<IQfcTipsDetails>(),
                tipsExpanded: new List<IQfcTipsDetails>(),
                textboxSearch: new TextBox(),
                textboxBody: new TextBox(),
                breadcrumbWebView2: null,
                topicThread: new BrightIdeasSoftware.FastObjectListView(),
                webView2: new Microsoft.Web.WebView2.WinForms.WebView2(),
                viewer: new Panel(),
                mailRead: mailRead,
                web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Auto,
                htmlConverter: _ => { },
                htmlDark: Enums.ToggleState.Off,
                navBackgColor: Color.Black,
                navForeColor: Color.White,
                tlpBackColor: Color.Black,
                tipsForeColor: Color.White,
                tipsBackColor: Color.Black,
                mailReadForeColor: ReadFore,
                mailReadBackColor: ReadBack,
                mailUnreadForeColor: UnreadFore,
                mailUnreadBackColor: UnreadBack,
                tipsDetailsBackColor: Color.Black,
                tipsDetailsForeColor: Color.White,
                buttonBackColor: Color.Black,
                buttonMouseOverColor: Color.White,
                buttonClickedColor: Color.Gray,
                txtboxSearchBackColor: Color.Black,
                txtboxSearchForeColor: Color.White,
                txtboxBodyBackColor: Color.Black,
                txtboxBodyForeColor: Color.White,
                cboFoldersBackColor: Color.Black,
                cboFoldersForeColor: Color.White,
                defaultBackColor: Color.Black,
                defaultForeColor: Color.White
            );
        }

        [TestMethod]
        public void Theme_MailLabelTheming_WhenReadProbeThrows_LabelsStillReThemeToUnread()
        {
            // Arrange — the read-state probe simulates a stale/moved Outlook MailItem whose
            // UnRead access throws a COMException.
            Theme theme = BuildTheme(
                () => throw new COMException("The item has been moved or deleted."),
                out Label lblSender,
                out Label lblSubject
            );

            // Act — synchronous render on the test thread (handle-less labels report
            // InvokeRequired == false, so SetQfcTheme(async: false) runs SetQfcTheme()).
            Action act = () => theme.SetQfcTheme(async: false);

            // Assert — the render must not surface the probe fault, and both labels must be
            // re-themed (defaulting to the unread colors) rather than retaining the
            // previous-theme sentinel.
            act.Should().NotThrow();
            lblSender.BackColor.Should().Be(UnreadBack);
            lblSubject.BackColor.Should().Be(UnreadBack);
            lblSender.BackColor.Should().NotBe(PreviousThemeSentinel);
            lblSubject.BackColor.Should().NotBe(PreviousThemeSentinel);
        }

        [TestMethod]
        public void Theme_MailLabelTheming_WhenProbeReturnsFalse_AppliesUnreadColors()
        {
            // Arrange
            Theme theme = BuildTheme(() => false, out Label lblSender, out Label lblSubject);

            // Act
            theme.SetQfcTheme(async: false);

            // Assert
            lblSender.BackColor.Should().Be(UnreadBack);
            lblSender.ForeColor.Should().Be(UnreadFore);
            lblSubject.BackColor.Should().Be(UnreadBack);
            lblSubject.ForeColor.Should().Be(UnreadFore);
        }

        [TestMethod]
        public void Theme_MailLabelTheming_WhenProbeReturnsTrue_AppliesReadColors()
        {
            // Arrange
            Theme theme = BuildTheme(() => true, out Label lblSender, out Label lblSubject);

            // Act
            theme.SetQfcTheme(async: false);

            // Assert
            lblSender.BackColor.Should().Be(ReadBack);
            lblSender.ForeColor.Should().Be(ReadFore);
            lblSubject.BackColor.Should().Be(ReadBack);
            lblSubject.ForeColor.Should().Be(ReadFore);
        }
    }
}
