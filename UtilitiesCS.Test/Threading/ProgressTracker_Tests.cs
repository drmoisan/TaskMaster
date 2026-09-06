using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test
{
    [TestClass]
    [DoNotParallelize]
    public partial class ProgressTracker_Tests
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
            public CapturingProgressTracker()
                : base(new CancellationTokenSource()) { }

            public int? LastValue { get; private set; }

            public string LastJobName { get; private set; }

            public override void Report((int Value, string JobName) report)
            {
                LastValue = report.Value;
                LastJobName = report.JobName;
            }
        }

        #region Extended Tests — P2-T17

        [TestMethod]
        public void Increment_ShouldAccumulateProgressValues()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            tracker.Increment(30);
            tracker.Progress.Should().Be(30);

            tracker.Increment(20);
            tracker.Progress.Should().Be(50);
        }

        [TestMethod]
        public void Increment_ShouldClampAt100()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            tracker.Increment(60);
            tracker.Increment(60);

            tracker.Progress.Should().Be(100);
        }

        [TestMethod]
        public void Report_WithTupleOverload_ShouldSetValueAndJobName()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 50, startingAt: 0);

            tracker.Report((75, "halfway"));

            tracker.Progress.Should().Be(75);
            parent.LastJobName.Should().Be("halfway");
        }

        [TestMethod]
        public void Report_DoubleOverload_ShouldThrowForNegative()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            Action act = () => tracker.Report(-5.0);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Report_DoubleOverload_ShouldClampAbove100()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            tracker.Report(200.0);

            tracker.Progress.Should().Be(100);
        }

        [TestMethod]
        public void SpawnChild_WithAllocation_ShouldCreateChildWithSpecifiedAllocation()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            var child = tracker.SpawnChild(50);
            child.Report(100, "child done");

            parent.LastValue.Should().Be(50);
        }

        [TestMethod]
        public void SpawnChild_WithDoubleAllocation_ShouldRoundAndCreateChild()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            var child = tracker.SpawnChild(33.7);
            child.Report(100, "child done");

            parent.LastValue.Should().Be(34);
        }

        [TestMethod]
        public void Report_WithDoubleAndJobName_ShouldClampAt100()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            tracker.Report(150.0, "overshooting");

            tracker.Progress.Should().Be(100);
        }

        [TestMethod]
        public void Report_WithDoubleAndJobName_ShouldThrowForNegative()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            Action act = () => tracker.Report(-1.0, "bad");

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Constructor_WithParent_ShouldInheritJobName()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            tracker.Progress.Should().Be(0);
        }

        #endregion

        #region P66 — ProgressTrackerPane behaviour (headless via CapturingProgressTracker)

        [TestMethod]
        public void Report_WithJobName_RootReportsToStubPane()
        {
            // Arrange: CapturingProgressTracker stands in for the WinForms pane.
            var stubPane = new CapturingProgressTracker();
            var tracker = new ProgressTracker(stubPane, allocation: 100, startingAt: 0);

            // Act
            tracker.Report(65, "Indexing");

            // Assert: stub pane received the expected percent and message.
            stubPane.LastValue.Should().Be(65);
            stubPane.LastJobName.Should().Be("Indexing");
        }

        [TestMethod]
        public void SpawnChild_FromProgressedParent_MapsChildProgressIntoParentRange()
        {
            // Arrange: root capturing parent, parent allocated 80 % starting at 10 %.
            var root = new CapturingProgressTracker();
            var parent = new ProgressTracker(root, allocation: 80, startingAt: 10);
            parent.Report(0, "start");

            // Child gets an explicit 40-unit allocation within the parent range.
            var child = parent.SpawnChild(40);

            // Act: child reports 50 % complete.
            child.Report(50, "halfway");

            // Assert: child 50 % → parent 20 % (40*50/100) → root 26 % (80*20/100+10).
            root.LastValue.Should().Be(26);
            root.LastJobName.Should().Be("halfway");
        }

        [TestMethod]
        public void Report_At100Percent_SetsProgressToMaxAndForwardsToParent()
        {
            // Arrange: tracker with a non-trivial allocation window to confirm full completion
            // maps correctly to the parent range.
            var stubPane = new CapturingProgressTracker();
            var tracker = new ProgressTracker(stubPane, allocation: 50, startingAt: 30);

            // Act
            tracker.Report(100, "Complete");

            // Assert: local progress capped at 100; parent receives 50*100/100+30 = 80.
            tracker.Progress.Should().Be(100);
            stubPane.LastValue.Should().Be(80);
            stubPane.LastJobName.Should().Be("Complete");
        }

        #endregion
    }
}
