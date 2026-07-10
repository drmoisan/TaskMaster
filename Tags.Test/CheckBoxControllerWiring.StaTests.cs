using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tags.Test.Fakes;

namespace Tags.Test
{
    /// <summary>
    /// Dedicated STA tests for the narrowed register E6 wiring — the <see cref="CheckBoxController.CtrlCB"/>
    /// subscribe/unsubscribe path and the <c>ctrlCB_Click</c> wrapper. Clicks are raised via the
    /// protected <c>Control.InvokeOnClick</c> reflection raiser (no message pump, no <c>CanSelect</c>
    /// gate) on unshown STA <see cref="CheckBox"/> controls (never a <see cref="Form"/>). No window is
    /// shown; no timer/sleep/pump is used; every control is disposed.
    /// </summary>
    [STATestClass]
    public class CheckBoxControllerWiringStaTests
    {
        [STATestMethod]
        public void CtrlCBSetter_WhenSubscribedCheckboxClicked_TogglesParentSelection()
        {
            var fake = new FakeTagViewer();
            var controller = BuildController(fake, out var prompt);
            using (var checkBoxA = new CheckBox { Tag = "Alpha" })
            {
                var wiring = new CheckBoxController();
                wiring.Init(controller, string.Empty);

                wiring.CtrlCB = checkBoxA; // subscribe

                controller.GetSelections().Should().BeEmpty();
                RaiseClick(checkBoxA);

                controller.GetSelections().Should().Contain("Alpha");
                DisposeOptions(fake);
            }
        }

        [STATestMethod]
        public void CtrlCBSetter_WhenReassigned_UnsubscribesPreviousCheckbox()
        {
            var fake = new FakeTagViewer();
            var controller = BuildController(fake, out var prompt);
            using (var checkBoxA = new CheckBox { Tag = "Alpha" })
            using (var checkBoxB = new CheckBox { Tag = "Beta" })
            {
                var wiring = new CheckBoxController();
                wiring.Init(controller, string.Empty);

                wiring.CtrlCB = checkBoxA; // subscribe A
                wiring.CtrlCB = checkBoxB; // reassign -> unsubscribe A, subscribe B

                RaiseClick(checkBoxA); // A no longer wired -> no state change

                controller.GetSelections().Should().BeEmpty();
                DisposeOptions(fake);
            }
        }

        [STATestMethod]
        public void CheckBoxConstructor_WiresSuppliedCheckbox()
        {
            using (var checkBox = new CheckBox())
            {
                var wiring = new CheckBoxController(checkBox);

                wiring.CtrlCB.Should().BeSameAs(checkBox);
            }
        }

        private static TagController BuildController(
            FakeTagViewer fake,
            out Mock<IUserPrompt> prompt
        )
        {
            prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            var options = new SortedDictionary<string, bool> { ["Alpha"] = false };
            return new TagController(fake.Object, options, null, null, prompt.Object, _ => { });
        }

        private static void RaiseClick(Control control)
        {
            var invokeOnClick = typeof(Control).GetMethod(
                "InvokeOnClick",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            invokeOnClick.Should().NotBeNull();
            invokeOnClick.Invoke(control, new object[] { control, EventArgs.Empty });
        }

        private static void DisposeOptions(FakeTagViewer viewer)
        {
            foreach (var control in viewer.OptionControls.ToList())
            {
                control.Dispose();
            }
        }
    }
}
