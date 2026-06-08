using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Dialogs
{
    /// <summary>
    /// Unit tests for <see cref="MyBox"/> static helper and its nested types.
    ///
    /// Purpose:
    ///     Verify that GetStandardButtons preserves DialogResult ordering, that
    ///     ToActionButtons assigns button mappings correctly, and that
    ///     FunctionButtonGroup&lt;T&gt; routes results through the delegate correctly.
    ///
    /// Constraints:
    ///     This class runs under MSTest's STA class execution mode because its
    ///     tests create WinForms controls directly or through MyBox helpers.
    ///     Internal members are accessible via InternalsVisibleTo("UtilitiesCS.Test").
    /// </summary>
    [STATestClass]
    public class MyBox_Tests
    {
        // ---------------------------------------------------------------------------
        // P4-T1: GetStandardButtons preserves DialogResult ordering
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void GetStandardButtons_OkCancel_ReturnsOkThenCancel()
        {
            // Arrange / Act
            IList<ActionButton> buttons = MyBox.GetStandardButtons(MessageBoxButtons.OKCancel);

            // Assert — OK must come first to preserve dialog-result ordering contract
            buttons.Count.Should().Be(2);
            buttons[0].Button.DialogResult.Should().Be(DialogResult.OK);
            buttons[1].Button.DialogResult.Should().Be(DialogResult.Cancel);
        }

        [TestMethod]
        public void GetStandardButtons_YesNo_ReturnsYesThenNo()
        {
            // Arrange / Act
            IList<ActionButton> buttons = MyBox.GetStandardButtons(MessageBoxButtons.YesNo);

            // Assert
            buttons.Count.Should().Be(2);
            buttons[0].Button.DialogResult.Should().Be(DialogResult.Yes);
            buttons[1].Button.DialogResult.Should().Be(DialogResult.No);
        }

        [TestMethod]
        public void GetStandardButtons_YesNoCancel_ReturnsThreeButtonsInOrder()
        {
            // Arrange / Act
            IList<ActionButton> buttons = MyBox.GetStandardButtons(MessageBoxButtons.YesNoCancel);

            // Assert — three-button sets must preserve Yes → No → Cancel ordering
            buttons.Count.Should().Be(3);
            buttons[0].Button.DialogResult.Should().Be(DialogResult.Yes);
            buttons[1].Button.DialogResult.Should().Be(DialogResult.No);
            buttons[2].Button.DialogResult.Should().Be(DialogResult.Cancel);
        }

        [TestMethod]
        public void GetStandardButtons_OK_ReturnsSingleOkButton()
        {
            // Arrange / Act
            IList<ActionButton> buttons = MyBox.GetStandardButtons(MessageBoxButtons.OK);

            // Assert
            buttons.Count.Should().Be(1);
            buttons[0].Button.DialogResult.Should().Be(DialogResult.OK);
        }

        // ---------------------------------------------------------------------------
        // P4-T2: ToActionButtons assigns Cancel/OK dialog results by key content
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void ToActionButtons_KeyContainingCancel_AssignsCancelDialogResult()
        {
            // Arrange — create actions where one key contains "Cancel" and one does not
            using var viewer = new MyBoxViewer();
            var actions = new Dictionary<string, Action>
            {
                { "Confirm", () => { } },
                { "Cancel This", () => { } },
            };

            // Act — ToActionButtons is extension method on Dictionary<string,Action>
            IList<ActionButton> result = actions.ToActionButtons(viewer);

            // Assert — "Confirm" key → OK, "Cancel This" key → Cancel
            result[0].Button.DialogResult.Should().Be(DialogResult.OK);
            result[1].Button.DialogResult.Should().Be(DialogResult.Cancel);
        }

        [TestMethod]
        public void ToActionButtons_NoKeyContainingCancel_AllGetOkDialogResult()
        {
            // Arrange
            using var viewer = new MyBoxViewer();
            var actions = new Dictionary<string, Action>
            {
                { "Yes", () => { } },
                { "No", () => { } },
            };

            // Act
            IList<ActionButton> result = actions.ToActionButtons(viewer);

            // Assert — neither key contains Cancel, so both should map to DialogResult.OK
            result[0].Button.DialogResult.Should().Be(DialogResult.OK);
            result[1].Button.DialogResult.Should().Be(DialogResult.OK);
        }

        // ---------------------------------------------------------------------------
        // P4-T3: FunctionButtonGroup<T> routing returns the mapped result value
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void ToFunctionButtonsAsync_WhenFunctionInvoked_SetsGroupResult()
        {
            // Arrange — create a single-entry dictionary mapping a label to an async function
            using var viewer = new MyBoxViewer();
            const string expected = "the-result";
            var functions = new Dictionary<string, Func<Task<string>>>
            {
                { "RunAction", () => Task.FromResult(expected) },
            };

            // Act — build the FunctionButtonGroup from the dictionary
            MyBox.FunctionButtonGroup<string> group = functions.ToFunctionButtonsAsync(viewer);

            // Invoke the wrapped function on the first button — this sets group.Result
            group.FunctionButtons[0].ButtonClickedAsync.Invoke().GetAwaiter().GetResult();

            // Assert — after invoking, the group result must equal the expected return value
            group.Result.Should().Be(expected);
        }

        [TestMethod]
        public void ToFunctionButtonsAsync_MultipleFunctions_EachButtonHasOkDialogResult()
        {
            // Arrange
            using var viewer = new MyBoxViewer();
            var functions = new Dictionary<string, Func<Task<int>>>
            {
                { "ActionA", () => Task.FromResult(1) },
                { "ActionB", () => Task.FromResult(2) },
            };

            // Act
            MyBox.FunctionButtonGroup<int> group = functions.ToFunctionButtonsAsync(viewer);

            // Assert — all function buttons default to DialogResult.OK per implementation
            group.FunctionButtons.Should().HaveCount(2);
            group.FunctionButtons[0].Button.DialogResult.Should().Be(DialogResult.OK);
            group.FunctionButtons[1].Button.DialogResult.Should().Be(DialogResult.OK);
        }

        // ---------------------------------------------------------------------------
        // P54-T1 — Mapped delegate is invoked when the corresponding button is clicked
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that clicking Button1 in a map-constructed MyBoxViewer invokes
        /// the delegate associated with the first map key.
        ///
        /// Purpose:
        ///     Confirm that Button1_Click dispatches to the delegate stored for keys[0]
        ///     and that the invocation is observable by the caller.
        ///
        /// Returns:
        ///     Passes when <c>wasCalled</c> is true after Button1.PerformClick().
        /// </summary>
        [TestMethod]
        public void MappedDelegate_IsInvokedWhenButton1IsClicked()
        {
            // Arrange: build a map whose first delegate records a call
            bool wasCalled = false;
            var map = new Dictionary<string, Delegate>
            {
                ["Action1"] =
                    (Func<DialogResult>)(
                        () =>
                        {
                            wasCalled = true;
                            return DialogResult.OK;
                        }
                    ),
                ["Action2"] = (Func<DialogResult>)(() => DialogResult.Cancel),
            };
            using var viewer = new MyBoxViewer("Title", "Message", map);

            // Invoke the private Button1_Click handler directly via reflection
            // (PerformClick() silently skips on non-shown forms due to CanSelect=false)
            var button1Click = typeof(MyBoxViewer).GetMethod(
                "Button1_Click",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Act: fire the click handler as if Button1 were clicked
            button1Click.Invoke(viewer, new object[] { viewer, EventArgs.Empty });

            // Assert: the delegate mapped to keys[0] was invoked
            wasCalled.Should().BeTrue();
        }

        // ---------------------------------------------------------------------------
        // P54-T2 — RemoveStandardButtons leaves L2Bottom with no button controls
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that calling <c>RemoveStandardButtons()</c> removes Button1 and
        /// Button2 from the bottom panel, leaving no Button controls behind.
        ///
        /// Purpose:
        ///     Confirm that <c>RemoveStandardButtonControls</c> and
        ///     <c>RemoveStandardButtonColumns</c> are both called and produce the
        ///     expected empty-panel state.
        ///
        /// Returns:
        ///     Passes when L2Bottom.Controls contains zero Button instances.
        /// </summary>
        [TestMethod]
        public void RemoveStandardButtons_LeavesNoButtonControlsInBottomPanel()
        {
            // Arrange
            var map = new Dictionary<string, Delegate>
            {
                ["A"] = (Func<DialogResult>)(() => DialogResult.OK),
                ["B"] = (Func<DialogResult>)(() => DialogResult.Cancel),
            };
            using var viewer = new MyBoxViewer("Title", "Message", map);

            // Act
            viewer.RemoveStandardButtons();

            // Assert: L2Bottom no longer contains any Button controls
            var remainingButtons = viewer.L2Bottom.Controls.OfType<Button>().ToList();
            remainingButtons.Should().BeEmpty();
        }

        // ---------------------------------------------------------------------------
        // P54-T3 — CalcMinSize returns the recalculated minimum size
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that <c>CalcMinSize</c> subtracts the two standard button column
        /// widths from the form's current minimum width.
        ///
        /// Purpose:
        ///     Confirm that the recalculation logic correctly reads ColumnStyles[1]
        ///     and ColumnStyles[2] widths and subtracts them from the initial minimum
        ///     width. The expected value is computed dynamically because WinForms DPI
        ///     scaling adjusts both MinimumSize and column widths at runtime.
        ///
        /// Returns:
        ///     Passes when CalcMinSize().Width equals MinimumSize.Width minus the
        ///     sum of ColumnStyles[1] and ColumnStyles[2] widths.
        /// </summary>
        [TestMethod]
        public void CalcMinSize_AfterInit_ReturnsExpectedReducedWidth()
        {
            // Arrange: read the actual (DPI-scaled) values to avoid hardcoding
            using var viewer = new MyBoxViewer();
            int originalMinWidth = viewer.MinimumSize.Width;
            int colWidths = (int)
                Math.Round(
                    viewer.L2Bottom.ColumnStyles[1].Width + viewer.L2Bottom.ColumnStyles[2].Width,
                    0
                );
            int expected = originalMinWidth > colWidths ? originalMinWidth - colWidths : 0;

            // Act
            var result = viewer.CalcMinSize();

            // Assert: width was reduced by exactly the two column widths
            result.Width.Should().Be(expected);
        }
    }
}
