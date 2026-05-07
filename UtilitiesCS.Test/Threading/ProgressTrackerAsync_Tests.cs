using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    [TestClass]
    public class ProgressTrackerAsync_Tests
    {
        [TestMethod]
        public void Constructor_WithTokenSource_ShouldSetDefaultProperties()
        {
            var cts = new CancellationTokenSource();

            var tracker = new ProgressTrackerAsync(cts);

            tracker.Allocation.Should().Be(100);
            tracker.StartingAt.Should().Be(0);
            tracker.ProgressViewer.Should().BeNull();
            tracker.UiDispatcher.Should().BeNull();
        }

        [TestMethod]
        public void Allocation_ShouldBeSettable()
        {
            var tracker = new ProgressTrackerAsync(new CancellationTokenSource());

            tracker.Allocation = 50;

            tracker.Allocation.Should().Be(50);
        }

        [TestMethod]
        public void StartingAt_ShouldBeSettable()
        {
            var tracker = new ProgressTrackerAsync(new CancellationTokenSource());

            tracker.StartingAt = 25;

            tracker.StartingAt.Should().Be(25);
        }

        [TestMethod]
        public void JobName_ShouldBeSettable()
        {
            var tracker = new ProgressTrackerAsync(new CancellationTokenSource());

            tracker.JobName = "Processing";

            tracker.JobName.Should().Be("Processing");
        }

        // -----------------------------------------------------------------------
        // P62-T1 — The screen-variant constructor produces the same default
        //           root-tracker state as the basic constructor.
        // -----------------------------------------------------------------------

        [TestMethod]
        public void Constructor_WithScreenOverload_HasSameDefaultsAsBasicConstructor()
        {
            // Arrange: null Screen is accepted and is the only headless-safe value.
            var cts = new CancellationTokenSource();

            // Act: use the (tokenSource, screen) overload with a null screen.
            var tracker = new ProgressTrackerAsync(cts, null);

            // Assert: root-tracker defaults are the same as the single-arg constructor.
            tracker.Allocation.Should().Be(100);
            tracker.StartingAt.Should().Be(0);
            tracker.ProgressViewer.Should().BeNull();
            tracker.UiDispatcher.Should().BeNull();
        }

        // -----------------------------------------------------------------------
        // P62-T2 — After setting Allocation and JobName together the tracker
        //           reflects both values (models a "Report percent + message" update).
        // -----------------------------------------------------------------------

        [TestMethod]
        public void Tracker_SetAllocationAndJobName_BothPropertiesReflectUpdatedValues()
        {
            // Arrange: a freshly constructed tracker.
            var tracker = new ProgressTrackerAsync(new CancellationTokenSource());

            // Act: update the two "report" fields simultaneously.
            tracker.Allocation = 75;
            tracker.JobName = "Classifying mails";

            // Assert: both fields survive the round-trip.
            tracker.Allocation.Should().Be(75);
            tracker.JobName.Should().Be("Classifying mails");
        }

        // -----------------------------------------------------------------------
        // P62-T3 — A child tracker configured with a sub-range of the parent's
        //           allocation correctly stores StartingAt and Allocation as
        //           provided, confirming the child-configuration contract.
        // -----------------------------------------------------------------------

        [TestMethod]
        public void ChildTracker_ConfiguredWithSubRange_AllocationAndStartingAtArePreserved()
        {
            // Arrange: parent and child share the same CancellationTokenSource, which
            // is the mechanism through which cancellation propagates from parent to child.
            var cts = new CancellationTokenSource();
            var parent = new ProgressTrackerAsync(cts) { Allocation = 100, StartingAt = 0 };

            // Act: construct a child tracker that owns the 10-60 range of the parent.
            var child = new ProgressTrackerAsync(cts)
            {
                Allocation = 50,
                StartingAt = parent.StartingAt + 10,
            };

            // Assert: child's allocation parameters reflect the configured sub-range.
            child.Allocation.Should().Be(50);
            child.StartingAt.Should().Be(10);
        }

        [TestMethod]
        public void InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker()
        {
            // MSTest does not honour [STAThread] on test methods, and async Task tests
            // lose apartment state after the first await. The only reliable way to host
            // a WPF Dispatcher in a .NET Framework MSTest is to own a dedicated STA
            // thread and drive it with Dispatcher.Run() / BeginInvokeShutdown().
            Exception? testException = null;

            var staThread = new Thread(() =>
            {
                using var cts = new CancellationTokenSource();
                var tracker = new ProgressTrackerAsync(cts);
                ProgressViewer? shownViewer = null;

                var dispatcherField = typeof(UiThread).GetField(
                    "_dispatcher",
                    BindingFlags.NonPublic | BindingFlags.Static
                );
                dispatcherField.Should().NotBeNull();

                var currentDispatcher = Dispatcher.CurrentDispatcher;
                var previousDispatcher = (Dispatcher)dispatcherField!.GetValue(null);
                tracker.ShowProgressViewer = viewer => shownViewer = viewer;

                try
                {
                    dispatcherField.SetValue(null, currentDispatcher);

                    var initializeTask = tracker.InitializeAsync();

                    // When initializeTask completes its InvokeAsync work on the
                    // dispatcher, shut the loop down so Dispatcher.Run() returns.
                    _ = initializeTask.ContinueWith(
                        _ => currentDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal),
                        TaskScheduler.Default
                    );

                    // Run the full STA dispatcher message loop. This pumps Win32
                    // messages and processes all queued InvokeAsync work items.
                    Dispatcher.Run();

                    // Dispatcher.Run() returned: initializeTask is guaranteed complete.
                    var initializedTracker = initializeTask.Result;
                    var initializedViewer = tracker.ProgressViewer;

                    try
                    {
                        initializedTracker.Should().BeSameAs(tracker);
                        shownViewer.Should().BeSameAs(initializedViewer);
                        tracker.UiDispatcher.Should().BeSameAs(currentDispatcher);
                        initializedViewer.Should().NotBeNull();
                        initializedViewer!.CancelSource.Should().BeSameAs(cts);
                        initializedViewer.JobName.Text.Should().Be("Initializing...");
                        initializedViewer.Visible.Should().BeFalse();

                        tracker.ProgressViewer = initializedViewer;
                        tracker.ProgressViewer.Should().BeSameAs(initializedViewer);
                    }
                    finally
                    {
                        initializedViewer?.Close();
                    }
                }
                catch (Exception ex)
                {
                    testException = ex;
                    // Ensure the dispatcher loop exits even when assertions fail.
                    currentDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                }
                finally
                {
                    dispatcherField.SetValue(null, previousDispatcher);

                    if (tracker.ProgressViewer != null && !tracker.ProgressViewer.IsDisposed)
                    {
                        tracker.ProgressViewer.Close();
                    }
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            // Re-throw any assertion or unexpected exception on the MSTest thread
            // so Test Explorer reports the correct failure.
            if (testException != null)
            {
                System
                    .Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(testException)
                    .Throw();
            }
        }
    }
}
