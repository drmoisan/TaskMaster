using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Threading;
using DispatchMode = TaskMaster.Test.AppGlobals.AppOlObjectsFolderTreeServiceLifecycleTests.DispatchMode;
using Lifecycle = TaskMaster.Test.AppGlobals.AppOlObjectsFolderTreeServiceLifecycleTests;
using TreeService = UtilitiesCS.OutlookObjects.Folder.IOutlookFolderTreeService;

namespace TaskMaster.Test.AppGlobals
{
    public sealed partial class AppOlObjectsFolderTreeServiceLifecycleTests
    {
        [TestMethod]
        public Task NullDispatcherFactory_TerminalizesAndRetriesWithOneDispatcherInstance() =>
            VerifySetupFailureAndRetryAsync(SetupMode.NullDispatcher);

        [TestMethod]
        public async Task SameDispatcherReentry_TerminalizesAndPermitsRetry()
        {
            var dispatcher = new ControlledUiDispatcher(DispatchMode.Immediate);
            var sut = CreateSut(dispatcher);
            var terminal = sut.NextTerminal;
            sut.BeforeCompletion = _ =>
                GetException(() => _ = sut.FolderTreeService)
                    .Message.Should()
                    .Be("Folder tree service composition cannot reenter on its composing thread.");
            try
            {
                Action first = () => _ = sut.FolderTreeService;
                first.Should().Throw<InvalidOperationException>();
                (await GetExceptionAsync(await terminal))
                    .Should()
                    .BeOfType<InvalidOperationException>();
                sut.BeforeCompletion = null;
                sut.FolderTreeService.Should().BeSameAs(sut.Service);
                sut.LoadCount.Should().Be(2);
            }
            finally
            {
                sut.Dispose();
            }
        }

        [TestMethod]
        public Task WorkerFirst_AlreadyCanceledDispatch_PreservesCancellationToken() =>
            VerifyTerminalDispatchAsync(DispatchMode.Canceled, pending: false);

        [TestMethod]
        public Task WorkerFirst_FaultedOperationCanceledException_RemainsFaulted() =>
            VerifyTerminalDispatchAsync(DispatchMode.Faulted, pending: false);

        [TestMethod]
        public Task WorkerFirst_PendingCancellation_PreservesCancellationTokenAndSkipsCallback() =>
            VerifyTerminalDispatchAsync(DispatchMode.Canceled, pending: true);

        [TestMethod]
        public Task WorkerFirst_PendingFault_PreservesOriginalException() =>
            VerifyTerminalDispatchAsync(DispatchMode.Faulted, pending: true);

        [TestMethod]
        public async Task WorkerFirst_NullDispatchTask_ResetsOwnershipAndPermitsSingleServiceRetry()
        {
            var dispatcher = new ControlledUiDispatcher(DispatchMode.Null);
            var sut = CreateSut(dispatcher);
            var firstRun = await StartWorkerAsync(sut, dispatcher);
            try
            {
                var terminalException = await GetExceptionAsync(await firstRun.Terminal);
                var workerException = await GetExceptionAsync(firstRun.Worker);
                workerException
                    .Should()
                    .BeSameAs(terminalException)
                    .And.BeOfType<InvalidOperationException>();
                workerException
                    .Message.Should()
                    .Be("Folder tree service dispatcher returned a null task.");
                await firstRun.Operation.ReleaseAsync();
                await ObserveAsync(firstRun.Operation.Task);
                dispatcher.Mode = DispatchMode.Pending;
                var secondRun = await StartWorkerAsync(sut, dispatcher);
                await secondRun.Operation.ReleaseAsync();
                (await secondRun.Worker).Should().BeSameAs(sut.Service);
                sut.LoadCount.Should().Be(1);
                await CleanupAsync(sut, secondRun.Operation, secondRun.Worker);
            }
            finally
            {
                await CleanupAsync(sut, firstRun.Operation, firstRun.Worker);
            }
        }

        [TestMethod]
        public Task DisposeBeforeQueuedCallback_CompletesWorkerWithExactObjectDisposedException() =>
            VerifyBlockingDisposalAsync();

