using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #486 move-option menu contracts. The four pins fix the already-correct
    /// <see cref="ToolStripMenuItemCb"/> setter behaviour so that the deletions in Phase 2 cannot
    /// silently change it; the fifth test is failure-first and asserts that the redundant
    /// <c>MenuItem_CheckedChanged</c> handler no longer exists on <c>ItemViewerExpanded</c>.
    /// </summary>
    /// <remarks>
    /// <see cref="ToolStripMenuItemCb"/> derives from <see cref="ToolStripMenuItem"/> and therefore
    /// from <see cref="Component"/>, not from <see cref="Control"/>. It needs no window handle and
    /// no live form, so every test here constructs it directly.
    /// </remarks>
    [TestClass]
    public sealed class ToolStripMenuItemCbTests
    {
        /// <summary>
        /// Renders an image to a stable byte sequence so two distinct <see cref="Bitmap"/>
        /// instances materialised from the same embedded resource compare equal by content.
        /// </summary>
        private static byte[] ToBytes(Image image)
        {
            return (byte[])new ImageConverter().ConvertTo(image, typeof(byte[]));
        }

        [TestMethod]
        public void Checked_WhenSetTrue_AssignsCheckedCheckBoxImage()
        {
            // Arrange
            using (var item = new ToolStripMenuItemCb())
            {
                // Act
                item.Checked = true;
                Image assigned = ((ToolStripMenuItem)item).Image;

                // Assert
                using (new AssertionScope())
                {
                    assigned
                        .Should()
                        .NotBeNull("the setter assigns the checked check-box image directly");
                    ToBytes(assigned)
                        .Should()
                        .Equal(
                            ToBytes(QuickFiler.Properties.Resources.CheckBoxChecked),
                            "the assigned image is the CheckBoxChecked resource"
                        );
                }
            }
        }

        [TestMethod]
        public void Checked_WhenSetFalse_AssignsNullImage()
        {
            // Arrange
            using (var item = new ToolStripMenuItemCb())
            {
                item.Checked = true;
                ((ToolStripMenuItem)item).Image.Should().NotBeNull();

                // Act
                item.Checked = false;

                // Assert
                ((ToolStripMenuItem)item)
                    .Image.Should()
                    .BeNull("clearing the flag clears the image in the same setter");
            }
        }

        [TestMethod]
        public void Checked_WhenSetTrue_RaisesShadowedCheckedChangedExactlyOnce()
        {
            // Arrange
            using (var item = new ToolStripMenuItemCb())
            {
                int raised = 0;
                item.CheckedChanged += (sender, args) => raised++;

                // Act
                item.Checked = true;

                // Assert
                raised
                    .Should()
                    .Be(
                        1,
                        "the shadowed CheckedChanged event is raised once per assignment to Checked"
                    );
            }
        }

        [TestMethod]
        public void ToolStripMenuItemCb_IsNotDerivedFromControl()
        {
            // Act
            Type type = typeof(ToolStripMenuItemCb);

            // Assert
            using (new AssertionScope())
            {
                type.IsSubclassOf(typeof(ToolStripMenuItem))
                    .Should()
                    .BeTrue("the control extends the framework menu item");
                type.IsSubclassOf(typeof(Component))
                    .Should()
                    .BeTrue("ToolStripItem derives from Component");
                type.IsSubclassOf(typeof(Control))
                    .Should()
                    .BeFalse(
                        "no window handle is required, so these tests do not trip the live-form structural guard"
                    );
            }
        }

        [TestMethod]
        public void ItemViewerExpanded_DeclaresNoMenuItemCheckedChangedHandler()
        {
            // Arrange
            const BindingFlags Flags =
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            // Act
            MethodInfo eventHandlerForm = typeof(QuickFiler.ItemViewerExpanded).GetMethod(
                "MenuItem_CheckedChanged",
                Flags,
                null,
                new[] { typeof(object), typeof(EventArgs) },
                null
            );
            MethodInfo typedForm = typeof(QuickFiler.ItemViewerExpanded).GetMethod(
                "MenuItem_CheckedChanged",
                Flags,
                null,
                new[] { typeof(ToolStripMenuItem) },
                null
            );

            // Assert
            using (new AssertionScope())
            {
                eventHandlerForm
                    .Should()
                    .BeNull(
                        "the EventHandler-shaped overload duplicates the ToolStripMenuItemCb setter and must be deleted"
                    );
                typedForm
                    .Should()
                    .BeNull(
                        "the typed overload clears the image the setter just assigned and must be deleted"
                    );
            }
        }
    }
}
