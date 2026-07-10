using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tags.Test
{
    /// <summary>
    /// Pure unit tests for the extracted <see cref="CheckBoxController.DecideClick"/> state machine.
    /// These tests construct no WinForms control; they exercise the decision logic with plain inputs.
    /// </summary>
    [TestClass]
    public class CheckBoxControllerDecisionTests
    {
        [TestMethod]
        public void DecideClick_WhenNotKeyTriggeredAndTagPresent_TogglesUsingTag()
        {
            var decision = CheckBoxController.DecideClick(
                trigByKeyChg: false,
                trigByValChg: false,
                tag: "MyTag",
                text: "Label",
                prefix: "Pre "
            );

            decision.Action.Should().Be(CheckBoxController.CheckBoxClickAction.Toggle);
            decision.ResolvedChoice.Should().Be("MyTag");
        }

        [TestMethod]
        public void DecideClick_WhenTagNullOrEmpty_TogglesUsingPrefixPlusText()
        {
            CheckBoxController
                .DecideClick(false, false, null, "Label", "Pre ")
                .ResolvedChoice.Should()
                .Be("Pre Label");

            CheckBoxController
                .DecideClick(false, false, "", "Label", "Pre ")
                .ResolvedChoice.Should()
                .Be("Pre Label");
        }

        [TestMethod]
        public void DecideClick_WhenKeyTriggeredAndValueChanged_ResetsBothFlags()
        {
            var decision = CheckBoxController.DecideClick(
                trigByKeyChg: true,
                trigByValChg: true,
                tag: "ignored",
                text: "ignored",
                prefix: ""
            );

            decision.Action.Should().Be(CheckBoxController.CheckBoxClickAction.ResetFlags);
            decision.NextTrigByKeyChg.Should().BeFalse();
            decision.NextTrigByValChg.Should().BeFalse();
        }

        [TestMethod]
        public void DecideClick_WhenKeyTriggeredAndValueNotChanged_FlipsCheckAndSetsValueFlag()
        {
            var decision = CheckBoxController.DecideClick(
                trigByKeyChg: true,
                trigByValChg: false,
                tag: "ignored",
                text: "ignored",
                prefix: ""
            );

            decision.Action.Should().Be(CheckBoxController.CheckBoxClickAction.FlipCheck);
            decision.NextTrigByKeyChg.Should().BeTrue();
            decision.NextTrigByValChg.Should().BeTrue();
        }
    }
}
