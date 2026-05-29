using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Threading;
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
    ///     This class runs under MSTest's STA class execution mode (required by WinForms).
    ///     Construction requires a non-null SynchronizationContext.Current so that
    ///     TaskScheduler.FromCurrentSynchronizationContext() succeeds; each test
    ///     installs and then restores the SynchronizationContext around the viewer.
    ///     CancelButton_Click calls this.Close(), which disposes an un-shown Form —
    ///     tests that invoke the cancel path must not use 'using' on the viewer,
    ///     and must capture the CancellationToken before invoking the handler.
    /// </summary>
    [STATestClass]
    public class ProgressViewer_Tests
    {
        private static ProgressViewer CreateHeadlessViewer() =>
            (ProgressViewer)FormatterServices.GetUninitializedObject(typeof(ProgressViewer));

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
        ///     None — WinForms automatically installs a WindowsFormsSynchronizationContext
        ///     during Form construction.
        ///
        /// Returns:
        ///     N/A (test assertion).
        ///
        /// Side Effects:
        ///     Temporarily installs a SynchronizationContext on the calling thread;
        ///     restores the prior context in the finally block. WinForms replaces
        ///     the installed context with a WindowsFormsSynchronizationContext during
        ///     InitializeComponent.
        /// </summary>
        [TestMethod]
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
                // WinForms will install a WindowsFormsSynchronizationContext during construction.
                using ProgressViewer viewer = new ProgressViewer();

                // Assert — UiSyncContext is non-null and is the WinForms context installed
                // during construction. UiScheduler is non-null (created via
                // FromCurrentSynchronizationContext).
                viewer.UiSyncContext.Should().NotBeNull();
                viewer
                    .UiSyncContext.Should()
                    .BeOfType<System.Windows.Forms.WindowsFormsSynchronizationContext>();
                viewer.UiScheduler.Should().NotBeNull();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        // ---------------------------------------------------------------------------
        // P30-T3: UiDispatcher getter and setter round-trips the assigned value
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the <see cref="ProgressViewer.UiDispatcher"/> property
        /// getter returns the same dispatcher that was assigned via the setter.
        ///
        /// Purpose:
        ///     Callers store the WPF Dispatcher on the viewer so that WPF-bound
        ///     continuations can marshal work back to the UI thread. The getter/setter
        ///     must faithfully store and retrieve the value.
        ///
        /// Side Effects:
        ///     Retrieves <c>Dispatcher.CurrentDispatcher</c> on the STA test thread
        ///     once; creates and disposes the viewer in the finally block.
        /// </summary>
        [TestMethod]
        public void UiDispatcher_SetterAndGetter_RoundTripAssignedValue()
        {
            // Arrange
            var viewer = CreateHeadlessViewer();

            // Use the STA thread's current dispatcher as a non-null sentinel value.
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

            // Act
            viewer.UiDispatcher = dispatcher;

            // Assert
            viewer
                .UiDispatcher.Should()
                .BeSameAs(
                    dispatcher,
                    "the setter must store and the getter must return the assigned Dispatcher"
                );
        }

        // ---------------------------------------------------------------------------
        // P30-T4: UiThreadNumber getter and setter round-trips the assigned value
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the <see cref="ProgressViewer.UiThreadNumber"/> property
        /// getter returns the value assigned via the setter.
        ///
        /// Purpose:
        ///     Threading utilities compare the current thread ID against
        ///     UiThreadNumber to decide whether a marshal is required. The setter
        ///     must overwrite the ID captured at construction time, and the getter
        ///     must return the updated value.
        ///
        /// Side Effects:
        ///     Creates and disposes a ProgressViewer in the finally block.
        /// </summary>
        [TestMethod]
        public void UiThreadNumber_SetterAndGetter_RoundTripAssignedValue()
        {
            // Arrange
            var viewer = CreateHeadlessViewer();
            const int expectedThreadId = 99;

            // Act
            viewer.UiThreadNumber = expectedThreadId;

            // Assert
            viewer
                .UiThreadNumber.Should()
                .Be(
                    expectedThreadId,
                    "the setter must store and the getter must return the assigned thread ID"
                );
        }

        // ---------------------------------------------------------------------------
        // P30-T5: CancelSource getter and setter round-trips the assigned value
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the <see cref="ProgressViewer.CancelSource"/> property
        /// getter returns the same <see cref="CancellationTokenSource"/> that was
        /// assigned via the setter.
        ///
        /// Purpose:
        ///     External code may need to replace or read back the cancel source after
        ///     initial setup. The setter must overwrite and the getter must faithfully
        ///     return the new value.
        ///
        /// Side Effects:
        ///     Creates and disposes the viewer and the CancellationTokenSource in the
        ///     finally block.
        /// </summary>
        [TestMethod]
        public void CancelSource_SetterAndGetter_RoundTripAssignedValue()
        {
            // Arrange
            var viewer = CreateHeadlessViewer();
            using var cts = new CancellationTokenSource();

            // Act: use the setter directly (not SetCancellationTokenSource)
            viewer.CancelSource = cts;

            // Assert: getter must return the same instance
            viewer
                .CancelSource.Should()
                .BeSameAs(
                    cts,
                    "the setter must store and the getter must return the assigned CancellationTokenSource"
                );
        }
    }
}
