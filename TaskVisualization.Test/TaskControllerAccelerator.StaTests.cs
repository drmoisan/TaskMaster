using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskVisualization;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// STA-bound coverage of the measured accelerator state machine in
    /// <c>TaskController.Accelerator</c> (nav-group activation, RecurseXl, ExecuteXlAction dispatch,
    /// Initialize). Tests drive the internal methods by direct invocation over a real
    /// <see cref="StaControlHarness"/> (never-shown in-memory controls). No message pump is used, no
    /// window is shown, no <see cref="Form"/>-derived type is constructed, and the exempt
    /// DateTimePicker/PostMessage focus residue is not exercised. Controls are disposed per test.
    /// The keyboard/mouse handlers are covered in
    /// <see cref="TaskControllerAcceleratorKeyboardStaTests"/>.
    /// </summary>
    [STATestClass]
    public class TaskControllerAcceleratorStaTests
    {
        [STATestMethod]
        public void RecurseXl_InitialActivation_TogglesAndCaptionsRealControls()
        {
            // The !altActive seeding branch of RecurseXl runs ToggleXl + UpdateCaptions over the
            // real, always-on accelerator labels (OK/Cancel/Autotag/sectors).
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.None
                );

                var (dictActive, altActive, level) = controller.RecurseXl(null, false, '\0', 0);

                altActive.Should().BeTrue();
                level.Should().Be(1);
                dictActive.Should().NotBeEmpty();
                // UpdateCaptions replaced XlOk's text with the single uppercase caption initial.
                harness.XlOk.Text.Should().Be("O");
            }
        }

        [STATestMethod]
        public void RecurseXl_SingleMatch_CheckBoxControl_TogglesChecked()
        {
            // A single-letter match drives ExecuteXlAction; the XlScBullpin accelerator maps to the
            // CbxBullpin CheckBox, whose Checked state flips.
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);
                // Match the production precondition: the nav tips are initialized before any
                // keyboard handling (InitializeAccelerators warms _navTips).
                var unused = controller.NavTips;
                var seed = new Dictionary<Label, char> { { harness.XlScBullpin, 'Z' } };

                controller.RecurseXl(seed, true, 'Z', 0);

                harness.CbxBullpin.Checked.Should().BeTrue();
            }
        }

        [STATestMethod]
        public void RecurseXl_SingleMatch_LabelControl_InvokesAssignSeam()
        {
            // XlProject maps to the LblProject Label; ExecuteXlAction routes it to AssignProject,
            // which uses the injected ITagPromptService seam.
            using (var harness = new StaControlHarness())
            {
                var prompt = TaskControllerFixtures.TagPrompt(
                    cancelled: false,
                    selection: "Zephyr"
                );
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    tagPrompt: prompt
                );
                var unused = controller.NavTips;
                var seed = new Dictionary<Label, char> { { harness.XlProject, 'Z' } };

                controller.RecurseXl(seed, true, 'Z', 0);

                controller.Active.Projects.AsStringNoPrefix.Should().Be("Zephyr");
            }
        }

        [STATestMethod]
        public void RecurseXl_SingleMatch_ButtonControl_DispatchesWithoutThrowing()
        {
            // XlOk maps to the OKButton; PerformClick on a never-shown button no-ops (CanSelect is
            // false) but the branch is line-covered by dispatch.
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);
                var unused = controller.NavTips;
                var seed = new Dictionary<Label, char> { { harness.XlOk, 'Z' } };

                System.Action act = () => controller.RecurseXl(seed, true, 'Z', 0);

                act.Should().NotThrow();
            }
        }

        [STATestMethod]
        public void ExecuteXlAction_ViaRecurseXl_LabelBranches_InvokeAssignSeams()
        {
            using (var harness = new StaControlHarness())
            {
                var prompt = TaskControllerFixtures.TagPrompt(cancelled: false, selection: "Val");
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    tagPrompt: prompt
                );
                var unused = controller.NavTips;
                var people = harness.ControlNamed("XlPeople") as Label;
                var topic = harness.ControlNamed("XlTopic") as Label;
                var context = harness.ControlNamed("XlContext") as Label;

                controller.RecurseXl(new Dictionary<Label, char> { { people, 'Z' } }, true, 'Z', 0);
                controller.RecurseXl(new Dictionary<Label, char> { { topic, 'Z' } }, true, 'Z', 0);
                controller.RecurseXl(
                    new Dictionary<Label, char> { { context, 'Z' } },
                    true,
                    'Z',
                    0
                );

                controller.Active.People.AsStringNoPrefix.Should().Be("Val");
                controller.Active.Topics.AsStringNoPrefix.Should().Be("Val");
                controller.Active.Context.AsStringNoPrefix.Should().Be("Val");
            }
        }

        [STATestMethod]
        public void ExecuteXlAction_ViaRecurseXl_UnmappedLabel_Throws()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);
                var unused = controller.NavTips;
                var sector = harness.ControlNamed("XlSector1") as Label;

                System.Action act = () =>
                    controller.RecurseXl(
                        new Dictionary<Label, char> { { sector, 'Z' } },
                        true,
                        'Z',
                        0
                    );

                act.Should().Throw<System.ArgumentException>();
            }
        }

        [STATestMethod]
        public void RecurseXl_NoMatch_ReturnsSeedUnchanged()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);
                var seed = new Dictionary<Label, char> { { harness.XlOk, 'A' } };

                var (dictActive, altActive, level) = controller.RecurseXl(seed, true, 'Z', 0);

                dictActive.Should().BeSameAs(seed);
                altActive.Should().BeTrue();
                level.Should().Be(0);
            }
        }

        [STATestMethod]
        public void RecurseXl_MultipleMatches_KeepsSearchingNextLevel()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);
                var people = harness.ControlNamed("XlPeople") as Label;
                var seed = new Dictionary<Label, char>
                {
                    { harness.XlProject, 'Z' },
                    { people, 'Z' },
                };

                var (dictActive, altActive, level) = controller.RecurseXl(seed, true, 'Z', 0);

                dictActive.Should().HaveCount(2);
                altActive.Should().BeTrue();
                level.Should().Be(1);
            }
        }

        [STATestMethod]
        public void RecurseXl_NullChar_DeactivatesAndClears()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);
                var unused = controller.NavTips;
                var seed = new Dictionary<Label, char> { { harness.XlOk, 'A' } };

                var (dictActive, altActive, level) = controller.RecurseXl(seed, true, '\0', 0);

                dictActive.Should().BeNull();
                altActive.Should().BeFalse();
                level.Should().Be(0);
            }
        }

        [STATestMethod]
        public void ActivateXlGroup_ActiveGroup_HighlightsTlpAndReturnsActivation()
        {
            // With all options on, group 3 (shortcuts) has active controls, so ActivateXlGroup
            // highlights the shared TLP and returns the activation tuple.
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.All
                );

                var (dictActive, altActive, level) = controller.ActivateXlGroup('3', 3);

                altActive.Should().BeTrue();
                level.Should().Be(1);
                dictActive.Should().NotBeEmpty();
                harness.TlpBackColor.Should().Be(Color.LightCyan);
            }
        }

        [STATestMethod]
        public void DeactivateActiveXlGroup_AfterActivation_ResetsTlpAndReturnsCleared()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.All
                );
                controller.ActivateXlGroup('3', 3);

                var (dictActive, altActive, level) = controller.DeactivateActiveXlGroup();

                dictActive.Should().BeNull();
                altActive.Should().BeTrue();
                level.Should().Be(0);
                harness.TlpBackColor.Should().Be(SystemColors.Control);
            }
        }

        [STATestMethod]
        public void ActivateXlGroup_EmptyGroup_ReturnsClearedWithoutHighlight()
        {
            // With no options set, group 1 has no active controls, so ActivateXlGroup returns the
            // cleared tuple without touching the TLP highlight.
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.None
                );

                var (dictActive, altActive, level) = controller.ActivateXlGroup(1);

                dictActive.Should().BeNull();
                altActive.Should().BeTrue();
                level.Should().Be(0);
            }
        }

        [STATestMethod]
        public void ActivateXlGroup_CharOverload_RoutesToNumberedGroup()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.All
                );

                var (dictActive, altActive, level) = controller.ActivateXlGroup('3');

                altActive.Should().BeTrue();
                dictActive.Should().NotBeEmpty();
                level.Should().Be(1);
            }
        }

        [STATestMethod]
        public void ActivateXlGroup_ZeroChar_ReturnsCleared()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);

                var (dictActive, altActive, level) = controller.ActivateXlGroup('0');

                dictActive.Should().BeNull();
                altActive.Should().BeTrue();
                level.Should().Be(0);
            }
        }

        [STATestMethod]
        public void ActivateXlGroup_ZeroInt_ReturnsCleared()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);

                var (dictActive, altActive, level) = controller.ActivateXlGroup(0);

                dictActive.Should().BeNull();
                altActive.Should().BeTrue();
                level.Should().Be(0);
            }
        }

        [STATestMethod]
        public void ToggleXlGroupNav_Off_TogglesSectorTipsWithoutThrowing()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);
                var unused = controller.NavTips;

                System.Action act = () => controller.ToggleXlGroupNav(Enums.ToggleState.Off);

                act.Should().NotThrow();
            }
        }

        [STATestMethod]
        public void Initialize_WithRealControls_RunsDataAndAcceleratorSetup()
        {
            // Initialize -> InitializeData + InitializeAccelerators runs end to end against real
            // parented controls (NavTips deactivation, ToggleXl off, option activation). The
            // Form-bound WireKeyPressHandlers is skipped because the viewer is not a TaskViewer.
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.Context
                );

                System.Action act = () => controller.Initialize();

                act.Should().NotThrow();
            }
        }
    }
}
