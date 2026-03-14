using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Reflection;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class TraceUtility_Tests
    {
        [TestMethod]
        public void GetMethodCallLogString_ShouldIncludeCallerMethodAndParameters()
        {
            // Act
            var result = CaptureMethodCallLogString(7, "Ada");

            // Assert
            result.Should().StartWith("TRACE\t");
            result.Should().Contain(nameof(GetMethodCallLogString_ShouldIncludeCallerMethodAndParameters));
            result.Should().Contain(nameof(CaptureMethodCallLogString));
            result.Should().Contain("count=7");
            result.Should().Contain("name=Ada");
        }

        [TestMethod]
        public void GetMethodTraceString_ShouldIncludeCurrentCallChain()
        {
            // Act
            var result = CaptureMethodTraceString("value");

            // Assert
            result.Should().Contain(nameof(GetMethodTraceString_ShouldIncludeCurrentCallChain));
            result.Should().Contain(nameof(CaptureMethodTraceString));
            result.Should().Contain(nameof(FinishMethodTraceStringCapture));
            result.Should().Contain("/* Please update to pass in all parameters */");
        }

        [TestMethod]
        public void TryGetMyTraceString_WhenTraceIsNull_ReturnsFallback()
        {
            // Arrange
            StackTrace trace = null;

            // Act
            var result = trace.TryGetMyTraceString("fallback");

            // Assert
            result.Should().Be("fallback");
        }

        [TestMethod]
        public void GetMyTraceString_WithCurrentTrace_ReturnsProjectTrace()
        {
            // Act
            var result = new StackTrace().GetMyTraceString();

            // Assert
            result.Should().Contain(nameof(GetMyTraceString_WithCurrentTrace_ReturnsProjectTrace));
        }

        [TestMethod]
        public void GetAssembly_ShouldReturnDeclaringAssemblyForInstanceMethod()
        {
            // Arrange
            var method = typeof(TraceUtility_Tests).GetMethod(nameof(CaptureMethodCallLogString), BindingFlags.Instance | BindingFlags.NonPublic);

            // Act
            var assembly = method.GetAssembly();

            // Assert
            assembly.Should().NotBeNull();
            assembly.FullName.Should().Be(typeof(TraceUtility_Tests).Assembly.FullName);
        }

        private string CaptureMethodCallLogString(int count, string name)
        {
            return TraceUtility.GetMethodCallLogString(count, name);
        }

        private string CaptureMethodTraceString(string value)
        {
            return FinishMethodTraceStringCapture(value);
        }

        private string FinishMethodTraceStringCapture(string value)
        {
            return TraceUtility.GetMethodTraceString(value);
        }
    }
}