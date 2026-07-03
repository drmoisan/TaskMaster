using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
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
        /// </summary>
        [TestMethod]
        public void Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread()
        {
            // Arrange
            FieldInfo field = typeof(UiThread).GetField(
                "_dispatcher",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            field.Should().NotBeNull(because: "UiThread._dispatcher backing field must exist");
            object original = field.GetValue(null);
            Dispatcher dispatcher = QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                field.SetValue(null, dispatcher);
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
                field.SetValue(null, original);
                QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher);
            }
        }
    }
}
