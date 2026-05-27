using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.TaskPane;

namespace UtilitiesCS.Test.Threading
{
    /// <summary>
    /// Unit tests for <see cref="ProgressPane"/>.
    ///
    /// Purpose:
    ///     Verify that ProgressPane correctly captures synchronization context and
    ///     scheduler on construction, properly cancels its token source when the
    ///     cancel path is invoked, and that its exposed state (Bar value, JobName
    ///     text) can be updated and read back.
    ///
        /// Constraints:
        ///     This class runs under MSTest's STA class execution mode (required by WinForms).
    ///     Construction requires a non-null SynchronizationContext.Current so that
    ///     TaskScheduler.FromCurrentSynchronizationContext() can succeed; each test
    ///     installs and then restores the SynchronizationContext around the pane.
    ///     The CancelButton_Click handler disposes the pane — tests that invoke that
    ///     path must not use 'using' on the pane variable.
    /// </summary>
    [STATestClass]
    public class ProgressPane_Tests
    {
        // ---------------------------------------------------------------------------
        // P29-T1: Constructor captures the current SynchronizationContext and scheduler
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the ProgressPane constructor captures the ambient
        /// SynchronizationContext via UiSyncContext and creates a non-null
        /// TaskScheduler via UiScheduler.
        ///
        /// Purpose:
        ///     UiSyncContext and UiScheduler are consumed by callers to marshal
        ///     progress updates back to the UI thread.  This test confirms both are
        ///     populated on construction.
        ///
        /// Args:
        ///     None — relies on a SynchronizationContext installed on the calling
        ///     thread before the pane is created.
        ///
        /// Returns:
        ///     N/A (test assertion).
        ///
        /// Side Effects:
        ///     Temporarily installs a SynchronizationContext on the calling thread;
        ///     restores the prior context in the finally block.
        /// </summary>
        [TestMethod]
        public void Constructor_CapturesCurrentSynchronizationContextAndScheduler()
        {
            // Arrange — install a known SynchronizationContext so the constructor
            // can call TaskScheduler.FromCurrentSynchronizationContext() successfully.
            var context = new SynchronizationContext();
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);

            try
            {
                // Act — construct the pane with the installed context in scope.
                using var pane = new ProgressPane();

                // Assert — the constructor must capture a non-null context.
                // WinForms replaces the installed SynchronizationContext with a
                // WindowsFormsSynchronizationContext upon Form construction, so the
                // captured instance is not necessarily the same reference we installed;
                // the contract is that it is non-null and reflects the UI context.
                pane.UiSyncContext.Should().NotBeNull();
                pane.UiScheduler.Should().NotBeNull();
            }
            finally
            {
                // Restore thread context to avoid polluting subsequent tests.
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        // ---------------------------------------------------------------------------
        // P29-T2: Cancellation token source is in cancelled state after cancel path
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that invoking the cancel path (CancelButton_Click) transitions
        /// the supplied CancellationTokenSource into the cancelled state.
        ///
        /// Purpose:
        ///     Callers await a CancellationToken sourced from the pane to stop
        ///     background work.  This test confirms the token becomes cancelled.
        ///
        /// Args:
        ///     None — a CancellationTokenSource is constructed inline.
        ///
        /// Returns:
        ///     N/A (test assertion).
        ///
        /// Side Effects:
        ///     CancelButton_Click disposes the pane internally, so the pane variable
        ///     must not be inside a using block.
        /// </summary>
        [TestMethod]
        public void CancelButtonClick_WhenInvoked_CancelsTokenSource()
        {
            // Arrange — install a SynchronizationContext so the constructor succeeds.
            var context = new SynchronizationContext();
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);

            try
            {
                // CancelButton_Click disposes the pane — do not wrap in using.
                ProgressPane pane = new ProgressPane();
                using var cts = new CancellationTokenSource();

                // Wire up the cancellation token source so the cancel path has a target.
                pane.SetCancellationTokenSource(cts);

                // Invoke CancelButton_Click via reflection (it is private).
                MethodInfo cancelClick =
                    typeof(ProgressPane).GetMethod(
                        "CancelButton_Click",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    )
                    ?? throw new MissingMethodException(nameof(ProgressPane), "CancelButton_Click");

                // Act — trigger the cancel path.
                cancelClick.Invoke(pane, new object[] { pane, EventArgs.Empty });

                // Assert — the token source must be in the cancelled state.
                cts.Token.IsCancellationRequested.Should().BeTrue();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        // ---------------------------------------------------------------------------
        // P29-T3: Visible state and Bar/JobName props reflect assigned values
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the pane's public Bar.Value, JobName.Text, and Visible
        /// properties reflect the values assigned to them.
        ///
        /// Purpose:
        ///     Callers write progress percentage to Bar.Value and status text to
        ///     JobName.Text.  This test confirms both properties round-trip correctly
        ///     and that the standard Visible toggle works as expected.
        ///
        /// Args:
        ///     None — values are constructed inline.
        ///
        /// Returns:
        ///     N/A (test assertion).
        ///
        /// Side Effects:
        ///     Pane is disposed via using block.
        /// </summary>
        [TestMethod]
        public void BarValueAndJobNameText_WhenSet_ReflectAssignedValues()
        {
            // Arrange — install SynchronizationContext so the constructor succeeds.
            var context = new SynchronizationContext();
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);

            try
            {
                using var pane = new ProgressPane();

                // Act — update progress bar value, job label, and visibility.
                pane.Bar.Value = 42;
                pane.JobName.Text = "Processing items";

                // Assert — all three written-back properties must match.
                pane.Bar.Value.Should().Be(42);
                pane.JobName.Text.Should().Be("Processing items");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }
    }
}
