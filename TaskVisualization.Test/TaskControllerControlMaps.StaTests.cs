using System.Linq;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskVisualization;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// STA-bound coverage of the control-identity builders in <c>TaskController.ControlMaps</c> /
    /// <c>TaskController.ControlRelationships</c>. Each test constructs a real
    /// <see cref="StaControlHarness"/> (real, never-shown, in-memory controls parented in a real
    /// <see cref="TableLayoutPanel"/>) and disposes it. No <see cref="Form"/>-derived type is
    /// constructed, nothing is shown, and no message pump is used.
    /// </summary>
    [STATestClass]
    public class TaskControllerControlMapsStaTests
    {
        [STATestMethod]
        public void GetControlLookup_KeysOnRealAcceleratorLabelIdentities()
        {
            // Seam-infeasibility (condition a): the lookup keys ARE real control object identities;
            // a mocked primitive cannot represent the identity semantics under test.
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);

                var lookup = controller.GetControlLookup();

                lookup.Should().NotBeEmpty();
                lookup.Keys.Should().OnlyHaveUniqueItems();
                lookup.Should().ContainKey(harness.XlProject);
            }
        }

        [STATestMethod]
        public void GetOptionsLookup_MarksAlwaysOnControlsActive()
        {
            // OK/Cancel/Autotag are hardcoded active regardless of options.
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.None
                );

                var options = controller.GetOptionsLookup();

                options.Should().NotBeEmpty();
                options[harness.XlOk].Should().BeTrue();
            }
        }

        [STATestMethod]
        public void GetCaptionLookup_ForGroup_ReturnsGroupCaptions()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(
                    harness.Object,
                    Enums.FlagsToSet.All
                );

                // Group 4 (OK / Cancel / Autotag) is always present.
                var captions = controller.GetCaptionLookup(4);

                captions.Should().ContainKey(harness.XlOk);
                captions[harness.XlOk].Should().Be("OKButton");
            }
        }

        [STATestMethod]
        public void OptionsGroups_ExposesRealControlsPerFlag()
        {
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);

                var groups = controller.OptionsGroups;

                groups.Should().ContainKey(Enums.FlagsToSet.Context);
                groups[Enums.FlagsToSet.Context].Should().OnlyContain(c => c != null);
            }
        }

        [STATestMethod]
        public void NavTips_ConstructsTipsControllersAgainstRealParentedLabels()
        {
            // Seam-infeasibility (condition a): TipsController throws unless its label has a real
            // parented TableLayoutPanel/Panel; a mock cannot supply that parenting.
            using (var harness = new StaControlHarness())
            {
                var controller = TaskControllerFixtures.BuildControllerOver(harness.Object);

                var navTips = controller.NavTips.ToList();

                navTips.Should().HaveCount(15);
                navTips.Should().OnlyContain(t => t.LabelControl != null);
            }
        }
    }
}
