using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Moq;
using QuickFiler.Viewers;
using OperationEntry = System.Tuple<string, System.Threading.SynchronizationContext>;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Verifies that popup controls and cleanup stay on their owning UI boundary.</summary>
    [TestClass]
    public sealed class BreadcrumbPopupControlDispatchTests
    {
        [TestMethod]
        public async Task SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup()
        {
            var fixture = new SurfaceFactoryFixture();
            var initialization = NewCompletionSource();
            var readiness = NewCompletionSource();
            BreadcrumbPopupUiOperations operations = Operations(
                fixture,
                initialization.Task,
                readiness.Task
            );
            var factory = Factory(operations);
            Task<Tuple<Control, IWebViewMessenger, Task>> creating = Task.Run(() =>
                factory(Uninitialized<CoreWebView2Environment>())
            );
            await Task.Run(() => initialization.SetResult(true)).ConfigureAwait(false);
            Tuple<Control, IWebViewMessenger, Task> created = await creating.ConfigureAwait(false);
            await Task.Run(() => readiness.SetResult(true)).ConfigureAwait(false);
            await created.Item3.ConfigureAwait(false);
            await operations
                .DisposeSurfaceAsync(created.Item1, created.Item2)
                .ConfigureAwait(false);

            fixture.Log.Names.Should().Equal("create", "initialize", "core", "navigate", "cleanup");
            fixture.Log.OffBoundary.Should().BeEmpty();
            fixture.Context.PostCount.Should().Be(5);
            fixture.Errors.Should().BeEmpty();
            fixture.Control.DisposeCount.Should().Be(1);
            fixture.Messenger.DisposeCount.Should().Be(1);
        }

        [TestMethod]
        public async Task SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUp()
        {
            var initialization = NewCompletionSource();
            var failure = new InvalidOperationException("initialization failed");
            var cleanupFailure = new InvalidOperationException("cleanup failed");
            var fixture = new SurfaceFactoryFixture(
                new TrackingControl { DisposeFailure = cleanupFailure }
            );
            BreadcrumbPopupUiOperations operations = Operations(
                fixture,
                initialization.Task,
                Task.CompletedTask
            );
            Task<Tuple<Control, IWebViewMessenger, Task>> creating = Factory(operations)(
                Uninitialized<CoreWebView2Environment>()
            );
            initialization.SetException(failure);
            InvalidOperationException thrown = await CaptureFailure<InvalidOperationException>(
                creating
            );

            thrown.Should().BeSameAs(failure);
            fixture.Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
            fixture.Log.Names.Should().Equal("create", "initialize", "cleanup");
            fixture.Control.DisposeCount.Should().Be(1);
        }

        [TestMethod]
        public async Task SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUp()
        {
            var fixture = new SurfaceFactoryFixture();
            var failure = new InvalidOperationException("navigation rejected");
            BreadcrumbPopupUiOperations operations = Operations(
                fixture,
                Task.CompletedTask,
                Task.CompletedTask,
                () => throw failure
            );
            InvalidOperationException thrown = await CaptureFailure<InvalidOperationException>(
                Factory(operations)(Uninitialized<CoreWebView2Environment>())
            );

            thrown.Should().BeSameAs(failure);
            fixture.Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
            fixture.Log.Names.Should().Equal("create", "initialize", "core", "navigate", "cleanup");
            fixture.Control.DisposeCount.Should().Be(1);
        }

        [TestMethod]
        public async Task SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface()
        {
            var fixture = new SurfaceFactoryFixture();
            var readiness = NewCompletionSource();
            BreadcrumbPopupUiOperations operations = Operations(
                fixture,
                Task.CompletedTask,
                readiness.Task
            );
            Tuple<Control, IWebViewMessenger, Task> created = await Factory(operations)(
                Uninitialized<CoreWebView2Environment>()
            )
                .ConfigureAwait(false);
            var failure = new InvalidOperationException("readiness failed");
            readiness.SetException(failure);
            InvalidOperationException thrown = await CaptureFailure<InvalidOperationException>(
                created.Item3
            );
            await operations
                .DisposeSurfaceAsync(created.Item1, created.Item2)
                .ConfigureAwait(false);

            thrown.Should().BeSameAs(failure);
            fixture.Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
            fixture.Control.DisposeCount.Should().Be(1);
            fixture.Messenger.DisposeCount.Should().Be(1);
        }

        [TestMethod]
        public void Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment()
        {
            var context = new RecordingSynchronizationContext();
            var errors = new ConcurrentQueue<Exception>();
            var log = new OperationRecorder(context);
            var dispatcher = new BreadcrumbUiDispatcher(context, errors.Enqueue);
            BreadcrumbNavigationReadiness readiness =
                BreadcrumbPopupUiOperations.CreateDispatchedReadiness(
                    dispatcher,
                    "Popup",
                    () => log.Record("detach")
                );
            Task.Run(() =>
                {
                    SynchronizationContext.SetSynchronizationContext(null);
                    readiness.Dispose();
                })
                .GetAwaiter()
                .GetResult();

            log.Names.Should().Equal("detach");
            log.OffBoundary.Should().BeEmpty();
            context.PostCount.Should().Be(1);
            errors.Should().BeEmpty();
        }

        [TestMethod]
        public void Readiness_DetachSchedulingFailure_ReportsOnceWithoutDirectDetach()
        {
            var failure = new InvalidOperationException("detach scheduling failed");
            var context = new RecordingSynchronizationContext(failure);
            var errors = new ConcurrentQueue<Exception>();
            var dispatcher = new BreadcrumbUiDispatcher(context, errors.Enqueue);
            int detachCount = 0;
            BreadcrumbNavigationReadiness readiness =
                BreadcrumbPopupUiOperations.CreateDispatchedReadiness(
                    dispatcher,
                    "Popup",
                    () => detachCount++
                );
            readiness.Dispose();

            context.PostCount.Should().Be(1);
            detachCount.Should().Be(0);
            errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
        }

        [TestMethod]
        public async Task DisposeSurfaceAsync_MessengerFailure_StillDisposesControlAndReportsOnce()
        {
            var context = new RecordingSynchronizationContext();
            var errors = new ConcurrentQueue<Exception>();
            var dispatcher = new BreadcrumbUiDispatcher(context, errors.Enqueue);
            var operations = new BreadcrumbPopupUiOperations(dispatcher);
            var control = new TrackingControl();
            var failure = new InvalidOperationException("messenger dispose failed");
            var messenger = new TrackingMessenger { DisposeFailure = failure };
            InvalidOperationException thrown = await CaptureFailure<InvalidOperationException>(
                operations.DisposeSurfaceAsync(control, messenger)
            );

            thrown.Should().BeSameAs(failure);
            messenger.DisposeCount.Should().Be(1);
            control.DisposeCount.Should().Be(1);
            errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
        }

        [TestMethod]
        public Task CreateAndInstall_CancellationCleanupFailure_RetriesOnlyFailedResource() =>
            VerifyCreateAndInstallCleanupAsync(cancellationWins: true);

        [TestMethod]
        public Task CreateAndInstall_StaleHostCleanup_DoesNotDisposeOwnedControlDirectly() =>
            VerifyCreateAndInstallCleanupAsync(cancellationWins: false);

        [TestMethod]
        public void DirectAdapters_CreateGuardAndReportThroughOwnedBoundary()
        {
            var errors = new ConcurrentQueue<Exception>();
            var operations = new BreadcrumbPopupUiOperations(
                new BreadcrumbUiDispatcher(new RecordingSynchronizationContext(), errors.Enqueue)
            );
            var failure = new InvalidOperationException("reported");
            BreadcrumbPopupUiOperations.CreateForCurrentThreadTests().Should().NotBeNull();
            Action normalize = () => BreadcrumbPopupUiOperations.NormalizeFactory(null);
            normalize.Should().Throw<ArgumentNullException>().WithParameterName("factory");
            operations.Report(failure);
            errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public async Task SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp(int kind)
        {
            var fixture = new SurfaceFactoryFixture();
            Func<Tuple<IWebViewMessenger, Task>> navigation = () =>
                kind == 0
                    ? null
                    : Tuple.Create<IWebViewMessenger, Task>(
                        kind == 1 ? null : fixture.Messenger,
                        kind == 2 ? null : Task.CompletedTask
                    );
            BreadcrumbPopupUiOperations operations = Operations(
                fixture,
                Task.CompletedTask,
                Task.CompletedTask,
                navigation
            );
            InvalidOperationException thrown = await CaptureFailure<InvalidOperationException>(
                Factory(operations)(Uninitialized<CoreWebView2Environment>())
            );
            fixture.Errors.Should().ContainSingle().Which.Should().BeSameAs(thrown);
            fixture.Control.DisposeCount.Should().Be(1);
            fixture.Messenger.DisposeCount.Should().Be(kind == 2 ? 1 : 0);
        }

        private static BreadcrumbPopupUiOperations Operations(
            SurfaceFactoryFixture fixture,
            Task initialization,
            Task readiness,
            Func<Tuple<IWebViewMessenger, Task>> navigation = null
        ) =>
            new BreadcrumbPopupUiOperations(
                new BreadcrumbUiDispatcher(fixture.Context, fixture.Errors.Enqueue),
                () => fixture.Log.Record("create", fixture.Control),
                (initializer, value, environment) =>
                    fixture.Log.Record("initialize", initialization),
                value => fixture.Log.Record<CoreWebView2>("core", null),
                (core, value, html) =>
                {
                    fixture.Log.Record("navigate");
                    return navigation == null
                        ? Tuple.Create<IWebViewMessenger, Task>(fixture.Messenger, readiness)
                        : navigation();
                },
                (value, surfaceMessenger) =>
                {
                    fixture.Log.Record("cleanup");
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

        private sealed class SurfaceFactoryFixture
        {
            internal SurfaceFactoryFixture(
                TrackingControl control = null,
                TrackingMessenger messenger = null
            )
            {
                Control = control ?? new TrackingControl();
                Messenger = messenger ?? new TrackingMessenger();
                Log = new OperationRecorder(Context);
            }

            internal RecordingSynchronizationContext Context { get; } =
                new RecordingSynchronizationContext();
            internal ConcurrentQueue<Exception> Errors { get; } = new ConcurrentQueue<Exception>();
            internal OperationRecorder Log { get; }
            internal TrackingControl Control { get; }
            internal TrackingMessenger Messenger { get; }
        }

        private static Func<
            CoreWebView2Environment,
            Task<Tuple<Control, IWebViewMessenger, Task>>
        > Factory(BreadcrumbPopupUiOperations operations) =>
            BreadcrumbWebViewSurfaceFactory.Create(
                new Mock<IWebViewCoreInitializer>(MockBehavior.Strict).Object,
                "<html></html>",
                operations
            );

        private static async Task VerifyCreateAndInstallCleanupAsync(bool cancellationWins)
        {
            var errors = new ConcurrentQueue<Exception>();
            var operations = new BreadcrumbPopupUiOperations(
                new BreadcrumbUiDispatcher(new RecordingSynchronizationContext(), errors.Enqueue)
            );
            var failure = new InvalidOperationException("cleanup failed");
            var control = new TrackingControl { SuppressBaseDisposal = !cancellationWins };
            var messenger = new TrackingMessenger
            {
                DisposeFailure = cancellationWins ? failure : null,
                FailOnlyFirstDispose = true,
            };
            var ready = Tuple.Create<Control, IWebViewMessenger, Task>(
                control,
                messenger,
                cancellationWins ? NewCompletionSource().Task : Task.CompletedTask
            );
            int currentChecks = 0;
            using (var dropDown = new ToolStripDropDown())
            {
                Task<Tuple<ToolStripControlHost, Control, IWebViewMessenger>> opening =
                    operations.CreateAndInstallSurfaceAsync(
                        environment => Task.FromResult(ready),
                        Uninitialized<CoreWebView2Environment>(),
                        dropDown,
                        () => cancellationWins || ++currentChecks == 1,
                        cancellationWins ? Task.CompletedTask : NewCompletionSource().Task
                    );
                InvalidOperationException thrown = null;
                Tuple<ToolStripControlHost, Control, IWebViewMessenger> installed = null;
                if (cancellationWins)
                    thrown = await CaptureFailure<InvalidOperationException>(opening);
                else
                    installed = await opening.ConfigureAwait(false);
                int controlDisposals = control.DisposeCount;
                control.SuppressBaseDisposal = false;
                if (!control.IsDisposed)
                    control.Dispose();

                if (cancellationWins)
                {
                    thrown.Should().BeSameAs(failure);
                    errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
                }
                else
                {
                    installed.Should().BeNull();
                    errors.Should().BeEmpty();
                }
                dropDown.Items.Count.Should().Be(0);
                currentChecks.Should().Be(cancellationWins ? 0 : 2);
                controlDisposals.Should().Be(1);
                messenger.DisposeCount.Should().Be(cancellationWins ? 2 : 1);
            }
        }

        private static async Task<TException> CaptureFailure<TException>(Task operation)
            where TException : Exception
        {
            Func<Task> action = () => operation;
            return (await action.Should().ThrowAsync<TException>()).Which;
        }

        private static T Uninitialized<T>()
            where T : class => (T)FormatterServices.GetUninitializedObject(typeof(T));

        private static TaskCompletionSource<bool> NewCompletionSource() =>
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private sealed class OperationRecorder
        {
            private readonly object _sync = new object();
            private readonly SynchronizationContext _expected;
            private readonly List<OperationEntry> _values = new List<OperationEntry>();

            internal OperationRecorder(SynchronizationContext expected) => _expected = expected;

            internal IReadOnlyList<string> Names => ReadNames(value => true);
            internal IReadOnlyList<string> OffBoundary =>
                ReadNames(value => !ReferenceEquals(value.Item2, _expected));

            internal void Record(string name)
            {
                lock (_sync)
                    _values.Add(Tuple.Create(name, SynchronizationContext.Current));
            }

            internal T Record<T>(string name, T value)
            {
                Record(name);
                return value;
            }

            private IReadOnlyList<string> ReadNames(Predicate<OperationEntry> predicate)
            {
                lock (_sync)
                    return _values.FindAll(predicate).ConvertAll(value => value.Item1);
            }
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

        private sealed class RecordingSynchronizationContext : SynchronizationContext
        {
            private readonly Exception _postFailure;

            internal RecordingSynchronizationContext(Exception postFailure = null) =>
                _postFailure = postFailure;

            internal int PostCount { get; private set; }

            public override void Post(SendOrPostCallback callback, object state)
            {
                PostCount++;
                if (_postFailure != null)
                    throw _postFailure;
                SynchronizationContext previous = Current;
                try
                {
                    SetSynchronizationContext(this);
                    callback(state);
                }
                finally
                {
                    SetSynchronizationContext(previous);
                }
            }
        }
    }
}
