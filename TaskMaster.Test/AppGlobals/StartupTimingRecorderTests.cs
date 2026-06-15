using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Unit tests for <see cref="StartupTimingRecorder"/> and
    /// <see cref="NullStartupTimingRecorder"/>. All durations are injected deterministically so
    /// the assertions are stable; no clock, stopwatch, or external dependency is used.
    /// </summary>
    [TestClass]
    public class StartupTimingRecorderTests
    {
        [TestMethod]
        public void RecordPhase_WithPositiveDurations_PreservesPhaseNamesInRecordedOrder()
        {
            // Arrange
            var recorder = new StartupTimingRecorder();
            recorder.RecordPhase("IntelConfig", TimeSpan.FromMilliseconds(120));
            recorder.RecordPhase("OlObjects", TimeSpan.FromMilliseconds(450));
            recorder.RecordPhase("Events", TimeSpan.FromMilliseconds(75));

            // Act
            var table = recorder.FormatTable();

            // Assert: every phase name appears, and they appear in the order recorded.
            table.Should().Contain("IntelConfig");
            table.Should().Contain("OlObjects");
            table.Should().Contain("Events");
            table
                .IndexOf("IntelConfig", StringComparison.Ordinal)
                .Should()
                .BeLessThan(
                    table.IndexOf("OlObjects", StringComparison.Ordinal),
                    "phases must be rendered in the order they were recorded."
                );
            table
                .IndexOf("OlObjects", StringComparison.Ordinal)
                .Should()
                .BeLessThan(
                    table.IndexOf("Events", StringComparison.Ordinal),
                    "phases must be rendered in the order they were recorded."
                );
        }

        [TestMethod]
        public void RecordPhase_WithZeroDuration_IsCapturedAndRenderedWithoutError()
        {
            // Arrange
            var recorder = new StartupTimingRecorder();
            recorder.RecordPhase("ToDo", TimeSpan.Zero);

            // Act
            Action act = () => recorder.FormatTable();

            // Assert
            act.Should().NotThrow();
            recorder.FormatTable().Should().Contain("ToDo");
        }

        [TestMethod]
        public void FormatTable_ContainsHeadersPhaseNamesAndTotalEqualToSumOfInjectedSpans()
        {
            // Arrange: distinct non-zero spans injected deterministically.
            var intel = TimeSpan.FromMilliseconds(100);
            var ol = TimeSpan.FromMilliseconds(250);
            var events = TimeSpan.FromMilliseconds(50);
            var recorder = new StartupTimingRecorder();
            recorder.RecordPhase("IntelConfig", intel);
            recorder.RecordPhase("OlObjects", ol);
            recorder.RecordPhase("Events", events);

            // Act
            var table = recorder.FormatTable();

            // Assert: headers, phase names, and a TOTAL row are present.
            table.Should().Contain("Duration");
            table.Should().Contain("Action");
            table.Should().Contain("TOTAL");
            table.Should().Contain("IntelConfig");
            table.Should().Contain("OlObjects");
            table.Should().Contain("Events");

            // Assert: the rendered TOTAL row duration equals the sum of the injected spans and
            // is non-zero. The duration format is "%m\:ss\.ff" (same as SegmentStopWatch).
            var expectedTotal = intel + ol + events;
            var expectedTotalText = expectedTotal.ToString("%m\\:ss\\.ff");
            expectedTotal.Should().NotBe(TimeSpan.Zero);

            var totalIndex = table.IndexOf("TOTAL", StringComparison.Ordinal);
            totalIndex.Should().BeGreaterThanOrEqualTo(0);
            var lineStart = table.LastIndexOf('\n', totalIndex) + 1;
            var lineEnd = table.IndexOf('\n', totalIndex);
            var totalLine =
                lineEnd >= 0
                    ? table.Substring(lineStart, lineEnd - lineStart)
                    : table.Substring(lineStart);

            totalLine
                .Should()
                .Contain(
                    expectedTotalText,
                    "the TOTAL row duration must equal the sum of the injected non-zero phase spans."
                );
        }

        [TestMethod]
        public void RecordPhase_WithNullPhaseName_ThrowsArgumentNullException()
        {
            // Arrange
            var recorder = new StartupTimingRecorder();

            // Act
            Action act = () => recorder.RecordPhase(null!, TimeSpan.FromMilliseconds(10));

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(
                    "phaseName",
                    "RecordPhase must reject a null phase name per its contract."
                );
        }

        [TestMethod]
        public void EmitTable_LogsFormattedTableViaLoggerInfoWithStartupTimingPrefix()
        {
            // Arrange
            var recorder = new StartupTimingRecorder();
            recorder.RecordPhase("IntelConfig", TimeSpan.FromMilliseconds(100));
            recorder.RecordPhase("Events", TimeSpan.FromMilliseconds(50));
            var loggerMock = new Mock<log4net.ILog>(MockBehavior.Loose);
            object emitted = null;
            loggerMock.Setup(x => x.Info(It.IsAny<object>())).Callback<object>(o => emitted = o);

            // Act
            recorder.EmitTable(loggerMock.Object);

            // Assert: emitted exactly once, prefixed, and containing the table contents.
            loggerMock.Verify(x => x.Info(It.IsAny<object>()), Times.Once);
            var text = emitted as string;
            text.Should().NotBeNull();
            text.Should().Contain("[Startup timing]");
            text.Should().Contain("IntelConfig");
            text.Should().Contain("Events");
            text.Should().Contain("TOTAL");
        }

        [TestMethod]
        public void EmitTable_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var recorder = new StartupTimingRecorder();
            recorder.RecordPhase("IntelConfig", TimeSpan.FromMilliseconds(100));

            // Act
            Action act = () => recorder.EmitTable(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod]
        public void NullStartupTimingRecorder_IsNoOp_ForFormatAndEmit()
        {
            // Arrange
            IStartupTimingRecorder recorder = new NullStartupTimingRecorder();
            var loggerMock = new Mock<log4net.ILog>(MockBehavior.Strict);

            // Act
            recorder.RecordPhase("IntelConfig", TimeSpan.FromMilliseconds(100));
            recorder.RecordPhase("Events", TimeSpan.FromMilliseconds(200));
            var table = recorder.FormatTable();
            recorder.EmitTable(loggerMock.Object);

            // Assert: no table content and no logger interaction (strict mock would throw on any).
            table.Should().BeEmpty();
            loggerMock.Verify(x => x.Info(It.IsAny<object>()), Times.Never);
            loggerMock.VerifyNoOtherCalls();
        }
    }
}
