using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Threading
{
    [TestClass]
    [DoNotParallelize]
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

        [TestMethod]
        public void IsCompleted_WhenAmbientContextIsTheCapturedInstance_ReturnsTrue()
        {
            // Arrange: the ambient context is the very instance the awaiter captured.
            using (UiThreadStateScope.Enter())
            {
                var context = new SynchronizationContext();
                SynchronizationContext prior = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(context);
                try
                {
                    // Act
                    bool result = new UiThread.SynchronizationContextAwaiter(context).IsCompleted;

                    // Assert: the reference fast path admits it before any static is read.
                    result.Should().BeTrue();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(prior);
                }
            }
        }

        [TestMethod]
        public void IsCompleted_WhenAmbientContextIsNullAndCapturedContextIsNotNull_ReturnsFalse()
        {
            // Arrange: no ambient context, so there is nothing to resume onto. Continuing inline
            // here would break TaskScheduler.FromCurrentSynchronizationContext() at the two
            // WebView2 setup sites in QfcItemController.ViewerSetup and EfcItemController.
            using (UiThreadStateScope.Enter())
            {
                SynchronizationContext prior = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);
                try
                {
                    // Act
                    bool result = new UiThread.SynchronizationContextAwaiter(
                        new SynchronizationContext()
                    ).IsCompleted;

                    // Assert
                    result.Should().BeFalse();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(prior);
                }
            }
        }

        [TestMethod]
        public void IsCompleted_WhenUiThreadIdIsTheMinusOneSentinel_ReturnsFalse()
        {
            // Arrange: the scope reset _uiThreadId to the pre-Init() sentinel.
            using (UiThreadStateScope.Enter())
            {
                UiThreadStateScope.UiThreadIdField.Should().Be(-1);
                var ambient = new SynchronizationContext();
                SynchronizationContext prior = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(ambient);
                try
                {
                    // Act
                    bool result = new UiThread.SynchronizationContextAwaiter(
                        new SynchronizationContext()
                    ).IsCompleted;

                    // Assert
                    result.Should().BeFalse();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(prior);
                }
            }
        }

        [TestMethod]
        public void IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue()
        {
            // Arrange: make the host thread the owning UI thread and its dispatcher the UI one.
            using (UiThreadStateScope.Enter())
            using (var host = new StaDispatcherHost())
            {
                UiThreadStateScope.SetUiThreadId(
                    host.Dispatcher.Invoke(() => Thread.CurrentThread.ManagedThreadId)
                );
                UiThreadStateScope.SetDispatcher(host.Dispatcher);

                // Act: capture the dispatcher context inside an Invoke, restore a different
                // ambient, and evaluate on that same host thread outside any dispatcher operation.
                bool result = host.Dispatcher.Invoke(() =>
                {
                    SynchronizationContext captured = SynchronizationContext.Current;
                    SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                    try
                    {
                        return new UiThread.SynchronizationContextAwaiter(captured).IsCompleted;
                    }
                    finally
                    {
                        SynchronizationContext.SetSynchronizationContext(captured);
                    }
                });

                // Assert: a dispatcher context is UI-owned on the thread whose dispatcher is the
                // UI dispatcher, so the continuation may run inline.
                result.Should().BeTrue();
            }
        }

        [TestMethod]
        public void IsCompleted_WhenTheDispatcherContextBelongsToADifferentThreadsDispatcher_ReturnsFalse()
        {
            // Arrange: the evaluating thread owns the UI thread id but a different host owns the
            // UI dispatcher, so this thread's dispatcher is not the UI dispatcher.
            using (UiThreadStateScope.Enter())
            using (var owner = new StaDispatcherHost())
            using (var other = new StaDispatcherHost())
            {
                SynchronizationContext foreignDispatcherContext = other.Dispatcher.Invoke(() =>
                    SynchronizationContext.Current
                );
                UiThreadStateScope.SetDispatcher(other.Dispatcher);

                // Act
                bool result = owner.Dispatcher.Invoke(() =>
                {
                    UiThreadStateScope.SetUiThreadId(Thread.CurrentThread.ManagedThreadId);
                    SynchronizationContext captured = SynchronizationContext.Current;
                    SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                    try
                    {
                        return new UiThread.SynchronizationContextAwaiter(
                            foreignDispatcherContext
                        ).IsCompleted;
                    }
                    finally
                    {
                        SynchronizationContext.SetSynchronizationContext(captured);
                    }
                });

                // Assert
                result.Should().BeFalse();
            }
        }

        [TestMethod]
        public void IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse()
        {
            // Arrange: a WindowsFormsSynchronizationContext that is neither the captured UI context
            // nor a dispatcher context, evaluated while the owning thread id does match. This pins
            // the reason a bare owning-thread-identity predicate was rejected, and guards the
            // WinFormsPumpHostTests failure mode.
            using (UiThreadStateScope.Enter())
            using (var host = new StaDispatcherHost())
            {
                // Act
                bool result = host.Dispatcher.Invoke(() =>
                {
                    UiThreadStateScope.SetUiThreadId(Thread.CurrentThread.ManagedThreadId);
                    using (var foreign = new WindowsFormsSynchronizationContext())
                    {
                        SynchronizationContext captured = SynchronizationContext.Current;
                        SynchronizationContext.SetSynchronizationContext(
                            new SynchronizationContext()
                        );
                        try
                        {
                            return new UiThread.SynchronizationContextAwaiter(foreign).IsCompleted;
                        }
                        finally
                        {
                            SynchronizationContext.SetSynchronizationContext(captured);
                        }
                    }
                });

                // Assert
                result.Should().BeFalse();
            }
        }

        [TestMethod]
        public void IsCompleted_OnDefaultAwaiterOnAContextFreeThread_ReturnsTrue()
        {
            // Arrange: the default instance has a null captured context, and this thread has none.
            using (UiThreadStateScope.Enter())
            {
                SynchronizationContext prior = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);
                try
                {
                    // Act
                    bool result = default(UiThread.SynchronizationContextAwaiter).IsCompleted;

                    // Assert: unchanged from the pre-change behaviour of this default instance.
                    result.Should().BeTrue();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(prior);
                }
            }
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

        /// <summary>
        /// Owns a dedicated STA thread running a real dispatcher frame. See the fuller remarks on
        /// the copy nested in <c>UiThread_Dispatcher_Tests</c> below for why it is required.
        /// </summary>
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
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage(UiThread.DispatcherNotInitializedMessage);
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
