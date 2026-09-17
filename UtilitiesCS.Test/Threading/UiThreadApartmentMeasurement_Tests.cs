using System;
using System.Threading;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace UtilitiesCS.Test.Threading
{
    /// <summary>
    /// Regression coverage for issue #816: the captured-UI-context exit of
    /// <c>UiThread.SynchronizationContextAwaiter.IsCompleted</c> must fail closed when the
    /// executing thread is not the thread that owns the captured UI dispatcher.
    /// </summary>
    /// <remarks>
    /// Both cases install process-global <c>UiThread</c> statics through the shared install scope,
    /// which is documented as not internally synchronized, so this class carries the non-parallel
    /// attribute. Each case creates its own thread with an explicit apartment and joins it before
    /// returning, so neither depends on the apartment of the ambient test worker.
    /// </remarks>
    [TestClass]
    [DoNotParallelize]
    public class UiThreadPredicateHardening_Tests
    {
        /// <summary>
        /// The recycled-managed-thread-id leg. The captured id matches the executing thread by
        /// construction, the captured context matches by reference, and the executing thread owns
        /// no WPF dispatcher, so the predicate must return false rather than resume inline.
        /// </summary>
        [TestMethod]
        public void IsCompleted_WhenTheCapturedUiContextMatchesButTheExecutingThreadOwnsNoDispatcher_ReturnsFalse()
        {
            // Arrange
            using (UiThreadStateScope.Enter())
            using (var host = new SharedStaDispatcherHost())
            {
                var capturedContext = new SynchronizationContext();
                UiThreadStateScope.SetDispatcher(host.Dispatcher);
                UiThreadStateScope.SetUiSyncContext(capturedContext);
                bool observed = true;

                // Act
                Exception thrown = ApartmentThreadRunner.RunOnThread(
                    ApartmentState.MTA,
                    () =>
                    {
                        UiThreadStateScope.SetUiThreadId(Thread.CurrentThread.ManagedThreadId);
                        SynchronizationContext.SetSynchronizationContext(
                            new SynchronizationContext()
                        );
                        var awaiter = new UiThread.SynchronizationContextAwaiter(capturedContext);
                        observed = awaiter.IsCompleted;
                    }
                );

                // Assert
                thrown.Should().BeNull();
                observed.Should().BeFalse();
            }
        }

        /// <summary>
        /// The fail-closed leg. No captured UI dispatcher is installed and the executing thread has
        /// none of its own, so a bare reference comparison would match null against null and let
        /// the exit return true on the very thread shape this hardening exists to reject.
        /// </summary>
        [TestMethod]
        public void IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse()
        {
            // Arrange
            using (UiThreadStateScope.Enter())
            {
                var capturedContext = new SynchronizationContext();
                UiThreadStateScope.SetDispatcher(null);
                UiThreadStateScope.SetUiSyncContext(capturedContext);
                bool observed = true;

                // Act
                Exception thrown = ApartmentThreadRunner.RunOnThread(
                    ApartmentState.MTA,
                    () =>
                    {
                        UiThreadStateScope.SetUiThreadId(Thread.CurrentThread.ManagedThreadId);
                        SynchronizationContext.SetSynchronizationContext(
                            new SynchronizationContext()
                        );
                        var awaiter = new UiThread.SynchronizationContextAwaiter(capturedContext);
                        observed = awaiter.IsCompleted;
                    }
                );

                // Assert
                thrown.Should().BeNull();
                observed.Should().BeFalse();
            }
        }
    }

    /// <summary>
    /// Discharges clause (i) of issue #809's AC5 by reading the apartment of the executing thread
    /// at runtime, then recording what the production capture form does when it is shown on a
    /// thread measured as MTA.
    /// </summary>
    /// <remarks>
    /// The settling value is recorded rather than asserted, because that clause is a measurement
    /// obligation and not a gate. The guard value is asserted, because a probe that ran on an STA
    /// thread would have measured nothing about MTA behaviour and the run would be void. The
    /// apartment is read on the executing thread and is never inferred from a settings file, from
    /// the assembly-level parallelization attribute, or from documented attribute behaviour.
    /// </remarks>
    [TestClass]
    [DoNotParallelize]
    public class UiThreadApartmentMeasurement_Tests
    {
        /// <summary>Gets or sets the context MSTest supplies, used for the record lines.</summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Reads the apartment on a dedicated thread created as MTA, then constructs the
        /// production capture form and shows it, recording whether that completed or threw.
        /// </summary>
        [TestMethod]
        public void SyncContextFormShow_OnAThreadMeasuredAsMta_RecordsTheOutcome()
        {
            // Arrange
            ApartmentState measuredApartment = ApartmentState.Unknown;

            // Act
            Exception thrown = ApartmentThreadRunner.RunOnThread(
                ApartmentState.MTA,
                () =>
                {
                    measuredApartment = Thread.CurrentThread.GetApartmentState();
                    var form = new SyncContextForm();
                    try
                    {
                        // Mirrors production initialization, which sets both before showing. An
                        // unattended run must not display a real window.
                        form.ShowInTaskbar = false;
                        form.WindowState = FormWindowState.Minimized;
                        form.Show();
                    }
                    finally
                    {
                        form.Dispose();
                    }
                }
            );

            // Assert
            measuredApartment.Should().Be(ApartmentState.MTA);
            TestContext.WriteLine("MTA_GUARD_APARTMENT: " + measuredApartment);
            if (thrown is null)
            {
                TestContext.WriteLine("MTA_INITIALIZE_OUTCOME: COMPLETED");
            }
            else
            {
                string detail = thrown.GetType().FullName + ": " + thrown.Message;
                TestContext.WriteLine("MTA_INITIALIZE_OUTCOME: THREW " + detail);
            }
        }
    }
}
