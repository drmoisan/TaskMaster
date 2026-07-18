using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// FocusAndTheme-cluster tests (cycle-2 Phase 5, AC8). Covers the de-exempted focus/navigation/
    /// tips/theme members that route their side effects through the narrowed <see cref="IItemViewer"/>
    /// dispatch surface (<c>Invoke</c>/<c>BeginInvoke</c>) or a mockable <see cref="IQfcTipsDetails"/>
    /// collaborator. Members that unconditionally await the out-of-scope
    /// <c>Theme.SetQfcThemeAsync()</c> (the two <c>ToggleFocusAsync</c> overloads) retain a per-member
    /// bucket-(iii) exemption and are excluded here; the synchronous overloads are covered by verifying
    /// the dispatch call without executing the theme-application delegate against a handle-less Theme.
    /// </summary>
    [TestClass]
    public class QfcItemController_FocusAndThemeTests
    {
        private sealed class FocusController : QfcItemController
        {
            internal FocusController()
                : base() { }
        }

        private static void SetField(QfcItemController c, string name, object value) =>
            QfcItemControllerTestSupport.SetField(c, name, value);

        private static object GetField(QfcItemController c, string name) =>
            QfcItemControllerTestSupport.GetField(c, name);

        private static Dictionary<string, Theme> BuildAllThemes()
        {
            Theme theme = QfcItemControllerTestSupport.BuildColorTheme(
                Color.Red,
                Color.Green,
                Color.Blue
            );
            return new Dictionary<string, Theme>
            {
                ["LightNormal"] = theme,
                ["LightActive"] = theme,
                ["DarkNormal"] = theme,
                ["DarkActive"] = theme,
            };
        }

        private static (
            Mock<IQfcKeyboardHandler> mock,
            KbdActions<
                System.Windows.Forms.Keys,
                KaKeyAsync,
                Func<System.Windows.Forms.Keys, Task>
            > keyAsync,
            KbdActions<char, KaCharAsync, Func<char, Task>> charAsync
        ) BuildKbdStub()
        {
            var mock = new Mock<IQfcKeyboardHandler>();
            var keyAsync =
                new KbdActions<
                    System.Windows.Forms.Keys,
                    KaKeyAsync,
                    Func<System.Windows.Forms.Keys, Task>
                >();
            var charAsync = new KbdActions<char, KaCharAsync, Func<char, Task>>();
            mock.Setup(k => k.KeyActionsAsync).Returns(keyAsync);
            mock.Setup(k => k.CharActionsAsync).Returns(charAsync);
            return (mock, keyAsync, charAsync);
        }

        /// <summary>
        /// Builds a controller wired with an active theme, empty tips collections, a keyboard-handler
        /// stub, and an item helper carrying an entry id, so the private focus-toggle members can run
        /// without a live view.
        /// </summary>
        private static FocusController BuildFocusController()
        {
            var controller = new FocusController();
            SetField(controller, "_themes", BuildAllThemes());
            SetField(controller, "_activeTheme", "LightNormal");
            SetField(controller, "_listTipsDetails", new List<IQfcTipsDetails>());
            SetField(controller, "_listTipsExpanded", new List<IQfcTipsDetails>());
            var (kbd, _, _) = BuildKbdStub();
            SetField(controller, "_kbdHandler", kbd.Object);
            var helper = new MailItemHelper { EntryId = "focus-entry" };
            controller.ItemHelper = helper;
            return controller;
        }

        private static Mock<IItemViewer> BuildExecutingViewer()
        {
            var viewer = new Mock<IItemViewer>();
            viewer
                .Setup(v => v.Invoke(It.IsAny<Delegate>()))
                .Returns((Delegate d) => d.DynamicInvoke());
            viewer
                .Setup(v => v.BeginInvoke(It.IsAny<Delegate>()))
                .Returns(
                    (Delegate d) =>
                    {
                        d.DynamicInvoke();
                        return Mock.Of<IAsyncResult>();
                    }
                );
            return viewer;
        }

        /// <summary>
        /// Cycle-4 remediation (R1): reflection-injects handle-less doubles for every private field
        /// touched by <see cref="Theme.SetQfcTheme(bool)"/> (<c>Theme.cs:414-432</c>) and the recursive
        /// <c>SetQfcTheme()</c> it falls through to (<c>Theme.Rendering.cs:8-103</c>), on every
        /// <see cref="Theme"/> in <paramref name="controller"/>'s <c>_themes</c> dictionary. A
        /// handle-less <see cref="Label"/> avoids the first NRE (<c>_lblItemNumber.InvokeRequired</c>
        /// on a null field); the other 14 fields avoid the second NRE thrown by the recursive
        /// <c>SetQfcTheme()</c> body. Doubles mirror the shape proven in
        /// <c>Theme_DispatcherTests.Constructor_BigOverload_WithNullUiDispatcher_DefaultsToWpfUiDispatcher</c>
        /// (<c>Theme.DispatcherTests.cs:91-134</c>); none require a live handle or STA thread.
        /// <c>BuildFocusController</c>/<c>BuildAllThemes</c>/<c>BuildColorTheme</c> stay unmodified.
        /// <c>_topicThread</c>/<c>_webView2</c> use <see cref="Activator.CreateInstance(Type)"/> against
        /// the field's runtime <see cref="Type"/> (not a source-level <c>new</c>) because this test
        /// project has no direct compile-time reference to <c>ObjectListView.dll</c>/
        /// <c>Microsoft.Web.WebView2.WinForms.dll</c> — only <c>QuickFiler.csproj</c>/
        /// <c>UtilitiesCS.csproj</c> do, and legacy <c>ProjectReference</c>s do not flow transitive
        /// compile-time references. Both assemblies still load at run time via those project
        /// references, so this produces the identical concrete instance without a project-file edit.
        /// </summary>
        private static void EnableHandlelessThemeInvoke(FocusController controller)
        {
            var themes = (Dictionary<string, Theme>)GetField(controller, "_themes");
            foreach (var theme in themes.Values)
            {
                SetThemeField(theme, "_lblItemNumber", new Label());
                SetThemeField(theme, "_lblSender", new Label());
                SetThemeField(theme, "_lblSubject", new Label());
                SetThemeField(theme, "_tableLayoutPanels", new List<TableLayoutPanel>());
                SetThemeField(theme, "_buttons", new List<Button>());
                SetThemeField(theme, "_menuItems", new List<System.ComponentModel.Component>());
                SetThemeField(theme, "_menuStrip", new MenuStrip());
                SetThemeField(theme, "_tipsDetailsLabels", new List<IQfcTipsDetails>());
                SetThemeField(theme, "_tipsExpanded", new List<IQfcTipsDetails>());
                SetThemeField(theme, "_textboxSearch", new TextBox());
                SetThemeField(theme, "_textboxBody", new TextBox());
                SetThemeFieldViaActivator(theme, "_breadcrumbWebView2");
                SetThemeFieldViaActivator(theme, "_topicThread");
                SetThemeFieldViaActivator(theme, "_webView2");
                SetThemeField(theme, "_viewer", (Control)new Panel());
                SetThemeField(theme, "MailRead", (Func<bool>)(() => true));
            }
        }

        private static void SetThemeField(Theme theme, string name, object value)
        {
            FieldInfo field = typeof(Theme).GetField(
                name,
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            field.Should().NotBeNull(because: "field '" + name + "' must exist on Theme");
            field.SetValue(theme, value);
        }

        private static void SetThemeFieldViaActivator(Theme theme, string name)
        {
            FieldInfo field = typeof(Theme).GetField(
                name,
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            field.Should().NotBeNull(because: "field '" + name + "' must exist on Theme");
            field.SetValue(theme, Activator.CreateInstance(field.FieldType));
        }

        // ------------------------- ToggleFocus / ToggleFocus(ToggleState) -------------------------
        // Cycle-3 P9-T5/P9-T6 (members #33/#35, de-exempted); cycle-4 remediation R1: the entire body
        // runs inside a single _itemViewer.Invoke(...) delegate. BuildExecutingViewer() executes the
        // delegate synchronously and EnableHandlelessThemeInvoke() populates the terminal
        // _themes[_activeTheme].SetQfcTheme(async: false) call's dependencies with handle-less doubles,
        // so these tests exercise the full method body (the _activeUI/_activeTheme state machine) and
        // assert the resulting state transitions, not merely the Invoke marshal.

        [TestMethod]
        public void ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke()
        {
            // Arrange — _tableLayoutPanels (QfcItemController's own field, distinct from Theme's field
            // of the same name) is dereferenced by ToggleTips inside the executed delegate body.
            var viewer = BuildExecutingViewer();
            var controller = BuildFocusController();
            SetField(controller, "_itemViewer", viewer.Object);
            SetField(controller, "_tableLayoutPanels", new List<TableLayoutPanel>());
            EnableHandlelessThemeInvoke(controller);

            // Act
            controller.ToggleFocus(Enums.ToggleState.On);

            // Assert — the delegate body actually runs, transitioning inactive->active. Invoke fires
            // twice: the outer wrapper plus the nested dispatch inside ToggleTips(async: false).
            viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Exactly(2));
            GetField(controller, "_activeUI").Should().Be(true);
            GetField(controller, "_activeTheme").Should().Be("LightActive");
        }

        [TestMethod]
        public void ToggleFocus_StateOverload_Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme()
        {
            // Arrange
            var viewer = BuildExecutingViewer();
            var controller = BuildFocusController();
            SetField(controller, "_itemViewer", viewer.Object);
            SetField(controller, "_activeUI", true);
            SetField(controller, "_activeTheme", "LightActive");
            SetField(controller, "_tableLayoutPanels", new List<TableLayoutPanel>());
            EnableHandlelessThemeInvoke(controller);

            // Act
            controller.ToggleFocus(Enums.ToggleState.Off);

            // Assert — deactivates the UI and switches to the normal theme; Invoke fires twice (see above).
            GetField(controller, "_activeUI").Should().Be(false);
            GetField(controller, "_activeTheme").Should().Be("LightNormal");
            viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Exactly(2));
        }

        [TestMethod]
        public void ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke()
        {
            // Arrange — BuildFocusController() leaves _activeUI at its default false, so this reaches
            // the inactive->active branch.
            var viewer = BuildExecutingViewer();
            var controller = BuildFocusController();
            SetField(controller, "_itemViewer", viewer.Object);
            SetField(controller, "_tableLayoutPanels", new List<TableLayoutPanel>());
            EnableHandlelessThemeInvoke(controller);

            // Act
            controller.ToggleFocus();

            // Assert — Invoke fires twice (see StateOverload test above for why).
            viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Exactly(2));
            GetField(controller, "_activeUI").Should().Be(true);
            GetField(controller, "_activeTheme").Should().Be("LightActive");
        }

        [TestMethod]
        public void ToggleFocus_ParameterlessOverload_FromActive_DeactivatesUiAndSwitchesToNormalTheme()
        {
            // Arrange
            var viewer = BuildExecutingViewer();
            var controller = BuildFocusController();
            SetField(controller, "_itemViewer", viewer.Object);
            SetField(controller, "_activeUI", true);
            SetField(controller, "_activeTheme", "LightActive");
            SetField(controller, "_tableLayoutPanels", new List<TableLayoutPanel>());
            EnableHandlelessThemeInvoke(controller);

            // Act
            controller.ToggleFocus();

            // Assert — deactivates the UI and switches to the normal theme; Invoke fires twice (see above).
            GetField(controller, "_activeUI").Should().Be(false);
            GetField(controller, "_activeTheme").Should().Be("LightNormal");
            viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Exactly(2));
        }

        // ------------------------- ToggleFocusOnAsync / OffAsync (private) -------------------------

        [TestMethod]
        public async Task ToggleFocusOnAsync_ActivatesUiAndSwitchesToActiveTheme()
        {
            // Arrange
            var controller = BuildFocusController();

            // Act
            var task = (Task)
                QfcItemControllerTestSupport.InvokeNonPublic(controller, "ToggleFocusOnAsync");
            await task;

            // Assert
            GetField(controller, "_activeUI").Should().Be(true);
            GetField(controller, "_activeTheme").Should().Be("LightActive");
        }

        [TestMethod]
        public async Task ToggleFocusOffAsync_DeactivatesUiAndSwitchesToNormalTheme()
        {
            // Arrange
            var controller = BuildFocusController();
            SetField(controller, "_activeUI", true);
            SetField(controller, "_activeTheme", "LightActive");

            // Act
            var task = (Task)
                QfcItemControllerTestSupport.InvokeNonPublic(controller, "ToggleFocusOffAsync");
            await task;

            // Assert
            GetField(controller, "_activeUI").Should().Be(false);
            GetField(controller, "_activeTheme").Should().Be("LightNormal");
        }

        // ------------------------- ToggleNavigation overloads -------------------------

        [TestMethod]
        public void ToggleNavigation_Synchronous_TogglesPositionTips()
        {
            // Arrange
            var tips = new Mock<IQfcTipsDetails>();
            var viewer = BuildExecutingViewer();
            var controller = new FocusController();
            SetField(controller, "_itemPositionTips", tips.Object);
            SetField(controller, "_itemViewer", viewer.Object);

            // Act
            controller.ToggleNavigation(async: false);

            // Assert
            tips.Verify(t => t.Toggle(false), Times.AtLeastOnce());
        }

        [TestMethod]
        public void ToggleNavigation_WithState_TogglesPositionTipsWithState()
        {
            // Arrange
            var tips = new Mock<IQfcTipsDetails>();
            var viewer = BuildExecutingViewer();
            var controller = new FocusController();
            SetField(controller, "_itemPositionTips", tips.Object);
            SetField(controller, "_itemViewer", viewer.Object);

            // Act
            controller.ToggleNavigation(async: false, desiredState: Enums.ToggleState.On);

            // Assert
            tips.Verify(t => t.Toggle(Enums.ToggleState.On, false), Times.Once());
        }

        [TestMethod]
        public async Task ToggleNavigationAsync_AwaitsPositionTipsToggleAsync()
        {
            // Arrange
            var tips = new Mock<IQfcTipsDetails>();
            tips.Setup(t => t.ToggleAsync(Enums.ToggleState.Off, false))
                .Returns(Task.CompletedTask);
            var controller = new FocusController();
            SetField(controller, "_itemPositionTips", tips.Object);

            // Act
            await controller.ToggleNavigationAsync(Enums.ToggleState.Off);

            // Assert
            tips.Verify(t => t.ToggleAsync(Enums.ToggleState.Off, false), Times.Once());
        }

        // ------------------------- ToggleTips / ToggleTipsAsync -------------------------

        [TestMethod]
        public void ToggleTips_Synchronous_DispatchesAndExecutesDelegate()
        {
            // Arrange — an executing viewer runs the dispatched delegate; empty tips/panels collections
            // keep the executed body free of live-control work so the tips-toggle logic is exercised.
            var viewer = BuildExecutingViewer();
            var controller = new FocusController();
            SetField(controller, "_itemViewer", viewer.Object);
            SetField(controller, "_listTipsDetails", new List<IQfcTipsDetails>());
            SetField(controller, "_listTipsExpanded", new List<IQfcTipsDetails>());
            SetField(controller, "_tableLayoutPanels", new List<TableLayoutPanel>());

            // Act
            controller.ToggleTips(async: false, desiredState: Enums.ToggleState.On);

            // Assert
            viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Once());
        }

        [TestMethod]
        public async Task ToggleTipsAsync_WithEmptyCollections_Completes()
        {
            // Arrange
            var controller = new FocusController();
            SetField(controller, "_listTipsDetails", new List<IQfcTipsDetails>());
            SetField(controller, "_listTipsExpanded", new List<IQfcTipsDetails>());

            // Act
            Func<Task> act = () => controller.ToggleTipsAsync(Enums.ToggleState.On);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ------------------------- InvokeBeginInvoke -------------------------

        [TestMethod]
        public void InvokeBeginInvoke_WhenAsync_UsesBeginInvoke()
        {
            // Arrange
            var viewer = new Mock<IItemViewer>();
            var controller = new FocusController();
            SetField(controller, "_itemViewer", viewer.Object);

            // Act
            controller.InvokeBeginInvoke(async: true, action: () => { });

            // Assert
            viewer.Verify(v => v.BeginInvoke(It.IsAny<Delegate>()), Times.Once());
            viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Never());
        }

        [TestMethod]
        public void InvokeBeginInvoke_WhenSynchronous_UsesInvoke()
        {
            // Arrange
            var viewer = new Mock<IItemViewer>();
            var controller = new FocusController();
            SetField(controller, "_itemViewer", viewer.Object);

            // Act
            controller.InvokeBeginInvoke(async: false, action: () => { });

            // Assert
            viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Once());
            viewer.Verify(v => v.BeginInvoke(It.IsAny<Delegate>()), Times.Never());
        }

        // ------------------------- ToggleSaveAttachments -------------------------

        [TestMethod]
        public void ToggleSaveAttachments_DoesNotThrow()
        {
            // Arrange
            var controller = new FocusController();

            // Act
            Action act = () => controller.ToggleSaveAttachments();

            // Assert
            act.Should().NotThrow();
        }

        // ------------------------- SetThemeDark / SetThemeLight -------------------------

        [TestMethod]
        public void SetThemeDark_FromNormal_SelectsDarkNormalTheme()
        {
            // Arrange — async:true queues the theme application on the dispatcher without executing it,
            // so no handle-less control is touched; the observable effect is the active-theme switch.
            QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
            var controller = new FocusController();
            SetField(controller, "_themes", BuildAllThemes());
            SetField(controller, "_activeTheme", null);

            // Act
            controller.SetThemeDark(async: true);

            // Assert
            GetField(controller, "_activeTheme").Should().Be("DarkNormal");
        }

        [TestMethod]
        public void SetThemeLight_FromNormal_SelectsLightNormalTheme()
        {
            // Arrange
            QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
            var controller = new FocusController();
            SetField(controller, "_themes", BuildAllThemes());
            SetField(controller, "_activeTheme", null);

            // Act
            controller.SetThemeLight(async: true);

            // Assert
            GetField(controller, "_activeTheme").Should().Be("LightNormal");
        }

        // ------------------------- HtmlDarkConverter -------------------------

        [TestMethod]
        public void HtmlDarkConverter_WhenWebViewNotInitialized_DoesNotNavigate()
        {
            // Arrange — _isWebViewerInitialized defaults to false, so the method must be a no-op.
            var viewer = new Mock<IItemViewer>();
            var controller = new FocusController();
            SetField(controller, "_itemViewer", viewer.Object);

            // Act
            controller.HtmlDarkConverter(Enums.ToggleState.On);

            // Assert
            viewer.Verify(v => v.NavigateToString(It.IsAny<string>()), Times.Never());
        }
    }
}
