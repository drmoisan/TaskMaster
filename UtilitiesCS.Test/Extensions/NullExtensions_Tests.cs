using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class NullExtensions_Tests
    {
        [TestMethod]
        public void ThrowIfNull_ReturnsOriginalValueForReferenceAndValueTypes()
        {
            // Arrange
            const string text = "value";
            int number = 42;
            int? nullableNumber = 7;

            // Act / Assert
            text.ThrowIfNull().Should().BeSameAs(text);
            number.ThrowIfNull().Should().Be(42);
            nullableNumber.ThrowIfNull().Should().Be(7);
        }

        [TestMethod]
        public void ThrowIfNull_WhenArgumentIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            string text = null;
            int? nullableNumber = null;

            // Act
            Action referenceAction = () => text.ThrowIfNull();
            Action nullableValueAction = () => nullableNumber.ThrowIfNull("number is required");

            // Assert
            referenceAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("text");

            nullableValueAction
                .Should()
                .Throw<ArgumentNullException>()
                .Where(exception =>
                    exception.ParamName == "nullableNumber"
                    && exception.Message.Contains("number is required")
                );
        }

        [TestMethod]
        public void IsNullOrEmpty_ReturnsExpectedValuesForNullEmptyAndPopulatedSequences()
        {
            // Arrange
            IEnumerable<int> nullValues = null;
            IEnumerable<int> emptyValues = Array.Empty<int>();
            IEnumerable<int> values = new[] { 1, 2, 3 };

            // Act / Assert
            nullValues.IsNullOrEmpty().Should().BeTrue();
            emptyValues.IsNullOrEmpty().Should().BeTrue();
            values.IsNullOrEmpty().Should().BeFalse();
        }

        [TestMethod]
        public void ThrowIfNullOrEmpty_ForEnumerable_ReturnsSequenceOrThrowsForNullAndEmpty()
        {
            // Arrange
            IEnumerable<string> nullValues = null;
            IEnumerable<string> emptyValues = Array.Empty<string>();
            IEnumerable<string> values = new[] { "alpha" };

            // Act
            Action nullAction = () => nullValues.ThrowIfNullOrEmpty();
            Action emptyAction = () => emptyValues.ThrowIfNullOrEmpty("sequence required");
            var result = values.ThrowIfNullOrEmpty();

            // Assert
            result.Should().Equal("alpha");
            nullAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("nullValues");
            emptyAction
                .Should()
                .Throw<ArgumentNullException>()
                .Where(exception =>
                    exception.ParamName == "emptyValues"
                    && exception.Message.Contains("sequence required")
                );
        }

        [TestMethod]
        public void ThrowIfNullOrEmpty_ForCollectionsAndStrings_UsesCallerParameterName()
        {
            // Arrange
            var values = new List<int> { 1 };

            // Act
            var collectionResult = PassThroughCollection(values);
            var stringResult = PassThroughString("text");
            Action nullCollectionAction = () => PassThroughCollection(null);
            Action emptyCollectionAction = () => PassThroughCollection(new List<int>());
            Action nullStringAction = () => PassThroughString(null);
            Action emptyStringAction = () => PassThroughString(string.Empty);

            // Assert
            collectionResult.Should().BeSameAs(values);
            stringResult.Should().Be("text");
            nullCollectionAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("values");
            emptyCollectionAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("values");
            nullStringAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("text");
            emptyStringAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("text");
        }

        private static List<int> PassThroughCollection(List<int> values)
        {
            return values.ThrowIfNullOrEmpty();
        }

        private static string PassThroughString(string text)
        {
            return text.ThrowIfNullOrEmpty();
        }
    }
}
