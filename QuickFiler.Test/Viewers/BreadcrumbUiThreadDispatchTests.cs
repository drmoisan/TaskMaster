using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first UI-dispatch contracts for asynchronous breadcrumb delivery.</summary>
    [TestClass]
    public sealed class BreadcrumbUiThreadDispatchTests
    {
        private const string FolderPath = "\\Inbox\\Dispatch";

        [TestMethod]
        public async Task SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext()
        {
            // Arrange
            var context = new RecordingSynchronizationContext();
            var gate = new TaskCompletionSource<FolderTreeNodeKey>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            FolderTreeNodeKey key = Key("dispatch", FolderPath);
            var provider = ProviderForGate(gate, key);
            var messenger = new TrackingMessenger();
            BreadcrumbBridgeCoordinator coordinator;
            Task population;
            SynchronizationContext previous = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
                population = coordinator.SetSuggestionsAsync(
                    new[] { Scored(FolderPath, 0.64) },
                    CancellationToken.None
                );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }

            // Act
            await Task.Run(() => gate.SetResult(key)).ConfigureAwait(false);
            await Task.WhenAny(population, context.FirstPost).ConfigureAwait(false);

            // Assert before and after the owning context runs queued work
            context
                .PostCount.Should()
                .BeGreaterThan(0, "worker completion must cross the captured UI dispatcher");
            messenger.Posted.Should().BeEmpty("the owning context has not run queued work yet");
            await context.DrainUntilAsync(population).ConfigureAwait(false);
            messenger.Posted.Should().NotBeEmpty();
            messenger.PostContexts.Should().OnlyContain(value => ReferenceEquals(value, context));
            provider.VerifyAll();
        }

        [TestMethod]
        public async Task InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext()
        {
            // Arrange
            var context = new RecordingSynchronizationContext();
            var messenger = new TrackingMessenger();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            BreadcrumbBridgeCoordinator coordinator;
            var callbackContexts = new List<SynchronizationContext>();
            SynchronizationContext previous = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
                coordinator.AddItems(new[] { FolderPath });
                messenger.Clear();
                coordinator.SelectionChanged += (sender, args) =>
                    callbackContexts.Add(SynchronizationContext.Current);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }

            // Act
            await Task.Run(() => messenger.Raise("{\"type\":\"selectionChange\",\"rowIndex\":0}"))
                .ConfigureAwait(false);
            Task dispatch = coordinator.LastDispatch;
            await Task.WhenAny(dispatch, context.FirstPost).ConfigureAwait(false);

            // Assert before and after dispatcher execution
            context
                .PostCount.Should()
                .BeGreaterThan(0, "worker-originated posts and callbacks require UI scheduling");
            messenger.Posted.Should().BeEmpty();
            callbackContexts.Should().BeEmpty();
            await context.DrainUntilAsync(dispatch).ConfigureAwait(false);
            messenger.Posted.Should().HaveCount(2);
            messenger.PostContexts.Should().OnlyContain(value => ReferenceEquals(value, context));
            callbackContexts.Should().ContainSingle().Which.Should().BeSameAs(context);
        }

        [TestMethod]
        public void DispatcherSchedulingFailure_IsReportedThroughObservableErrorSink()
        {
            // Arrange
            Type dispatcherType = typeof(BreadcrumbBridgeCoordinator).Assembly.GetType(
                "QuickFiler.Viewers.BreadcrumbUiDispatcher"
            );
            dispatcherType
                .Should()
                .NotBeNull("dispatch failures require the planned host-neutral observable seam");
            ConstructorInfo constructor = dispatcherType
                .GetConstructors(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                )
                .SingleOrDefault(candidate =>
                {
                    ParameterInfo[] parameters = candidate.GetParameters();
                    return parameters.Length == 2
                        && parameters[0].ParameterType == typeof(SynchronizationContext)
                        && parameters[1].ParameterType == typeof(Action<Exception>);
                });
            constructor
                .Should()
                .NotBeNull(
                    "the dispatcher must accept its owning context and observable error sink"
                );
            MethodInfo dispatch = dispatcherType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(candidate =>
                {
                    ParameterInfo[] parameters = candidate.GetParameters();
                    return (candidate.Name == "Dispatch" || candidate.Name == "Post")
                        && parameters.Length == 1
                        && parameters[0].ParameterType == typeof(Action);
                });
            dispatch.Should().NotBeNull("the dispatcher requires one focused Action boundary");
            var failure = new InvalidOperationException("UI scheduling rejected");
            var context = new ThrowingSynchronizationContext(failure);
            var observed = new List<Exception>();
            object dispatcher = constructor.Invoke(
                new object[] { context, new Action<Exception>(error => observed.Add(error)) }
            );

            // Act
            object result = dispatch.Invoke(dispatcher, new object[] { new Action(() => { }) });
            (result as Task)?.GetAwaiter().GetResult();

            // Assert
            context.PostAttempts.Should().Be(1);
            observed.Should().ContainSingle().Which.Should().BeSameAs(failure);
        }

        [TestMethod]
        public void DispatcherActionFailure_IsReportedExactlyOnce()
        {
            // Arrange
            var context = new SynchronizationContext();
            var failure = new InvalidOperationException("UI action failed");
            var observed = new List<Exception>();
            var dispatcher = new BreadcrumbUiDispatcher(context, observed.Add);
            SynchronizationContext previous = SynchronizationContext.Current;
            Task dispatch;

            // Act
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                dispatch = dispatcher.Dispatch(() => throw failure);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
            dispatch.GetAwaiter().GetResult();

            // Assert
            observed.Should().ContainSingle().Which.Should().BeSameAs(failure);
        }

        [TestMethod]
        public async Task DispatchValue_AmbientOwningContext_StillSchedulesBeforeControlAccess()
        {
            // Arrange
            var context = new RecordingSynchronizationContext();
            var observed = new List<Exception>();
            var dispatcher = new BreadcrumbUiDispatcher(context, observed.Add);
            Task<int> dispatch;
            Action dispatchNull = () => dispatcher.DispatchValue<int>(null);
            SynchronizationContext previous = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);

                // Act
                dispatch = dispatcher.DispatchValue(() => 42);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }

            // Assert
            dispatchNull.Should().Throw<ArgumentNullException>().WithParameterName("action");
            dispatch.IsCompleted.Should().BeFalse("ambient context alone is not an inline proof");
            context.PostCount.Should().Be(1);
            await context.DrainUntilAsync(dispatch).ConfigureAwait(false);
            (await dispatch.ConfigureAwait(false)).Should().Be(42);
            observed.Should().BeEmpty();
        }

        [TestMethod]
        public void DispatchValue_NestedSynchronousDispatch_ExecutesInlineWithoutAnotherPost()
        {
            // Arrange
            var context = new RecordingSynchronizationContext();
            var observed = new List<Exception>();
            var dispatcher = new BreadcrumbUiDispatcher(context, observed.Add);
            Task<int> nested = null;
            Task<int> nestedFailure = null;
            var failure = new InvalidOperationException("nested value action failed");
            SynchronizationContext previous = SynchronizationContext.Current;

            // Act
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                dispatcher.Dispatch(() =>
                {
                    nested = dispatcher.DispatchValue(() => 17);
                    nestedFailure = dispatcher.DispatchValue<int>(() => throw failure);
                });
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }

            // Assert
            nested.Should().NotBeNull();
            nested.Status.Should().Be(TaskStatus.RanToCompletion);
            nested.GetAwaiter().GetResult().Should().Be(17);
            Action observeFailure = () => nestedFailure.GetAwaiter().GetResult();
            observeFailure.Should().Throw<InvalidOperationException>().Which.Should().Be(failure);
            context.PostCount.Should().Be(0);
            observed.Should().ContainSingle().Which.Should().BeSameAs(failure);
        }

        [TestMethod]
        public async Task DispatchValue_SchedulingFailure_ReportsOnceAndFaultsReturnedTask()
        {
            // Arrange
            var failure = new InvalidOperationException("value scheduling rejected");
            var context = new ThrowingSynchronizationContext(failure);
            var observed = new List<Exception>();
            var dispatcher = new BreadcrumbUiDispatcher(context, observed.Add);

            // Act
            Task<int> dispatch = dispatcher.DispatchValue(() => 1);
            Func<Task> observeFailure = async () => await dispatch.ConfigureAwait(false);

            // Assert
            await observeFailure
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .Where(value => ReferenceEquals(value, failure));
            context.PostAttempts.Should().Be(1);
            observed.Should().ContainSingle().Which.Should().BeSameAs(failure);
        }

        [TestMethod]
        public void ProductionCaptureWithoutUiContext_FailsFast()
        {
            // Arrange
            SynchronizationContext previous = SynchronizationContext.Current;

            // Act
            Action captureWithoutUiContext;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                captureWithoutUiContext = () => BreadcrumbUiDispatcher.CaptureCurrent();
                captureWithoutUiContext
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("*owning UI synchronization context*");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }

            BreadcrumbUiDispatcher testDispatcher =
                BreadcrumbUiDispatcher.CreateForCurrentThreadTests();
            Func<Task> dispatchWithoutContext = async () =>
                await Task.Run(() => testDispatcher.DispatchValue(() => 1)).ConfigureAwait(false);
            dispatchWithoutContext
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*cannot marshal cross-thread UI work*")
                .GetAwaiter()
                .GetResult();
        }

        [TestMethod]
        public void InboundCurrentDispatchFailure_IsObservedWithoutEscapingEventBoundary()
        {
            // Arrange
            var context = new SynchronizationContext();
            var messenger = new TrackingMessenger();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var observed = new List<Exception>();
            var dispatcher = new BreadcrumbUiDispatcher(context, observed.Add);
            var coordinator = new BreadcrumbBridgeCoordinator(
                messenger,
                provider.Object,
                dispatcher
            );

            // Act
            Action raiseInvalidCurrentMessage = () => messenger.Raise(null);

            // Assert
            raiseInvalidCurrentMessage.Should().NotThrow();
            coordinator.LastDispatch.GetAwaiter().GetResult();
            coordinator.LastDispatch.Status.Should().Be(TaskStatus.RanToCompletion);
            observed.Should().ContainSingle();
            observed.Single().Should().BeOfType<ArgumentNullException>();
        }

        private static Mock<IFolderHierarchyProvider> ProviderForGate(
            TaskCompletionSource<FolderTreeNodeKey> gate,
            FolderTreeNodeKey key
        )
        {
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(value =>
                    value.ResolveLeafKeyAsync(FolderPath, It.IsAny<CancellationToken>())
                )
                .Returns(gate.Task);
            provider
                .Setup(value => value.GetAncestorChainAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[] { new FolderBreadcrumbSegment(key, "Dispatch", FolderPath, false) }
                );
            return provider;
        }

        private static FolderRow Scored(string path, double probability) =>
            new FolderRow(path, FolderRowKind.Suggestion, new FolderScore(path, 100, probability));

        private static FolderTreeNodeKey Key(string entryId, string path) =>
            new FolderTreeNodeKey("store", entryId, path);

        private sealed class RecordingSynchronizationContext : SynchronizationContext
        {
            private readonly object _sync = new object();
            private readonly Queue<Tuple<SendOrPostCallback, object>> _pending =
                new Queue<Tuple<SendOrPostCallback, object>>();
            private readonly SemaphoreSlim _available = new SemaphoreSlim(0);
            private readonly TaskCompletionSource<bool> _firstPost = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

            internal int PostCount { get; private set; }
            internal Task FirstPost => _firstPost.Task;

            public override void Post(SendOrPostCallback callback, object state)
            {
                lock (_sync)
                {
                    _pending.Enqueue(Tuple.Create(callback, state));
                    PostCount++;
                }
                _firstPost.TrySetResult(true);
                _available.Release();
            }

            internal async Task DrainUntilAsync(Task operation)
            {
                while (!operation.IsCompleted)
                {
                    if (!DrainOne())
                    {
                        Task available = _available.WaitAsync();
                        await Task.WhenAny(operation, available).ConfigureAwait(false);
                    }
                }
                while (DrainOne()) { }
                await operation.ConfigureAwait(false);
            }

            private bool DrainOne()
            {
                Tuple<SendOrPostCallback, object> work;
                lock (_sync)
                {
                    if (_pending.Count == 0)
                    {
                        return false;
                    }
                    work = _pending.Dequeue();
                }
                _available.Wait(0);

                SynchronizationContext previous = Current;
                try
                {
                    SetSynchronizationContext(this);
                    work.Item1(work.Item2);
                }
                finally
                {
                    SetSynchronizationContext(previous);
                }
                return true;
            }
        }

        private sealed class ThrowingSynchronizationContext : SynchronizationContext
        {
            private readonly Exception _failure;

            internal ThrowingSynchronizationContext(Exception failure)
            {
                _failure = failure;
            }

            internal int PostAttempts { get; private set; }

            public override void Post(SendOrPostCallback callback, object state)
            {
                PostAttempts++;
                throw _failure;
            }
        }

        private sealed class TrackingMessenger : IWebViewMessenger
        {
            private readonly object _sync = new object();
            private EventHandler<string> _messageReceived;

            internal List<string> Posted { get; } = new List<string>();
            internal List<SynchronizationContext> PostContexts { get; } =
                new List<SynchronizationContext>();

            public event EventHandler<string> MessageReceived
            {
                add => _messageReceived += value;
                remove => _messageReceived -= value;
            }

            public void PostJson(string json)
            {
                lock (_sync)
                {
                    Posted.Add(json);
                    PostContexts.Add(SynchronizationContext.Current);
                }
            }

            internal void Raise(string json) => _messageReceived?.Invoke(this, json);

            internal void Clear()
            {
                lock (_sync)
                {
                    Posted.Clear();
                    PostContexts.Clear();
                }
            }
        }
    }
}
