#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    [DoNotParallelize]
    public sealed class WpfDispatcherYieldTests
    {
        [TestMethod]
        public async Task YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield()
        {
            // Arrange: both lookups are counting delegates so the test can prove the cancellation
            // guard runs before either dispatcher lookup is consulted.
            var threadProvider = new CountingDispatcherProvider(null);
            var fallbackProvider = new CountingDispatcherProvider(null);
            var dispatcherYield = new WpfDispatcherYield(
                threadProvider.Provide,
                fallbackProvider.Provide
            );

            using (var source = new CancellationTokenSource())
            {
                source.Cancel();

                // Act / Assert
                await dispatcherYield
                    .Invoking(item => item.YieldAsync(source.Token))
                    .Should()
                    .ThrowAsync<OperationCanceledException>();
            }

            threadProvider
                .InvocationCount.Should()
                .Be(
                    0,
                    "an already-canceled token must short-circuit before the thread-affinitized dispatcher is resolved"
                );
            fallbackProvider
                .InvocationCount.Should()
                .Be(
                    0,
                    "an already-canceled token must short-circuit before the fallback dispatcher is resolved"
                );
        }

        [TestMethod]
        public async Task YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback()
        {
            // Arrange: the thread-affinitized lookup supplies a dispatcher the test itself owns.
            using (var host = new StaDispatcherHost())
            {
                var threadProvider = new CountingDispatcherProvider(host.Dispatcher);
                var fallbackProvider = new CountingDispatcherProvider(host.Dispatcher);
                var dispatcherYield = new WpfDispatcherYield(
                    threadProvider.Provide,
                    fallbackProvider.Provide
                );

                // Act
                await dispatcherYield
                    .Invoking(item => item.YieldAsync(CancellationToken.None))
                    .Should()
                    .NotThrowAsync();

                // Assert
                threadProvider
                    .InvocationCount.Should()
                    .Be(1, "the thread-affinitized dispatcher is resolved exactly once");
                fallbackProvider
                    .InvocationCount.Should()
                    .Be(
                        0,
                        "the process-global fallback must not be consulted when the calling thread already has a dispatcher"
                    );
            }
        }

        [TestMethod]
        public async Task YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher()
        {
            // Arrange: the thread-affinitized lookup returns null, so resolution must fall through
            // to the process-global provider.
            using (var host = new StaDispatcherHost())
            {
                var threadProvider = new CountingDispatcherProvider(null);
                var fallbackProvider = new CountingDispatcherProvider(host.Dispatcher);
                var dispatcherYield = new WpfDispatcherYield(
                    threadProvider.Provide,
                    fallbackProvider.Provide
                );

                // Act
                await dispatcherYield
                    .Invoking(item => item.YieldAsync(CancellationToken.None))
                    .Should()
                    .NotThrowAsync();

                // Assert
                threadProvider
                    .InvocationCount.Should()
                    .Be(1, "the thread-affinitized dispatcher is always tried first");
                fallbackProvider
                    .InvocationCount.Should()
                    .Be(
                        1,
                        "the fallback is consulted exactly once when the calling thread has no dispatcher"
                    );
            }
        }

        [TestMethod]
        public async Task YieldAsync_WithoutDispatcher_RemainsStrict()
        {
            // Arrange: the dispatcher-free precondition is arranged explicitly. Both lookups return
            // null, so the outcome cannot depend on which pooled thread this test runs on, on test
            // execution order, or on whether UiThread.Init() ran earlier in the process.
            var threadProvider = new CountingDispatcherProvider(null);
            var fallbackProvider = new CountingDispatcherProvider(null);
            var dispatcherYield = new WpfDispatcherYield(
                threadProvider.Provide,
                fallbackProvider.Provide
            );

            // Act / Assert
            await dispatcherYield
                .Invoking(item => item.YieldAsync(CancellationToken.None))
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*UiThread.Init()*");

            threadProvider
                .InvocationCount.Should()
                .Be(1, "the thread-affinitized dispatcher is always tried first");
            fallbackProvider
                .InvocationCount.Should()
                .Be(1, "the fallback is tried before the strict contract is enforced");
        }

        /// <summary>
        /// Pins the production resolution path rather than an injected one. The yielder is built
        /// through its public parameterless constructor, so its fallback lookup is the real
        /// process-global accessor; the process-global value is uninstalled for the duration of
        /// the Act, so that accessor is exercised in its uncaptured state and must surface the
        /// shared guard message naming the public initialization entry point.
        ///
        /// The Act runs on a dedicated fresh thread rather than on the MSTest worker. On a pooled
        /// worker, <c>Dispatcher.FromThread</c> returns a non-null instance if any earlier test on
        /// that same thread ever resolved the thread's dispatcher; the thread-affinitized provider
        /// would then win and the fallback under test would never run. The class-level
        /// <c>[DoNotParallelize]</c> serializes the write to the process-global static but cannot
        /// supply thread freshness, so both are required.
        /// </summary>
        [TestMethod]
        public void YieldAsync_ProductionFallbackWithoutDispatcher_ThrowsNamingInit()
        {
            // Arrange
            var dispatcherYield = new WpfDispatcherYield();
            Exception? observed = null;

            using (UiThreadDispatcherScope.InstallNull())
            {
                var worker = new Thread(() =>
                {
                    try
                    {
                        dispatcherYield.YieldAsync(CancellationToken.None).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        observed = ex;
                    }
                });
                worker.IsBackground = true;

                // Act: the worker is joined inside the scope so the uninstalled state is still in
                // force for the whole of its run.
                worker.Start();
                worker.Join();
            }

            // Assert. A null capture means the Act completed without throwing, which the type
            // assertion reports directly, so the null-forgiving operator here loses no diagnostic.
            Exception observedException = observed!;
            observedException
                .Should()
                .BeOfType<InvalidOperationException>(
                    "the production fallback must surface the uncaptured-dispatcher guard"
                );
            observedException.Message.Should().Contain("UiThread.Init()");
        }

        /// <summary>
        /// Records how many times the seam consulted a dispatcher lookup and what that lookup
        /// returned, so tests can pin the resolution order rather than only the outcome.
        /// </summary>
        private sealed class CountingDispatcherProvider
        {
            private readonly Dispatcher? _dispatcher;
            private int _invocationCount;

            public CountingDispatcherProvider(Dispatcher? dispatcher)
            {
                _dispatcher = dispatcher;
            }

            public int InvocationCount => _invocationCount;

            public Dispatcher? Provide()
            {
                _invocationCount++;
                return _dispatcher;
            }
        }

        /// <summary>
        /// Owns a pumping STA thread whose dispatcher the tests can yield through. The dispatcher
        /// must genuinely pump, because a background-priority operation posted to a non-pumping
        /// dispatcher never completes.
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

            public Dispatcher Dispatcher { get; private set; } = null!;

            public void Dispose()
            {
                Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                _thread.Join();
                _ready.Dispose();
            }
        }
    }
}
