using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            result
                .Should()
                .Contain(nameof(GetMethodCallLogString_ShouldIncludeCallerMethodAndParameters));
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
            var method = typeof(TraceUtility_Tests).GetMethod(
                nameof(CaptureMethodCallLogString),
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            var assembly = method.GetAssembly();

            // Assert
            assembly.Should().NotBeNull();
            assembly.FullName.Should().Be(typeof(TraceUtility_Tests).Assembly.FullName);
        }

        [TestMethod]
        public void TryGetMyTraceString_WithValidTrace_ReturnsNonEmptyString()
        {
            // Act
            var result = new StackTrace().TryGetMyTraceString("fallback");

            // Assert
            result.Should().NotBe("fallback");
            result.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void GetCallerParameters_ShouldReturnParameterInfoArray()
        {
            // Arrange
            var trace = new StackTrace();

            // Act
            var parameters = trace.GetCallerParameters();

            // Assert
            parameters.Should().NotBeNull();
        }

        [TestMethod]
        public void GetMethodTraceString_ExtensionOnStackTrace_ReturnsChain()
        {
            // Act
            var result = CaptureMethodTraceStringViaExtension("data");

            // Assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Contain("GetMethodTraceString_ExtensionOnStackTrace_ReturnsChain");
        }

        [TestMethod]
        public void GetMethodCallLogString_WithoutParams_StillReturnsTrace()
        {
            // Act
            var result = CaptureCallWithNoParams();

            // Assert
            result.Should().StartWith("TRACE");
        }

        [TestMethod]
        public void LogMethodCallOld_MatchingAndMismatchedParameters_DoNotThrow()
        {
            // Act
            Action act = () =>
            {
                CaptureLogMethodCallOldMatched(3, "Ada");
                CaptureLogMethodCallOldMismatched(5, "Grace");
            };

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void LogMethodCallAndLogMethodTrace_DoNotThrow()
        {
            // Act
            Action act = () =>
            {
                CaptureLogMethodCall(9, "Alan");
                CaptureLogMethodTrace("payload");
            };

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void GetAssembly_ShouldReturnModuleAssemblyForStaticMethod()
        {
            // Arrange
            var method = typeof(TraceUtility_Tests).GetMethod(
                nameof(CaptureStaticTrace),
                BindingFlags.Static | BindingFlags.NonPublic
            );

            // Act
            var assembly = method.GetAssembly();

            // Assert
            assembly.Should().BeSameAs(method.Module.Assembly);
        }

        [TestMethod]
        public void GetCallerMethod_WithOutOfRangeFrameLevel_ReturnsNull()
        {
            // Arrange
            var trace = new StackTrace();
            var frameLevel = trace.FrameCount + 5;

            // Act
            var method = trace.GetCallerMethod(ref frameLevel);

            // Assert
            method.Should().BeNull();
            frameLevel.Should().Be(trace.FrameCount + 6);
        }

        [TestMethod]
        public void GetMyFrames_WithCurrentTrace_ReturnsOnlyProjectMethods()
        {
            // Act
            var frames = new StackTrace().GetMyFrames();

            // Assert
            frames.Should().NotBeEmpty();
            frames
                .Select(frame => frame.Method.GetAssembly().GetName().Name)
                .Should()
                .OnlyContain(name => !string.IsNullOrWhiteSpace(name));
        }

        [TestMethod]
        public void InternalIsMine_ShouldReturnFalseForFrameworkAssembly()
        {
            // Arrange
            var method = typeof(TraceUtility).GetMethod(
                "IsMine",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            // Act
            var result = (bool)method.Invoke(null, new object[] { typeof(string).Assembly });

            // Assert
            result.Should().BeFalse();
        }

        private string CaptureCallWithNoParams()
        {
            return TraceUtility.GetMethodCallLogString();
        }

        private static void CaptureStaticTrace() { }

        private void CaptureLogMethodCallOldMatched(int count, string name)
        {
            TraceUtility.LogMethodCallOld(count, name);
        }

        private void CaptureLogMethodCallOldMismatched(int count, string name)
        {
            TraceUtility.LogMethodCallOld();
        }

        private void CaptureLogMethodCall(int count, string name)
        {
            TraceUtility.LogMethodCall(count, name);
        }

        private void CaptureLogMethodTrace(string value)
        {
            TraceUtility.LogMethodTrace(value);
        }

        private string CaptureMethodTraceStringViaExtension(string value)
        {
            return new StackTrace(1).GetMethodTraceString(value);
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
