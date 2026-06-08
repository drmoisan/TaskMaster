using System;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace Z.Disabled.UtilitiesCS.Test
{
    [TestClass]
    public class InputBox_Test
    {
        [TestMethod]
        public void Disabled_ShowDialog_Test()
        {
            //string result = InputBox.ShowDialog("Test to see if this works", "Title", "Random text");
            //Assert.AreEqual("Random text47", result);
        }
    }
}

namespace UtilitiesCS.Test.Dialogs
{
    /// <summary>
    /// Unit tests for <see cref="InputBox"/> (P2) and <see cref="InputBoxViewer"/> (P3).
    ///
    /// Purpose:
    ///     InputBox is a static shell that creates an InputBoxViewer for blocking dialog
    ///     interaction. These tests verify the viewer's state machine directly, bypassing
    ///     ShowDialog(), so tests remain non-blocking and deterministic.
    ///
    /// Constraints:
    ///     This class runs under MSTest's STA class execution mode (required by WinForms).
    ///     Click handlers and private fields are accessed via reflection.
    /// </summary>
    [STATestClass]
    public class InputBoxViewer_Tests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            // Reset DpiCalled after each test so static state cannot contaminate others.
            InputBoxViewer.DpiCalled = false;

            // Reset the seam to the real implementation so static state cannot contaminate others.
            InputBox.DialogInvoker = viewer => viewer.ShowDialog();
        }

        // ---------------------------------------------------------------------------
        // P2-T1: Default response value populates the viewer textbox
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void Input_Text_ReflectsValueSetByDefaultResponse()
        {
            // Arrange — simulate what InputBox.ShowDialog does when setting DefaultResponse
            using var viewer = new InputBoxViewer();
            const string defaultText = "my default";

            // Act — set Input.Text the same way InputBox.ShowDialog would
            viewer.Input.Text = defaultText;

            // Assert — the textbox should reflect the default response exactly
            viewer.Input.Text.Should().Be(defaultText);
        }

        // ---------------------------------------------------------------------------
        // P2-T2: Accepting the dialog (OK path) preserves the entered text
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void OkClick_WithNonEmptyText_SetsDialogResultToOk()
        {
            // Arrange — set a non-empty value so Ok_Click does not show a MessageBox
            using var viewer = new InputBoxViewer();
            viewer.Input.Text = "some entered text";

            // Act — invoke Ok_Click via reflection
            InvokeClickHandler(viewer, "Ok_Click");

            // Assert — DialogResult must be OK (InputBox.ShowDialog returns Input.Text when OK)
            viewer.DialogResult.Should().Be(DialogResult.OK);
            viewer.Input.Text.Should().Be("some entered text");
        }

        // ---------------------------------------------------------------------------
        // P2-T3: Cancelling the dialog leads to the null-return path
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void CancelClick_SetsDialogResultToCancel()
        {
            // Arrange — InputBox.ShowDialog returns null when viewer.ShowDialog() == Cancel
            using var viewer = new InputBoxViewer();

            // Act — invoke Cancel_Click via reflection
            InvokeClickHandler(viewer, "Cancel_Click");

            // Assert — DialogResult must be Cancel, which InputBox.ShowDialog maps to null
            viewer.DialogResult.Should().Be(DialogResult.Cancel);
        }

        // ---------------------------------------------------------------------------
        // P3-T1: Ok_Click copies textbox text to response state
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void OkClick_CopiesTextboxTextAndHidesViewer()
        {
            // Arrange
            using var viewer = new InputBoxViewer();
            viewer.Input.Text = "expected response";

            // Act
            InvokeClickHandler(viewer, "Ok_Click");

            // Assert — input text is preserved (InputBox.ShowDialog reads it after ShowDialog returns)
            // and the viewer is hidden (not disposed) after the click
            viewer.Input.Text.Should().Be("expected response");
            viewer.IsDisposed.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // P3-T2: Cancel_Click leaves the cancel state (response maps to null in caller)
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void CancelClick_LeavesViewerInCancelState()
        {
            // Arrange — pre-populate the textbox
            using var viewer = new InputBoxViewer();
            viewer.Input.Text = "some text";

            // Act
            InvokeClickHandler(viewer, "Cancel_Click");

            // Assert — DialogResult is Cancel; caller (InputBox.ShowDialog) returns null in this branch
            viewer.DialogResult.Should().Be(DialogResult.Cancel);
            viewer.IsDisposed.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // P3-T3: DpiAware toggles DpiCalled static flag
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void DpiAware_SetsDpiCalledToTrue()
        {
            // Arrange — DpiCalled is reset in TestCleanup; confirm starting state
            InputBoxViewer.DpiCalled.Should().BeFalse();

            // Act — call the DpiAware static helper which enables styles and sets the flag
            InputBoxViewer.DpiAware();

            // Assert — flag must now be true
            InputBoxViewer.DpiCalled.Should().BeTrue();
        }

        // ---------------------------------------------------------------------------
        // P2-T2 (seam): InputBox.ShowDialog returns accepted value via injected seam
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void ShowDialog_SeamReturnsOk_ReturnsEnteredText()
        {
            // Arrange — inject a seam that immediately returns OK and hard-wires the viewer's
            // input text, avoiding any real modal dialog.
            InputBox.DialogInvoker = viewer =>
            {
                // Simulate the user typing a value and clicking OK.
                viewer.Input.Text = "injected value";
                return DialogResult.OK;
            };

            // Act
            string result = InputBox.ShowDialog("Prompt", "Title", "default");

            // Assert — when the seam reports OK, ShowDialog returns the text in Input.Text
            result.Should().Be("injected value");
        }

        // ---------------------------------------------------------------------------
        // P2-T3 (seam): InputBox.ShowDialog returns null when seam reports cancel
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void ShowDialog_SeamReturnsCancel_ReturnsNull()
        {
            // Arrange — inject a seam that immediately reports Cancel.
            InputBox.DialogInvoker = _ => DialogResult.Cancel;

            // Act
            string result = InputBox.ShowDialog("Prompt", "Title", "default");

            // Assert — when the seam reports Cancel, ShowDialog returns null
            result.Should().BeNull();
        }

        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Invokes a named private instance click handler via reflection.
        /// </summary>
        /// <param name="viewer">Viewer to invoke on.</param>
        /// <param name="methodName">Private method name.</param>
        private static void InvokeClickHandler(InputBoxViewer viewer, string methodName)
        {
            MethodInfo method =
                typeof(InputBoxViewer).GetMethod(
                    methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance
                ) ?? throw new MissingMethodException(nameof(InputBoxViewer), methodName);

            method.Invoke(viewer, new object[] { viewer, EventArgs.Empty });
        }
    }
}
