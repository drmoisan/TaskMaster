using System;
using System.Diagnostics;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.NewtonsoftHelpers;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class NConsoleTraceWriter_Tests
    {
        [TestMethod]
        public void LevelFilter_DefaultsToVerbose()
        {
            // Act
            var writer = new NConsoleTraceWriter();

            // Assert
            writer.LevelFilter.Should().Be(TraceLevel.Verbose);
        }

        [TestMethod]
        public void LevelFilter_CanBeSet()
        {
            // Arrange
            var writer = new NConsoleTraceWriter();

            // Act
            writer.LevelFilter = TraceLevel.Warning;

            // Assert
            writer.LevelFilter.Should().Be(TraceLevel.Warning);
        }

        [TestMethod]
        public void MessageFilter_DefaultsToDeserializedAndSerializedJson()
        {
            // Act
            var writer = new NConsoleTraceWriter();

            // Assert
            writer.MessageFilter.Should().Contain("Deserialized JSON:");
            writer.MessageFilter.Should().Contain("Serialized JSON:");
        }

        [TestMethod]
        public void Trace_WhenMessageDoesNotContainFilterTerms_InvokesLog()
        {
            // Arrange
            var writer = new NConsoleTraceWriter();
            string loggedMessage = null;
            Exception loggedException = null;
            writer.Log = (msg, ex) =>
            {
                loggedMessage = msg;
                loggedException = ex;
            };

            // Act
            writer.Trace(TraceLevel.Info, "Normal message", null);

            // Assert
            loggedMessage.Should().Be("Normal message");
            loggedException.Should().BeNull();
        }

        [TestMethod]
        public void Trace_WhenMessageContainsFilterTerm_DoesNotInvokeLog()
        {
            // Arrange
            var writer = new NConsoleTraceWriter();
            bool logInvoked = false;
            writer.Log = (msg, ex) => logInvoked = true;

            // Act
            writer.Trace(TraceLevel.Info, "Deserialized JSON: result", null);

            // Assert
            logInvoked.Should().BeFalse();
        }

        [TestMethod]
        public void Trace_WhenMessageContainsSerializedJsonFilter_DoesNotInvokeLog()
        {
            // Arrange
            var writer = new NConsoleTraceWriter();
            bool logInvoked = false;
            writer.Log = (msg, ex) => logInvoked = true;

            // Act
            writer.Trace(TraceLevel.Info, "Serialized JSON: output", null);

            // Assert
            logInvoked.Should().BeFalse();
        }

        [TestMethod]
        public void Trace_WhenLogIsNull_DoesNotThrow()
        {
            // Arrange
            var writer = new NConsoleTraceWriter();
            writer.Log = null;

            // Act
            Action act = () => writer.Trace(TraceLevel.Info, "Message", null);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Trace_WithExceptionAndNonFilteredMessage_PassesExceptionToLog()
        {
            // Arrange
            var writer = new NConsoleTraceWriter();
            Exception capturedEx = null;
            writer.Log = (msg, ex) => capturedEx = ex;
            var expected = new InvalidOperationException("test");

            // Act
            writer.Trace(TraceLevel.Error, "Error occurred", expected);

            // Assert
            capturedEx.Should().BeSameAs(expected);
        }

        [TestMethod]
        public void MessageFilter_CanBeReplaced()
        {
            // Arrange
            var writer = new NConsoleTraceWriter();
            bool logInvoked = false;
            writer.Log = (msg, ex) => logInvoked = true;

            // Act
            writer.MessageFilter = new System.Collections.Generic.List<string> { "BLOCK" };
            writer.Trace(TraceLevel.Info, "Deserialized JSON: now allowed", null);

            // Assert
            logInvoked.Should().BeTrue();
        }
    }
}
