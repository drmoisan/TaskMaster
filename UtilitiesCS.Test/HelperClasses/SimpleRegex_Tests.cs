using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class SimpleRegex_Tests
    {
        [TestMethod]
        public void MakeSearchPattern_WithWildcardsAndSpecialCharacters_EscapesLiteralsAndPreservesWildcards()
        {
            // Arrange
            const string input = "file(1).txt*backup?.json";

            // Act
            var pattern = SimpleRegex.MakeSearchPattern(input);

            // Assert
            pattern.Should().Be(@"^file\(1\)\.txt(.*)backup\?\.json$");
        }

        [TestMethod]
        public void MakeSearchPattern_WithEmptyPattern_ReturnsAnchoredEmptyExpression()
        {
            // Act
            var pattern = SimpleRegex.MakeSearchPattern(string.Empty);

            // Assert
            pattern.Should().Be("^$");
        }

        [TestMethod]
        public void MakeSearchPattern_WithNullInput_ThrowsNullReferenceException()
        {
            // Arrange
            Action act = () => SimpleRegex.MakeSearchPattern(null);

            // Assert
            act.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void MakeReplacePattern_WithNoWildcards_ReturnsEmptyReplacement()
        {
            // Act
            var replacePattern = SimpleRegex.MakeReplacePattern("^literal$");

            // Assert
            replacePattern.Should().BeEmpty();
        }

        [TestMethod]
        public void MakeRegex_CreatesCaseInsensitiveRegexAndReturnsPattern()
        {
            // Act
            var (regex, pattern) = SimpleRegex.MakeRegex("alpha*omega");
            var matched = regex.IsMatch("ALPHA-middle-OMEGA");

            // Assert
            pattern.Should().Be("^alpha(.*)omega$");
            regex.Options.Should().HaveFlag(RegexOptions.IgnoreCase);
            matched.Should().BeTrue();
        }

        [TestMethod]
        public void GetRegexGroups_WhenInputMatches_ReturnsAllWildcardGroups()
        {
            // Arrange
            var regex = new Regex(SimpleRegex.MakeSearchPattern("start*middle*end"));

            // Act
            var groups = regex.GetRegexGroups("start-1-middle-2-end");

            // Assert
            groups.Should().Equal("-1-", "-2-");
        }

        [TestMethod]
        public void GetRegexGroups_WhenInputDoesNotMatch_ReturnsEmptyArray()
        {
            // Arrange
            var regex = new Regex(SimpleRegex.MakeSearchPattern("prefix*suffix"));

            // Act
            var groups = regex.GetRegexGroups("prefix-only");

            // Assert
            groups.Should().BeEmpty();
        }

        [TestMethod]
        public void GetRegexGroups_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            var regex = new Regex("^value$");
            Action act = () => regex.GetRegexGroups(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetRegexGroups_CanBeUsedAcrossMultipleMatchesIndependently()
        {
            // Arrange
            var regex = new Regex(SimpleRegex.MakeSearchPattern("a*b*c"));

            // Act
            var first = regex.GetRegexGroups("a12b34c");
            var second = regex.GetRegexGroups("aXYbZZc");

            // Assert
            first.Should().Equal("12", "34");
            second.Should().Equal("XY", "ZZ");
            first.SequenceEqual(second).Should().BeFalse();
        }
    }
}
