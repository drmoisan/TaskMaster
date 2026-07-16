using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Tests for <see cref="UtilitiesCS.PercentageFormatter.FormatPercent"/>: whole-number percent
    /// rendering with no decimals, midpoint-away-from-zero rounding at the <c>.5</c> boundary, the
    /// <c>0</c> and <c>1</c> endpoints, and a blank string for a null probability.
    /// </summary>
    [TestClass]
    public class PercentageFormatterTests
    {
        [TestMethod]
        public void FormatPercent_Zero_ReturnsZeroPercent()
        {
            PercentageFormatter.FormatPercent(0.0).Should().Be("0%");
        }

        [TestMethod]
        public void FormatPercent_One_ReturnsHundredPercent()
        {
            PercentageFormatter.FormatPercent(1.0).Should().Be("100%");
        }

        [TestMethod]
        public void FormatPercent_TypicalValue_RoundsToWholePercent()
        {
            PercentageFormatter.FormatPercent(0.732).Should().Be("73%");
        }

        [TestMethod]
        public void FormatPercent_RoundsDownBelowMidpoint()
        {
            PercentageFormatter.FormatPercent(0.734).Should().Be("73%");
        }

        [TestMethod]
        public void FormatPercent_AtMidpoint_RoundsAwayFromZero()
        {
            // 0.735 * 100 = 73.5 -> away-from-zero -> 74
            PercentageFormatter.FormatPercent(0.735).Should().Be("74%");
        }

        [TestMethod]
        public void FormatPercent_SmallMidpoint_RoundsAwayFromZero()
        {
            // 0.005 * 100 = 0.5 -> away-from-zero -> 1
            PercentageFormatter.FormatPercent(0.005).Should().Be("1%");
        }

        [TestMethod]
        public void FormatPercent_Null_ReturnsEmptyString()
        {
            PercentageFormatter.FormatPercent(null).Should().BeEmpty();
        }
    }
}
