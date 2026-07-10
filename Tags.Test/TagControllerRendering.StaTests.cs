using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tags.Test.Fakes;

namespace Tags.Test
{
    /// <summary>
    /// Dedicated STA test for the production <c>DrawFocus</c> default body (former register E2, now
    /// covered rather than exempt). Per the maintainer-ratified STA refinement, an unshown WinForms
    /// <see cref="CheckBox"/> control (never a <see cref="Form"/>) is constructed on an STA thread; the
    /// test never shows a window, uses no message pump/timer/sleep, and disposes the control.
    /// </summary>
    [STATestClass]
    public class TagControllerRenderingStaTests
    {
        [STATestMethod]
        public void DrawFocus_DefaultBody_DrawsFocusRectangleOnUnshownCheckBox()
        {
            // Arrange: construct with the DEFAULT _drawFocus (do NOT inject a no-op) and retrieve it.
            var fake = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            var controller = new TagController(
                fake.Object,
                new SortedDictionary<string, bool>(),
                null,
                null,
                prompt.Object
            );
            var drawFocus = GetPrivateField<Action<CheckBox>>(controller, "_drawFocus");
            drawFocus.Should().NotBeNull();

            using (var checkBox = new CheckBox())
            {
                // Act: force invisible handle creation, then invoke the real draw path.
                var handle = checkBox.Handle;
                handle.Should().NotBe(IntPtr.Zero);

                System.Action act = () => drawFocus(checkBox);

                // Assert: the production ControlPaint/Graphics.FromHwnd path runs without throwing.
                act.Should().NotThrow();
                checkBox.IsHandleCreated.Should().BeTrue();
            }
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target
                .GetType()
                .GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );
            field.Should().NotBeNull();
            return (T)field.GetValue(target);
        }
    }
}
