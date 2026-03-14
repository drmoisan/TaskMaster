using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Reflection;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class TraceExtensions_Tests
    {
        [TestMethod]
        public void GetCallerByName_ReturnsMatchingMethodAndHandlesEmptySpecialAndMissingNames()
        {
            // Act
            var found = ResolveCaller(nameof(GetCallerByName_ReturnsMatchingMethodAndHandlesEmptySpecialAndMissingNames));
            var missing = ResolveCaller("DoesNotExist");
            var empty = ResolveCaller(string.Empty);
            var moveNext = ResolveCaller("MoveNext");

            // Assert
            found.Should().NotBeNull();
            found.Name.Should().Be(nameof(GetCallerByName_ReturnsMatchingMethodAndHandlesEmptySpecialAndMissingNames));
            missing.Should().BeNull();
            empty.Should().BeNull();
            moveNext.Should().BeNull();
        }

        [TestMethod]
        public void GetCallerByName_WhenStackTraceIsNull_ReturnsNull()
        {
            // Arrange
            StackTrace trace = null;

            // Act
            var result = trace.GetCallerByName("anything");

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetParameterNameAndNames_ReturnExpectedValuesForValidMethods()
        {
            // Arrange
            var method = typeof(TraceExtensions_Tests).GetMethod(nameof(SampleMethod), BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var firstName = method.GetParameterName(0);
            var secondName = method.GetParameterName(1);
            var allNames = method.GetParameterNames();

            // Assert
            firstName.Should().Be("count");
            secondName.Should().Be("text");
            allNames.Should().Equal("count", "text");
        }

        [TestMethod]
        public void GetParameterName_ThrowsForNegativeOutOfRangeAndParameterlessMethods()
        {
            // Arrange
            var method = typeof(TraceExtensions_Tests).GetMethod(nameof(SampleMethod), BindingFlags.NonPublic | BindingFlags.Static);
            var parameterless = typeof(TraceExtensions_Tests).GetMethod(nameof(ParameterlessMethod), BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            Action negativeAction = () => method.GetParameterName(-1);
            Action outOfRangeAction = () => method.GetParameterName(2);
            Action parameterlessAction = () => parameterless.GetParameterName(0);

            // Assert
            negativeAction.Should().Throw<ArgumentOutOfRangeException>();
            outOfRangeAction.Should().Throw<ArgumentOutOfRangeException>();
            parameterlessAction.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TryGetParameterName_ReturnsEmptyStringForInvalidIndexOrNullMethod()
        {
            // Arrange
            var method = typeof(TraceExtensions_Tests).GetMethod(nameof(SampleMethod), BindingFlags.NonPublic | BindingFlags.Static);
            MethodBase nullMethod = null;

            // Act / Assert
            method.TryGetParameterName(99).Should().BeEmpty();
            nullMethod.TryGetParameterName(0).Should().BeEmpty();
        }

        private static MethodBase ResolveCaller(string methodName)
        {
            var trace = new StackTrace();
            return trace.GetCallerByName(methodName);
        }

        private static void SampleMethod(int count, string text)
        {
        }

        private static void ParameterlessMethod()
        {
        }
    }
}
