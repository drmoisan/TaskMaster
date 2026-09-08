using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    /// <summary>
    /// A capture object that stands in for <c>SyncContextForm</c> so <c>UiThread.Initialize()</c>
    /// can be driven, and made to fail, without a live WinForms form or an STA host.
    /// </summary>
    /// <remarks>
    /// It deliberately does not derive from <see cref="System.Windows.Forms.Form"/>, because
    /// <c>UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs</c> asserts this assembly compiles no
    /// <c>Form</c>-derived type. <see cref="ConstructionCount"/> is process-global because the
    /// factory is; each consuming class is <c>[DoNotParallelize]</c> and resets it in its scope.
    /// </remarks>
    internal sealed class FakeUiCaptureSource : IUiCaptureSource
    {
        /// <summary>The message a capture failure carries, so a test can assert on it.</summary>
        internal const string CaptureFailureMessage =
            "FakeUiCaptureSource was configured to fail during CaptureUiVariables().";

        /// <summary>The deterministic auto-scale factor this fake reports.</summary>
        internal static readonly System.Drawing.SizeF DeterministicAutoScaleFactor =
            new System.Drawing.SizeF(2f, 3f);

        private static int _constructionCount;

        internal FakeUiCaptureSource()
        {
            Interlocked.Increment(ref _constructionCount);
        }

        /// <summary>Gets how many instances have been constructed since the last reset.</summary>
        internal static int ConstructionCount => Volatile.Read(ref _constructionCount);

        /// <summary>Sets the construction counter back to zero.</summary>
        internal static void ResetConstructionCount() => Volatile.Write(ref _constructionCount, 0);

        /// <summary>Gets or sets whether capture throws instead of assigning the four values.</summary>
        internal bool ThrowOnCapture { get; set; }

        /// <summary>
        /// Gets or sets the dispatcher this fake reports after a successful capture. A test
        /// supplies one owned by a host it shuts down, so no dispatcher is left on a pooled worker.
        /// </summary>
        internal Dispatcher DispatcherToCapture { get; set; }

        public bool ShowInTaskbar { get; set; }
        public FormWindowState WindowState { get; set; }
        public SynchronizationContext UiSyncContext { get; private set; }
        public System.Drawing.SizeF FormAutoScaleFactor { get; private set; }
        public Dispatcher UiDispatcher { get; private set; }
        public int UiThreadId { get; private set; }

        public void Show() { }

        public void Hide() { }

        public void CaptureUiVariables()
        {
            if (ThrowOnCapture)
            {
                throw new InvalidOperationException(CaptureFailureMessage);
            }

            UiSyncContext = new SynchronizationContext();
            FormAutoScaleFactor = DeterministicAutoScaleFactor;
            UiDispatcher = DispatcherToCapture;
            UiThreadId = Thread.CurrentThread.ManagedThreadId;
        }
    }

    /// <summary>Runs a delegate on a dedicated thread in a chosen apartment.</summary>
    internal static class ApartmentThreadRunner
    {
        /// <summary>Runs <paramref name="action"/> on a dedicated thread in that apartment.</summary>
        /// <param name="apartment">The apartment state to set before starting the thread.</param>
        /// <param name="action">The delegate to run.</param>
        /// <returns>The thrown exception, or null when the delegate completed normally.</returns>
        internal static Exception RunOnThread(ApartmentState apartment, Action action)
        {
            Exception captured = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    captured = ex;
                }
            });
            thread.IsBackground = true;
            thread.SetApartmentState(apartment);
            thread.Start();
            thread.Join();
            return captured;
        }

        /// <summary>Starts an STA thread that waits on the gate, then calls <c>UiThread.Init()</c>.</summary>
        /// <param name="gate">The gate both racers wait on before calling.</param>
        /// <returns>The started thread, for the caller to join.</returns>
        internal static Thread StartStaInitWaiter(ManualResetEventSlim gate)
        {
            var thread = new Thread(() =>
            {
                gate.Wait();
                try
                {
                    UiThread.Init();
                }
                catch (InvalidOperationException)
                {
                    // The measured quantity is the factory invocation count, not a racer's outcome.
                }
            });
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return thread;
        }
    }

    /// <summary>
    /// Owns a dedicated STA thread running a real dispatcher frame, and shuts it down on disposal.
    /// </summary>
    /// <remarks>
    /// A dedicated thread is required rather than the pooled worker's ambient dispatcher, which
    /// would never be shut down. See the fuller remarks on the copy in
    /// <c>UiThread_Dispatcher_Tests</c>.
    /// </remarks>
    internal sealed class SharedStaDispatcherHost : IDisposable
    {
        private readonly AutoResetEvent _ready = new AutoResetEvent(false);
        private readonly Thread _thread;

        internal SharedStaDispatcherHost()
        {
            _thread = new Thread(() =>
            {
                Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                _ready.Set();
                System.Windows.Threading.Dispatcher.Run();
            });
            _thread.IsBackground = true;
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
            _ready.WaitOne();
        }

        /// <summary>Gets the dispatcher captured on the owned STA thread.</summary>
        internal Dispatcher Dispatcher { get; private set; }

        public void Dispose()
        {
            Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            _thread.Join();
            _ready.Dispose();
        }
    }

    /// <summary>
    /// Regression coverage for issue #787 (AC1): <c>UiThread.Init()</c> must reject a caller whose
    /// apartment state is not <see cref="ApartmentState.STA"/>, before it mutates any global.
    /// </summary>
    /// <remarks>
    /// The boundary is <c>== STA</c> rather than <c>!= MTA</c>.
    /// <see cref="ApartmentState.Unknown"/> is not directly constructible under MSTest on this
    /// host and is recorded as untested rather than asserted; the equality-shaped boundary rejects
    /// it without a case that produces it. MSTest's default apartment here is MTA, so a plain
    /// <c>[TestMethod]</c> is the rejection case and <c>[STATestMethod]</c> the acceptance case.
    /// </remarks>
    [TestClass]
    [DoNotParallelize]
    public class UiThreadInitApartmentContract_Tests
    {
        [TestMethod]
        public void Init_OnMtaThread_ThrowsInvalidOperationExceptionNamingTheObservedApartmentState()
        {
            // Arrange: install a fake so a red run cannot build a real form. The Act runs on a
            // dedicated MTA thread rather than on the ambient worker, whose apartment was measured
            // to be STA when this [DoNotParallelize] class shares the serial bucket with an
            // [STATestClass]; an ambient-apartment test would then assert nothing about MTA.
            using (UiThreadStateScope.Enter())
            {
                UiThread.SyncContextFormFactory = () => new FakeUiCaptureSource();

                // Act
                Exception observed = ApartmentThreadRunner.RunOnThread(
                    ApartmentState.MTA,
                    () => UiThread.Init()
                );

                // Assert
                observed
                    .Should()
                    .BeOfType<InvalidOperationException>()
                    .Which.Message.Should()
                    .StartWith(UiThread.NonStaInitMessagePrefix)
                    .And.Contain("MTA");
            }
        }

        [TestMethod]
        public void Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged()
        {
            // Arrange: the scope reset every static, so the values it installed are the declared
            // initial values that a rejected Init() must leave untouched.
            using (UiThreadStateScope.Enter())
            {
                UiThread.SyncContextFormFactory = () => new FakeUiCaptureSource();
                var clock = new FakeTimeProvider();
                Action<LockupAttribution> callback = _ => { };

                // Act: on a dedicated MTA thread, for the reason recorded on the case above.
                Exception observed = ApartmentThreadRunner.RunOnThread(
                    ApartmentState.MTA,
                    () =>
                        UiThread.Init(
                            monitorUiThread: true,
                            onLockupDetected: callback,
                            timeProvider: clock,
                            lockupAttributionThresholdMs: 1234
                        )
                );

                // Assert: it was rejected, and the four monitoring fields and the four capture
                // fields are unchanged.
                observed.Should().BeOfType<InvalidOperationException>();
                UiThreadStateScope.MonitorUiThread.Should().BeFalse();
                UiThreadStateScope.OnLockupDetected.Should().BeNull();
                UiThreadStateScope.MonitorTimeProvider.Should().BeNull();
                UiThreadStateScope.LockupAttributionThresholdMs.Should().Be(5000);
                UiThreadStateScope.UiSyncContextField.Should().BeNull();
                UiThreadStateScope.AutoScaleFactorField.Should().BeNull();
                UiThreadStateScope.UiThreadIdField.Should().Be(-1);
                UiThreadStateScope.DispatcherField.Should().BeNull();
            }
        }

        [STATestMethod]
        public void Init_OnStaThread_DoesNotThrowAndPopulatesAllFourCaptureFields()
        {
            // Arrange
            Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.STA);
            using (UiThreadStateScope.Enter())
            using (var host = new SharedStaDispatcherHost())
            {
                FakeUiCaptureSource fake = null;
                UiThread.SyncContextFormFactory = () =>
                    fake = new FakeUiCaptureSource { DispatcherToCapture = host.Dispatcher };

                // Act
                Action act = () => UiThread.Init();

                // Assert
                act.Should().NotThrow();
                fake.Should().NotBeNull();
                UiThreadStateScope.UiSyncContextField.Should().BeSameAs(fake.UiSyncContext);
                UiThreadStateScope
                    .AutoScaleFactorField.Should()
                    .Be(FakeUiCaptureSource.DeterministicAutoScaleFactor);
                UiThreadStateScope.UiThreadIdField.Should().Be(fake.UiThreadId);
                UiThreadStateScope.DispatcherField.Should().BeSameAs(host.Dispatcher);
            }
        }

        [TestMethod]
        public void Init_ApartmentBoundaryIsStaEqualityNotMtaInequality_RejectsFromMtaAndAcceptsFromSta()
        {
            // Arrange
            using (UiThreadStateScope.Enter())
            using (var host = new SharedStaDispatcherHost())
            {
                UiThread.SyncContextFormFactory = () =>
                    new FakeUiCaptureSource { DispatcherToCapture = host.Dispatcher };

                // Act: drive the same call from both apartments.
                Exception mtaOutcome = ApartmentThreadRunner.RunOnThread(
                    ApartmentState.MTA,
                    () => UiThread.Init()
                );
                Exception staOutcome = ApartmentThreadRunner.RunOnThread(
                    ApartmentState.STA,
                    () => UiThread.Init()
                );

                // Assert: the boundary admits STA and rejects everything else, so it is an
                // equality test against STA rather than an inequality test against MTA.
                mtaOutcome.Should().BeOfType<InvalidOperationException>();
                staOutcome.Should().BeNull();
            }
        }
    }

    /// <summary>
    /// Regression coverage for issue #788 (AC2): a failed <c>Initialize()</c> must not record
    /// initialization, so a later <c>UiThread.Init()</c> from an STA caller retries and succeeds.
    /// </summary>
    /// <remarks>
    /// The class is <c>[STATestClass]</c> because <c>Initialize()</c> must succeed here. The
    /// anti-regression assertion is a factory invocation count, never a wall-clock duration: a
    /// duration assertion would be a timing hack and is prohibited by repository policy.
    /// </remarks>
    [STATestClass]
    [DoNotParallelize]
    public class UiThreadInitRetryContract_Tests
    {
        [TestMethod]
        public void Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields()
        {
            // Arrange: the first factory fails during capture, the second succeeds.
            using (UiThreadStateScope.Enter())
            using (var host = new SharedStaDispatcherHost())
            {
                UiThread.SyncContextFormFactory = () =>
                    new FakeUiCaptureSource { ThrowOnCapture = true };
                Action failing = () => UiThread.Init();
                failing.Should().Throw<InvalidOperationException>();

                FakeUiCaptureSource working = null;
                UiThread.SyncContextFormFactory = () =>
                    working = new FakeUiCaptureSource { DispatcherToCapture = host.Dispatcher };

                // Act
                Action retry = () => UiThread.Init();

                // Assert: the retry ran Initialize() and captured all four values.
                retry.Should().NotThrow();
                working.Should().NotBeNull();
                UiThreadStateScope.UiSyncContextField.Should().BeSameAs(working.UiSyncContext);
                UiThreadStateScope
                    .AutoScaleFactorField.Should()
                    .Be(FakeUiCaptureSource.DeterministicAutoScaleFactor);
                UiThreadStateScope.UiThreadIdField.Should().Be(working.UiThreadId);
                UiThreadStateScope.DispatcherField.Should().BeSameAs(host.Dispatcher);
            }
        }

        [TestMethod]
        public void Init_WhenInitializeThrows_LeavesAllFourCaptureFieldsUnset()
        {
            // Arrange
            using (UiThreadStateScope.Enter())
            {
                UiThread.SyncContextFormFactory = () =>
                    new FakeUiCaptureSource { ThrowOnCapture = true };

                // Act
                Action act = () => UiThread.Init();

                // Assert: the exception propagates and nothing was captured.
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage(FakeUiCaptureSource.CaptureFailureMessage);
                UiThreadStateScope.UiSyncContextField.Should().BeNull();
                UiThreadStateScope.AutoScaleFactorField.Should().BeNull();
                UiThreadStateScope.UiThreadIdField.Should().Be(-1);
                UiThreadStateScope.DispatcherField.Should().BeNull();
            }
        }

        [TestMethod]
        public void AutoScaleFactor_ReadFromMtaThreadAfterAFailedInit_ThrowsAndDoesNotReEnterTheFactory()
        {
            // Arrange: fail the first Init(), then record how many capture objects were built.
            using (UiThreadStateScope.Enter())
            {
                FakeUiCaptureSource.ResetConstructionCount();
                UiThread.SyncContextFormFactory = () =>
                    new FakeUiCaptureSource { ThrowOnCapture = true };
                Action failing = () => UiThread.Init();
                failing.Should().Throw<InvalidOperationException>();
                int countAfterFailedInit = FakeUiCaptureSource.ConstructionCount;

                // Act: read the lazy accessor from an MTA thread, where AC1 must reject it.
                Exception observed = ApartmentThreadRunner.RunOnThread(
                    ApartmentState.MTA,
                    () => _ = UiThread.AutoScaleFactor
                );

                // Assert: it threw the apartment exception and built no further capture object,
                // which is the #782 retry storm expressed as an invocation count.
                observed
                    .Should()
                    .BeOfType<InvalidOperationException>()
                    .Which.Message.Should()
                    .StartWith(UiThread.NonStaInitMessagePrefix);
                FakeUiCaptureSource.ConstructionCount.Should().Be(countAfterFailedInit);
            }
        }

        [TestMethod]
        public void Init_CalledConcurrentlyFromTwoStaThreads_InvokesTheFactoryExactlyOnce()
        {
            // Arrange
            using (UiThreadStateScope.Enter())
            using (var host = new SharedStaDispatcherHost())
            using (var gate = new ManualResetEventSlim(false))
            {
                FakeUiCaptureSource.ResetConstructionCount();
                UiThread.SyncContextFormFactory = () =>
                    new FakeUiCaptureSource { DispatcherToCapture = host.Dispatcher };

                // Act: two STA callers race the first initialization behind one gate.
                Thread first = ApartmentThreadRunner.StartStaInitWaiter(gate);
                Thread second = ApartmentThreadRunner.StartStaInitWaiter(gate);
                gate.Set();
                first.Join();
                second.Join();

                // Assert: exactly one of them was admitted into Initialize().
                FakeUiCaptureSource.ConstructionCount.Should().Be(1);
            }
        }

        [TestMethod]
        public void Init_WithMonitorUiThreadEnabled_ConstructsAndRunsTheThreadMonitorWithTheInjectedTimeProvider()
        {
            // Arrange: the fake clock is never advanced, so the timer ThreadMonitor.Run() creates
            // never fires and the test leaves no live watchdog.
            using (UiThreadStateScope.Enter())
            using (var host = new SharedStaDispatcherHost())
            {
                var clock = new FakeTimeProvider();
                UiThread.SyncContextFormFactory = () =>
                    new FakeUiCaptureSource { DispatcherToCapture = host.Dispatcher };

                // Act
                Action act = () => UiThread.Init(monitorUiThread: true, timeProvider: clock);

                // Assert: the monitor branch ran and installed a monitor.
                act.Should().NotThrow();
                UiThreadStateScope.ThreadMonitorField.Should().NotBeNull();
                UiThreadStateScope.MonitorTimeProvider.Should().BeSameAs(clock);
            }
        }

        [TestMethod]
        public void UiSyncContext_ReadWithNullBackingFieldFromStaThread_InitializesThroughTheLazyPath()
        {
            // Arrange: the scope already cleared _uiSyncContext, which is the state the lazy branch
            // of the UiSyncContext getter has never been measured in.
            using (UiThreadStateScope.Enter())
            using (var host = new SharedStaDispatcherHost())
            {
                UiThreadStateScope.UiSyncContextField.Should().BeNull();
                FakeUiCaptureSource captured = null;
                UiThread.SyncContextFormFactory = () =>
                    captured = new FakeUiCaptureSource { DispatcherToCapture = host.Dispatcher };

                // Act
                SynchronizationContext observed = UiThread.UiSyncContext;

                // Assert: the value came from the fake, so the lazy path ran Initialize().
                captured.Should().NotBeNull();
                observed.Should().BeSameAs(captured.UiSyncContext);
            }
        }
    }
}
