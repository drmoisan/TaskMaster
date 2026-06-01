using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster.Properties;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Unit tests for the high-confidence ribbon helpers on <see cref="RibbonController"/>
    /// (Issue #169). RibbonController reads/writes the high-confidence settings through its
    /// concrete <see cref="ApplicationGlobals"/> (<c>Globals</c>). To exercise the helpers without
    /// constructing the full Outlook-backed globals, an uninitialized <see cref="ApplicationGlobals"/>
    /// is created and its <c>_quickFilerSettings</c> field is set to a real
    /// <see cref="AppQuickFilerSettings"/>; that settings object round-trips through
    /// <see cref="Settings.Default"/>, which is snapshotted in <see cref="TestInitialize"/> and
    /// restored in <see cref="TestCleanup"/> so the tests are independent and leave no machine
    /// state mutated.
    /// </summary>
    [TestClass]
    public class RibbonControllerTests
    {
        private bool _originalModeEnabled;
        private double _originalThreshold;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalModeEnabled = Settings.Default.HighConfidenceModeEnabled;
            _originalThreshold = Settings.Default.HighConfidenceThreshold;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Settings.Default.HighConfidenceModeEnabled = _originalModeEnabled;
            Settings.Default.HighConfidenceThreshold = _originalThreshold;
        }

        /// <summary>
        /// Builds a RibbonController whose Globals is an uninitialized ApplicationGlobals carrying a
        /// real AppQuickFilerSettings, so the high-confidence helpers read/write Settings.Default.
        /// </summary>
        private static RibbonController CreateController()
        {
            var globals = (ApplicationGlobals)
                FormatterServices.GetUninitializedObject(typeof(ApplicationGlobals));
            typeof(ApplicationGlobals)
                .GetField("_quickFilerSettings", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(globals, new AppQuickFilerSettings());

            var controller = new RibbonController();
            typeof(RibbonController)
                .GetProperty(
                    "Globals",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
                )
                .SetValue(controller, globals);

            return controller;
        }

        [TestMethod]
        public void IsHighConfidenceModeActive_ReturnsStoredValue()
        {
            // Arrange
            Settings.Default.HighConfidenceModeEnabled = true;
            var controller = CreateController();

            // Act
            var result = controller.IsHighConfidenceModeActive();

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void ToggleHighConfidenceMode_FlipsStoredValue()
        {
            // Arrange
            Settings.Default.HighConfidenceModeEnabled = false;
            var controller = CreateController();

            // Act
            controller.ToggleHighConfidenceMode();

            // Assert
            controller.IsHighConfidenceModeActive().Should().BeTrue();
        }

        [TestMethod]
        public void SetHighConfidenceModeForLaunch_True_EnablesMode()
        {
            // Arrange: start from the disabled state.
            Settings.Default.HighConfidenceModeEnabled = false;
            var controller = CreateController();

            // Act
            controller.SetHighConfidenceModeForLaunch(true);

            // Assert: the high-confidence launch path enables the mode.
            controller.IsHighConfidenceModeActive().Should().BeTrue();
        }

        [TestMethod]
        public void StandardLaunchAfterHighConfidenceLaunch_DoesNotEnableMode()
        {
            // Arrange: simulate a prior high-confidence launch having enabled the mode.
            var controller = CreateController();
            controller.SetHighConfidenceModeForLaunch(true);

            // Act: a subsequent standard launch (or release) resets the launch-scoped flag.
            controller.SetHighConfidenceModeForLaunch(false);

            // Assert: the standard entry point does not inherit high-confidence mode, so it
            // never filters (AC6).
            controller.IsHighConfidenceModeActive().Should().BeFalse();
        }

        [TestMethod]
        public void GetHighConfidenceThresholdText_ReturnsPercentageForm()
        {
            // Arrange: stored probability 0.9 should render as "90".
            Settings.Default.HighConfidenceThreshold = 0.9;
            var controller = CreateController();

            // Act
            var result = controller.GetHighConfidenceThresholdText();

            // Assert
            result.Should().Be("90");
        }

        [TestMethod]
        public void SetHighConfidenceThresholdText_WithValidPercentage_PersistsProbability()
        {
            // Arrange
            var controller = CreateController();

            // Act
            controller.SetHighConfidenceThresholdText("75");

            // Assert
            Settings.Default.HighConfidenceThreshold.Should().Be(0.75);
        }

        [TestMethod]
        public void SetHighConfidenceThresholdText_WithNonNumericInput_LeavesValueUnchanged()
        {
            // Arrange
            Settings.Default.HighConfidenceThreshold = 0.9;
            var controller = CreateController();

            // Act
            controller.SetHighConfidenceThresholdText("not-a-number");

            // Assert
            Settings.Default.HighConfidenceThreshold.Should().Be(0.9);
        }

        [TestMethod]
        public void SetHighConfidenceThresholdText_WithOutOfRangeInput_LeavesValueUnchanged()
        {
            // Arrange: 150% is out of the [0, 100] range.
            Settings.Default.HighConfidenceThreshold = 0.9;
            var controller = CreateController();

            // Act
            controller.SetHighConfidenceThresholdText("150");

            // Assert
            Settings.Default.HighConfidenceThreshold.Should().Be(0.9);
        }
    }
}
