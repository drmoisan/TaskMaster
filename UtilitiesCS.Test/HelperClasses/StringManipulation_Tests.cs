using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class StringManipulation_Tests
    {
        [TestMethod]
        public void GetStrippedText_WithAsciiText_ReturnsUnchanged()
        {
            // Arrange
            var input = "Hello, World! 123";

            // Act
            var result = StringManipulation.GetStrippedText(input);

            // Assert
            result.Should().Be("Hello, World! 123");
        }

        [TestMethod]
        public void GetStrippedText_WithNonAsciiCharacters_RemovesThem()
        {
            // Arrange
            var input = "Hello\u00A9World\u00AE";

            // Act
            var result = StringManipulation.GetStrippedText(input);

            // Assert
            result.Should().Be("HelloWorld");
        }

        [TestMethod]
        public void GetStrippedText_WithEmptyString_ReturnsEmpty()
        {
            // Act
            var result = StringManipulation.GetStrippedText("");

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetStrippedText_WithTabsAndNewlines_RemovesThem()
        {
            // Arrange
            var input = "Line1\tTab\nLine2\rLine3";

            // Act
            var result = StringManipulation.GetStrippedText(input);

            // Assert
            result.Should().Be("Line1TabLine2Line3");
        }

        [TestMethod]
        public void GetStrippedText_ClosingBraceAtBoundary_KeepsIt()
        {
            // Arrange - closing brace \u007D is the upper bound of the kept range
            var input = "test}value";

            // Act
            var result = StringManipulation.GetStrippedText(input);

            // Assert
            result.Should().Be("test}value");
        }

        [TestMethod]
        public void GetStrippedText_WithOnlyNonAscii_ReturnsEmpty()
        {
            // Arrange
            var input = "\u00A0\u00FF\u0100";

            // Act
            var result = StringManipulation.GetStrippedText(input);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
