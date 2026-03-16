using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class SegmentStopWatch_Tests
    {
        [TestMethod]
        public void StartAndStop_ReturnSameInstance_AndResetClearsElapsed()
        {
            // Arrange
            var sut = new SegmentStopWatch();

            // Act
            var started = sut.Start();
            Thread.Sleep(20);
            var stopped = sut.Stop();
            var elapsedBeforeReset = sut.Elapsed;
            sut.Reset();

            // Assert
            started.Should().BeSameAs(sut);
            stopped.Should().BeSameAs(sut);
            elapsedBeforeReset.Should().BeGreaterThan(TimeSpan.Zero);
            sut.Elapsed.Should().Be(TimeSpan.Zero);
            sut.IsRunning.Should().BeFalse();
        }

        [TestMethod]
        public void LogDuration_CapturesMultipleNamedSegments()
        {
            // Arrange
            var sut = new SegmentStopWatch().Start();

            // Act
            Thread.Sleep(20);
            sut.LogDuration("first");
            Thread.Sleep(20);
            sut.LogDuration("second", logImmediately: true);
            sut.Stop();

            // Assert
            sut.Durations.Should().HaveCount(2);
            sut.Durations.Should().ContainSingle(x => x.ActionName == "first" && x.Duration > TimeSpan.Zero);
            sut.Durations.Should().ContainSingle(x => x.ActionName == "second" && x.Duration > TimeSpan.Zero);
        }

        [TestMethod]
        public void GroupByActionName_ReturnsGroupedCopy_AndCanReplaceDurationsInPlace()
        {
            // Arrange
            var sut = new SegmentStopWatch();
            sut.Durations.Push(("alpha", TimeSpan.FromMilliseconds(15)));
            sut.Durations.Push(("beta", TimeSpan.FromMilliseconds(10)));
            sut.Durations.Push(("alpha", TimeSpan.FromMilliseconds(5)));

            // Act
            var groupedCopy = sut.GroupByActionName();
            var inPlaceResult = sut.GroupByActionName(inplace: true);

            // Assert
            groupedCopy.Should().NotBeNull();
            groupedCopy.Should().HaveCount(2);
            groupedCopy.Should().Contain(x => x.ActionName == "alpha" && x.Duration == TimeSpan.FromMilliseconds(20));
            groupedCopy.Should().Contain(x => x.ActionName == "beta" && x.Duration == TimeSpan.FromMilliseconds(10));
            inPlaceResult.Should().BeNull();
            sut.Durations.Should().HaveCount(2);
            sut.Durations.Should().Contain(x => x.ActionName == "alpha" && x.Duration == TimeSpan.FromMilliseconds(20));
        }

        [TestMethod]
        public void MergeDurations_AndGroupDurations_CombineDurationsByActionName()
        {
            // Arrange
            var sut = new SegmentStopWatch();
            sut.Durations.Push(("existing", TimeSpan.FromMilliseconds(10)));
            sut.Durations.Push(("shared", TimeSpan.FromMilliseconds(15)));

            var incoming = new System.Collections.Generic.Stack<(string ActionName, TimeSpan Duration)>();
            incoming.Push(("shared", TimeSpan.FromMilliseconds(5)));
            incoming.Push(("incoming", TimeSpan.FromMilliseconds(8)));

            // Act
            sut.MergeDurations(incoming);
            var grouped = SegmentStopWatch.GroupDurations(
                new System.Collections.Generic.Stack<(string ActionName, TimeSpan Duration)>(
                    new[] { ("x", TimeSpan.FromMilliseconds(1)), ("shared", TimeSpan.FromMilliseconds(2)) }),
                new System.Collections.Generic.Stack<(string ActionName, TimeSpan Duration)>(
                    new[] { ("shared", TimeSpan.FromMilliseconds(3)), ("y", TimeSpan.FromMilliseconds(4)) }));

            // Assert
            sut.Durations.Should().Contain(x => x.ActionName == "existing" && x.Duration == TimeSpan.FromMilliseconds(10));
            sut.Durations.Should().Contain(x => x.ActionName == "incoming" && x.Duration == TimeSpan.FromMilliseconds(8));
            sut.Durations.Should().Contain(x => x.ActionName == "shared" && x.Duration == TimeSpan.FromMilliseconds(20));
            grouped.Should().Contain(x => x.ActionName == "shared" && x.Duration == TimeSpan.FromMilliseconds(5));
            grouped.Should().Contain(x => x.ActionName == "x" && x.Duration == TimeSpan.FromMilliseconds(1));
            grouped.Should().Contain(x => x.ActionName == "y" && x.Duration == TimeSpan.FromMilliseconds(4));
        }

        [TestMethod]
        public void GetDurations_FormatsReportAndWriteToLogCanClearDurations()
        {
            // Arrange
            var sut = new SegmentStopWatch();
            sut.Durations.Push(("alpha", TimeSpan.FromSeconds(1)));
            sut.Durations.Push(("beta", TimeSpan.FromSeconds(2)));

            // Act
            var report = sut.GetDurations("SegmentStopWatch_Tests");
            sut.WriteToLog("SegmentStopWatch_Tests", clear: true);

            // Assert
            report.Should().Contain("SEGMENT DURATIONS");
            report.Should().Contain("SEGMENTSTOPWATCH_TESTS");
            report.Should().Contain("Duration");
            report.Should().Contain("Action");
            report.Should().Contain("alpha");
            report.Should().Contain("beta");
            report.Should().Contain("TOTAL");
            sut.Durations.Should().BeEmpty();
        }
    }
}