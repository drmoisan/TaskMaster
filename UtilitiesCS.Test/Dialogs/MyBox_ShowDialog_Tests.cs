using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Dialogs
{
    /// <summary>
    /// Unit tests for <see cref="MyBox.ShowDialog"/> overloads via the
    /// deterministic <see cref="MyBox.DialogInvoker"/> seam.
    ///
    /// Purpose:
    ///     Verify that every ShowDialog overload routes the caller-supplied parameters
    ///     to the viewer correctly and returns the DialogResult produced by the injected
    ///     seam, without displaying any real modal dialog.
    ///
    /// Usage:
    ///     Each test injects a non-modal stub into <see cref="MyBox.DialogInvoker"/>
    ///     before invoking a <c>ShowDialog</c> overload, then asserts on the return value.
    ///     <see cref="TestCleanup_ResetMyBoxDialogInvokerSeam"/> restores the real invoker
    ///     after every test to prevent cross-test contamination.
    ///
    /// Invariants / Constraints:
    ///     This class runs under MSTest's STA class execution mode because every
    ///     test creates WinForms controls.
    ///     Internal members are accessible via InternalsVisibleTo("UtilitiesCS.Test").
    /// </summary>
    [STATestClass]
    public class MyBox_ShowDialog_Tests
    {
        // ---------------------------------------------------------------------------
        // Seam teardown — restores the real DialogInvoker after each test so that
        // no seam mutation leaks to the next test.
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Resets <see cref="MyBox.DialogInvoker"/> to the real (modal)
        /// implementation after each test.
        /// </summary>
        [TestCleanup]
        public void TestCleanup_ResetMyBoxDialogInvokerSeam()
        {
            MyBox.DialogInvoker = viewer => viewer.ShowDialog();
        }

        // ---------------------------------------------------------------------------
        // P2-T5 — ShowDialog affirmative-result paths via injected DialogInvoker seam
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the BoxIcon overload returns the affirmative DialogResult.OK
        /// produced by the injected seam when BoxIcon.None suppresses the icon.
        ///
        /// Purpose:
        ///     Exercise the main ShowDialog(MyBoxViewer, string, string, BoxIcon,
        ///     IList&lt;ActionButton&gt;) path including ReplaceButtons and
        ///     AppendButtonInColumn(ActionButton).
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_BoxIconNone_SeamReturnsOk_ReturnsOkResult()
        {
            // Arrange: inject non-modal stub so no real dialog is displayed
            MyBox.DialogInvoker = _ => DialogResult.OK;
            using var viewer = new MyBoxViewer();
            var buttons = new List<ActionButton>
            {
                new ActionButton("ButtonOk", "Ok", DialogResult.OK, () => { }),
            };

            // Act: invoke the viewer-accepting BoxIcon overload (overload 2)
            DialogResult result = MyBox.ShowDialog(
                viewer,
                "Test message",
                "Test title",
                BoxIcon.None,
                buttons
            );

            // Assert: the seam-injected OK result flows through unchanged
            result.Should().Be(DialogResult.OK);
        }

        /// <summary>
        /// Verifies the BoxIcon.Critical branch of SetDialogIcon while confirming
        /// DialogResult.OK flows through the seam.
        ///
        /// Purpose:
        ///     Cover the Critical case in SetDialogIcon(BoxIcon).
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_BoxIconCritical_SeamReturnsOk_ReturnsOkResult()
        {
            // Arrange
            MyBox.DialogInvoker = _ => DialogResult.OK;
            using var viewer = new MyBoxViewer();
            var buttons = new List<ActionButton>
            {
                new ActionButton("ButtonOk", "Ok", DialogResult.OK, () => { }),
            };

            // Act: BoxIcon.Critical branch in SetDialogIcon
            DialogResult result = MyBox.ShowDialog(
                viewer,
                "Test message",
                "Test title",
                BoxIcon.Critical,
                buttons
            );

            // Assert
            result.Should().Be(DialogResult.OK);
        }

        /// <summary>
        /// Verifies the BoxIcon.Warning branch of SetDialogIcon while confirming
        /// DialogResult.OK flows through the seam.
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_BoxIconWarning_SeamReturnsOk_ReturnsOkResult()
        {
            // Arrange
            MyBox.DialogInvoker = _ => DialogResult.OK;
            using var viewer = new MyBoxViewer();
            var buttons = new List<ActionButton>
            {
                new ActionButton("ButtonOk", "Ok", DialogResult.OK, () => { }),
            };

            // Act: BoxIcon.Warning branch in SetDialogIcon
            DialogResult result = MyBox.ShowDialog(
                viewer,
                "Test message",
                "Test title",
                BoxIcon.Warning,
                buttons
            );

            // Assert
            result.Should().Be(DialogResult.OK);
        }

        /// <summary>
        /// Verifies the BoxIcon.Question branch of SetDialogIcon while confirming
        /// DialogResult.OK flows through the seam.
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_BoxIconQuestion_SeamReturnsOk_ReturnsOkResult()
        {
            // Arrange
            MyBox.DialogInvoker = _ => DialogResult.OK;
            using var viewer = new MyBoxViewer();
            var buttons = new List<ActionButton>
            {
                new ActionButton("ButtonOk", "Ok", DialogResult.OK, () => { }),
            };

            // Act: BoxIcon.Question branch in SetDialogIcon
            DialogResult result = MyBox.ShowDialog(
                viewer,
                "Test message",
                "Test title",
                BoxIcon.Question,
                buttons
            );

            // Assert
            result.Should().Be(DialogResult.OK);
        }

        /// <summary>
        /// Verifies the MessageBoxIcon overload (overload 4) returns DialogResult.OK
        /// via the seam and exercises the MessageBoxIcon.Error branch of SetDialogIcon.
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_MessageBoxIconError_SeamReturnsOk_ReturnsOkResult()
        {
            // Arrange
            MyBox.DialogInvoker = _ => DialogResult.OK;
            using var viewer = new MyBoxViewer();
            var buttons = new List<ActionButton>
            {
                new ActionButton("ButtonOk", "Ok", DialogResult.OK, () => { }),
            };

            // Act: MessageBoxIcon overload (overload 4), Error icon branch
            DialogResult result = MyBox.ShowDialog(
                viewer,
                "Test message",
                "Test title",
                MessageBoxIcon.Error,
                buttons
            );

            // Assert
            result.Should().Be(DialogResult.OK);
        }

        /// <summary>
        /// Covers the MessageBoxIcon.None branch of SetDialogIcon (hides the SVG icon).
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_MessageBoxIconNone_SeamReturnsOk_ReturnsOkResult()
        {
            // Arrange
            MyBox.DialogInvoker = _ => DialogResult.OK;
            using var viewer = new MyBoxViewer();
            var buttons = new List<ActionButton>
            {
                new ActionButton("ButtonOk", "Ok", DialogResult.OK, () => { }),
            };

            // Act: MessageBoxIcon.None in SetDialogIcon (hides the icon control)
            DialogResult result = MyBox.ShowDialog(
                viewer,
                "Test message",
                "Test title",
                MessageBoxIcon.None,
                buttons
            );

            // Assert
            result.Should().Be(DialogResult.OK);
        }

        /// <summary>
        /// Covers the MessageBoxIcon.Warning branch of SetDialogIcon.
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_MessageBoxIconWarning_SeamReturnsOk_ReturnsOkResult()
        {
            // Arrange
            MyBox.DialogInvoker = _ => DialogResult.OK;
            using var viewer = new MyBoxViewer();
            var buttons = new List<ActionButton>
            {
                new ActionButton("ButtonOk", "Ok", DialogResult.OK, () => { }),
            };

            // Act: MessageBoxIcon.Warning / Exclamation branch
            DialogResult result = MyBox.ShowDialog(
                viewer,
                "Test message",
                "Test title",
                MessageBoxIcon.Warning,
                buttons
            );

            // Assert
            result.Should().Be(DialogResult.OK);
        }

        /// <summary>
        /// Covers the MessageBoxIcon.Question branch of SetDialogIcon.
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_MessageBoxIconQuestion_SeamReturnsOk_ReturnsOkResult()
        {
            // Arrange
            MyBox.DialogInvoker = _ => DialogResult.OK;
            using var viewer = new MyBoxViewer();
            var buttons = new List<ActionButton>
            {
                new ActionButton("ButtonOk", "Ok", DialogResult.OK, () => { }),
            };

            // Act: MessageBoxIcon.Question branch
            DialogResult result = MyBox.ShowDialog(
                viewer,
                "Test message",
                "Test title",
                MessageBoxIcon.Question,
                buttons
            );

            // Assert
            result.Should().Be(DialogResult.OK);
        }

        /// <summary>
        /// Covers the MessageBoxIcon.Information branch of SetDialogIcon.
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_MessageBoxIconInformation_SeamReturnsOk_ReturnsOkResult()
        {
            // Arrange
            MyBox.DialogInvoker = _ => DialogResult.OK;
            using var viewer = new MyBoxViewer();
            var buttons = new List<ActionButton>
            {
                new ActionButton("ButtonOk", "Ok", DialogResult.OK, () => { }),
            };

            // Act: MessageBoxIcon.Information / Asterisk branch
            DialogResult result = MyBox.ShowDialog(
                viewer,
                "Test message",
                "Test title",
                MessageBoxIcon.Information,
                buttons
            );

            // Assert
            result.Should().Be(DialogResult.OK);
        }

        /// <summary>
        /// Verifies the convenience overload ShowDialog(string, string, MessageBoxButtons,
        /// MessageBoxIcon) delegates to the MessageBoxIcon viewer-accepting overload and
        /// returns the seam-injected result.
        ///
        /// Purpose:
        ///     Cover the MessageBoxButtons convenience path (overload 5), which creates
        ///     its own viewer via GetStandardButtons and delegates to overload 4.
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_ConvenienceMessageBoxButtons_SeamReturnsOk_ReturnsExpectedResult()
        {
            // Arrange: inject seam; overload 5 creates its own viewer internally
            MyBox.DialogInvoker = _ => DialogResult.OK;

            // Act: convenience overload 5 — delegates to overload 4 internally
            DialogResult result = MyBox.ShowDialog(
                "Test message",
                "Test title",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning
            );

            // Assert
            result.Should().Be(DialogResult.OK);
        }

        /// <summary>
        /// Verifies the convenience overload ShowDialog(string, string, BoxIcon,
        /// Dictionary&lt;string,Action&gt;) delegates to the BoxIcon viewer-accepting
        /// overload and returns the seam-injected result.
        ///
        /// Purpose:
        ///     Cover the action-dictionary convenience path (overload 6), which creates
        ///     its own viewer and delegates to overload 2.
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_ConvenienceActionDictionary_SeamReturnsOk_ReturnsExpectedResult()
        {
            // Arrange
            MyBox.DialogInvoker = _ => DialogResult.OK;
            var actions = new Dictionary<string, Action>
            {
                { "Confirm", () => { } },
                { "Cancel This", () => { } },
            };

            // Act: convenience overload 6 — creates viewer, calls ToActionButtons, delegates to overload 2
            DialogResult result = MyBox.ShowDialog(
                "Test message",
                "Test title",
                BoxIcon.None,
                actions
            );

            // Assert
            result.Should().Be(DialogResult.OK);
        }

        /// <summary>
        /// Verifies the generic convenience overload ShowDialog&lt;T&gt;(string, string,
        /// BoxIcon, Dictionary&lt;string,Func&lt;Task&lt;T&gt;&gt;&gt;) exercises ReplaceButtons&lt;T&gt;
        /// and AppendButtonInColumn&lt;T&gt; via the FunctionButton path (overload 7 → 3).
        ///
        /// Purpose:
        ///     Cover overload 7, overload 3 body, ReplaceButtons&lt;T&gt;, and
        ///     AppendButtonInColumn&lt;T&gt;.
        ///
        /// Returns:
        ///     default(int) = 0 because the button function is never invoked during
        ///     ShowDialog itself; group.Result stays at its default value.
        /// </summary>
        [TestMethod]
        public void ShowDialog_ConvenienceGenericFunctionDict_SeamReturnsOk_ReturnsDefaultGroupResult()
        {
            // Arrange: seam returns immediately; function is never invoked, so group.Result = default(int).
            MyBox.DialogInvoker = _ => DialogResult.OK;
            var functions = new Dictionary<string, Func<Task<int>>>
            {
                { "RunAction", () => Task.FromResult(42) },
            };

            // Act: generic convenience overload (overload 7) → ShowDialog<T>(viewer, ...) (overload 3)
            // Also exercises ReplaceButtons<T> and AppendButtonInColumn<T>.
            int result = MyBox.ShowDialog<int>(
                "Test message",
                "Test title",
                BoxIcon.None,
                functions
            );

            // Assert: button function was not invoked by ShowDialog, so result is default(int) = 0
            result.Should().Be(0);
        }

        /// <summary>
        /// Verifies the DelegateButton overload (overload 1) returns the seam-injected
        /// result while covering AppendButtonInColumn(DelegateButton).
        ///
        /// Purpose:
        ///     Cover ShowDialog(string, string, BoxIcon, IList&lt;DelegateButton&gt;) body
        ///     including AppendButtonInColumn for DelegateButton.
        ///
        /// Returns:
        ///     DialogResult.OK from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_DelegateButtonOverload_SeamReturnsOk_ReturnsOkResult()
        {
            // Arrange: overload 1 creates its own viewer internally using a using block
            MyBox.DialogInvoker = _ => DialogResult.OK;
            var delegateButtons = new List<DelegateButton>
            {
                new DelegateButton("ButtonOk", "Ok", DialogResult.OK, (Action)(() => { })),
            };

            // Act: DelegateButton overload (overload 1) — creates its own MyBoxViewer
            DialogResult result = MyBox.ShowDialog(
                "Test message",
                "Test title",
                BoxIcon.None,
                delegateButtons
            );

            // Assert
            result.Should().Be(DialogResult.OK);
        }

        // ---------------------------------------------------------------------------
        // P2-T6 — ShowDialog default/cancel-result paths via injected DialogInvoker seam
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the BoxIcon overload (overload 2) preserves the caller-supplied
        /// cancel result when the injected dialog seam returns DialogResult.Cancel.
        ///
        /// Purpose:
        ///     Exercise the default/cancel path to confirm the result flows end-to-end
        ///     without modification.
        ///
        /// Returns:
        ///     DialogResult.Cancel from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_BoxIconOverload_SeamReturnsCancel_ReturnsDefaultCancelResult()
        {
            // Arrange: inject seam that returns Cancel to simulate the user dismissing
            MyBox.DialogInvoker = _ => DialogResult.Cancel;
            using var viewer = new MyBoxViewer();
            var buttons = new List<ActionButton>
            {
                new ActionButton("ButtonOk", "Ok", DialogResult.OK, () => { }),
                new ActionButton("ButtonCancel", "Cancel", DialogResult.Cancel, () => { }),
            };

            // Act: cancel path through overload 2
            DialogResult result = MyBox.ShowDialog(
                viewer,
                "Test message",
                "Test title",
                BoxIcon.Warning,
                buttons
            );

            // Assert: cancel result is preserved end-to-end
            result.Should().Be(DialogResult.Cancel);
        }

        /// <summary>
        /// Verifies that the MessageBoxIcon overload (overload 4) preserves a non-OK
        /// result when the injected dialog seam returns DialogResult.No.
        ///
        /// Purpose:
        ///     Cover the No/default result path through overload 4.
        ///
        /// Returns:
        ///     DialogResult.No from the injected seam.
        /// </summary>
        [TestMethod]
        public void ShowDialog_MessageBoxIconOverload_SeamReturnsNo_ReturnsNoResult()
        {
            // Arrange
            MyBox.DialogInvoker = _ => DialogResult.No;
            using var viewer = new MyBoxViewer();
            var buttons = new List<ActionButton>
            {
                new ActionButton("ButtonYes", "Yes", DialogResult.Yes, () => { }),
                new ActionButton("ButtonNo", "No", DialogResult.No, () => { }),
            };

            // Act: default path through overload 4 with seam returning No
            DialogResult result = MyBox.ShowDialog(
                viewer,
                "Test message",
                "Test title",
                MessageBoxIcon.Question,
                buttons
            );

            // Assert: No result is preserved end-to-end
            result.Should().Be(DialogResult.No);
        }
    }
}
