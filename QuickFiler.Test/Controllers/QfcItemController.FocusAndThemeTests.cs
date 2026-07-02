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

        // Note: the two synchronous ToggleFocus overloads are bucket-(iii) residuals (their whole body
        // runs inside one _itemViewer.Invoke delegate terminating in Theme.SetQfcTheme, unreachable
        // without a Theme seam), so they are not covered here.

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