        [TestMethod]
        public async Task TerminalNotificationHookFailure_DoesNotReplaceDispatchFault()
        {
            var fault = new InvalidOperationException("hook containment fault");
            var dispatcher = new ControlledUiDispatcher(DispatchMode.Pending, fault: fault);
            var sut = CreateSut(dispatcher, throwFromTerminalHook: true);
            var run = await StartWorkerAsync(sut, dispatcher);
            try
            {
                dispatcher.Complete(run.Operation, DispatchMode.Faulted);
                (await GetExceptionAsync(run.Worker)).Should().BeSameAs(fault);
                await run.Operation.ReleaseAsync();
                (await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);
                sut.LoadCount.Should().Be(0);
                Volatile.Read(ref sut.InvokedTerminalHookCount).Should().Be(1);
            }
            finally
            {
                await CleanupAsync(sut, run.Operation, run.Worker);
            }
        }

        private static async Task VerifyTerminalDispatchAsync(
            DispatchMode terminalMode,
            bool pending
        )
        {
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();
            Exception fault = pending
                ? new InvalidOperationException("controlled dispatch fault")
                : new OperationCanceledException(cancellationSource.Token);
            var dispatcher = new ControlledUiDispatcher(
                pending ? DispatchMode.Pending : terminalMode,
                cancellationSource.Token,
                fault
            );
            var sut = CreateSut(dispatcher);
            var run = await StartWorkerAsync(sut, dispatcher);
            try
            {
                if (pending)
                    dispatcher.Complete(run.Operation, terminalMode);
                var terminal = await run.Terminal;
                terminal.IsCanceled.Should().Be(terminalMode == DispatchMode.Canceled);
                terminal.IsFaulted.Should().Be(terminalMode == DispatchMode.Faulted);
                if (terminalMode == DispatchMode.Canceled)
                {
                    await AssertCanceledAsync(terminal, cancellationSource.Token);
                    await AssertCanceledAsync(run.Worker, cancellationSource.Token);
                }
                else
                {
                    terminal.Exception.InnerException.Should().BeSameAs(fault);
                    (await GetExceptionAsync(run.Worker)).Should().BeSameAs(fault);
                }
                await run.Operation.ReleaseAsync();
                (sut.LoadCount, dispatcher.Invokes, dispatcher.Begins).Should().Be((0, 1, 0));
            }
            finally
            {
                await CleanupAsync(sut, run.Operation, run.Worker);
            }
        }

        private static async Task VerifySetupFailureAndRetryAsync(SetupMode mode)
        {
            var dispatcher = new ControlledUiDispatcher(DispatchMode.Immediate);
            var failure = new InvalidOperationException("controlled setup failure");
            var factoryFailure = mode == SetupMode.DispatcherFactory ? failure : null;
            var threadFailure = mode == SetupMode.DispatcherThreadCheck ? failure : null;
            var sut = CreateSut(
                mode == SetupMode.NullDispatcher ? null : dispatcher,
                dispatcherFactoryFailure: factoryFailure,
                dispatcherThreadCheckFailure: threadFailure
            );
            var firstFactoryCall = sut.NextDispatcherFactoryCall;
            var firstThreadCheckCall = sut.NextDispatcherThreadCheckCall;
            var terminalSignal = sut.NextTerminal;
            try
            {
                var thrown = GetException(() => _ = sut.FolderTreeService);
                if (mode == SetupMode.NullDispatcher)
                {
                    thrown.Should().BeOfType<InvalidOperationException>();
                    thrown
                        .Message.Should()
                        .Be("Folder tree service dispatcher factory returned null.");
                    sut.Dispatcher = dispatcher;
                }
                else
                {
                    thrown.Should().BeSameAs(failure);
                }
                await firstFactoryCall;
                if (mode == SetupMode.DispatcherThreadCheck)
                    await firstThreadCheckCall;
                (await GetExceptionAsync(await terminalSignal)).Should().BeSameAs(thrown);
                dispatcher.ForceQueue = true;
                var retryFactoryCall = sut.NextDispatcherFactoryCall;
                var retryThreadCheckCall = sut.NextDispatcherThreadCheckCall;
                sut.FolderTreeService.Should().BeSameAs(sut.Service);
                await retryFactoryCall;
                await retryThreadCheckCall;
                (sut.LoadCount, dispatcher.Invokes, dispatcher.Begins).Should().Be((1, 1, 0));
                sut.LastLoadDispatcher.Should().BeSameAs(dispatcher);
            }
            finally
            {
                sut.Dispose();
            }
        }

