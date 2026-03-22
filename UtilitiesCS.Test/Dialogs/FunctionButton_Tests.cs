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
    }
}
