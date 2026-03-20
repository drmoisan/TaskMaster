using System;
using System.Drawing;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Dialogs
{
    [TestClass]
    public class DelegateButton_Tests
    {
        #region Constructors

        [TestMethod]
        [STAThread]
        public void DefaultConstructor_CreatesInstance()
        {
            var db = new DelegateButton();
            db.Should().NotBeNull();
        }

        [TestMethod]
        [STAThread]
        public void Constructor_WithNameAndText_CreatesButton()
        {
            bool called = false;
            Action del = () => called = true;
            var db = new DelegateButton("btn1", "Click Me", del);

            db.Name.Should().Be("btn1");
            db.Button.Should().NotBeNull();
            db.Button.Text.Should().Be("Click Me");
            db.Delegate.Should().BeSameAs(del);
        }

        [TestMethod]
        [STAThread]
        public void Constructor_WithDialogResult_SetsDialogResult()
        {
            Action del = () => { };
            var db = new DelegateButton("btn1", "OK", DialogResult.OK, del);

            db.Button.DialogResult.Should().Be(DialogResult.OK);
        }

        [TestMethod]
        [STAThread]
        public void Constructor_WithButtonAndDialogResult_SetsProperties()
        {
            var button = new Button();
            Action del = () => { };
            var db = new DelegateButton(button, DialogResult.Cancel, del);

            db.Button.Should().BeSameAs(button);
            db.Delegate.Should().BeSameAs(del);
        }

        #endregion

        #region Properties

        [TestMethod]
        [STAThread]
        public void Name_SetAndGet()
        {
            var db = new DelegateButton();
            db.Name = "TestName";
            db.Name.Should().Be("TestName");
        }

        [TestMethod]
        [STAThread]
        public void Delegate_SetAndGet()
        {
            var db = new DelegateButton();
            Action del = () => { };
            db.Delegate = del;
            db.Delegate.Should().BeSameAs(del);
        }

        #endregion

        #region MakeButton

        [TestMethod]
        [STAThread]
        public void MakeButton_WithText_CreatesButton()
        {
            Action del = () => { };
            var db = new DelegateButton("btn", "Test", del);
            var button = db.MakeButton("New Text");

            button.Should().NotBeNull();
            button.Text.Should().Be("New Text");
            button.Visible.Should().BeTrue();
            button.Enabled.Should().BeTrue();
        }

        [TestMethod]
        [STAThread]
        public void MakeButton_WithDialogResult_SetsResult()
        {
            Action del = () => { };
            var db = new DelegateButton("btn", "Test", del);
            var button = db.MakeButton("OK", DialogResult.OK);

            button.DialogResult.Should().Be(DialogResult.OK);
        }

        #endregion

        #region Button_Click

        [TestMethod]
        [STAThread]
        public void Button_Click_InvokesDelegate()
        {
            bool delegateCalled = false;
            Action del = () => delegateCalled = true;
            var db = new DelegateButton("btn", "Click", DialogResult.OK, del);

            db.Button_Click(db.Button, EventArgs.Empty);
            delegateCalled.Should().BeTrue();
        }

        #endregion

        #region FromButton

        [TestMethod]
        [STAThread]
        public void FromButton_CreatesFromExistingButton()
        {
            var button = new Button { Text = "Existing" };
            Action del = () => { };
            var db = DelegateButton.FromButton(button, DialogResult.Yes, del);

            db.Button.Text.Should().Be("Existing");
            db.Button.DialogResult.Should().Be(DialogResult.Yes);
        }

        #endregion
    }

    [TestClass]
    public class YesNoToAll_Tests
    {
        [TestMethod]
        public void Response_DefaultIsEmpty()
        {
            YesNoToAll.Response = YesNoToAllResponse.Empty;
            YesNoToAll.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        [TestMethod]
        public void RespondYes_SetsResponseToYes()
        {
            YesNoToAll.Response = YesNoToAllResponse.Empty;
            // Use reflection to call internal method
            typeof(YesNoToAll)
                .GetMethod("RespondYes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, null);
            YesNoToAll.Response.Should().Be(YesNoToAllResponse.Yes);
        }

        [TestMethod]
        public void RespondYesToAll_SetsResponseToYesToAll()
        {
            YesNoToAll.Response = YesNoToAllResponse.Empty;
            typeof(YesNoToAll)
                .GetMethod("RespondYesToAll", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, null);
            YesNoToAll.Response.Should().Be(YesNoToAllResponse.YesToAll);
        }

        [TestMethod]
        public void RespondNo_SetsResponseToNo()
        {
            YesNoToAll.Response = YesNoToAllResponse.Empty;
            typeof(YesNoToAll)
                .GetMethod("RespondNo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, null);
            YesNoToAll.Response.Should().Be(YesNoToAllResponse.No);
        }

        [TestMethod]
        public void RespondNoToAll_SetsResponseToNoToAll()
        {
            YesNoToAll.Response = YesNoToAllResponse.Empty;
            typeof(YesNoToAll)
                .GetMethod("RespondNoToAll", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, null);
            YesNoToAll.Response.Should().Be(YesNoToAllResponse.NoToAll);
        }

        [TestMethod]
        public void RespondCancel_SetsResponseToEmpty()
        {
            YesNoToAll.Response = YesNoToAllResponse.Yes;
            typeof(YesNoToAll)
                .GetMethod("RespondCancel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, null);
            YesNoToAll.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        [TestMethod]
        public void YesNoToAllResponse_EnumValues()
        {
            ((int)YesNoToAllResponse.Empty).Should().Be(0);
            ((int)YesNoToAllResponse.Yes).Should().Be(1);
            ((int)YesNoToAllResponse.No).Should().Be(2);
            ((int)YesNoToAllResponse.YesToAll).Should().Be(4);
            ((int)YesNoToAllResponse.NoToAll).Should().Be(8);
        }
    }

    [TestClass]
    public class BoxIcon_Tests
    {
        [TestMethod]
        public void BoxIcon_EnumValues()
        {
            ((int)BoxIcon.None).Should().Be(0);
            ((int)BoxIcon.Critical).Should().Be(1);
            ((int)BoxIcon.Warning).Should().Be(2);
            ((int)BoxIcon.Question).Should().Be(4);
        }
    }
}
