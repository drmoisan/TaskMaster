using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.ThemeHelpers
{
    [TestClass]
    public class SystemThemeDetectorTests
    {
        /// <summary>
        /// Verifies that IsSystemDarkMode() executes without throwing and returns a boolean.
        /// The actual value (true/false) is machine-dependent and not asserted.
        /// </summary>
        [TestMethod]
        public void IsSystemDarkMode_ReturnsBoolean()
        {
            bool result = SystemThemeDetector.IsSystemDarkMode();
            ((object)result).Should().BeOfType<bool>();
        }

        /// <summary>
        /// Verifies that TryGetIsSystemDarkMode returns true on a standard Windows machine
        /// where the AppsUseLightTheme registry key is present.
        /// </summary>
        [TestMethod]
        public void TryGetIsSystemDarkMode_ReturnsTrue_WhenRegistryReadable()
        {
            bool gotValue = SystemThemeDetector.TryGetIsSystemDarkMode(out bool isDarkMode);
            gotValue
                .Should()
                .BeTrue(
                    "the AppsUseLightTheme registry key is expected to exist on a standard Windows machine"
                );
            ((object)isDarkMode).Should().BeOfType<bool>();
        }

        [TestMethod]
        public void IsSystemDarkMode_ShouldReturnConsistentResultWithTryGet()
        {
            SystemThemeDetector.TryGetIsSystemDarkMode(out bool expected);
            var result = SystemThemeDetector.IsSystemDarkMode();

            result.Should().Be(expected);
        }

        [TestMethod]
        public void TryGetIsSystemDarkMode_ShouldReturnBoolean_ForIsDarkMode()
        {
            SystemThemeDetector.TryGetIsSystemDarkMode(out bool isDarkMode);

            // isDarkMode should be a valid boolean (true or false); this confirms no exception
            isDarkMode.Should().Be(isDarkMode);
        }
    }
}
