using System;
using System.Drawing;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class DelegateButton_Tests
    {
        [TestMethod]
        public void Constructor_WithButtonAndDelegate_InitializesProperties()
        {
            // Arrange
            var button = new Button();
            var callback = new Action(() => { });

            // Act
            var delegateButton = new DelegateButton(button, DialogResult.OK, callback);

            // Assert
            delegateButton.Button.Should().BeSameAs(button);
            delegateButton.Delegate.Should().BeSameAs(callback);
            delegateButton.Button.DialogResult.Should().Be(DialogResult.OK);
        }

        [TestMethod]
        public void Constructor_WithNameTextAndTemplate_ClonesTemplateAndCreatesVisibleEnabledButton()
        {
            // Arrange
            var template = new Button
            {
                BackColor = Color.CadetBlue,
                Enabled = false,
                Visible = false,
            };
            var callback = new Action(() => { });

            // Act
            var delegateButton = new DelegateButton("Archive", "Archive Now", callback, template);

            // Assert
            delegateButton.Name.Should().Be("Archive");
            delegateButton.Button.Should().NotBeSameAs(template);
            delegateButton.Button.Text.Should().Be("Archive Now");
            delegateButton.Button.Visible.Should().BeTrue();
            delegateButton.Button.Enabled.Should().BeTrue();
            delegateButton.Button.BackColor.Should().Be(Color.CadetBlue);
        }

        [TestMethod]
        public void MakeButton_WithImageAndDialogResult_PreservesTextImageAndDialogResult()
        {
            // Arrange
            using var image = new Bitmap(8, 8);
            var delegateButton = new DelegateButton("Save", "Save", new Action(() => { }));

            // Act
            var button = delegateButton.MakeButton("Save All", image, DialogResult.Yes);

            // Assert
            button.Text.Should().Be("Save All");
            button.Image.Should().BeSameAs(image);
            button.DialogResult.Should().Be(DialogResult.Yes);
            button.TextImageRelation.Should().Be(TextImageRelation.ImageBeforeText);
        }

        [TestMethod]
        public void Constructor_WithDialogResultAndTemplate_ClonesTemplateAndSetsDialogResult()
        {
            // Arrange
            var template = new Button { BackColor = Color.DarkSeaGreen };
            var callback = new Action(() => { });

            // Act
            var delegateButton = new DelegateButton(
                "Apply",
                "Apply Changes",
                DialogResult.Yes,
                callback,
                template
            );

            // Assert
            delegateButton.Name.Should().Be("Apply");
            delegateButton.Button.Should().NotBeSameAs(template);
            delegateButton.Button.BackColor.Should().Be(Color.DarkSeaGreen);
            delegateButton.Button.DialogResult.Should().Be(DialogResult.Yes);
            delegateButton.Delegate.Should().BeSameAs(callback);
        }

        [TestMethod]
        public void Constructor_WithImageAndDialogResult_CreatesButtonWithImageBeforeText()
        {
            // Arrange
            using var image = new Bitmap(10, 10);
            var callback = new Action(() => { });

            // Act
            var delegateButton = new DelegateButton(
                "Retry",
                image,
                "Retry Now",
                DialogResult.Retry,
                callback
            );

            // Assert
            delegateButton.Name.Should().Be("Retry");
            delegateButton.Button.Text.Should().Be("Retry Now");
            delegateButton.Button.Image.Should().BeSameAs(image);
            delegateButton.Button.DialogResult.Should().Be(DialogResult.Retry);
            delegateButton.Button.TextImageRelation.Should().Be(TextImageRelation.ImageBeforeText);
            delegateButton.Delegate.Should().BeSameAs(callback);
        }

        [TestMethod]
        public void ButtonTemplate_SetterClonesTemplate_AndMakeButtonReplacesExistingImage()
        {
            // Arrange
            using var templateImage = new Bitmap(6, 6);
            using var replacementImage = new Bitmap(12, 12);
            var template = new Button { BackColor = Color.CornflowerBlue, Image = templateImage };
            var delegateButton = new DelegateButton();

            // Act
            delegateButton.ButtonTemplate = template;
            var storedTemplate = delegateButton.ButtonTemplate;
            var button = delegateButton.MakeButton("Replace Image", replacementImage);

            // Assert
            storedTemplate.Should().NotBeSameAs(template);
            storedTemplate.BackColor.Should().Be(Color.CornflowerBlue);
            storedTemplate.Image.Should().NotBeNull();
            button.Image.Should().BeSameAs(replacementImage);
            button.TextImageRelation.Should().Be(TextImageRelation.ImageBeforeText);
        }

        [TestMethod]
        public void FromButton_ShouldAttachDelegateAndInvokeOnClick()
        {
            // Arrange
            var clicked = false;
            var button = new Button();
            var delegateButton = DelegateButton.FromButton(
                button,
                DialogResult.Retry,
                new Action(() => clicked = true)
            );

            // Act
            button.PerformClick();

            // Assert
            delegateButton.Button.Should().BeSameAs(button);
            delegateButton.Button.DialogResult.Should().Be(DialogResult.Retry);
            clicked.Should().BeTrue();
        }

        [TestMethod]
        public void ButtonSetter_ShouldDetachOldButtonHandler_WhenReplacingButton()
        {
            // Arrange
            var oldClicked = false;
            var newClicked = false;
            var oldButton = new Button();
            var newButton = new Button();
            var delegateButton = new DelegateButton(
                oldButton,
                DialogResult.OK,
                new Action(() => oldClicked = true)
            );
            delegateButton.Delegate = new Action(() => newClicked = true);

            // Act
            delegateButton.Button = newButton;
            oldButton.PerformClick();
            newButton.PerformClick();

            // Assert
            oldClicked.Should().BeFalse();
            newClicked.Should().BeTrue();
        }
    }
}
