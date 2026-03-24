using System;
using System.Collections.Generic;
using System.Threading;
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
    ///     Tests that instantiate WinForms controls require STA threads.
    ///     Internal members are accessible via InternalsVisibleTo("UtilitiesCS.Test").
    /// </summary>
    [TestClass]
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
        [STAThread]
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
        [STAThread]
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
        [STAThread]
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
        [STAThread]
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
    }
}