        internal static ControlledAppOlObjects CreateSut(
            ControlledUiDispatcher dispatcher,
            bool throwFromTerminalHook = false,
            Exception dispatcherFactoryFailure = null,
            Exception dispatcherThreadCheckFailure = null,
            TreeService service = null
        )
        {
            if (service is null)
            {
                var mockService = new Mock<TreeService>(MockBehavior.Strict);
                mockService.Setup(value => value.Dispose());
                service = mockService.Object;
            }
            return new ControlledAppOlObjects(
                dispatcher,
                service,
                throwFromTerminalHook,
                dispatcherFactoryFailure,
                dispatcherThreadCheckFailure
            );
        }

        internal static ControlledAppOlObjects CreateSut(
            DispatchMode mode,
            out ControlledUiDispatcher dispatcher,
            out Mock<TreeService> service,
            Exception fault = null
        )
        {
            dispatcher = new ControlledUiDispatcher(mode, fault: fault);
            service = new Mock<TreeService>(MockBehavior.Strict);
            service.Setup(value => value.Dispose());
            return CreateSut(dispatcher, service: service.Object);
        }

        internal static async Task<(
            Task<TreeService> Worker,
            ControlledDispatchOperation Operation,
            Task<Task<TreeService>> Terminal
        )> StartWorkerAsync(ControlledAppOlObjects sut, ControlledUiDispatcher dispatcher)
        {
            dispatcher.ForceQueue = true;
            var callbackCaptured = dispatcher.NextCallbackCaptured;
            var terminal = sut.NextTerminal;
            var worker = Task.Run(() => sut.FolderTreeService);
            if (await Task.WhenAny(callbackCaptured, worker) == worker)
                callbackCaptured.IsCompleted.Should().BeTrue();
            return (worker, await callbackCaptured, terminal);
        }

        internal static async Task<Exception> GetExceptionAsync(Task task) =>
            (await new Func<Task>(() => task).Should().ThrowAsync<Exception>()).Which;

        internal static Task ObserveAsync(Task task) =>
            task.ContinueWith(completed => _ = completed.Exception, TaskScheduler.Default);

        internal static async Task CleanupAsync(
            ControlledAppOlObjects sut,
            ControlledDispatchOperation operation,
            Task worker
        )
        {
            sut.Dispose();
            await operation.ReleaseAsync();
            await ObserveAsync(worker);
            await ObserveAsync(operation.Task);
        }

