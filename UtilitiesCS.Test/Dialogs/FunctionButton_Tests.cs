using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Dialogs;

namespace UtilitiesCS.Test.Dialogs
{
    [STATestClass]
    public class FunctionButton_Tests
    {
        #region Constructors

        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            var fb = new FunctionButton<int>();
            fb.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithNameAndText_CreatesButton()
        {
            Func<int> func = () => 42;
            var fb = new FunctionButton<int>("btn1", "Click", func);

            fb.Name.Should().Be("btn1");
            fb.Button.Should().NotBeNull();
            fb.Button.Text.Should().Be("Click");
        }

        [TestMethod]
        public void Constructor_WithDialogResult_SetsResult()
        {
            Func<int> func = () => 42;
            var fb = new FunctionButton<int>("btn1", "OK", DialogResult.OK, func);

            fb.Button.DialogResult.Should().Be(DialogResult.OK);
        }

        [TestMethod]
        public void SyncConstructorOverloads_InitializeButtonsTemplatesAndImages()
        {
            // Arrange
            var existingButton = new Button();
            var template = new Button { BackColor = Color.CadetBlue };
            using var image = new Bitmap(8, 8);

            // Act
            var fromButton = new FunctionButton<int>(existingButton, DialogResult.Yes, () => 7);
            var withTemplate = new FunctionButton<int>("templ", "Template", () => 8, template);
            var withDialogTemplate = new FunctionButton<int>(
                "templDialog",
                "TemplateDialog",
                DialogResult.OK,
                () => 9,
                template
            );
            var withImage = new FunctionButton<int>(
                "img",
                image,
                "Image",
                DialogResult.Retry,
                () => 10
            );
            var withImageTemplate = new FunctionButton<int>(
                "imgTempl",
                image,
                "ImageTemplate",
                DialogResult.Ignore,
                () => 11,
                template
            );

            // Assert
            fromButton.Button.Should().BeSameAs(existingButton);
            fromButton.Button.DialogResult.Should().Be(DialogResult.Yes);

            withTemplate.Button.Should().NotBeSameAs(template);
            withTemplate.Button.BackColor.Should().Be(Color.CadetBlue);
            withTemplate.Button.Text.Should().Be("Template");

            withDialogTemplate.Button.DialogResult.Should().Be(DialogResult.OK);
            withDialogTemplate.Button.BackColor.Should().Be(Color.CadetBlue);

            withImage.Button.Image.Should().BeSameAs(image);
            withImage.Button.TextImageRelation.Should().Be(TextImageRelation.ImageBeforeText);
            withImage.Button.DialogResult.Should().Be(DialogResult.Retry);

            withImageTemplate.Button.Image.Should().BeSameAs(image);
            withImageTemplate.Button.BackColor.Should().Be(Color.CadetBlue);
            withImageTemplate.Button.DialogResult.Should().Be(DialogResult.Ignore);
        }

        [TestMethod]
        public void AsyncConstructorOverloads_InitializeButtonsTemplatesAndImages()
        {
            // Arrange
            var existingButton = new Button();
            var template = new Button { BackColor = Color.CadetBlue };
            using var image = new Bitmap(8, 8);

            // Act
            var fromButton = new FunctionButton<int>(
                existingButton,
                DialogResult.Yes,
                () => Task.FromResult(12)
            );
            var nameText = new FunctionButton<int>("asyncText", "Async", () => Task.FromResult(13));
            var withTemplate = new FunctionButton<int>(
                "asyncTempl",
                "AsyncTemplate",
                () => Task.FromResult(14),
                template
            );
            var withDialog = new FunctionButton<int>(
                "asyncDialog",
                "AsyncDialog",
                DialogResult.OK,
                () => Task.FromResult(15)
            );
            var withDialogTemplate = new FunctionButton<int>(
                "asyncDialogTempl",
                "AsyncDialogTemplate",
                DialogResult.Retry,
                () => Task.FromResult(16),
                template
            );
            var withImage = new FunctionButton<int>(
                "asyncImage",
                image,
                "AsyncImage",
                DialogResult.Abort,
                () => Task.FromResult(17)
            );
            var withImageTemplate = new FunctionButton<int>(
                "asyncImageTempl",
                image,
                "AsyncImageTemplate",
                DialogResult.Cancel,
                () => Task.FromResult(18),
                template
            );

            // Assert
            fromButton.Button.Should().BeSameAs(existingButton);
            fromButton.Button.DialogResult.Should().Be(DialogResult.Yes);
            fromButton.ButtonClickedAsync.Should().NotBeNull();

            nameText.Button.Text.Should().Be("Async");
            nameText.ButtonClickedAsync.Should().NotBeNull();

            withTemplate.Button.BackColor.Should().Be(Color.CadetBlue);
            withTemplate.Button.Text.Should().Be("AsyncTemplate");

            withDialog.Button.DialogResult.Should().Be(DialogResult.OK);
            withDialogTemplate.Button.DialogResult.Should().Be(DialogResult.Retry);
            withDialogTemplate.Button.BackColor.Should().Be(Color.CadetBlue);

            withImage.Button.Image.Should().BeSameAs(image);
            withImage.Button.TextImageRelation.Should().Be(TextImageRelation.ImageBeforeText);

            withImageTemplate.Button.Image.Should().BeSameAs(image);
            withImageTemplate.Button.BackColor.Should().Be(Color.CadetBlue);
            withImageTemplate.Button.DialogResult.Should().Be(DialogResult.Cancel);
        }

        #endregion

        #region MakeButton

        [TestMethod]
        public void MakeButton_SetsText()
        {
            Func<int> func = () => 1;
            var fb = new FunctionButton<int>("btn", "Initial", func);
            var button = fb.MakeButton("NewText");

            button.Text.Should().Be("NewText");
            button.Visible.Should().BeTrue();
        }

        [TestMethod]
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
        public void Name_SetAndGet()
        {
            var fb = new FunctionButton<int>();
            fb.Name = "TestName";
            fb.Name.Should().Be("TestName");
        }

        [TestMethod]
        public void Delegate_SetAndGet()
        {
            var fb = new FunctionButton<int>();
            Func<int> func = () => 99;
            fb.Delegate = func;
            fb.Delegate.Should().BeSameAs(func);
        }

        [TestMethod]
        public void ButtonTemplate_SetAndGet_ClonesAssignedTemplate()
        {
            // Arrange
            var fb = new FunctionButton<int>();
            var template = new Button { BackColor = Color.DarkSeaGreen };

            // Act
            fb.ButtonTemplate = template;

            // Assert
            fb.ButtonTemplate.Should().NotBeSameAs(template);
            fb.ButtonTemplate.BackColor.Should().Be(Color.DarkSeaGreen);
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

        [TestMethod]
        public void ButtonClicked_Setter_ReplacesOldHandler()
        {
            // Arrange
            var fb = new FunctionButton<int> { Button = new Button() };
            fb.ButtonClicked = () => 1;

            // Act
            fb.ButtonClicked = () => 2;
            fb.Button_Click(fb.Button, EventArgs.Empty);

            // Assert
            fb.Value.Should().Be(2);
        }

        [TestMethod]
        public void ButtonClickedAsync_Setter_ReplacesOldHandler()
        {
            // Arrange
            var fb = new FunctionButton<int> { Button = new Button() };
            fb.ButtonClickedAsync = () => Task.FromResult(3);

            // Act
            fb.ButtonClickedAsync = () => Task.FromResult(4);
            fb.Button_ClickAsync(fb.Button, EventArgs.Empty);

            // Assert
            fb.Value.Should().Be(4);
        }

        #endregion
    }
}
