using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class Tokenizer_Tests
    {
        [TestMethod]
        public void Tokenize_DefaultOverload_WithEmptyInput_ReturnsNoTokens()
        {
            // Act
            var tokens = string.Empty.Tokenize();

            // Assert
            tokens.Should().BeEmpty();
        }

        [TestMethod]
        public void Tokenize_DefaultOverload_WithSingleAndShortWords_OnlyReturnsEligibleSingleToken()
        {
            // Act
            var tokens = "a Alpha i".Tokenize();

            // Assert
            tokens.Should().Equal("alpha");
        }

        [TestMethod]
        public void Tokenize_DefaultOverload_WithWhitespaceAndMultipleTokens_NormalizesToLowercase()
        {
            // Arrange
            const string document = "  First\tSECOND\r\nthird   fourth  ";

            // Act
            var tokens = document.Tokenize();

            // Assert
            tokens.Should().Equal("first", "second", "third", "fourth");
        }

        [TestMethod]
        public void Tokenize_MinimumLengthOverload_FiltersShorterTokens()
        {
            // Act
            var tokens = "one three seven to eleven".Tokenize(4);

            // Assert
            tokens.Should().Equal("three", "seven", "eleven");
        }

        [TestMethod]
        public void Tokenize_CustomCharacterOverload_KeepsQuotedAndDelimitedWordsTogether()
        {
            // Arrange
            char[] literalChars = { '\'', '-' };
            const string document = "We heard can't and 'high-speed' clearly";

            // Act
            var tokens = document.Tokenize(literalChars);

            // Assert
            tokens.Should().ContainInOrder("we", "heard", "can't", "and", "high-speed", "clearly");
        }

        [TestMethod]
        public void Tokenize_RegexOverload_UsesProvidedPatternForDelimiterControl()
        {
            // Arrange
            const string document = "alpha, beta; gamma";
            var regex = new Regex(@"[^,;\s]+");

            // Act
            var tokens = document.Tokenize(regex);

            // Assert
            tokens.Should().Equal("alpha", "beta", "gamma");
        }

        [TestMethod]
        public void AsTokenPattern_WithCustomCharactersAndMinimumLength_ReturnsExpandedPattern()
        {
            // Arrange
            char[] literalChars = { '&' };

            // Act
            var pattern = literalChars.AsTokenPattern(3);

            // Assert
            pattern.Should().Be(@"\b[\w&][\w&][\w&]+\b");
        }

        [TestMethod]
        public void GetRegex_DefaultAndCustomOverloads_ReturnExpectedPatterns()
        {
            // Act
            var defaultRegex = Tokenizer.GetRegex();
            var customRegex = Tokenizer.GetRegex(@"\b[\w-][\w-]+\b");

            // Assert
            defaultRegex.ToString().Should().Be(@"\b\w\w+\b");
            customRegex.ToString().Should().Be(@"\b[\w-][\w-]+\b");
        }

        [TestMethod]
        public void GetTokenPattern_WithMinZeroOrOne_UsesSingleRequiredWordPattern()
        {
            // Act
            var minZero = Tokenizer.GetTokenPattern(@"[\w']", 0);
            var minOne = Tokenizer.GetTokenPattern(@"[\w']", 1);

            // Assert
            minZero.Should().Be(@"\b[\w']+\b");
            minOne.Should().Be(@"\b[\w']+\b");
        }

        [TestMethod]
        public void AsRegexWord_WithNullEmptyAndCustomCharacters_ReturnsExpectedWordPatterns()
        {
            // Arrange
            char[] empty = Array.Empty<char>();
            char[] custom = { '&', '!' };
            char[] missing = null;

            // Act / Assert
            missing.AsRegexWord().Should().Be(@"\w");
            empty.AsRegexWord().Should().Be(@"\w");
            custom.AsRegexWord().Should().Be(@"[\w&!]");
        }
    }
}
