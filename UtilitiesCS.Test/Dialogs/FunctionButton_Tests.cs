using System;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Dialogs;

namespace UtilitiesCS.Test.Dialogs
{
    [TestClass]
    public class FunctionButton_Tests
    {
        #region Constructors

        [TestMethod]
        [STAThread]
        public void DefaultConstructor_CreatesInstance()
        {
            var fb = new FunctionButton<int>();
            fb.Should().NotBeNull();
        }

        [TestMethod]
        [STAThread]
        public void Constructor_WithNameAndText_CreatesButton()
        {
            Func<int> func = () => 42;
            var fb = new FunctionButton<int>("btn1", "Click", func);

            fb.Name.Should().Be("btn1");
            fb.Button.Should().NotBeNull();
            fb.Button.Text.Should().Be("Click");
        }

        [TestMethod]
        [STAThread]
        public void Constructor_WithDialogResult_SetsResult()
        {
            Func<int> func = () => 42;
            var fb = new FunctionButton<int>("btn1", "OK", DialogResult.OK, func);

            fb.Button.DialogResult.Should().Be(DialogResult.OK);
        }

        #endregion

        #region MakeButton

        [TestMethod]
        [STAThread]
        public void MakeButton_SetsText()
        {
            Func<int> func = () => 1;
            var fb = new FunctionButton<int>("btn", "Initial", func);
            var button = fb.MakeButton("NewText");

            button.Text.Should().Be("NewText");
            button.Visible.Should().BeTrue();
        }

        [TestMethod]
        [STAThread]
        public void MakeButton_WithDialogResult_SetsDialogResult()
        {
            Func<int> func = () => 1;
            var fb = new FunctionButton<int>("btn", "Test", func);
            var button = fb.MakeButton("OK", DialogResult.OK);

            button.DialogResult.Should().Be(DialogResult.OK);
        }

        #endregion

        #region Button_Click

        [TestMethod]
        [STAThread]
        public void Button_Click_InvokesFunction()
        {
            Func<int> func = () => 42;
            var fb = new FunctionButton<int>("btn", "Click", DialogResult.OK, func);

            fb.Button_Click(fb.Button, EventArgs.Empty);
            fb.Value.Should().Be(42);
        }

        #endregion

        #region FromButton

        [TestMethod]
        [STAThread]
        public void FromButton_CreatesFromExistingButton()
        {
            var button = new Button { Text = "Existing" };
            Func<string> func = () => "result";
            var fb = FunctionButton<string>.FromButton(button, DialogResult.Yes, func);

            fb.Button.Text.Should().Be("Existing");
            fb.Button.DialogResult.Should().Be(DialogResult.Yes);
        }

        #endregion

        #region Properties

        [TestMethod]
        [STAThread]
        public void Name_SetAndGet()
        {
            var fb = new FunctionButton<int>();
            fb.Name = "TestName";
            fb.Name.Should().Be("TestName");
        }

        [TestMethod]
        [STAThread]
        public void Delegate_SetAndGet()
        {
            var fb = new FunctionButton<int>();
            Func<int> func = () => 99;
            fb.Delegate = func;
            fb.Delegate.Should().BeSameAs(func);
        }

        #endregion

        #region ButtonReassignment

        // -----------------------------------------------------------------------
        // P53-T2 — Reassigning Button unwires the old click handler
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that reassigning the underlying Button property unwires the
        /// synchronous click handler from the previous button instance.
        ///
        /// Purpose:
        ///     Confirm that after <c>fb.Button = newButton</c>, firing the old
        ///     button's Click event does not invoke <c>ButtonClicked</c> and
        ///     therefore does not update <c>Value</c>.
        ///
        /// Returns:
        ///     Passes when Value remains at its default (0) after the old button
        ///     is clicked following reassignment.
        /// </summary>
        [TestMethod]
        [STAThread]
        public void ReassignButton_UnwiresOldClickHandler()
        {
            // Arrange: create a FunctionButton whose ButtonClicked wires Button_Click
            Func<int> func = () => 42;
            var fb = new FunctionButton<int>("btn", "Click", DialogResult.OK, func);
            var oldButton = fb.Button;

            // Act: replace the underlying button; the setter should unwire from oldButton
            fb.Button = new Button();

            // Simulate a click on the old button — handler is now detached
            oldButton.PerformClick();

            // Assert: Value was never set (handler was unwired before the click)
            fb.Value.Should().Be(0);
        }

        #endregion

        #region ButtonClickAsync

        // -----------------------------------------------------------------------
        // P53-T3 — Async callback executes exactly once when button is clicked
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that calling <c>Button_ClickAsync</c> directly executes the
        /// async callback exactly once and stores the result in <c>Value</c>.
        ///
        /// Purpose:
        ///     Confirm the async click path awaits <c>ButtonClickedAsync</c> and
        ///     writes the returned value to the Value property exactly once.
        ///
        /// Returns:
        ///     Passes when Value equals 99 and the delegate was invoked exactly once.
        /// </summary>
        [TestMethod]
        [STAThread]
        public void ButtonClickAsync_ExecutesCallbackExactlyOnce()
        {
            // Arrange: wire an already-complete async callback so the await resolves
            // synchronously (no SynchronizationContext in MSTest → Task.FromResult
            // continuation runs inline). Count invocations to assert exactly-once.
            var fb = new FunctionButton<int>();
            fb.Button = new Button();
            int callCount = 0;
            fb.ButtonClickedAsync = () =>
            {
                callCount++;
                return System.Threading.Tasks.Task.FromResult(99);
            };

            // Act: invoke the internal async handler directly
            fb.Button_ClickAsync(fb.Button, EventArgs.Empty);

            // Assert: delegate invoked exactly once and Value reflects the result
            callCount.Should().Be(1);
            fb.Value.Should().Be(99);
        }

        #endregion
    }
}
