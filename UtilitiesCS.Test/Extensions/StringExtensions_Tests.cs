using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class StringExtensions_Tests
    {
        [TestMethod]
        public void IsNullOrEmpty_ReturnsExpectedValuesForNullEmptyWhitespaceAndText()
        {
            // Act / Assert
            ((string)null).IsNullOrEmpty().Should().BeTrue();
            string.Empty.IsNullOrEmpty().Should().BeTrue();
            " ".IsNullOrEmpty().Should().BeFalse();
            "x".IsNullOrEmpty().Should().BeFalse();
        }

        [TestMethod]
        public void Split_WithCharSeparator_CanTrimOrPreserveWhitespace()
        {
            // Arrange
            const string value = "a, b ,c";

            // Act
            var trimmed = value.Split(',', trim: true);
            var preserved = value.Split(',', trim: false);

            // Assert
            trimmed.Should().Equal("a", "b", "c");
            preserved.Should().Equal("a", " b ", "c");
        }

        [TestMethod]
        public void Split_WithStringDelimiter_SupportsSingleCharacterUnicodeAndTrim()
        {
            // Arrange
            const string unicodeValue = "α-- β --γ";
            const string singleCharacterValue = "x|y|z";

            // Act
            var trimmedUnicode = unicodeValue.Split("--", trim: true);
            var singleCharacter = singleCharacterValue.Split("|");

            // Assert
            trimmedUnicode.Should().Equal("α", "β", "γ");
            singleCharacter.Should().Equal("x", "y", "z");
        }

        [TestMethod]
        public void Split_WhenSourceIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            string value = null;

            // Act
            Action charSplit = () => value.Split(',', trim: true);
            Action stringSplit = () => value.Split("--");

            // Assert
            charSplit.Should().Throw<NullReferenceException>();
            stringSplit.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void SearchDelimitedString_FiltersAndTransformsDelimitedContent()
        {
            // Arrange
            const string source = "hello|yellow|world|héllo";

            // Act
            var standard = source.SearchDelimitedString("*ell*", "|");
            var deleteFromMatches = source.SearchDelimitedString("h*o", "|", ArrayExtensions.SearchOptions.DeleteFromMatches);
            var exactComplement = source.SearchDelimitedString("world", "|", ArrayExtensions.SearchOptions.ExactComplement);

            // Assert
            standard.Should().Be("hello|yellow");
            deleteFromMatches.Should().Be("ell|éll");
            exactComplement.Should().Be("hello|yellow|héllo");
        }

        [TestMethod]
        public void FirstDiffIndex_ReturnsExpectedIndexesForEqualDifferentAndBoundaryLengths()
        {
            // Act / Assert
            "match".FirstDiffIndex("match").Should().Be(-1);
            "match".FirstDiffIndex("mismatch").Should().Be(1);
            "abc".FirstDiffIndex("ab").Should().Be(2);
            "héllo".FirstDiffIndex("héLlo").Should().Be(2);
        }

        [TestMethod]
        public void PadToCenter_ReturnsOriginalForBoundaryWidthsAndPadsSymmetricallyOtherwise()
        {
            // Act / Assert
            "wide".PadToCenter(2).Should().Be("wide");
            "x".PadToCenter(1).Should().Be("x");
            "x".PadToCenter(5, '.').Should().Be("..x..");
            "π".PadToCenter(4, '_').Should().Be("_π__");
        }
    }
}