        internal static async Task VerifyCandidateOwnershipAsync(CandidateScenario scenario)
        {
            var retry = scenario == CandidateScenario.StaleRetry;
            var sinkCounts = new int[retry ? 2 : 1];
            var sinkThreads = new int[sinkCounts.Length];
            var sinks = new OutlookFolderNotificationSink[sinkCounts.Length];
            var services = new Mock<TreeService>[sinkCounts.Length];
            for (var index = 0; index < sinkCounts.Length; index++)
            {
                sinks[index] = CreateSink(sinkCounts, sinkThreads, index);
                services[index] = new Mock<TreeService>(MockBehavior.Strict);
                services[index].Setup(value => value.Dispose()).Callback(sinks[index].Dispose);
            }
            var fault = new InvalidOperationException(retry ? "stale dispatch" : "service cleanup");
            if (!retry)
                services[0].Setup(value => value.Dispose()).Throws(fault);
            var dispatcher = new ControlledUiDispatcher(DispatchMode.Pending, fault: fault);
            var sut = CreateSut(dispatcher, service: services[0].Object);
            sut.CandidateFactory = index => (services[index].Object, sinks[index]);
            if (!retry)
            {
                sut.BeforeCompletion = _ => sut.Dispose();
                var run = await StartWorkerAsync(sut, dispatcher);
                try
                {
                    await run.Operation.ReleaseAsync();
                    var terminalException = await GetExceptionAsync(await run.Terminal);
                    var workerException = await GetExceptionAsync(run.Worker);
                    AssertSameObjectDisposed(workerException, terminalException);
                    services[0].Verify(value => value.Dispose(), Times.Once);
                    Volatile.Read(ref sinkCounts[0]).Should().Be(1);
                    Volatile.Read(ref sinkThreads[0]).Should().Be(sut.LoadThreadId);
                    sut.LastLoadDispatcher.Should().BeSameAs(dispatcher);
                    (sut.LoadCount, dispatcher.Invokes, dispatcher.Begins).Should().Be((1, 1, 0));
                    AssertDisposedAccess(() => _ = sut.FolderNotificationSink);
                }
                finally
                {
                    await CleanupAsync(sut, run.Operation, run.Worker);
                }
                return;
            }
            var staleStarted = Signal<int>();
            var releaseStale = Signal<bool>();
            sut.BeforeCompletion = service =>
            {
                if (ReferenceEquals(service, services[0].Object))
                {
                    staleStarted.TrySetResult(Thread.CurrentThread.ManagedThreadId);
                    releaseStale.Task.GetAwaiter().GetResult();
                }
            };
            var staleRun = await StartWorkerAsync(sut, dispatcher);
            var releaseTask = Task.Run(() => staleRun.Operation.ReleaseAsync());
            Func<Task> cleanupRetry = () => Task.CompletedTask;
            try
            {
                var staleThread = await staleStarted.Task;
                dispatcher.Complete(staleRun.Operation, DispatchMode.Faulted);
                var terminalFault = await GetExceptionAsync(await staleRun.Terminal);
                (await GetExceptionAsync(staleRun.Operation.Task)).Should().BeSameAs(fault);
                var workerFault = await GetExceptionAsync(staleRun.Worker);
                workerFault.Should().BeSameAs(terminalFault).And.BeSameAs(fault);
                var retryRun = await StartWorkerAsync(sut, dispatcher);
                cleanupRetry = () => CleanupAsync(sut, retryRun.Operation, retryRun.Worker);
                await retryRun.Operation.ReleaseAsync();
                (await retryRun.Worker).Should().BeSameAs(services[1].Object);
                releaseStale.TrySetResult(true);
                await releaseTask;
                services[0].Verify(value => value.Dispose(), Times.Once);
                Volatile.Read(ref sinkCounts[0]).Should().Be(1);
                Volatile.Read(ref sinkThreads[0]).Should().Be(staleThread);
                sut.FolderTreeService.Should().BeSameAs(services[1].Object);
                sut.FolderNotificationSink.Should().BeSameAs(sinks[1]);
                services[1].Verify(value => value.Dispose(), Times.Never);
                Volatile.Read(ref sinkCounts[1]).Should().Be(0);
                sut.LastLoadDispatcher.Should().BeSameAs(dispatcher);
                (sut.LoadCount, dispatcher.Invokes, dispatcher.Begins).Should().Be((2, 2, 0));
            }
            finally
            {
                releaseStale.TrySetResult(true);
                await ObserveAsync(releaseTask);
                await cleanupRetry();
                await CleanupAsync(sut, staleRun.Operation, staleRun.Worker);
            }
            services[1].Verify(value => value.Dispose(), Times.Once);
            Volatile.Read(ref sinkCounts[1]).Should().Be(1);
        }

        internal static void VerifyDisposedGetter(bool publishFirst)
        {
            using var sut = CreateSut(DispatchMode.Immediate, out _, out _);
            if (publishFirst)
            {
                sut.FolderTreeService.Should().BeSameAs(sut.Service);
                sut.LoadCount.Should().Be(1);
            }
            sut.Dispose();
            AssertDisposedAccess(() => _ = sut.FolderTreeService);
        }

        internal static async Task VerifyCompositionFailureRetryAsync()
        {
            var fault = new InvalidOperationException("controlled composition failure");
            var sut = CreateSut(DispatchMode.Faulted, out var dispatcher, out _, fault);
            var run = await StartWorkerAsync(sut, dispatcher);
            try
            {
                (await GetExceptionAsync(run.Worker)).Should().BeSameAs(fault);
                await run.Operation.ReleaseAsync();
                dispatcher.Mode = DispatchMode.Immediate;
                sut.FolderTreeService.Should().BeSameAs(sut.Service);
                sut.LoadCount.Should().Be(1);
            }
            finally
            {
                await CleanupAsync(sut, run.Operation, run.Worker);
            }
        }

