using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Moq;
using QuickFiler.Viewers;
using OperationEntry = System.Tuple<string, int>;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Verifies that popup controls and cleanup stay on their owning UI boundary.</summary>
    [TestClass]
    public sealed class BreadcrumbPopupControlDispatchTests
    {
        [TestMethod]
        public void SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup()
        {
            var fixture = new SurfaceFactoryFixture();
            var initialization = fixture.Completion();
            var readiness = fixture.Completion();
            var operations = fixture.Operations(initialization.Task, readiness.Task);
            var creating = Task.Run(() => fixture.CreateSurface(operations));
            fixture.Drain(creating, 2);
            fixture.CompleteOnWorker(initialization);
            var created = fixture.Complete(creating);
            fixture.CompleteOnWorker(readiness);
            fixture.Drain(created.Item3);
            fixture.Drain(operations.DisposeSurfaceAsync(created.Item1, created.Item2));

            fixture.Log.Names.Should().Equal("create", "initialize", "core", "navigate", "cleanup");
            fixture.Log.OffBoundary.Should().BeEmpty();
            fixture.PostCount.Should().Be(5);
            fixture.Errors.Should().BeEmpty();
            fixture.Control.DisposeCount.Should().Be(1);
            fixture.Messenger.DisposeCount.Should().Be(1);
        }

        [TestMethod]
        public void SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUp()
        {
            var failure = new InvalidOperationException("initialization failed");
            var cleanupFailure = new InvalidOperationException("cleanup failed");
            var fixture = new SurfaceFactoryFixture(disposeFailure: cleanupFailure);
            var initialization = fixture.Completion();
            var operations = fixture.Operations(initialization.Task, Task.CompletedTask);
            var creating = fixture.CreateSurface(operations);
            fixture.Drain(creating, 2);
            initialization.SetException(failure);
            var thrown = fixture.Failure<InvalidOperationException>(creating);

            thrown.Should().BeSameAs(failure);
            fixture.Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
            fixture.Log.Names.Should().Equal("create", "initialize", "cleanup");
            fixture.Control.DisposeCount.Should().Be(1);
        }

        [TestMethod]
        public void SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUp()
        {
            var fixture = new SurfaceFactoryFixture();
            var failure = new InvalidOperationException("navigation rejected");
            var operations = fixture.Operations(
                Task.CompletedTask,
                Task.CompletedTask,
                () => throw failure
            );
            var creating = fixture.CreateSurface(operations);
            var thrown = fixture.Failure<InvalidOperationException>(creating);

            thrown.Should().BeSameAs(failure);
            fixture.Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
            fixture.Log.Names.Should().Equal("create", "initialize", "core", "navigate", "cleanup");
            fixture.Control.DisposeCount.Should().Be(1);
        }

        [TestMethod]
        public void SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface()
        {
            var fixture = new SurfaceFactoryFixture();
            var readiness = fixture.Completion();
            var operations = fixture.Operations(Task.CompletedTask, readiness.Task);
            var created = fixture.Complete(fixture.CreateSurface(operations));
            var failure = new InvalidOperationException("readiness failed");
            readiness.SetException(failure);
            var thrown = fixture.Failure<InvalidOperationException>(created.Item3);
            fixture.Drain(operations.DisposeSurfaceAsync(created.Item1, created.Item2));

            thrown.Should().BeSameAs(failure);
            fixture.Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
            fixture.Control.DisposeCount.Should().Be(1);
            fixture.Messenger.DisposeCount.Should().Be(1);
        }

        [TestMethod]
        public void Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment()
        {
            var fixture = new SurfaceFactoryFixture();
            var dispatcher = new BreadcrumbUiDispatcher(fixture, fixture.Errors.Enqueue);
            var readiness = BreadcrumbPopupUiOperations.CreateDispatchedReadiness(
                dispatcher,
                "Popup",
                () => fixture.Log.Record("detach")
            );
            var disposing = Task.Run(() =>
            {
                SynchronizationContext.SetSynchronizationContext(null);
                readiness.Dispose();
            });
            fixture.Drain(disposing);

            fixture.Log.Names.Should().Equal("detach");
            fixture.Log.OffBoundary.Should().BeEmpty();
            fixture.PostCount.Should().Be(1);
            fixture.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public void Readiness_DetachSchedulingFailure_ReportsOnceWithoutDirectDetach()
        {
            var failure = new InvalidOperationException("detach scheduling failed");
            var fixture = new SurfaceFactoryFixture(postFailure: failure);
            var dispatcher = new BreadcrumbUiDispatcher(fixture, fixture.Errors.Enqueue);
            int detachCount = 0;
            var readiness = BreadcrumbPopupUiOperations.CreateDispatchedReadiness(
                dispatcher,
                "Popup",
                () => detachCount++
            );
            readiness.Dispose();
            fixture.Drain();

            fixture.PostCount.Should().Be(1);
            detachCount.Should().Be(0);
            fixture.Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
        }

        [TestMethod]
        public void DisposeSurfaceAsync_MessengerFailure_StillDisposesControlAndReportsOnce()
        {
            var fixture = new SurfaceFactoryFixture();
            var operations = fixture.Operations();
            var failure = new InvalidOperationException("messenger dispose failed");
            fixture.Messenger.DisposeFailure = failure;
            var thrown = fixture.Failure<InvalidOperationException>(
                operations.DisposeSurfaceAsync(fixture.Control, fixture.Messenger)
            );

            thrown.Should().BeSameAs(failure);
            fixture.Messenger.DisposeCount.Should().Be(1);
            fixture.Control.DisposeCount.Should().Be(1);
            fixture.Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
        }

        [TestMethod]
        public void CreateAndInstall_CancellationCleanupFailure_RetriesOnlyFailedResource() =>
            new SurfaceFactoryFixture().VerifyCreateAndInstallCleanup(cancellationWins: true);

        [TestMethod]
        public void CreateAndInstall_StaleHostCleanup_DoesNotDisposeOwnedControlDirectly() =>
            new SurfaceFactoryFixture().VerifyCreateAndInstallCleanup(cancellationWins: false);

        [TestMethod]
        public void DirectAdapters_CreateGuardAndReportThroughOwnedBoundary()
        {
            var fixture = new SurfaceFactoryFixture();
            var operations = fixture.Operations();
            var failure = new InvalidOperationException("reported");
            BreadcrumbPopupUiOperations.CreateForCurrentThreadTests().Should().NotBeNull();
            Action normalize = () => BreadcrumbPopupUiOperations.NormalizeFactory(null);
            normalize.Should().Throw<ArgumentNullException>().WithParameterName("factory");
            operations.Report(failure);
            fixture.Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp(int kind)
        {
            var fixture = new SurfaceFactoryFixture();
            Func<Tuple<IWebViewMessenger, Task>> navigation = () =>
                kind == 0
                    ? null
                    : Tuple.Create<IWebViewMessenger, Task>(
                        kind == 1 ? null : fixture.Messenger,
                        kind == 2 ? null : Task.CompletedTask
                    );
            var operations = fixture.Operations(Task.CompletedTask, Task.CompletedTask, navigation);
            var thrown = fixture.Failure<InvalidOperationException>(
                fixture.CreateSurface(operations)
            );
            fixture.Errors.Should().ContainSingle().Which.Should().BeSameAs(thrown);
            fixture.Control.DisposeCount.Should().Be(1);
            fixture.Messenger.DisposeCount.Should().Be(kind == 2 ? 1 : 0);
        }

        private sealed class SurfaceFactoryFixture : SynchronizationContext
        {
            private readonly Queue<Tuple<SendOrPostCallback, object>> _pending =
                new Queue<Tuple<SendOrPostCallback, object>>();
            private readonly Exception _postFailure;
            private int _postCount;

            internal SurfaceFactoryFixture(
                Exception postFailure = null,
                Exception disposeFailure = null
            )
            {
                _postFailure = postFailure;
                CreatorThreadId = Environment.CurrentManagedThreadId;
                Log = new OperationRecorder(CreatorThreadId);
                Control = Invoke(() => new TrackingControl { DisposeFailure = disposeFailure });
                WebEnvironment = (CoreWebView2Environment)
                    FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
            }

            internal ConcurrentQueue<Exception> Errors { get; } = new ConcurrentQueue<Exception>();
            internal OperationRecorder Log { get; }
            internal TrackingControl Control { get; }
            internal TrackingMessenger Messenger { get; } = new TrackingMessenger();
            internal CoreWebView2Environment WebEnvironment { get; }
            internal int PostCount => Volatile.Read(ref _postCount);
            internal int CreatorThreadId { get; }

            public override void Post(SendOrPostCallback callback, object state)
            {
                Interlocked.Increment(ref _postCount);
                if (_postFailure != null)
                    throw _postFailure;
                lock (_pending)
                    _pending.Enqueue(Tuple.Create(callback, state));
                Signal();
            }

            internal void Drain(Task operation = null, int workLimit = int.MaxValue)
            {
                EnsureCreatorThread();
                if (operation != null && !operation.IsCompleted)
                    operation.ConfigureAwait(false).GetAwaiter().OnCompleted(Signal);
                int drained = 0;
                while (drained < workLimit)
                {
                    Tuple<SendOrPostCallback, object> work = null;
                    lock (_pending)
                    {
                        if (_pending.Count != 0)
                            work = _pending.Dequeue();
                        else if (operation == null || operation.IsCompleted)
                            break;
                        else
                        {
                            Monitor.Wait(_pending);
                            continue;
                        }
                    }
                    Invoke(() =>
                    {
                        work.Item1(work.Item2);
                        return true;
                    });
                    drained++;
                }
                if (operation != null && workLimit == int.MaxValue)
                    operation.GetAwaiter().GetResult();
            }

            internal T Invoke<T>(Func<T> action)
            {
                EnsureCreatorThread();
                SynchronizationContext previous = Current;
                try
                {
                    SetSynchronizationContext(this);
                    return action();
                }
                finally
                {
                    SetSynchronizationContext(previous);
                }
            }

            internal TaskCompletionSource<bool> Completion() =>
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            internal void CompleteOnWorker(TaskCompletionSource<bool> completion) =>
                Task.Run(() => completion.SetResult(true)).GetAwaiter().GetResult();

            internal BreadcrumbPopupUiOperations Operations() =>
                new BreadcrumbPopupUiOperations(new BreadcrumbUiDispatcher(this, Errors.Enqueue));

            internal BreadcrumbPopupUiOperations Operations(
                Task initialization,
                Task readiness,
                Func<Tuple<IWebViewMessenger, Task>> navigation = null
            ) =>
                new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(this, Errors.Enqueue),
                    () => Log.Record("create", Control),
                    (initializer, value, environment) => Log.Record("initialize", initialization),
                    value => Log.Record<CoreWebView2>("core", null),
                    (core, value, html) =>
                    {
                        Log.Record("navigate");
                        return navigation == null
                            ? Tuple.Create<IWebViewMessenger, Task>(Messenger, readiness)
                            : navigation();
                    },
                    (value, surfaceMessenger) =>
                    {
                        Log.Record("cleanup");
                        try
                        {
                            (surfaceMessenger as IDisposable)?.Dispose();
                        }
                        finally
                        {
                            value?.Dispose();
                        }
                    }
                );

            internal Task<Tuple<Control, IWebViewMessenger, Task>> CreateSurface(
                BreadcrumbPopupUiOperations operations
            ) =>
                BreadcrumbWebViewSurfaceFactory.Create(
                    new Mock<IWebViewCoreInitializer>(MockBehavior.Strict).Object,
                    "<html></html>",
                    operations
                )(WebEnvironment);

            internal T Complete<T>(Task<T> operation)
            {
                Drain(operation);
                return operation.GetAwaiter().GetResult();
            }

            internal TException Failure<TException>(Task operation)
                where TException : Exception
            {
                Action action = () => Drain(operation);
                return action.Should().Throw<TException>().Which;
            }

            internal void VerifyCreateAndInstallCleanup(bool cancellationWins)
            {
                BreadcrumbPopupUiOperations operations = Operations();
                var failure = new InvalidOperationException("cleanup failed");
                Control.SuppressBaseDisposal = !cancellationWins;
                Messenger.DisposeFailure = cancellationWins ? failure : null;
                Messenger.FailOnlyFirstDispose = true;
                var ready = Tuple.Create<Control, IWebViewMessenger, Task>(
                    Control,
                    Messenger,
                    cancellationWins ? Completion().Task : Task.CompletedTask
                );
                int currentChecks = 0;
                using (var dropDown = Invoke(() => new ToolStripDropDown()))
                {
                    Task<Tuple<ToolStripControlHost, Control, IWebViewMessenger>> opening =
                        operations.CreateAndInstallSurfaceAsync(
                            environment => Task.FromResult(ready),
                            WebEnvironment,
                            dropDown,
                            () => cancellationWins || ++currentChecks == 1,
                            cancellationWins ? Task.CompletedTask : Completion().Task
                        );
                    InvalidOperationException thrown = null;
                    Tuple<ToolStripControlHost, Control, IWebViewMessenger> installed = null;
                    if (cancellationWins)
                        thrown = Failure<InvalidOperationException>(opening);
                    else
                        installed = Complete(opening);
                    int controlDisposals = Control.DisposeCount;
                    Control.SuppressBaseDisposal = false;
                    if (!Control.IsDisposed)
                        Control.Dispose();
                    if (cancellationWins)
                    {
                        thrown.Should().BeSameAs(failure);
                        Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
                    }
                    else
                    {
                        installed.Should().BeNull();
                        Errors.Should().BeEmpty();
                    }
                    dropDown.Items.Count.Should().Be(0);
                    currentChecks.Should().Be(cancellationWins ? 0 : 2);
                    controlDisposals.Should().Be(1);
                    Messenger.DisposeCount.Should().Be(cancellationWins ? 2 : 1);
                }
            }

            private void Signal()
            {
                lock (_pending)
                    Monitor.PulseAll(_pending);
            }

            private void EnsureCreatorThread()
            {
                if (Environment.CurrentManagedThreadId != CreatorThreadId)
                    throw new InvalidOperationException("Only the creator thread may drain.");
            }
        }

        private sealed class OperationRecorder
        {
            private readonly int _expectedThreadId;
            private readonly ConcurrentQueue<OperationEntry> _values =
                new ConcurrentQueue<OperationEntry>();

            internal OperationRecorder(int expectedThreadId) =>
                _expectedThreadId = expectedThreadId;

            internal IReadOnlyList<string> Names => ReadNames(value => true);
            internal IReadOnlyList<string> OffBoundary =>
                ReadNames(value => value.Item2 != _expectedThreadId);

            internal void Record(string name) =>
                _values.Enqueue(Tuple.Create(name, Environment.CurrentManagedThreadId));

            internal T Record<T>(string name, T value)
            {
                Record(name);
                return value;
            }

            private IReadOnlyList<string> ReadNames(Func<OperationEntry, bool> predicate) =>
                _values.Where(predicate).Select(value => value.Item1).ToArray();
        }

        private sealed class TrackingControl : Panel
        {
            internal Exception DisposeFailure { get; set; }
            internal int DisposeCount { get; private set; }
            internal bool SuppressBaseDisposal { get; set; }

            protected override void Dispose(bool disposing)
            {
                bool firstDisposal = disposing && !IsDisposed;
                if (disposing)
                    DisposeCount++;
                if (!SuppressBaseDisposal)
                    base.Dispose(disposing);
                if (firstDisposal && DisposeFailure != null)
                    throw DisposeFailure;
            }
        }

        private sealed class TrackingMessenger : IWebViewMessenger, IDisposable
        {
            internal Exception DisposeFailure { get; set; }
            internal int DisposeCount { get; private set; }
            internal bool FailOnlyFirstDispose { get; set; }
            public event EventHandler<string> MessageReceived
            {
                add { }
                remove { }
            }

            public void PostJson(string json) { }

            public void Dispose()
            {
                DisposeCount++;
                if (DisposeFailure != null && (!FailOnlyFirstDispose || DisposeCount == 1))
                    throw DisposeFailure;
            }
        }
    }
}
