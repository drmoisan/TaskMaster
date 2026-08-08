using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace QuickFiler.Test.TestSupport
{
    /// <summary>
    /// Self-tests for <see cref="WinFormsPumpHost"/>. Beyond verifying the seam, this file is the
    /// worked example of the host's usage contract: construct one host per test, run work through
    /// the <see cref="Task"/>-returning members, observe failures as faulted awaited tasks or as
    /// <c>StopAsync</c> errors, and always release the host in <c>finally</c>/<c>using</c>.
    /// </summary>
    /// <remarks>
    /// The MSTest <c>[Timeout]</c> attributes are a harness bound (in-repo precedent
    /// <c>TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs</c>), not a wall-clock wait in test
    /// logic: every wait in these tests is on a deterministic completion signal.
    /// </remarks>
    [TestClass]
    public class WinFormsPumpHostTests
    {
        private const int TimeoutMs = 30000;

        /// <summary>
        /// The constructor must return a live, fully-captured host: a non-null WinForms
        /// synchronization context bound to a pump thread that is not the MSTest thread.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public void Constructor_WhenHostStarts_CapturesWinFormsContextOnADistinctThread()
        {
            // Arrange / Act
            using (var host = new WinFormsPumpHost())
            {
                // Assert
                host.SyncContext.Should()
                    .NotBeNull(
                        because: "the pump thread installs its context before signalling ready"
                    );
                host.SyncContext.Should()
                    .BeOfType<System.Windows.Forms.WindowsFormsSynchronizationContext>(
                        because: "the seam exists to drain WinForms-marshalled continuations"
                    );
                host.ThreadId.Should()
                    .NotBe(
                        Thread.CurrentThread.ManagedThreadId,
                        because: "the pump must run on its own dedicated thread"
                    );
            }
        }

        /// <summary>
        /// Synchronous work handed to <c>InvokeAsync(Action)</c> must execute on the pump thread.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public async Task InvokeAsyncAction_WhenPosted_RunsOnThePumpThread()
        {
            // Arrange
            var host = new WinFormsPumpHost();
            try
            {
                int observedThreadId = 0;

                // Act
                await host.InvokeAsync(() =>
                        observedThreadId = Thread.CurrentThread.ManagedThreadId
                    )
                    .ConfigureAwait(false);

                // Assert
                observedThreadId.Should().Be(host.ThreadId);
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The generic <c>InvokeAsync</c> overload must run its factory on the pump thread and
        /// return the produced value to the awaiting test thread.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public async Task InvokeAsyncFactory_WhenPosted_RunsOnThePumpThreadAndReturnsTheValue()
        {
            // Arrange
            var host = new WinFormsPumpHost();
            try
            {
                // Act
                int producedThreadId = await host.InvokeAsync(() =>
                        Thread.CurrentThread.ManagedThreadId
                    )
                    .ConfigureAwait(false);

                // Assert
                producedThreadId.Should().Be(host.ThreadId);
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronous work started through <c>RunAsync(Func&lt;Task&gt;)</c> must both start and
        /// resume (after an inner await) on the pump thread.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public async Task RunAsyncVoid_WhenPosted_StartsAndResumesOnThePumpThread()
        {
            // Arrange
            var host = new WinFormsPumpHost();
            try
            {
                int startThreadId = 0;
                int resumeThreadId = 0;

                // Act
                await host.RunAsync(async () =>
                    {
                        startThreadId = Thread.CurrentThread.ManagedThreadId;
                        await Task.Yield();
                        resumeThreadId = Thread.CurrentThread.ManagedThreadId;
                    })
                    .ConfigureAwait(false);

                // Assert
                startThreadId.Should().Be(host.ThreadId);
                resumeThreadId
                    .Should()
                    .Be(
                        host.ThreadId,
                        because: "the pump drains the continuation posted back to its context"
                    );
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The generic <c>RunAsync</c> overload must run on the pump thread and unwrap its result.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public async Task RunAsyncResult_WhenPosted_RunsOnThePumpThreadAndReturnsTheValue()
        {
            // Arrange
            var host = new WinFormsPumpHost();
            try
            {
                // Act
                int producedThreadId = await host.RunAsync(async () =>
                    {
                        await Task.Yield();
                        return Thread.CurrentThread.ManagedThreadId;
                    })
                    .ConfigureAwait(false);

                // Assert
                producedThreadId.Should().Be(host.ThreadId);
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Awaiting <c>host.SyncContext</c> from the MSTest thread (the exact pattern production
        /// code uses via <c>await itemViewer.UiSyncContext</c>) must move the continuation onto the
        /// pump thread. Without the pump this await never completes.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public async Task AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread()
        {
            // Arrange
            var host = new WinFormsPumpHost();
            try
            {
                // Act
                await host.SyncContext;
                int resumedThreadId = Thread.CurrentThread.ManagedThreadId;

                // Assert
                resumedThreadId
                    .Should()
                    .Be(
                        host.ThreadId,
                        because: "UiThread.GetAwaiter posts the continuation to the WinForms context"
                    );
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Interop smoke test: proves both marshal routes onto the pump thread work in this
        /// environment before any controller test depends on them. Route 1 is
        /// <c>await host.SyncContext</c>; route 2 is a WPF
        /// <see cref="System.Windows.Threading.Dispatcher"/> created on the pump thread and driven
        /// from the test thread via <c>Dispatcher.FromThread(pump).InvokeAsync</c>. The WPF
        /// dispatcher has no frame of its own here — it is serviced entirely by the WinForms
        /// message loop, which is exactly what <c>Initialize(bool)</c>'s tail relies on.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public async Task BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread()
        {
            // Arrange: create the WPF dispatcher on the pump thread, then look it up by thread.
            var host = new WinFormsPumpHost();
            try
            {
                Thread pumpThread = await host.InvokeAsync(() =>
                    {
                        System.Windows.Threading.Dispatcher.CurrentDispatcher.Should().NotBeNull();
                        return Thread.CurrentThread;
                    })
                    .ConfigureAwait(false);
                pumpThread.ManagedThreadId.Should().Be(host.ThreadId);

                System.Windows.Threading.Dispatcher pumpDispatcher =
                    System.Windows.Threading.Dispatcher.FromThread(pumpThread);
                pumpDispatcher
                    .Should()
                    .NotBeNull(
                        because: "ItemViewer's constructor lazily creates this dispatcher on the pump"
                    );

                // Act
                int dispatcherThreadId = await pumpDispatcher.InvokeAsync(() =>
                    Thread.CurrentThread.ManagedThreadId
                );

                await host.SyncContext;
                int syncContextThreadId = Thread.CurrentThread.ManagedThreadId;

                // Assert
                dispatcherThreadId
                    .Should()
                    .Be(
                        host.ThreadId,
                        because: "a WPF dispatcher is serviced by any Win32 loop on its thread"
                    );
                syncContextThreadId.Should().Be(host.ThreadId);
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Negative flow: a synchronous throw inside <c>InvokeAsync</c> must fault the awaited task
        /// with the original exception type and message, and must not disturb the pump — the host
        /// still stops cleanly afterwards.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public async Task InvokeAsync_WhenWorkThrows_FaultsTheAwaitedTaskWithTheOriginalException()
        {
            // Arrange
            var host = new WinFormsPumpHost();
            try
            {
                // Act
                Func<Task> act = () =>
                    host.InvokeAsync(() =>
                    {
                        throw new InvalidTimeZoneException("sync-throw-marker");
                    });

                // Assert
                await act.Should()
                    .ThrowAsync<InvalidTimeZoneException>()
                    .WithMessage("sync-throw-marker")
                    .ConfigureAwait(false);
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Negative flow: an async fault inside <c>RunAsync(Func&lt;Task&gt;)</c> must surface the
        /// original exception unwrapped (not an <see cref="AggregateException"/>) on the awaited
        /// task.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public async Task RunAsyncVoid_WhenWorkFaults_SurfacesTheOriginalUnwrappedException()
        {
            // Arrange
            var host = new WinFormsPumpHost();
            try
            {
                // Act
                Func<Task> act = () =>
                    host.RunAsync(async () =>
                    {
                        await Task.Yield();
                        throw new InvalidTimeZoneException("async-void-fault-marker");
                    });

                // Assert
                await act.Should()
                    .ThrowAsync<InvalidTimeZoneException>()
                    .WithMessage("async-void-fault-marker")
                    .ConfigureAwait(false);
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Negative flow: the generic <c>RunAsync</c> overload propagates faults with the same
        /// unwrapped identity as the non-generic overload.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public async Task RunAsyncResult_WhenWorkFaults_SurfacesTheOriginalUnwrappedException()
        {
            // Arrange
            var host = new WinFormsPumpHost();
            try
            {
                // Act
                Func<Task> act = () =>
                    host.RunAsync<int>(async () =>
                    {
                        await Task.Yield();
                        throw new InvalidTimeZoneException("async-result-fault-marker");
                    });

                // Assert
                await act.Should()
                    .ThrowAsync<InvalidTimeZoneException>()
                    .WithMessage("async-result-fault-marker")
                    .ConfigureAwait(false);
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Edge case: after the host has stopped, posting members fail fast rather than queueing
        /// work to a dead message loop. The returned task faults with
        /// <see cref="ObjectDisposedException"/> for every posting member.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public async Task PostingMembers_AfterStop_FaultWithObjectDisposedException()
        {
            // Arrange
            var host = new WinFormsPumpHost();
            await host.StopAsync().ConfigureAwait(false);

            // Act
            Func<Task> invokeAction = () => host.InvokeAsync(() => { });
            Func<Task> invokeFactory = () => host.InvokeAsync(() => 1);
            Func<Task> runVoid = () => host.RunAsync(() => Task.CompletedTask);
            Func<Task> runResult = () => host.RunAsync(() => Task.FromResult(1));

            // Assert
            await invokeAction.Should().ThrowAsync<ObjectDisposedException>().ConfigureAwait(false);
            await invokeFactory
                .Should()
                .ThrowAsync<ObjectDisposedException>()
                .ConfigureAwait(false);
            await runVoid.Should().ThrowAsync<ObjectDisposedException>().ConfigureAwait(false);
            await runResult.Should().ThrowAsync<ObjectDisposedException>().ConfigureAwait(false);
        }

        /// <summary>
        /// Edge case: <see cref="WinFormsPumpHost.Dispose"/> is idempotent — the second call is a
        /// no-op and does not throw or attempt a second shutdown.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public void Dispose_CalledTwice_IsANoOp()
        {
            // Arrange
            var host = new WinFormsPumpHost();

            // Act
            host.Dispose();
            Action secondDispose = () => host.Dispose();

            // Assert
            secondDispose.Should().NotThrow();
        }

        /// <summary>
        /// Error handling: a stray exception raised inside the message loop (a raw context post
        /// that the host does not wrap) is captured by the <c>Application.ThreadException</c>
        /// recorder and rethrown by <c>StopAsync</c>, so a quiet pump-thread failure becomes a test
        /// failure at the disposal point instead of being swallowed.
        /// </summary>
        [TestMethod]
        [Timeout(TimeoutMs)]
        public async Task StopAsync_WhenThePumpLoopRecordedAnException_RethrowsIt()
        {
            // Arrange
            var host = new WinFormsPumpHost();
            var raised = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

            // Act: post a raw callback that throws inside the loop, bypassing the host's wrapper.
            host.SyncContext.Post(
                delegate
                {
                    raised.TrySetResult(true);
                    throw new InvalidTimeZoneException("pump-loop-marker");
                },
                null
            );
            await raised.Task.ConfigureAwait(false);
            Func<Task> stop = () => host.StopAsync();

            // Assert
            await stop.Should()
                .ThrowAsync<InvalidTimeZoneException>()
                .WithMessage("pump-loop-marker")
                .ConfigureAwait(false);
        }
    }
}
