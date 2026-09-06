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
    public partial class ProgressTracker_Tests
    {
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
        [STATestMethod]
        public void Report_At100Percent_WhenRootTracker_ClosesProgressViewer()
        {
            // Arrange — SynchronizationContext is required for ProgressViewer construction.
            var context = new SynchronizationContext();
            var priorContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);

            // This file has no project-level <Nullable> and no whole-file #nullable pragma;
            // this pre-existing `?` annotation needs an explicit annotations context to avoid
            // CS8632. Scoping narrowly to annotations-only avoids introducing new CS86xx
            // diagnostics elsewhere in this file (no behavior change per AC7).
#nullable enable annotations
            ProgressViewer? viewer = null;
#nullable restore annotations

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

        [STATestMethod]
        public void Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi()
        {
            using var cts = new CancellationTokenSource();
            var tracker = new ProgressTracker(cts, Screen.PrimaryScreen);
            // Same CS8632 annotations-context scoping as elsewhere in this file.
#nullable enable annotations
            ProgressViewer? shownViewer = null;
#nullable restore annotations
            var previousContext = SynchronizationContext.Current;
            var currentDispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            tracker.ShowProgressViewer = viewer => shownViewer = viewer;

            try
            {
                // The shared install scope captures the prior dispatcher and restores it on
                // disposal, so this test no longer performs its own reflection.
                using (UiThreadDispatcherScope.Install(currentDispatcher))
                {
                    tracker.Initialize().Should().BeSameAs(tracker);
                    shownViewer.Should().BeSameAs(tracker.ProgressViewer);
                    tracker.UiDispatcher.Should().BeSameAs(currentDispatcher);
                    tracker.ProgressViewer.Should().NotBeNull();
                    tracker.ProgressViewer.CancelSource.Should().BeSameAs(cts);
                    tracker.ProgressViewer.StartPosition.Should().Be(FormStartPosition.Manual);
                    tracker.ProgressViewer.Bar.Value.Should().Be(0);
                    tracker.ProgressViewer.Visible.Should().BeFalse();
                }
            }
            finally
            {
                if (tracker.ProgressViewer != null && !tracker.ProgressViewer.IsDisposed)
                {
                    tracker.ProgressViewer.Close();
                }

                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        [TestMethod]
        public async Task ReportAsync_WithNegativeValue_ThrowsArgumentOutOfRangeException()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            Func<Task> act = () => tracker.ReportAsync(-1);

            await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public async Task ReportAsync_WithValueOver100_ClampsTo100()
        {
            var parent = new CapturingProgressTracker();
            var tracker = new ProgressTracker(parent, allocation: 100, startingAt: 0);

            await tracker.ReportAsync(125);

            tracker.Progress.Should().Be(100);
            parent.LastValue.Should().Be(100);
        }

        [STATestMethod]
        public async Task ReportAsync_At100Percent_WhenRootTracker_ClosesProgressViewer()
        {
            var priorContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            // Same CS8632 annotations-context scoping as elsewhere in this file.
#nullable enable annotations
            ProgressViewer? viewer = null;
#nullable restore annotations

            try
            {
                var capture = new CapturingProgressTracker();
                var tracker = new ProgressTracker(capture, allocation: 100, startingAt: 0);

                viewer = new ProgressViewer { UiDispatcher = Dispatcher.CurrentDispatcher };
                typeof(ProgressTracker)
                    .GetField("_progressViewer", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .SetValue(tracker, viewer);
                typeof(ProgressTracker)
                    .GetField("_isRoot", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .SetValue(tracker, true);

                await tracker.ReportAsync(100);

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
