using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class VerboseLogger_Tests
    {
        [TestMethod]
        public void Constructor_ShouldInitializeKnownMethodsAsNonVerbose()
        {
            // Arrange
            var logger = new VerboseLogger<SampleTarget>();

            // Assert
            logger.VerboseMethods.Should().ContainKey(nameof(SampleTarget.Alpha));
            logger.VerboseMethods.Should().ContainKey(nameof(SampleTarget.Beta));
            logger.VerboseMethods[nameof(SampleTarget.Alpha)].Should().BeFalse();
            logger.VerboseMethods[nameof(SampleTarget.Beta)].Should().BeFalse();
        }

        [TestMethod]
        public void SetVerbose_ShouldEnableSingleMethodAndEnumerableMethods()
        {
            // Arrange
            var logger = new VerboseLogger<SampleTarget>();

            // Act
            logger.SetVerbose(nameof(SampleTarget.Alpha));
            logger.SetVerbose(new[] { nameof(SampleTarget.Beta) });

            // Assert
            logger.IsVerbose(nameof(SampleTarget.Alpha)).Should().BeTrue();
            logger.IsVerbose(nameof(SampleTarget.Beta)).Should().BeTrue();
        }

        [TestMethod]
        public void IsVerbose_ShouldReturnFalseForUnknownMember()
        {
            // Arrange
            var logger = new VerboseLogger<SampleTarget>();

            // Act
            var result = logger.IsVerbose("DoesNotExist");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void VerboseAction_ShouldOnlyExecuteWhenMemberIsVerbose()
        {
            // Arrange
            var logger = new VerboseLogger<SampleTarget>();
            int executionCount = 0;

            // Act
            logger.VerboseAction(() => executionCount++, nameof(SampleTarget.Alpha));
            logger.SetVerbose(nameof(SampleTarget.Alpha));
            logger.VerboseAction(() => executionCount++, nameof(SampleTarget.Alpha));

            // Assert
            executionCount.Should().Be(1);
        }

        [TestMethod]
        public void LogAndLogObject_ShouldAllowNullEmptyAndStructuredInputsWithoutThrowing()
        {
            // Arrange
            var logger = new VerboseLogger<SampleTarget>();
            logger.SetVerbose(nameof(SampleTarget.Alpha));
            var values = new Dictionary<string, long> { ["count"] = 5 };

            // Act
            Action act = () =>
            {
                logger.Log(null, nameof(SampleTarget.Alpha));
                logger.Log(string.Empty, nameof(SampleTarget.Alpha));
                logger.LogObject(values, "stats", nameof(SampleTarget.Alpha));
            };

            // Assert
            act.Should().NotThrow();
        }

        private sealed class SampleTarget
        {
            public void Alpha() { }

            public void Beta() { }
        }
    }
}
