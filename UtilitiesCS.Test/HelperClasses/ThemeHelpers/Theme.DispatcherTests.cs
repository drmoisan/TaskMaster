using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.HelperClasses.ThemeHelpers
{
    /// <summary>
    /// Cycle-3 (P10-T24 through P10-T28): dispatcher-routing tests for the <see cref="Theme"/> +
    /// <see cref="IUiDispatcher"/> retrofit. Hosted in a dedicated file (not <c>ThemeTests.cs</c>, 407
    /// lines) so the existing file stays unmodified and under the 500-line cap. All tests use
    /// non-executing <see cref="Mock{IUiDispatcher}"/> setups (no <c>.Callback</c> that invokes the
    /// delegate), so this cycle's coverage obligation is the changed dispatch-routing lines
    /// themselves, not <see cref="Theme"/>'s pre-existing, unrelated rendering internals.
    /// </summary>
    [TestClass]
    public class Theme_DispatcherTests
    {
        private static void SetField(Theme theme, string name, object value)
        {
            FieldInfo field = typeof(Theme).GetField(
                name,
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            field.Should().NotBeNull(because: "field '" + name + "' must exist on Theme");
            field.SetValue(theme, value);
        }

        [TestMethod]
        public async Task SetQfcThemeAsync_RoutesThroughInjectedDispatcher()
        {
            // Arrange
            Theme theme = new Theme();
            Mock<IUiDispatcher> dispatcher = new Mock<IUiDispatcher>();
            dispatcher.Setup(d => d.InvokeAsync(It.IsAny<Action>())).Returns(Task.CompletedTask);
            SetField(theme, "_uiDispatcher", dispatcher.Object);

            // Act
            await theme.SetQfcThemeAsync();

            // Assert
            dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once());
        }

        [TestMethod]
        public void SetQfcTheme_Async_RoutesThroughInjectedDispatcher()
        {
            // Arrange
            Theme theme = new Theme();
            Mock<IUiDispatcher> dispatcher = new Mock<IUiDispatcher>();
            dispatcher.Setup(d => d.InvokeAsync(It.IsAny<Action>())).Returns(Task.CompletedTask);
            SetField(theme, "_uiDispatcher", dispatcher.Object);

            // Act
            theme.SetQfcTheme(async: true);

            // Assert
            dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once());
        }

        [TestMethod]
        public void SetMailRead_Async_RoutesThroughInjectedDispatcherBeginInvoke()
        {
            // Arrange
            Theme theme = new Theme();
            Mock<IUiDispatcher> dispatcher = new Mock<IUiDispatcher>();
            dispatcher
                .Setup(d => d.BeginInvoke(It.IsAny<Action>()))
                .Returns(Mock.Of<IAsyncResult>());
            SetField(theme, "_uiDispatcher", dispatcher.Object);
            SetField(theme, "_lblSender", new Label());

            // Act
            theme.SetMailRead(async: true);

            // Assert
            dispatcher.Verify(d => d.BeginInvoke(It.IsAny<Action>()), Times.Once());
        }

        [TestMethod]
        public void Constructor_BigOverload_WithNullUiDispatcher_DefaultsToWpfUiDispatcher()
        {
            // Arrange / Act — every parameter is a minimal-but-real, handle-less double; uiDispatcher
            // is omitted so the production default applies.
            Theme theme = new Theme(
                name: "LightNormal",
                lblItemNumber: new Label(),
                lblSender: new Label(),
                lblSubject: new Label(),
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
                mailRead: () => true,
                web2ViewScheme: Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Auto,
                htmlConverter: _ => { },
                htmlDark: Enums.ToggleState.Off,
                navBackgColor: Color.Black,
                navForeColor: Color.White,
                tlpBackColor: Color.Black,
                tipsForeColor: Color.White,
                tipsBackColor: Color.Black,
                mailReadForeColor: Color.White,
                mailReadBackColor: Color.Black,
                mailUnreadForeColor: Color.White,
                mailUnreadBackColor: Color.Black,
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

            FieldInfo field = typeof(Theme).GetField(
                "_uiDispatcher",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            field.Should().NotBeNull(because: "field '_uiDispatcher' must exist on Theme");
            object dispatcher = field.GetValue(theme);

            // Assert — type check only; no dispatch method is invoked, so the real static
            // UiThread.Dispatcher is never touched.
            dispatcher.Should().BeOfType<WpfUiDispatcher>();
        }
    }
}
