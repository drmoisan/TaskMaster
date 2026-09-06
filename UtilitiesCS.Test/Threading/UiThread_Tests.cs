using System;
using System.Threading;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Threading
{
    [TestClass]
    public class SynchronizationContextAwaiter_Tests
    {
        [TestMethod]
        public void Constructor_NullContext_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new UiThread.SynchronizationContextAwaiter(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void IsCompleted_WhenContextIsNotCurrent_ReturnsFalse()
        {
            // Arrange
            var context = new SynchronizationContext();
            var awaiter = new UiThread.SynchronizationContextAwaiter(context);

            // Act
            var result = awaiter.IsCompleted;

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsCompleted_WhenContextMatchesCurrent_ReturnsTrue()
        {
            // Arrange: set the thread's synchronization context to the same instance captured
            // by the awaiter so that the equality check (_context == Current) evaluates true
            var context = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                var awaiter = new UiThread.SynchronizationContextAwaiter(context);

                // Act
                var result = awaiter.IsCompleted;

                // Assert
                result.Should().BeTrue();
            }
            finally
            {
                // Restore the context so this test does not influence other test-thread tests
                SynchronizationContext.SetSynchronizationContext(null);
            }
        }

        [TestMethod]
        public void GetResult_DoesNotThrow()
        {
            // Arrange
            var context = new SynchronizationContext();
            var awaiter = new UiThread.SynchronizationContextAwaiter(context);

            // Act
            Action act = () => awaiter.GetResult();

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void OnCompleted_PostsCallbackToContext()
        {
            // Arrange
            Action postedCallback = null;
            var mockContext = new TestSynchronizationContext(cb => postedCallback = cb);
            var awaiter = new UiThread.SynchronizationContextAwaiter(mockContext);
            Action continuation = () => { };

            // Act
            awaiter.OnCompleted(continuation);

            // Assert
            postedCallback.Should().NotBeNull();
        }

        private class TestSynchronizationContext : SynchronizationContext
        {
            private readonly Action<Action> _onPost;

            public TestSynchronizationContext(Action<Action> onPost)
            {
                _onPost = onPost;
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                _onPost?.Invoke((Action)state);
            }
        }
    }

    /// <summary>
    /// Regression coverage for issue #584: the accessor contract of
    /// <c>UiThread.Dispatcher</c>.
    ///
    /// Purpose:
    ///     Both tests drive the accessor through the shared <c>UiThreadDispatcherScope</c> install
    ///     scope, which writes the private static <c>UiThread._dispatcher</c> backing field for the
    ///     lifetime of a <c>using</c> statement and restores the prior value on disposal. The
    ///     property has a private setter whose only production writer is the hidden WinForms window
    ///     that <c>UiThread.Init()</c> shows, so the backing field is the one seam that lets a unit
    ///     test place the accessor in each of its two states. Driving the contract through that
    ///     seam makes both tests deterministic without any timing construct.
    ///
    ///     Reflection remains necessary because <c>InternalsVisibleTo</c> exposes internal members
    ///     only and does not expose private ones. It is centralised in the scope rather than
    ///     repeated here.
    ///
    ///     The accessor's contract after PR #778 is that it throws
    ///     <see cref="System.InvalidOperationException"/> synchronously when the field is null,
    ///     rather than returning null, and the exception message names <c>UiThread.Init()</c> as
    ///     the entry point a caller must invoke on the UI thread during host startup.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class UiThread_Dispatcher_Tests
    {
        [TestMethod]
        public void Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize()
        {
            // Arrange
            using (UiThreadDispatcherScope.InstallNull())
            {
                // Act
                Action act = () => _ = UiThread.Dispatcher;

                // Assert
                act.Should().Throw<InvalidOperationException>().WithMessage("*UiThread.Init()*");
            }
        }

        [TestMethod]
        public void Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance()
        {
            // Arrange: establish a known null prior explicitly rather than relying on the ambient
            // value. QfcHomeControllerRunAsyncTests calls UiThread.Init(false), which populates the
            // same process-global static, and QuickFiler.Test and UtilitiesCS.Test run in a single
            // vstest invocation, so an ambient non-null prior would be restored by the inner
            // disposal and the round-trip assertion below would fail for a reason outside this
            // delivery.
            using (UiThreadDispatcherScope.InstallNull())
            using (var host = new StaDispatcherHost())
            {
                var expected = host.Dispatcher;

                using (UiThreadDispatcherScope.Install(expected))
                {
                    // Act / Assert
                    UiThread.Dispatcher.Should().BeSameAs(expected);
                }

                // Assert: the inner scope restored the null prior it captured.
                UiThreadDispatcherScope.Current.Should().BeNull();
            }
        }

        /// <summary>
        /// Owns a dedicated STA thread and exposes the dispatcher captured on it, modelled on the
        /// <c>StaDispatcherHost</c> in
        /// <c>UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs</c>.
        /// </summary>
        /// <remarks>
        /// A dedicated thread is required rather than resolving the ambient current dispatcher on
        /// the pooled MSTest worker (C10). Resolving it there creates a dispatcher that is never
        /// shut down and that outlives the test, which a later test running on that same pooled
        /// thread can then observe. The host is constructed inside a <c>using</c> statement so that
        /// <c>BeginInvokeShutdown</c> and the thread join run on every exit path, including a
        /// failing assertion.
        /// </remarks>
        private sealed class StaDispatcherHost : IDisposable
        {
            private readonly AutoResetEvent _ready = new AutoResetEvent(false);
            private readonly Thread _thread;

            public StaDispatcherHost()
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

            public Dispatcher Dispatcher { get; private set; }

            public void Dispose()
            {
                Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                _thread.Join();
                _ready.Dispose();
            }
        }
    }
}
