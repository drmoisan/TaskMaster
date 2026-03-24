using System;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Threading
{
    /// <summary>
    /// Unit tests for <see cref="ProgressViewer"/>.
    ///
    /// Purpose:
    ///     Verify that ProgressViewer captures the synchronization context, thread
    ///     number, and task scheduler on construction, and that the cancel path
    ///     correctly transitions the supplied CancellationTokenSource into the
    ///     cancelled state.
    ///
    /// Constraints:
    ///     All tests run on an STA thread (required by WinForms).
    ///     Construction requires a non-null SynchronizationContext.Current so that
    ///     TaskScheduler.FromCurrentSynchronizationContext() succeeds; each test
    ///     installs and then restores the SynchronizationContext around the viewer.
    ///     CancelButton_Click calls this.Close(), which disposes an un-shown Form —
    ///     tests that invoke the cancel path must not use 'using' on the viewer,
    ///     and must capture the CancellationToken before invoking the handler.
    /// </summary>
    [TestClass]
    public class ProgressViewer_Tests
    {
        // ---------------------------------------------------------------------------
        // P30-T1: Cancel path transitions the CancellationToken to cancelled
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that invoking the cancel path (CancelButton_Click) cancels the
        /// associated CancellationTokenSource.
        ///
        /// Purpose:
        ///     Background workers observe the CancellationToken for cooperative
        ///     cancellation; this test confirms close/cancel in the UI transitions
        ///     the token.
        ///
        /// Args:
        ///     None — CancellationTokenSource is constructed inline.
        ///
        /// Returns:
        ///     N/A (test assertion).
        ///
        /// Side Effects:
        ///     CancelButton_Click calls Close(), which disposes the Form for an
        ///     un-shown window.  The CancellationTokenSource is still accessible
        ///     because it was created outside the viewer.
        /// </summary>
        [TestMethod]
        [STAThread]
        public void CancelPath_WhenInvoked_CancelsTokenSource()
        {
            // Arrange — install a SynchronizationContext so the constructor does not throw.
            var context = new SynchronizationContext();
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);

            try
            {
                // CancelButton_Click calls Close() which may dispose the Form —
                // do not wrap the viewer in a using block.
                ProgressViewer viewer = new ProgressViewer();
                using var cts = new CancellationTokenSource();

                // Wire the token source so CancelButton_Click has a target to cancel.
                viewer.SetCancellationTokenSource(cts);

                // Locate CancelButton_Click via reflection (it is private).
                MethodInfo cancelClick =
                    typeof(ProgressViewer).GetMethod(
                        "CancelButton_Click",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    )
                    ?? throw new MissingMethodException(
                        nameof(ProgressViewer),
                        "CancelButton_Click"
                    );

                // Act — trigger the cancel path.
                cancelClick.Invoke(viewer, new object[] { viewer, EventArgs.Empty });

                // Assert — the token source must now be in the cancelled state.
                cts.Token.IsCancellationRequested.Should().BeTrue();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        // ---------------------------------------------------------------------------
        // P30-T2: UiSyncContext and UiScheduler are populated after construction
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the ProgressViewer constructor populates the UiSyncContext
        /// and UiScheduler properties from the ambient SynchronizationContext so that
        /// callers can marshal updates back to the UI thread.
        ///
        /// Purpose:
        ///     Callers schedule continuation tasks via UiScheduler and post callbacks
        ///     via UiSyncContext.  Both must be non-null after construction.
        ///
        /// Args:
        ///     None — relies on a SynchronizationContext installed on the calling
        ///     thread before construction.
        ///
        /// Returns:
        ///     N/A (test assertion).
        ///
        /// Side Effects:
        ///     Temporarily installs a SynchronizationContext on the calling thread;
        ///     restores the prior context in the finally block.
        /// </summary>
        [TestMethod]
        [STAThread]
        public void Constructor_PopulatesSyncContextAndScheduler()
        {
            // Arrange — install a known SynchronizationContext so that
            // TaskScheduler.FromCurrentSynchronizationContext() can capture it.
            var context = new SynchronizationContext();
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);

            try
            {
                // Act — create the viewer with the installed context in scope.
                using ProgressViewer viewer = new ProgressViewer();

                // Assert — UiSyncContext references the installed context, and
                // UiScheduler is non-null (created via FromCurrentSynchronizationContext).
                viewer.UiSyncContext.Should().BeSameAs(context);
                viewer.UiScheduler.Should().NotBeNull();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }
    }
}