        private static async Task AssertCanceledAsync(Task task, CancellationToken expectedToken)
        {
            var exception = await GetExceptionAsync(task);
            exception.Should().BeAssignableTo<OperationCanceledException>();
            ((OperationCanceledException)exception).CancellationToken.Should().Be(expectedToken);
        }

        private static void AssertSameObjectDisposed(Exception actual, Exception expected)
        {
            actual.Should().BeSameAs(expected).And.BeOfType<ObjectDisposedException>();
            ((ObjectDisposedException)actual).ObjectName.Should().Be(nameof(AppOlObjects));
        }

        private static void AssertDisposedAccess(Action access)
        {
            var exception = GetException(access);
            AssertSameObjectDisposed(exception, exception);
        }
    }

    [TestClass]
    public sealed class AppOlObjectsFolderTreeServiceTests
    {
        [TestMethod]
        public void FolderTreeService_ReturnsSingleSessionScopedInstance()
        {
            using var sut = Lifecycle.CreateSut(DispatchMode.Immediate, out _, out _);
            var first = sut.FolderTreeService;
            sut.FolderTreeService.Should().BeSameAs(first);
            sut.LoadCount.Should().Be(1);
        }

        [TestMethod]
        public void Dispose_DisposesCachedFolderTreeServiceOnce()
        {
            var sut = Lifecycle.CreateSut(DispatchMode.Immediate, out _, out var service);
            _ = sut.FolderTreeService;
            sut.Dispose();
            sut.Dispose();
            service.Verify(value => value.Dispose(), Times.Once);
        }

        [TestMethod]
        public Task FolderTreeService_WorkerFirstAccess_ComposesOnCapturedStaDispatcher() =>
            Lifecycle.VerifyQueuedStaCompositionAsync();

        [TestMethod]
        public Task FolderTreeService_WorkerComposition_DisposeDoesNotWaitForDispatcherWork() =>
            Lifecycle.VerifyBlockingDisposalAsync();

        [TestMethod]
        public Task FolderTreeService_WorkerFirstComposition_AllowsCapturedStaQueuedWork() =>
            Lifecycle.VerifyQueuedStaCompositionAsync();

        [TestMethod]
        public void FolderTreeService_PublishThenDispose_DoesNotReturnDisposedServiceToWaiter() =>
            Lifecycle.VerifyDisposedGetter(publishFirst: true);

        [TestMethod]
        public void FolderTreeService_AfterDispose_ThrowsObjectDisposedException() =>
            Lifecycle.VerifyDisposedGetter(publishFirst: false);

        [TestMethod]
        public Task FolderTreeService_CompositionFailure_ClearsInitializationAndRetries() =>
            Lifecycle.VerifyCompositionFailureRetryAsync();

        [TestMethod]
        public Task StaleCandidate_RetryPublishesDistinctServiceAndSink() =>
            Lifecycle.VerifyCandidateOwnershipAsync(Lifecycle.CandidateScenario.StaleRetry);

        [TestMethod]
        public Task DiscardCandidate_ServiceDisposeFailureStillDisposesSink() =>
            Lifecycle.VerifyCandidateOwnershipAsync(Lifecycle.CandidateScenario.IndependentDiscard);

        [TestMethod]
        public void CreateFolderTreeServiceDispatcher_BaseFactory_ReturnsWpfUiDispatcher()
        {
            using var sut = new BaseDispatcherProbe();
            sut.CreateDispatcher().Should().BeOfType<WpfUiDispatcher>();
        }

        private sealed class BaseDispatcherProbe : AppOlObjects
        {
            internal BaseDispatcherProbe()
                : base(null, Mock.Of<IApplicationGlobals>()) { }

            internal IUiDispatcher CreateDispatcher() => base.CreateFolderTreeServiceDispatcher();
        }
    }
}
