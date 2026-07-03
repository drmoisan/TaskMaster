using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Navigation-cluster tests (research §5.2). Covers the pure KbdExecuteAsync keyboard-routing
    /// for both overloads: keyboard deactivation is conditional on the deactivateKbd flag, and the
    /// supplied action is always awaited.
    /// </summary>
    [TestClass]
    public class QfcItemController_NavigationTests
    {
        private sealed class NavController : QfcItemController
        {
            internal NavController(IFilerHomeController homeController)
                : base()
            {
                typeof(QfcItemController)
                    .GetField("_homeController", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, homeController);
            }
        }

        private static (NavController controller, Mock<IQfcKeyboardHandler> kbd) BuildController()
        {
            var mockKbd = new Mock<IQfcKeyboardHandler>();
            var mockHome = new Mock<IFilerHomeController>();
            mockHome.SetupGet(h => h.KeyboardHandler).Returns(mockKbd.Object);
            return (new NavController(mockHome.Object), mockKbd);
        }

        [TestMethod]
        public async Task KbdExecuteAsync_WhenDeactivateKbdTrue_TogglesKeyboardAndRunsAction()
        {
            // Arrange
            var (controller, mockKbd) = BuildController();
            var ran = false;

            // Act
            await controller.KbdExecuteAsync(
                () =>
                {
                    ran = true;
                    return Task.CompletedTask;
                },
                deactivateKbd: true
            );

            // Assert
            ran.Should().BeTrue();
            mockKbd.Verify(k => k.ToggleKeyboardDialog(), Times.Once());
        }

        [TestMethod]
        public async Task KbdExecuteAsync_WhenDeactivateKbdFalse_RunsActionWithoutToggling()
        {
            // Arrange
            var (controller, mockKbd) = BuildController();
            var ran = false;

            // Act
            await controller.KbdExecuteAsync(
                () =>
                {
                    ran = true;
                    return Task.CompletedTask;
                },
                deactivateKbd: false
            );

            // Assert
            ran.Should().BeTrue();
            mockKbd.Verify(k => k.ToggleKeyboardDialog(), Times.Never());
        }

        [TestMethod]
        public async Task KbdExecuteAsyncGeneric_WhenDeactivateKbdTrue_TogglesAndPassesArgument()
        {
            // Arrange
            var (controller, mockKbd) = BuildController();
            string captured = null;

            // Act
            await controller.KbdExecuteAsync(
                (string arg) =>
                {
                    captured = arg;
                    return Task.CompletedTask;
                },
                "payload",
                deactivateKbd: true
            );

            // Assert
            captured.Should().Be("payload");
            mockKbd.Verify(k => k.ToggleKeyboardDialog(), Times.Once());
        }

        [TestMethod]
        public async Task KbdExecuteAsyncGeneric_WhenDeactivateKbdFalse_DoesNotToggle()
        {
            // Arrange
            var (controller, mockKbd) = BuildController();
            string captured = null;

            // Act
            await controller.KbdExecuteAsync(
                (string arg) =>
                {
                    captured = arg;
                    return Task.CompletedTask;
                },
                "payload",
                deactivateKbd: false
            );

            // Assert
            captured.Should().Be("payload");
            mockKbd.Verify(k => k.ToggleKeyboardDialog(), Times.Never());
        }

        // ---------------------------------------------------------------------------
        // Cycle-2 Phase 5 (AC8) de-exemption coverage: JumpToFolderDropDown, JumpToSearchTextbox,
        // and the two parameterless ToggleExpansion overloads (routing verified via a spy subclass
        // that overrides the TlpCellSnapShot-bound, out-of-scope state-taking overloads).
        // ---------------------------------------------------------------------------

        private sealed class ExpansionSpyController : QfcItemController
        {
            internal Enums.ToggleState? LastSyncState;
            internal Enums.ToggleState? LastAsyncState;

            internal ExpansionSpyController()
                : base() { }

            public override void ToggleExpansion(Enums.ToggleState desiredState)
            {
                LastSyncState = desiredState;
            }

            public override async Task ToggleExpansionAsync(Enums.ToggleState desiredState)
            {
                LastAsyncState = desiredState;
                await Task.CompletedTask;
            }
        }

        [TestMethod]
        public void JumpToFolderDropDown_TogglesKeyboardAndFocusesFolderDropDown()
        {
            // Arrange — the Invoke callback runs the marshaled action so the folder-focus intent
            // members are exercised.
            Mock<IQfcKeyboardHandler> mockKbd = new Mock<IQfcKeyboardHandler>();
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer
                .Setup(v => v.Invoke(It.IsAny<Delegate>()))
                .Callback<Delegate>(d => d.DynamicInvoke())
                .Returns((object)null);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", mockKbd.Object);
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);

            // Act
            controller.JumpToFolderDropDown();

            // Assert
            mockKbd.Verify(k => k.ToggleKeyboardDialog(), Times.Once());
            viewer.Verify(v => v.FocusFolderDropDown(), Times.Once());
            viewer.Verify(v => v.SetFolderDroppedDown(true), Times.Once());
        }

        [TestMethod]
        public void JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch()
        {
            // Arrange
            Mock<IQfcKeyboardHandler> mockKbd = new Mock<IQfcKeyboardHandler>();
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", mockKbd.Object);
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);

            // Act
            controller.JumpToSearchTextbox();

            // Assert
            mockKbd.Verify(k => k.ToggleKeyboardDialog(), Times.Once());
            viewer.Verify(v => v.FocusSearch(), Times.Once());
        }

        /// <summary>
        /// Cycle-3 P9-T4 (member #28, de-exempted): <c>Control.Focus()</c> on a handle-less
        /// <c>new Control()</c> returns <c>false</c> silently — no live handle is required, mirroring the
        /// bare handle-less <c>Button</c>/<c>ToolStripMenuItem</c> sender technique already used for
        /// <c>Button_MouseEnter</c>/<c>MenuItem_MouseEnter</c>.
        /// </summary>
        [TestMethod]
        public async Task JumpToAsync_FocusesHandlelessControlAndTogglesKeyboardDialog()
        {
            // Arrange
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(
                controller,
                "_uiDispatcher",
                QfcItemControllerTestSupport.BuildSyncDispatcher().Object
            );
            Mock<IQfcKeyboardHandler> mockKbd = new Mock<IQfcKeyboardHandler>();
            QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", mockKbd.Object);
            Control control = new Control();

            // Act
            await (Task)
                QfcItemControllerTestSupport.InvokeNonPublic(controller, "JumpToAsync", control);

            // Assert
            mockKbd.Verify(k => k.ToggleKeyboardDialogAsync(), Times.Once());
        }

        [TestMethod]
        public void ToggleExpansion_WhenCollapsed_RoutesToOnState()
        {
            // Arrange — _expanded false: parameterless overload must request the On state.
            ExpansionSpyController controller = new ExpansionSpyController();
            QfcItemControllerTestSupport.SetField(controller, "_expanded", false);

            // Act
            controller.ToggleExpansion();

            // Assert
            controller.LastSyncState.Should().Be(Enums.ToggleState.On);
        }

        [TestMethod]
        public void ToggleExpansion_WhenExpanded_RoutesToOffState()
        {
            // Arrange — _expanded true: parameterless overload must request the Off state.
            ExpansionSpyController controller = new ExpansionSpyController();
            QfcItemControllerTestSupport.SetField(controller, "_expanded", true);

            // Act
            controller.ToggleExpansion();

            // Assert
            controller.LastSyncState.Should().Be(Enums.ToggleState.Off);
        }

        [TestMethod]
        public async Task ToggleExpansionAsync_WhenCollapsed_RoutesToOnState()
        {
            // Arrange
            ExpansionSpyController controller = new ExpansionSpyController();
            QfcItemControllerTestSupport.SetField(controller, "_expanded", false);

            // Act
            await controller.ToggleExpansionAsync();

            // Assert
            controller.LastAsyncState.Should().Be(Enums.ToggleState.On);
        }

        [TestMethod]
        public async Task ToggleExpansionAsync_WhenExpanded_RoutesToOffState()
        {
            // Arrange
            ExpansionSpyController controller = new ExpansionSpyController();
            QfcItemControllerTestSupport.SetField(controller, "_expanded", true);

            // Act
            await controller.ToggleExpansionAsync();

            // Assert
            controller.LastAsyncState.Should().Be(Enums.ToggleState.Off);
        }

        /// <summary>
        /// Cycle-5 (R2, de-exempted): <c>ToggleExpansionOff</c> now calls
        /// <c>_tlpStates["Compressed"].ApplyState(_itemViewer)</c> against the narrowed
        /// <c>IItemViewer</c> (no concrete <c>(ItemViewer)</c> cast). A bare <see cref="Control"/> host
        /// stands in for the control tree, proving the real snapshot-restore/flag-clear behavior runs.
        /// </summary>
        [TestMethod]
        public void ToggleExpansionOff_AppliesCompressedSnapshotAndClearsExpandedFlag()
        {
            // Arrange — snapshot the label while enabled/visible, then mutate it to disabled/hidden.
            var host = new Control();
            var tlp = new TableLayoutPanel
            {
                Name = "Compressed",
                ColumnCount = 1,
                RowCount = 1,
            };
            tlp.RowStyles.Add(new RowStyle());
            tlp.ColumnStyles.Add(new ColumnStyle());
            var label = new Label
            {
                Name = "LblAcOpen",
                Enabled = true,
                Visible = true,
            };
            tlp.Controls.Add(label, 0, 0);
            host.Controls.Add(tlp);

            var snapshot = new TlpCellSnapShot();
            snapshot.SnapCell(tlp, label);

            label.Enabled = false;
            label.Visible = false;

            var mockViewer = new Mock<IItemViewer>();
            mockViewer.Setup(v => v.Controls).Returns(host.Controls);

            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", mockViewer.Object);
            QfcItemControllerTestSupport.SetField(controller, "_expanded", true);
            var tlpStates = new TlpCellStates();
            tlpStates.TryAddState("Compressed", new List<TlpCellSnapShot> { snapshot });
            QfcItemControllerTestSupport.SetField(controller, "_tlpStates", tlpStates);

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(controller, "ToggleExpansionOff");

            // Assert — the snapshotted state is restored and the expanded flag is cleared.
            label.Enabled.Should().BeTrue();
            label.Visible.Should().BeTrue();
            QfcItemControllerTestSupport.GetField(controller, "_expanded").Should().Be(false);
        }

        /// <summary>
        /// Cycle-5 (R2, de-exempted): <c>ToggleExpansionOn</c> now calls
        /// <c>_tlpStates["Expanded"].ApplyState(_itemViewer)</c> against the narrowed
        /// <c>IItemViewer</c>. <c>ItemHelper</c> is left <c>null</c> so the read-timer branch is
        /// skipped.
        /// </summary>
        [TestMethod]
        public void ToggleExpansionOn_AppliesExpandedSnapshotAndSetsExpandedFlag()
        {
            // Arrange — snapshot the label while disabled/hidden, then mutate it to enabled/visible.
            var host = new Control();
            var tlp = new TableLayoutPanel
            {
                Name = "Expanded",
                ColumnCount = 1,
                RowCount = 1,
            };
            tlp.RowStyles.Add(new RowStyle());
            tlp.ColumnStyles.Add(new ColumnStyle());
            var label = new Label
            {
                Name = "LblAcOpen",
                Enabled = false,
                Visible = false,
            };
            tlp.Controls.Add(label, 0, 0);
            host.Controls.Add(tlp);

            var snapshot = new TlpCellSnapShot();
            snapshot.SnapCell(tlp, label);

            label.Enabled = true;
            label.Visible = true;

            var mockViewer = new Mock<IItemViewer>();
            mockViewer.Setup(v => v.Controls).Returns(host.Controls);

            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", mockViewer.Object);
            QfcItemControllerTestSupport.SetField(controller, "_expanded", false);
            var tlpStates = new TlpCellStates();
            tlpStates.TryAddState("Expanded", new List<TlpCellSnapShot> { snapshot });
            QfcItemControllerTestSupport.SetField(controller, "_tlpStates", tlpStates);

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(controller, "ToggleExpansionOn");

            // Assert — the snapshotted state is restored and the expanded flag is set.
            label.Enabled.Should().BeFalse();
            label.Visible.Should().BeFalse();
            QfcItemControllerTestSupport.GetField(controller, "_expanded").Should().Be(true);
        }
    }
}
