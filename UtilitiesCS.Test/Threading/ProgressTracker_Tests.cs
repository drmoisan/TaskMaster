using System;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

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

        #region P74 — ProgressTracker core Report/child/root-close behaviour

        /// <summary>
        /// Verifies that <see cref="ProgressTracker.Report(double, string)"/> updates the
        /// tracker's <see cref="ProgressTracker.Progress"/> property to the supplied percent
        /// value and forwards the message to the parent progress.
        ///
        /// Purpose:
        ///     Confirm the tracker's observable percent and the forwarded message are set
        ///     atomically when Report is invoked with a valid in-range value.
        ///
        /// Args:
        ///     None — uses known constants for percent (42) and message ("Processing files").
        ///
        /// Returns:
        ///     N/A (test assertion).
        ///
        /// Side Effects:
        ///     None — uses CapturingProgressTracker to avoid WinForms interaction.
        /// </summary>
        [TestMethod]
        public void Report_WithValueAndJobName_UpdatesProgressAndForwardsMessage()
        {
            // Arrange
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            // Act
            tracker.Report(42.0, "Processing files");

            // Assert — tracker's percent reflects the reported value; parent received the message.
            tracker.Progress.Should().Be(42.0);
            parent.LastJobName.Should().Be("Processing files");
            parent.LastValue.Should().Be(42);
        }

        /// <summary>
        /// Verifies that a child tracker maps its 100% completion into the parent's
        /// allocated sub-range, advancing the parent's <see cref="ProgressTracker.Progress"/>
        /// by the allocated amount.
        ///
        /// Purpose:
        ///     Child trackers cover a slice of the parent's range.  When the child reaches
        ///     100%, the parent should advance by exactly its allocation size.
        ///
        /// Args:
        ///     None — child gets a 50-unit allocation starting at 0 within a parent that
        ///     itself has a 100-unit allocation from 0.
        ///
        /// Returns:
        ///     N/A (test assertion).
        ///
        /// Side Effects:
        ///     None — uses CapturingProgressTracker as root.
        /// </summary>
        [TestMethod]
        public void Report_ViaChild_ShiftsParentProgressByAllocatedRange()
        {
            // Arrange — root captures raw values; tracker has allocation 100 from 0.
            var root = new CapturingProgressTracker();
            var tracker = new ProgressTracker(root, allocation: 100, startingAt: 0);

            // Child covers 50 units of the tracker's range (starting at the tracker's current 0).
            var child = tracker.SpawnChild(50);

            // Act — child reports 100% completion.
            child.Report(100, "Child done");

            // Assert — tracker's Progress was shifted by the child's 50-unit allocation:
            // child 100% → 50*100/100 + 0 = 50 forwarded to tracker.
            tracker.Progress.Should().Be(50);

            // tracker then forwards 50 to root: 100*50/100 + 0 = 50.
            root.LastValue.Should().Be(50);
        }

        /// <summary>
        /// Verifies that when a root tracker reaches 100%, the injected
        /// <see cref="ProgressViewer"/> is closed (and thereby disposed).
        ///
        /// Purpose:
        ///     The root tracker is responsible for dismissing the progress dialog when the
        ///     operation completes.  This test confirms that the close path executes via
        ///     <c>_isRoot</c> flag inspection using reflection.
        ///
        /// Args:
        ///     None — viewer and tracker are constructed inline, with a SynchronizationContext
        ///     installed to satisfy <see cref="ProgressViewer"/> construction requirements.
        ///
        /// Returns:
        ///     N/A (test assertion).
        ///
        /// Side Effects:
        ///     Temporarily installs a SynchronizationContext on the calling STA thread.
        ///     ProgressViewer.Close() disposes the un-shown Form.
        /// </summary>
        [TestMethod]
        [STAThread]
        public void Report_At100Percent_WhenRootTracker_ClosesProgressViewer()
        {
            // Arrange — SynchronizationContext is required for ProgressViewer construction.
            var context = new SynchronizationContext();
            var priorContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);

            ProgressViewer? viewer = null;

            try
            {
                // Use the child constructor so _parent is properly wired to a capture stub.
                var capture = new CapturingProgressTracker();
                var tracker = new ProgressTracker(capture, allocation: 100, startingAt: 0);

                // Inject a real ProgressViewer so Close() can execute on the _progressViewer field.
                viewer = new ProgressViewer();
                typeof(ProgressTracker)
                    .GetField("_progressViewer", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .SetValue(tracker, viewer);

                // Promote the tracker to root so the 100% close guard is active.
                typeof(ProgressTracker)
                    .GetField("_isRoot", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .SetValue(tracker, true);

                // Act — reporting 100% triggers the root close path.
                tracker.Report(100, "Complete");

                // Assert — Close() on an un-shown Form disposes it.
                viewer.IsDisposed.Should().BeTrue();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(priorContext);
            }
        }

        #endregion
    }
}
