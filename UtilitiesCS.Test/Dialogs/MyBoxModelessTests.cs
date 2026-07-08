using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.Dialogs;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Test.Dialogs
{
    /// <summary>
    /// Unit tests for the modeless store-lockup composition <see cref="MyBoxModeless"/> (issue #264).
    /// Runs under MSTest's STA class execution mode because it constructs WinForms controls. A
    /// non-displaying <c>showAction</c> stub is injected so no real window is ever shown, and each
    /// button action is invoked directly to assert it routes to the correct F1 call. Internal members
    /// are accessible via InternalsVisibleTo("UtilitiesCS.Test").
    /// </summary>
    [STATestClass]
    public class MyBoxModelessTests
    {
        [TestMethod]
        public void ShowStoreLockupNotification_InvokesShowActionOnce_NeverShowsRealWindow()
        {
            // Arrange
            var showCount = 0;
            MyBoxViewer captured = null;

            // Act
            MyBoxModeless.ShowStoreLockupNotification(
                "Mailbox A",
                () => { },
                () => { },
                () => { },
                viewer =>
                {
                    showCount++;
                    captured = viewer;
                }
            );

            // Assert: the injectable seam is used exactly once and the real Show() never runs.
            showCount.Should().Be(1);
            captured.Should().NotBeNull();
            captured
                .Visible.Should()
                .BeFalse("the real viewer.Show() must never be called in a test");

            captured.Dispose();
        }

        [TestMethod]
        public void BuildButtons_ProducesThreeLabelledButtons_RoutingToCorrectF1Calls()
        {
            // Arrange
            var disable = new Mock<IStoreDisableService>();
            disable
                .Setup(s => s.ReenableAsync(It.IsAny<StoreIdentity>()))
                .Returns(Task.CompletedTask);
            var identity = StoreIdentity.Resolve("Mailbox A");

            IList<ActionButton> buttons = MyBoxModeless.BuildButtons(
                () => disable.Object.DisableSessionOnly(identity),
                () => disable.Object.DisableForFutureSessions(identity),
                () => disable.Object.ReenableAsync(identity)
            );

            // Assert: three buttons with the confirmed labels, in order.
            buttons.Should().HaveCount(3);
            buttons[0].Button.Text.Should().Be("Disable This Session Only");
            buttons[1].Button.Text.Should().Be("Disable for Future Sessions");
            buttons[2].Button.Text.Should().Be("Reenable");

            // Act & Assert: each button action routes to the correct F1 call; no direct F3 call exists.
            buttons[0].Delegate();
            disable.Verify(s => s.DisableSessionOnly(It.IsAny<StoreIdentity>()), Times.Once);

            buttons[1].Delegate();
            disable.Verify(s => s.DisableForFutureSessions(It.IsAny<StoreIdentity>()), Times.Once);

            buttons[2].Delegate();
            disable.Verify(s => s.ReenableAsync(It.IsAny<StoreIdentity>()), Times.Once);

            disable.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void ShowStoreLockupNotification_WiresFormClosedDisposal()
        {
            // Arrange
            MyBoxViewer captured = null;
            MyBoxModeless.ShowStoreLockupNotification(
                "Mailbox A",
                () => { },
                () => { },
                () => { },
                viewer => captured = viewer
            );
            captured.Should().NotBeNull();
            captured.IsDisposed.Should().BeFalse();

            // Act: raise FormClosed (the viewer was never shown, so drive OnFormClosed directly).
            var onFormClosed = typeof(Form).GetMethod(
                "OnFormClosed",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            onFormClosed.Invoke(
                captured,
                new object[] { new FormClosedEventArgs(CloseReason.UserClosing) }
            );

            // Assert: the FormClosed handler disposed the viewer (it owns its own lifetime).
            captured.IsDisposed.Should().BeTrue();
        }
    }
}
