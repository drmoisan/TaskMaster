using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskVisualization;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// STA-bound coverage of the keyboard / mouse accelerator handlers
    /// (<c>KeyboardHandler_KeyDown</c>, <c>KeyboardHandler_KeyPress</c>,
    /// <c>MouseFilter_FormClicked</c>, <c>SuppressKeystrokes</c>) driven by direct invocation with
    /// synthetic <see cref="KeyEventArgs"/>/<see cref="KeyPressEventArgs"/> over a real
    /// <see cref="StaControlHarness"/>. No message pump, no shown window, no <see cref="Form"/>.
    /// Nav tips are warmed first to mirror the production precondition (InitializeAccelerators).
    /// </summary>
    [STATestClass]
    public class TaskControllerAcceleratorKeyboardStaTests
    {
        [STATestMethod]
        public void SuppressKeystrokes_IsFalseBeforeAnyAltActivation()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);

                controller.SuppressKeystrokes.Should().BeFalse();
            }
        }

        [STATestMethod]
        public void KeyboardHandler_KeyDown_NonAltWhileInactive_ReturnsFalse()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);

                var handled = controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.A));

                handled.Should().BeFalse();
            }
        }

        [STATestMethod]
        public void KeyboardHandler_KeyPress_WhileInactive_DoesNotHandle()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);
                var args = new KeyPressEventArgs('1');

                controller.KeyboardHandler_KeyPress(this, args);

                args.Handled.Should().BeFalse();
            }
        }

        [STATestMethod]
        public void MouseFilter_FormClicked_WhileInactive_IsNoOp()
        {
            // Seam-infeasibility (condition a): the accelerator visibility toggles operate on real
            // Control.Visible state; a mock cannot exercise the real toggle semantics.
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);

                System.Action act = () =>
                    controller.MouseFilter_FormClicked(this, System.EventArgs.Empty);

                act.Should().NotThrow();
                controller.SuppressKeystrokes.Should().BeFalse();
            }
        }

        [STATestMethod]
        public void KeyboardHandler_KeyDown_Alt_ActivatesNavThenDeactivates()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.All
                );
                var unused = controller.NavTips;

                var first = controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.Alt));
                controller.SuppressKeystrokes.Should().BeTrue();
                var second = controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.Alt));

                first.Should().BeTrue();
                second.Should().BeTrue();
                controller.SuppressKeystrokes.Should().BeFalse();
            }
        }

        [STATestMethod]
        public void KeyboardHandler_KeyDown_AltThenLetter_IsHandled()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.All
                );
                var unused = controller.NavTips;
                controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.Alt));

                var handled = controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.B));

                handled.Should().BeTrue();
            }
        }

        [STATestMethod]
        public void KeyboardHandler_KeyDown_AltThenDown_ActivatesFirstGroup()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.All
                );
                var unused = controller.NavTips;
                controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.Alt));

                var handled = controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.Down));

                handled.Should().BeTrue();
            }
        }

        [STATestMethod]
        public void KeyboardHandler_KeyDown_AltThenUp_ActivatesLastGroup()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.All
                );
                var unused = controller.NavTips;
                controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.Alt));

                var handled = controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.Up));

                handled.Should().BeTrue();
            }
        }

        [STATestMethod]
        public void KeyboardHandler_KeyPress_AltThenDigit_ActivatesGroupAndHandles()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.All
                );
                var unused = controller.NavTips;
                controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.Alt));
                var args = new KeyPressEventArgs('3');

                controller.KeyboardHandler_KeyPress(this, args);

                args.Handled.Should().BeTrue();
            }
        }

        [STATestMethod]
        public void MouseFilter_FormClicked_WhileActive_DeactivatesAccelerators()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.All
                );
                var unused = controller.NavTips;
                // Activate a group so _xlCtrlsActive is populated before the click filter runs.
                controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.Alt));
                controller.KeyboardHandler_KeyDown(this, new KeyEventArgs(Keys.Down));

                controller.MouseFilter_FormClicked(this, System.EventArgs.Empty);

                controller.SuppressKeystrokes.Should().BeFalse();
            }
        }
    }
}
