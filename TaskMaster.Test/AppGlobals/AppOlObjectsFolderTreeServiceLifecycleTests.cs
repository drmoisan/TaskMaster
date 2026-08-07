using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Threading;
using FolderSink = UtilitiesCS.OutlookObjects.Folder.OutlookFolderNotificationSink;
using Outlook = Microsoft.Office.Interop.Outlook;
using TreeService = UtilitiesCS.OutlookObjects.Folder.IOutlookFolderTreeService;
using TreeTask = System.Threading.Tasks.Task<UtilitiesCS.OutlookObjects.Folder.IOutlookFolderTreeService>;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public sealed partial class AppOlObjectsFolderTreeServiceLifecycleTests
    {
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public Task SetupFailure_ExposesExactExceptionAndRetriesWithFreshOwnership(bool factory) =>
            VerifySetupFailureAndRetryAsync(
                factory ? SetupMode.DispatcherFactory : SetupMode.DispatcherThreadCheck
            );

        [TestMethod]
        public async Task InitializationLinearization_DisposeBeforeCompletionDoesNotPublishCandidate()
        {
            var sut = CreateSut(DispatchMode.Pending, out var dispatcher, out var candidate);
            sut.BeforeCompletion = _ => sut.Dispose();
            var run = await StartWorkerAsync(sut, dispatcher);
            try
            {
                await run.Operation.ReleaseAsync();
                var terminalException = await GetExceptionAsync(await run.Terminal);
                var workerException = await GetExceptionAsync(run.Worker);
                AssertSameObjectDisposed(workerException, terminalException);
                candidate.Verify(value => value.Dispose(), Times.Once);
                sut.LoadCount.Should().Be(1);
                AssertDisposedAccess(() => _ = sut.FolderTreeService);
            }
            finally
            {
                await CleanupAsync(sut, run.Operation, run.Worker);
            }
        }

        [TestMethod]
        public async Task InitializationLinearization_CoalescedCallersReceiveOnePublishedService()
        {
            var dispatcher = new ControlledUiDispatcher(DispatchMode.Pending);
            var sut = CreateSut(dispatcher);
            var first = await StartWorkerAsync(sut, dispatcher);
            var secondThreadCheck = sut.NextDispatcherThreadCheckCall;
            var second = Task.Run(() => sut.FolderTreeService);
            try
            {
                await secondThreadCheck;
                await first.Operation.ReleaseAsync();
                var services = await Task.WhenAll(first.Worker, second);
                services[0].Should().BeSameAs(services[1]).And.BeSameAs(sut.Service);
                sut.LoadCount.Should().Be(1);
            }
            finally
            {
                await CleanupAsync(sut, first.Operation, first.Worker);
                await ObserveAsync(second);
            }
        }

        [TestMethod]
        public void InitializationLinearization_DisposeAfterGetterDoesNotExposeIncompleteService() =>
            VerifyDisposedGetter(publishFirst: true);

        internal static async Task VerifyQueuedStaCompositionAsync()
        {
            var dispatcher = new QueuedStaDispatcher();
            var sut = CreateSut(dispatcher);
            var callbackCaptured = dispatcher.NextCallbackCaptured;
            var worker = Task.Run(() => sut.FolderTreeService);
            ControlledDispatchOperation operation = null;
            try
            {
                (await Task.WhenAny(callbackCaptured, worker)).Should().BeSameAs(callbackCaptured);
                operation = await callbackCaptured;
                var staService = await dispatcher.RunOnStaAsync(() => sut.FolderTreeService);
                (await worker).Should().BeSameAs(staService).And.BeSameAs(sut.Service);
                sut.LastLoadDispatcher.Should().BeSameAs(dispatcher);
                sut.LoadThreadId.Should().Be(dispatcher.ThreadId);
                (sut.LoadCount, dispatcher.Invokes, dispatcher.Begins).Should().Be((1, 1, 0));
                await operation.ReleaseAsync();
                sut.LoadCount.Should().Be(1);
            }
            finally
            {
                if (operation is not null)
                {
                    await operation.ReleaseAsync();
                    await ObserveAsync(operation.Task);
                }
                await ObserveAsync(worker);
                sut.Dispose();
                await dispatcher.StopAsync();
            }
        }

        internal static async Task VerifyBlockingDisposalAsync()
        {
            var dispatcher = new BlockingUiDispatcher();
            var sut = CreateSut(dispatcher);
            var run = await StartWorkerAsync(sut, dispatcher);
            try
            {
                sut.Dispose();
                var terminalException = await GetExceptionAsync(await run.Terminal);
                var workerException = await GetExceptionAsync(run.Worker);
                AssertSameObjectDisposed(workerException, terminalException);
                (sut.LoadCount, dispatcher.Invokes, dispatcher.Begins).Should().Be((0, 1, 0));
            }
            finally
            {
                await CleanupAsync(sut, run.Operation, run.Worker);
            }
        }

        internal sealed class ControlledAppOlObjects : AppOlObjects
        {
            private readonly bool _throwFromTerminalHook;
            private Exception _factoryFailure,
                _threadFailure;
            private TaskCompletionSource<bool> _factorySignal = Signal<bool>();
            private TaskCompletionSource<bool> _threadSignal = Signal<bool>();
            private TaskCompletionSource<TreeTask> _terminalSignal = Signal<TreeTask>();
            private int _loadCount;

            internal ControlledAppOlObjects(
                IUiDispatcher dispatcher,
                TreeService service,
                bool throwFromTerminalHook,
                Exception dispatcherFactoryFailure,
                Exception dispatcherThreadCheckFailure
            )
                : base(null, Mock.Of<IApplicationGlobals>())
            {
                Dispatcher = dispatcher;
                Service = service;
                _throwFromTerminalHook = throwFromTerminalHook;
                _factoryFailure = dispatcherFactoryFailure;
                _threadFailure = dispatcherThreadCheckFailure;
            }

            internal Task NextDispatcherFactoryCall => Volatile.Read(ref _factorySignal).Task;
            internal Task NextDispatcherThreadCheckCall => Volatile.Read(ref _threadSignal).Task;
            internal Task<TreeTask> NextTerminal => Volatile.Read(ref _terminalSignal).Task;
            internal int InvokedTerminalHookCount,
                LoadThreadId;
            internal int LoadCount => Volatile.Read(ref _loadCount);
            internal IUiDispatcher LastLoadDispatcher,
                Dispatcher;
            internal readonly TreeService Service;
            internal Action<TreeService> BeforeCompletion;
            internal Func<int, (TreeService Service, FolderSink Sink)> CandidateFactory;

            protected internal override IUiDispatcher CreateFolderTreeServiceDispatcher() =>
                TakeFailure(ref _factoryFailure, ref _factorySignal) is Exception failure
                    ? throw failure
                    : Dispatcher;

            protected internal override bool IsFolderTreeServiceDispatcherThread(
                IUiDispatcher dispatcher
            ) =>
                TakeFailure(ref _threadFailure, ref _threadSignal) is Exception failure
                    ? throw failure
                    : dispatcher is ControlledUiDispatcher controlled && controlled.CheckAccess();

            protected internal override TreeService LoadFolderTreeService(
                IUiDispatcher dispatcher,
                out FolderSink notificationSink
            )
            {
                LastLoadDispatcher = dispatcher;
                LoadThreadId = Thread.CurrentThread.ManagedThreadId;
                var index = Interlocked.Increment(ref _loadCount) - 1;
                var candidate = CandidateFactory?.Invoke(index) ?? (Service, null);
                notificationSink = candidate.Sink;
                return candidate.Service;
            }

            protected internal override void OnFolderTreeServiceBeforeInitializationCompletion(
                TreeService service
            ) => BeforeCompletion?.Invoke(service);

            protected internal override void OnFolderTreeServiceInitializationTerminal(
                TreeTask terminalInitialization
            )
            {
                InvokedTerminalHookCount++;
                var signal = Interlocked.Exchange(ref _terminalSignal, Signal<TreeTask>());
                signal.TrySetResult(terminalInitialization);
                if (_throwFromTerminalHook)
                    throw new InvalidOperationException("Controlled terminal hook failure.");
            }

            private Exception TakeFailure(
                ref Exception failure,
                ref TaskCompletionSource<bool> signal
            )
            {
                var current = Interlocked.Exchange(ref signal, Signal<bool>());
                var result = Interlocked.Exchange(ref failure, null);
                current.TrySetResult(true);
                return result;
            }
        }

        internal class ControlledUiDispatcher : IUiDispatcher
        {
            private readonly CancellationToken _cancellationToken;
            private readonly Exception _fault;
            private readonly Func<Action, Task> _releaseBackend;
            private TaskCompletionSource<ControlledDispatchOperation> _nextCallback =
                Signal<ControlledDispatchOperation>();
            private int _beginInvokeCallCount,
                _invokeAsyncCallCount;
            private readonly int _threadId = Thread.CurrentThread.ManagedThreadId;

            internal ControlledUiDispatcher(
                DispatchMode mode = DispatchMode.Pending,
                CancellationToken cancellationToken = default,
                Exception fault = null,
                Func<Action, Task> releaseBackend = null
            )
            {
                Mode = mode;
                _cancellationToken = cancellationToken;
                _fault = fault;
                _releaseBackend = releaseBackend;
            }

            internal int Begins => Volatile.Read(ref _beginInvokeCallCount);
            internal int Invokes => Volatile.Read(ref _invokeAsyncCallCount);
            internal DispatchMode Mode;
            internal bool ForceQueue;
            internal Task<ControlledDispatchOperation> NextCallbackCaptured =>
                Volatile.Read(ref _nextCallback).Task;

            internal virtual bool CheckAccess() =>
                !ForceQueue && Thread.CurrentThread.ManagedThreadId == _threadId;

            public void Invoke(Action action) => action();

            public Task InvokeAsync(Action action)
            {
                Interlocked.Increment(ref _invokeAsyncCallCount);
                if (Mode == DispatchMode.Null)
                {
                    Capture(action);
                    return null;
                }

                var operation = Capture(action);
                if (Mode == DispatchMode.Immediate)
                    operation.ReleaseAsync().GetAwaiter().GetResult();
                else
                    operation.Complete(Mode, _cancellationToken, _fault);
                return operation.Task;
            }

            public Task InvokeAsync(
                Action action,
                DispatcherPriority priority,
                CancellationToken token
            )
            {
                token.ThrowIfCancellationRequested();
                return InvokeAsync(action);
            }

            public IAsyncResult BeginInvoke(Action action)
            {
                Interlocked.Increment(ref _beginInvokeCallCount);
                return Capture(action).Task;
            }

            public Task<TResult> InvokeAsync<TResult>(Func<TResult> func) =>
                Task.FromResult(func());

            public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func) => func();

            internal void Complete(ControlledDispatchOperation operation, DispatchMode mode) =>
                operation.Complete(mode, _cancellationToken, _fault);

            private ControlledDispatchOperation Capture(Action action)
            {
                var operation = new ControlledDispatchOperation(action, _releaseBackend);
                var nextCallback = Interlocked.Exchange(
                    ref _nextCallback,
                    Signal<ControlledDispatchOperation>()
                );
                nextCallback.TrySetResult(operation);
                return operation;
            }
        }

        internal sealed class BlockingUiDispatcher : ControlledUiDispatcher { }

        internal sealed class QueuedStaDispatcher : ControlledUiDispatcher
        {
            private readonly StaDispatcherHost _host;

            internal QueuedStaDispatcher()
                : this(new StaDispatcherHost()) { }

            private QueuedStaDispatcher(StaDispatcherHost host)
                : base(releaseBackend: action =>
                    host.RunOnStaAsync(() =>
                    {
                        action();
                        return true;
                    })
                ) => _host = host;

            internal int ThreadId => _host.ThreadId;

            internal override bool CheckAccess() => _host.CheckAccess();

            internal Task<T> RunOnStaAsync<T>(Func<T> action) => _host.RunOnStaAsync(action);

            internal Task StopAsync() => _host.StopAsync();

            private sealed class StaDispatcherHost
            {
                private readonly TaskCompletionSource<Dispatcher> _ready = Signal<Dispatcher>();
                private readonly TaskCompletionSource<bool> _stopped = Signal<bool>();
                private readonly Thread _thread;

                internal StaDispatcherHost()
                {
                    _thread = new Thread(Run) { IsBackground = true };
                    _thread.SetApartmentState(ApartmentState.STA);
                    _thread.Start();
                }

                internal int ThreadId;

                internal bool CheckAccess() => _ready.Task.GetAwaiter().GetResult().CheckAccess();

                internal async Task<T> RunOnStaAsync<T>(Func<T> action)
                {
                    var dispatcher = await _ready.Task.ConfigureAwait(false);
                    return await dispatcher.InvokeAsync(action).Task.ConfigureAwait(false);
                }

                internal async Task StopAsync()
                {
                    try
                    {
                        var dispatcher = await _ready.Task.ConfigureAwait(false);
                        dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                        await _stopped.Task.ConfigureAwait(false);
                    }
                    finally
                    {
                        _thread.Join();
                    }
                }

                private void Run()
                {
                    try
                    {
                        var dispatcher = Dispatcher.CurrentDispatcher;
                        ThreadId = Thread.CurrentThread.ManagedThreadId;
                        _ready.TrySetResult(dispatcher);
                        Dispatcher.Run();
                        _stopped.TrySetResult(true);
                    }
                    catch (Exception exception)
                    {
                        _ready.TrySetException(exception);
                        _stopped.TrySetException(exception);
                    }
                }
            }
        }

        internal sealed class ControlledDispatchOperation
        {
            private readonly Action _action;
            private readonly Func<Action, Task> _releaseBackend;
            private readonly Lazy<Task> _release;
            private readonly TaskCompletionSource<bool> _completion = Signal<bool>();

            internal ControlledDispatchOperation(Action action, Func<Action, Task> releaseBackend)
            {
                _action = action;
                _releaseBackend = releaseBackend;
                _release = new Lazy<Task>(ReleaseCoreAsync);
            }

            internal Task Task => _completion.Task;

            internal void Complete(
                DispatchMode mode,
                CancellationToken cancellationToken,
                Exception fault
            )
            {
                if (mode == DispatchMode.Canceled)
                    _completion.TrySetCanceled(cancellationToken);
                else if (mode == DispatchMode.Faulted)
                    _completion.TrySetException(fault);
            }

            internal Task ReleaseAsync() => _release.Value;

            private async Task ReleaseCoreAsync()
            {
                try
                {
                    if (_releaseBackend is null)
                        Execute();
                    else
                        await _releaseBackend(Execute).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    _completion.TrySetException(exception);
                }
            }

            private void Execute()
            {
                try
                {
                    _action();
                    _completion.TrySetResult(true);
                }
                catch (Exception exception)
                {
                    _completion.TrySetException(exception);
                }
            }
        }

        private static TaskCompletionSource<T> Signal<T>() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal static Exception GetException(Action action) =>
            action.Should().Throw<Exception>().Which;

        private static FolderSink CreateSink(int[] counts, int[] threads, int index)
        {
            var namespaceMapi = new Mock<Outlook.NameSpace>(MockBehavior.Strict);
            namespaceMapi.SetupGet(value => value.Stores).Returns((Outlook.Stores)null);
            var sink = new FolderSink(namespaceMapi.Object);
            sink.Disposed += (_, _) =>
            {
                Volatile.Write(ref threads[index], Thread.CurrentThread.ManagedThreadId);
                Interlocked.Increment(ref counts[index]);
            };
            return sink;
        }

        public enum CandidateScenario
        {
            StaleRetry,
            IndependentDiscard,
        }

        private enum SetupMode
        {
            NullDispatcher,
            DispatcherFactory,
            DispatcherThreadCheck,
        }

        public enum DispatchMode
        {
            Pending,
            Immediate,
            Canceled,
            Faulted,
            Null,
        }
    }
}
