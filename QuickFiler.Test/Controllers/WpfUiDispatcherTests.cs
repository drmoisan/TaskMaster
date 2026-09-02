using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Construction smoke test (cycle-2 Phase 6, P6-T1/P6-T12) plus, as of cycle-3 P9-T7, a
    /// live-dispatcher forwarding-body test for the production <see cref="WpfUiDispatcher"/>. The
    /// adapter forwards to the static <see cref="UtilitiesCS.UiThread.Dispatcher"/>; exercising it
    /// against a real, running WPF <see cref="Dispatcher"/> hosted on a dedicated STA thread (the
    /// same in-process, no-external-dependency technique already proven for
    /// <c>AssignControlsAsync</c>) requires no live WinForms/WPF application host.
    /// </summary>
    [TestClass]
    public class WpfUiDispatcherTests
    {
        private const int GateTimeoutMs = 60000;

        [TestMethod]
        public void Construction_YieldsAnIUiDispatcher()
        {
            IUiDispatcher dispatcher = new WpfUiDispatcher();

            dispatcher.Should().NotBeNull();
            dispatcher.Should().BeAssignableTo<IUiDispatcher>();
        }

        /// <summary>
        /// Cycle-3 P9-T7 (member #39, de-exempted): asserts that <c>Invoke</c>, <c>InvokeAsync</c>, and
        /// <c>BeginInvoke</c> each execute the supplied delegate on the dispatcher's own thread (not the
        /// test thread). <c>BeginInvoke</c> is fire-and-forget, so its completion is observed
        /// deterministically via a <see cref="ManualResetEventSlim"/> signal rather than polling.
        /// <para>
        /// Issue #648: the swap of the process-wide static <c>UtilitiesCS.UiThread._dispatcher</c> is
        /// routed through <see cref="UiThreadDispatcherFixture"/>, which is the single owner of that
        /// mutation for this assembly's owned files, rather than performed by raw reflection here.
        /// The gate is awaited, so the method is declared <c>async Task</c>, and it carries the same
        /// 60-second timeout the sibling issue #493 regression tests use so a genuine deadlock
        /// becomes a test failure rather than a hung run. The restore is
        /// <see cref="UiThreadDispatcherTransaction.Dispose"/>, which restores conditionally by
        /// reference comparison and then releases the gate, in that order.
        /// </para>
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread()
        {
            // Arrange
            Dispatcher dispatcher = QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                // Split across two statements so the qualified call stays on one line: CSharpier
                // wraps the single-expression form into a three-line member chain at this indent.
                Task<UiThreadDispatcherTransaction> gate =
                    UiThreadDispatcherFixture.BeginTransactionAsync();
                UiThreadDispatcherTransaction transaction = await gate.ConfigureAwait(false);
                try
                {
                    transaction.Install(dispatcher);
                    WpfUiDispatcher sut = new WpfUiDispatcher();
                    int dispatcherThreadId = dispatcher.Thread.ManagedThreadId;

                    // Act / Assert — Invoke (blocking, synchronous marshal)
                    int invokeThreadId = -1;
                    sut.Invoke(() => invokeThreadId = Thread.CurrentThread.ManagedThreadId);
                    invokeThreadId.Should().Be(dispatcherThreadId);

                    // Act / Assert — InvokeAsync
                    int invokeAsyncThreadId = -1;
                    Task invokeAsyncTask = sut.InvokeAsync(() =>
                        invokeAsyncThreadId = Thread.CurrentThread.ManagedThreadId
                    );
                    invokeAsyncTask.GetAwaiter().GetResult();
                    invokeAsyncThreadId.Should().Be(dispatcherThreadId);

                    // Act / Assert — BeginInvoke (fire-and-forget; observed deterministically via a signal)
                    int beginInvokeThreadId = -1;
                    using (ManualResetEventSlim signal = new ManualResetEventSlim(false))
                    {
                        sut.BeginInvoke(() =>
                        {
                            beginInvokeThreadId = Thread.CurrentThread.ManagedThreadId;
                            signal.Set();
                        });
                        signal.Wait();
                    }
                    beginInvokeThreadId.Should().Be(dispatcherThreadId);
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            finally
            {
                QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher);
            }
        }
    }
}
