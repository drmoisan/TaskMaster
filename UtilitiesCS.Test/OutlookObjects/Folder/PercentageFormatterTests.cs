using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Tests for <see cref="UtilitiesCS.PercentageFormatter"/>, the pure host-neutral seam that
    /// renders a <c>[0,1]</c> probability (sourced from <see cref="UtilitiesCS.FolderScore.Probability"/>)
    /// as a whole-number percentage string. Covers the representative examples from the spec, midpoint
    /// rounding away-from-zero, and clamping of out-of-range input.
    /// </summary>
    [TestClass]
    public class PercentageFormatterTests
    {
        [TestMethod]
        public void Format_TypicalFraction_RoundsToNearestWholePercent()
        {
            // Arrange / Act
            var result = UtilitiesCS.PercentageFormatter.Format(0.4267);

            // Assert: 0.4267 * 100 = 42.67 -> 43
            result.Should().Be("43%");
        }

        [TestMethod]
        public void Format_One_RendersHundredPercent()
        {
            UtilitiesCS.PercentageFormatter.Format(1.0).Should().Be("100%");
        }

        [TestMethod]
        public void Format_Zero_RendersZeroPercent()
        {
            UtilitiesCS.PercentageFormatter.Format(0.0).Should().Be("0%");
        }

        [TestMethod]
        public void Format_Midpoint_RoundsAwayFromZero()
        {
            // Arrange / Act: 0.125 * 100 = 12.5 exactly; away-from-zero rounds up to 13
            // (banker's/ToEven rounding would yield 12, so this proves MidpointRounding.AwayFromZero).
            var result = UtilitiesCS.PercentageFormatter.Format(0.125);

            // Assert
            result.Should().Be("13%");
        }

        [TestMethod]
        public void Format_InputAboveOne_ClampsToHundredPercent()
        {
            UtilitiesCS.PercentageFormatter.Format(1.5).Should().Be("100%");
        }

        [TestMethod]
        public void Format_NegativeInput_ClampsToZeroPercent()
        {
            UtilitiesCS.PercentageFormatter.Format(-0.3).Should().Be("0%");
        }
    }
}
