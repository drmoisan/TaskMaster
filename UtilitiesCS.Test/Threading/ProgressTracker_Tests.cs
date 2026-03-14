using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class ProgressTracker_Tests
    {
        [TestMethod]
        public void Increment_ShouldUpdateProgressAndForwardScaledValueAndJobName()
        {
            // Arrange
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 40, startingAt: 10);

            // Act
            tracker.Increment(25, "Load data");

            // Assert
            tracker.Progress.Should().Be(25);
            parent.LastValue.Should().Be(20);
            parent.LastJobName.Should().Be("Load data");
        }

        [TestMethod]
        public void Report_ShouldClampValuesAboveOneHundred()
        {
            // Arrange
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 50, startingAt: 10);

            // Act
            tracker.Report(150, null);

            // Assert
            tracker.Progress.Should().Be(100);
            parent.LastValue.Should().Be(60);
            parent.LastJobName.Should().BeNull();
        }

        [TestMethod]
        public void Report_ShouldThrowForNegativeValues()
        {
            // Arrange
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 50, startingAt: 10);

            // Act
            Action act = () => tracker.Report(-1);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void SpawnChild_ShouldUseRemainingAllocationFromCurrentProgress()
        {
            // Arrange
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);
            tracker.Report(30, "Parent");

            // Act
            ProgressTracker child = tracker.SpawnChild();
            child.Report(100, "Finish");

            // Assert
            parent.LastValue.Should().Be(100);
            parent.LastJobName.Should().Be("Finish");
            tracker.Progress.Should().Be(100);
        }

        private sealed class CapturingProgressTracker : ProgressTracker
        {
            public CapturingProgressTracker() : base(new CancellationTokenSource())
            {
            }

            public int? LastValue { get; private set; }

            public string LastJobName { get; private set; }

            public override void Report((int Value, string JobName) report)
            {
                LastValue = report.Value;
                LastJobName = report.JobName;
            }
        }
    }
}
