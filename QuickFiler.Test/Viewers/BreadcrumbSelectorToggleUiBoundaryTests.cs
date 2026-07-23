using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using DisposableMessenger = QuickFiler.Test.Viewers.BreadcrumbSelectorOpenRetryTests.DisposableMessenger;
using FailOnceSynchronizationContext = QuickFiler.Test.Viewers.BreadcrumbSelectorOpenRetryTests.FailOnceSynchronizationContext;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Regression contracts for selector-toggle entry through the P3 UI dispatcher.</summary>
    [TestClass]
    public sealed class BreadcrumbSelectorToggleUiBoundaryTests
    {
        private const string FolderPath = "\\Inbox\\Boundary";
        private static readonly Rectangle AnchorBounds = new Rectangle(120, 240, 390, 25);
        private static readonly Rectangle WorkingArea = new Rectangle(0, 0, 1920, 1040);

        [TestMethod]
        public void WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary()
        {
            var context = new CapturingSynchronizationContext();
            var messenger = new BoundaryMessenger();
            var providerGate = new TaskCompletionSource<FolderTreeNodeKey>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            FolderTreeNodeKey key = new FolderTreeNodeKey("store", "boundary", FolderPath);
            Mock<IFolderHierarchyProvider> provider = CreateProvider(providerGate, key);
            BreadcrumbBridgeCoordinator coordinator;
            Task population;
            SynchronizationContext previous = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
                population = coordinator.SetSuggestionsAsync(
                    new[]
                    {
                        new FolderRow(
                            FolderPath,
                            FolderRowKind.Suggestion,
                            new FolderScore(FolderPath, 100, 0.73)
                        ),
                    },
                    CancellationToken.None
                );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
            Task.Run(() => providerGate.SetResult(key)).GetAwaiter().GetResult();
            context.WaitForPost();

            context.PostCount.Should().BeGreaterThan(0);
            messenger.Posted.Should().BeEmpty();
            context.DrainUntil(population);
            messenger.Posted.Should().NotBeEmpty();
            messenger.PostContexts.Should().OnlyContain(value => ReferenceEquals(value, context));
            messenger.Clear();
            var callbackContexts = new List<SynchronizationContext>();
            coordinator.SelectorOpenStateChanged += (sender, args) =>
                callbackContexts.Add(SynchronizationContext.Current);
            int postsBeforeToggle = context.PostCount;
            const string toggle = "{\"type\":\"selectorToggle\"}";
            Task worker = Task.Run(() => InvokeAmbientNull(() => messenger.Receive(toggle)));
            context.WaitForPost();
            worker.GetAwaiter().GetResult();
            Task toggleDispatch = coordinator.LastDispatch;

            context.PostCount.Should().BeGreaterThan(postsBeforeToggle);
            callbackContexts.Should().BeEmpty("the owning boundary has not run the callback yet");
            context.DrainUntil(toggleDispatch);
            context.ExceptionSnapshot.Should().BeEmpty();
            callbackContexts.Should().ContainSingle().Which.Should().BeSameAs(context);
            coordinator.IsSelectorOpen.Should().BeTrue();
            provider.VerifyAll();
        }

        [TestMethod]
        public void PopupHost_WorkerCompletions_RunOnlyWhenCreatorThreadDrainsBoundary()
        {
            int creatorThread = Environment.CurrentManagedThreadId;
            var context = new CapturingSynchronizationContext();
            var callbackThreads = new List<int>();
            var errors = new List<Exception>();
            var operations = new BreadcrumbPopupUiOperations(
                new BreadcrumbUiDispatcher(context, errors.Add)
            );
            var surface = new Panel();
            var messenger = new DisposableMessenger();
            var readiness = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var factoryCompletion = new TaskCompletionSource<
                Tuple<Control, IWebViewMessenger, Task>
            >(TaskCreationOptions.RunContinuationsAsynchronously);
            SynchronizationContext previous = SynchronizationContext.Current;
            BreadcrumbDropDownHost host;
            Panel anchor;
            Task<bool> opening;
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                anchor = new Panel();
                host = new BreadcrumbDropDownHost(
                    anchor,
                    UninitializedEnvironment(),
                    environment => factoryCompletion.Task,
                    () => callbackThreads.Add(Environment.CurrentManagedThreadId),
                    () => callbackThreads.Add(Environment.CurrentManagedThreadId),
                    () => callbackThreads.Add(Environment.CurrentManagedThreadId),
                    (dropDown, owner, point) =>
                        callbackThreads.Add(Environment.CurrentManagedThreadId),
                    operations
                );
                host.PopupMessengerReady += (sender, args) =>
                    callbackThreads.Add(Environment.CurrentManagedThreadId);
                opening = host.OpenAsync(AnchorBounds, WorkingArea, new Size(390, 180));
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
            using (anchor)
            using (host)
            {
                Task.Run(() =>
                        factoryCompletion.SetResult(
                            Tuple.Create<Control, IWebViewMessenger, Task>(
                                surface,
                                messenger,
                                readiness.Task
                            )
                        )
                    )
                    .GetAwaiter()
                    .GetResult();
                Task.Run(() => readiness.SetResult(true)).GetAwaiter().GetResult();
                context.WaitForPost();
                context.DrainUntil(opening);
                opening.Result.Should().BeTrue();
                host.ControlHost.Size.Should().Be(new Size(390, 180));
                surface.Size.Should().Be(new Size(390, 180));
                Task.Run(() => host.Close(BreadcrumbDropDownCloseReason.Uncommitted))
                    .GetAwaiter()
                    .GetResult()
                    .Should()
                    .BeTrue();
                context.WaitForPost();
                context.DrainAll();
                Task.Run(host.Reset).GetAwaiter().GetResult();
                context.WaitForPost();
                context.DrainAll();
                callbackThreads.Should().NotBeEmpty();
                callbackThreads.Should().OnlyContain(value => value == creatorThread);
                context
                    .ExecutedThreadSnapshot.Should()
                    .OnlyContain(value => value == creatorThread);
                context.CreatorThreadId.Should().Be(creatorThread);
                context.ExceptionSnapshot.Should().BeEmpty();
                errors.Should().BeEmpty();
                surface.IsDisposed.Should().BeTrue();
                messenger.DisposeCount.Should().Be(1);
            }
        }

        [TestMethod]
        public void PopupHost_FocusFailureAfterShow_NativeClosesThenRetriesClosedSession()
        {
            var context = new CapturingSynchronizationContext();
            var errors = new List<Exception>();
            var operations = new BreadcrumbPopupUiOperations(
                new BreadcrumbUiDispatcher(context, errors.Add)
            );
            int showCount = 0;
            int closeCount = 0;
            int focusCount = 0;
            int cancelCount = 0;
            int anchorFocusCount = 0;
            var failure = new InvalidOperationException("focus failed");
            var lifecycleFailure = new InvalidOperationException("reset cleanup failed");
            using (var anchor = new Panel())
            using (var surface = new Panel())
            using (
                var host = new BreadcrumbDropDownHost(
                    anchor,
                    UninitializedEnvironment(),
                    async environment =>
                    {
                        await Task.Yield();
                        return Tuple.Create<Control, IWebViewMessenger, Task>(
                            surface,
                            new DisposableMessenger(),
                            Task.CompletedTask
                        );
                    },
                    () =>
                    {
                        if (++focusCount == 1)
                            throw failure;
                    },
                    () => anchorFocusCount++,
                    () =>
                    {
                        if (++cancelCount == 2)
                            throw lifecycleFailure;
                    },
                    (dropDown, owner, point) => showCount++,
                    operations,
                    (dropDown, reason) => closeCount++
                )
            )
            {
                Task<bool> stale = host.OpenAsync(AnchorBounds, WorkingArea, new Size(390, 180));
                context.DrainUntil(stale, () => host.OpenState &= showCount != 1);
                stale.Result.Should().BeFalse();
                Task<bool> first = host.OpenAsync(AnchorBounds, WorkingArea, new Size(390, 180));
                context.DrainUntil(first);
                first.Result.Should().BeFalse();
                host.IsOpen.Should().BeFalse();
                closeCount.Should().Be(1);
                cancelCount.Should().Be(1);
                anchorFocusCount.Should().Be(1);
                errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
                Task<bool> retry = host.OpenAsync(AnchorBounds, WorkingArea, new Size(390, 180));
                context.DrainUntil(retry);
                retry.Result.Should().BeTrue();
                host.IsOpen.Should().BeTrue();
                showCount.Should().Be(3);
                focusCount.Should().Be(2);
                host.Reset();
                context.DrainAll();
                host.IsOpen.Should().BeFalse();
                errors.Should().Contain(lifecycleFailure);
                context.ExceptionSnapshot.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void PopupHost_FirstSchedulingFailure_SettlesFalseThenRetriesAndObservesLifecycle()
        {
            var context = new FailOnceSynchronizationContext();
            var errors = new List<Exception>();
            var operations = new BreadcrumbPopupUiOperations(
                new BreadcrumbUiDispatcher(context, errors.Add)
            );
            using (var anchor = new Panel())
            using (var surface = new Panel())
            {
                var host = new BreadcrumbDropDownHost(
                    anchor,
                    UninitializedEnvironment(),
                    environment =>
                        Task.FromResult(
                            Tuple.Create<Control, IWebViewMessenger, Task>(
                                surface,
                                new DisposableMessenger(),
                                Task.CompletedTask
                            )
                        ),
                    () => { },
                    () => { },
                    () => { },
                    (dropDown, owner, point) => { },
                    operations
                );

                Task<bool> first = InvokeAmbientNull(() =>
                    host.OpenAsync(AnchorBounds, WorkingArea, new Size(390, 180))
                );
                first.GetAwaiter().GetResult().Should().BeFalse();
                host.IsOpen.Should().BeFalse();
                errors.Should().ContainSingle();

                Task<bool> retry = InvokeAmbientNull(() =>
                    host.OpenAsync(AnchorBounds, WorkingArea, new Size(390, 180))
                );
                retry.GetAwaiter().GetResult().Should().BeTrue();
                int afterOpen = context.PostCount;
                InvokeAmbientNull(() => host.Close(BreadcrumbDropDownCloseReason.Uncommitted));
                context.PostCount.Should().BeGreaterThan(afterOpen);
                int afterClose = context.PostCount;
                InvokeAmbientNull(host.Reset);
                context.PostCount.Should().BeGreaterThan(afterClose);
                int afterReset = context.PostCount;
                InvokeAmbientNull(host.Dispose);
                context.PostCount.Should().BeGreaterThan(afterReset);
                errors.Should().ContainSingle();
            }
        }

        private static Mock<IFolderHierarchyProvider> CreateProvider(
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
                    new[] { new FolderBreadcrumbSegment(key, "Boundary", FolderPath, false) }
                );
            return provider;
        }

        private static CoreWebView2Environment UninitializedEnvironment() =>
            (CoreWebView2Environment)
                FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));

        internal static T InvokeAmbientNull<T>(Func<T> operation)
        {
            SynchronizationContext previous = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                return operation();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        private static void InvokeAmbientNull(Action operation) =>
            InvokeAmbientNull(() =>
            {
                operation();
                return true;
            });

        internal sealed class CapturingSynchronizationContext : SynchronizationContext
        {
            private readonly object _sync = new object();
            private readonly Queue<Tuple<SendOrPostCallback, object>> _pending =
                new Queue<Tuple<SendOrPostCallback, object>>();
            private readonly SemaphoreSlim _available = new SemaphoreSlim(0);
            private readonly List<Exception> _exceptions = new List<Exception>();
            private readonly List<int> _executedThreads = new List<int>();

            internal int CreatorThreadId { get; } = Environment.CurrentManagedThreadId;
            internal Exception[] ExceptionSnapshot => ReadLocked(() => _exceptions.ToArray());
            internal int[] ExecutedThreadSnapshot => ReadLocked(() => _executedThreads.ToArray());
            internal int PendingCount => ReadLocked(() => _pending.Count);
            internal int PostCount => ReadLocked(() => _pending.Count + _executedThreads.Count);

            private T ReadLocked<T>(Func<T> read)
            {
                lock (_sync)
                    return read();
            }

            public override void Post(SendOrPostCallback callback, object state)
            {
                lock (_sync)
                {
                    _pending.Enqueue(Tuple.Create(callback, state));
                    _available.Release();
                }
            }

            internal void DrainUntil(Task operation, Action afterDispatch = null)
            {
                while (!operation.IsCompleted)
                {
                    if (!DrainOne())
                    {
                        WaitHandle.WaitAny(
                            new[]
                            {
                                ((IAsyncResult)operation).AsyncWaitHandle,
                                _available.AvailableWaitHandle,
                            }
                        );
                    }
                    else
                        afterDispatch?.Invoke();
                }
                while (DrainOne()) { }
                operation.GetAwaiter().GetResult();
            }

            internal void WaitForPost() => _available.AvailableWaitHandle.WaitOne();

            internal void DrainAll()
            {
                while (DrainOne()) { }
            }

            internal bool DrainOne()
            {
                if (Environment.CurrentManagedThreadId != CreatorThreadId)
                    throw new InvalidOperationException(
                        "Queued work must drain on its creator thread."
                    );
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
                    lock (_sync)
                        _executedThreads.Add(Environment.CurrentManagedThreadId);
                    work.Item1(work.Item2);
                }
                catch (Exception exception)
                {
                    lock (_sync)
                        _exceptions.Add(exception);
                }
                finally
                {
                    SetSynchronizationContext(previous);
                }
                return true;
            }
        }

        private sealed class BoundaryMessenger : IWebViewMessenger
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

            internal void Receive(string json) => _messageReceived?.Invoke(this, json);

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
