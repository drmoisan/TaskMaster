using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Moq;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    [TestClass]
    public sealed class BreadcrumbPopupBoundaryCoverageTests
    {
        [TestMethod]
        public void Dispatcher_NullInputsAndThrowingSink_AreHandledByContract()
        {
            Action nullContext = () => new BreadcrumbUiDispatcher(null, _ => { });
            Action nullSink = () => new BreadcrumbUiDispatcher(new SynchronizationContext(), null);
            var dispatcher = new BreadcrumbUiDispatcher(new SynchronizationContext(), _ => { });
            nullContext.Should().Throw<ArgumentNullException>().WithParameterName("context");
            nullSink.Should().Throw<ArgumentNullException>().WithParameterName("errorSink");
            ((Action)(() => dispatcher.Dispatch(null)))
                .Should().Throw<ArgumentNullException>().WithParameterName("action");
            ((Action)(() => dispatcher.DispatchValue<int>(null)))
                .Should().Throw<ArgumentNullException>().WithParameterName("action");
            ((Action)(() => dispatcher.Report(null)))
                .Should().Throw<ArgumentNullException>().WithParameterName("exception");
            ((Action)(() => new BreadcrumbUiDispatcher(new SynchronizationContext(), _ =>
                throw new InvalidOperationException("sink")).Report(new Exception("source"))))
                .Should().NotThrow();
        }

        [TestMethod]
        public void Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction()
        {
            var errors = new List<Exception>();
            BreadcrumbUiDispatcher dispatcher = CreateOwnerOnlyDispatcher(errors.Add);
            int executions = 0;
            Task dispatch = Task.Run(() => dispatcher.Dispatch(() => executions++));
            dispatch.GetAwaiter().GetResult();
            executions.Should().Be(0);
            errors.Should().ContainSingle().Which.Message.Should().Contain("cannot marshal");
        }

        [TestMethod]
        public void Dispatcher_PostedFailure_ReportsOnceAndRestoresBoundary()
        {
            var context = new PumpSynchronizationContext();
            var errors = new List<Exception>();
            var dispatcher = new BreadcrumbUiDispatcher(context, errors.Add);
            var failure = new InvalidOperationException("posted action");
            int actionThread = 0;
            Task first = dispatcher.Dispatch(() =>
            {
                actionThread = Environment.CurrentManagedThreadId;
                throw failure;
            });
            context.Drain(first);
            Task<int> second = dispatcher.DispatchValue(() => Environment.CurrentManagedThreadId);
            second.IsCompleted.Should().BeFalse("the previous callback must restore the boundary");
            int secondThread = context.Drain(second);
            errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
            context.PostCount.Should().Be(2);
            actionThread.Should().Be(context.OwnerThreadId);
            secondThread.Should().Be(context.OwnerThreadId);
        }

        [TestMethod]
        public void ProductionFactoryCreate_ControlledContext_CapturesWithoutInvokingAdapters()
        {
            var context = new PumpSynchronizationContext();
            Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger, Task>>> factory =
                WithContext(context, () => BreadcrumbWebViewSurfaceFactory.Create(
                    new Mock<IWebViewCoreInitializer>(MockBehavior.Strict).Object,
                    "<html></html>"
                ));
            factory.Should().NotBeNull();
            context.PostCount.Should().Be(0);
        }

        [TestMethod]
        public void InjectedFactory_Success_UsesOwnerBoundaryAndReturnsReadySurface()
        {
            using (var harness = new SurfaceHarness())
            {
                Tuple<Control, IWebViewMessenger, Task> created = harness.Create();
                harness.Context.Drain(harness.Operations.DisposeSurfaceAsync(created.Item1, created.Item2));
                created.Item1.Should().BeSameAs(harness.Control);
                created.Item2.Should().BeSameAs(harness.Messenger);
                created.Item3.Status.Should().Be(TaskStatus.RanToCompletion);
                harness.Calls.Should().Equal("create", "initialize", "core", "navigate", "cleanup");
                harness.AssertOwnerThreads();
                harness.Errors.Should().BeEmpty();
                harness.Control.DisposeCount.Should().Be(1);
                harness.Messenger.DisposeCount.Should().Be(1);
            }
        }

        [TestMethod]
        public void InjectedFactory_CreateFailure_ReportsOnceWithoutCleanup() =>
            VerifyFactoryFailure("create", 0, false, false, "create");

        [TestMethod]
        public void InjectedFactory_InitializationFailure_DisposesControlOnce() =>
            VerifyFactoryFailure("initialize", 1, true, false, "create", "initialize", "cleanup");

        [TestMethod]
        public void InjectedFactory_CoreFailure_DisposesControlOnce() =>
            VerifyFactoryFailure("core", 1, false, false, "create", "initialize", "core", "cleanup");

        [TestMethod]
        public void InjectedFactory_NavigationFailure_DisposesControlOnce() =>
            VerifyFactoryFailure(
                "navigate", 1, false, false, "create", "initialize", "core", "navigate", "cleanup"
            );

        [TestMethod]
        public void InjectedFactory_CleanupFailure_DoesNotReplacePrimaryFailure() =>
            VerifyFactoryFailure("core", 1, false, true, "create", "initialize", "core", "cleanup");

        [TestMethod]
        public void Readiness_ConstructorGuardsBlankNameAndNullDetach()
        {
            Action blank = () => new BreadcrumbNavigationReadiness(" ", () => { });
            Action nullDetach = () => new BreadcrumbNavigationReadiness("Popup", null);
            blank.Should().Throw<ArgumentException>().WithParameterName("surfaceName");
            nullDetach.Should().Throw<ArgumentNullException>().WithParameterName("detachHandlers");
        }

        [TestMethod]
        public void Readiness_BeginNavigationGuardsNullDuplicateAndTerminalRequests()
        {
            int detaches = 0;
            using (var readiness = new BreadcrumbNavigationReadiness("Popup", () => detaches++))
            {
                ((Action)(() => readiness.BeginNavigation(null)))
                    .Should().Throw<ArgumentNullException>().WithParameterName("navigate");
                readiness.BeginNavigation(() => { });
                ((Action)(() => readiness.BeginNavigation(() => { })))
                    .Should().Throw<InvalidOperationException>().WithMessage("*already*");
                readiness.Cancel();
                ((Action)(() => readiness.BeginNavigation(() => { })))
                    .Should().Throw<ObjectDisposedException>();
                readiness.Completion.IsCanceled.Should().BeTrue();
                detaches.Should().Be(1);
            }
        }

        [TestMethod]
        public void Readiness_UnrelatedAndDuplicateNotifications_CompleteCapturedSuccessOnce()
        {
            int detaches = 0;
            using (var readiness = new BreadcrumbNavigationReadiness("Popup", () => detaches++))
            {
                readiness.NavigationStarted(3);
                readiness.BeginNavigation(() => { });
                readiness.NavigationStarted(7);
                readiness.NavigationStarted(8);
                readiness.NavigationCompleted(8, true, null);
                readiness.Completion.IsCompleted.Should().BeFalse();
                readiness.NavigationCompleted(7, true, null);
                readiness.NavigationCompleted(7, false, "duplicate");
                readiness.Completion.Status.Should().Be(TaskStatus.RanToCompletion);
                detaches.Should().Be(1);
            }
        }

        [TestMethod]
        public void Readiness_Failure_NormalizesNullAndBlankStatuses()
        {
            foreach (string status in new string[] { null, " " })
            {
                int detaches = 0;
                using (var readiness = new BreadcrumbNavigationReadiness("Popup", () => detaches++))
                {
                    readiness.BeginNavigation(() => { });
                    readiness.NavigationStarted(5);
                    readiness.NavigationCompleted(5, false, status);
                    Action observe = () => readiness.Completion.GetAwaiter().GetResult();
                    observe.Should().Throw<InvalidOperationException>().WithMessage("*'Unknown'*");
                    detaches.Should().Be(1);
                }
            }
        }

        [TestMethod]
        public void Readiness_CancelAndDispose_AreIdempotent()
        {
            int detaches = 0;
            var readiness = new BreadcrumbNavigationReadiness("Popup", () => detaches++);
            readiness.BeginNavigation(() => { });
            readiness.Cancel();
            readiness.Cancel();
            readiness.Dispose();
            readiness.NavigationStarted(1);
            readiness.NavigationCompleted(1, true, null);
            readiness.Completion.IsCanceled.Should().BeTrue();
            detaches.Should().Be(1);
        }

        [TestMethod]
        public void Readiness_DetachFailure_IsContainedAndCompletionSucceeds()
        {
            int detaches = 0;
            var readiness = new BreadcrumbNavigationReadiness("Popup", () =>
            {
                detaches++;
                throw new InvalidOperationException("detach");
            });
            readiness.BeginNavigation(() => { });
            readiness.NavigationStarted(9);
            Action complete = () => readiness.NavigationCompleted(9, true, null);
            complete.Should().NotThrow();
            readiness.Completion.Status.Should().Be(TaskStatus.RanToCompletion);
            detaches.Should().Be(1);
            readiness.Dispose();
        }

        [TestMethod]
        public void CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries()
        {
            var context = new PumpSynchronizationContext();
            int testThread = 0;
            int capturedThread = 0;
            BreadcrumbPopupUiOperations testOperations = WithContext(
                null, BreadcrumbPopupUiOperations.CaptureCurrentOrTests
            );
            testOperations.PostAsync(() => testThread = Environment.CurrentManagedThreadId)
                .GetAwaiter().GetResult();
            BreadcrumbPopupUiOperations captured = WithContext(
                context, BreadcrumbPopupUiOperations.CaptureCurrentOrTests
            );
            Task post = Task.Run(() => captured.PostAsync(() =>
                capturedThread = Environment.CurrentManagedThreadId));
            context.Drain(post);
            testThread.Should().Be(context.OwnerThreadId);
            capturedThread.Should().Be(context.OwnerThreadId);
            context.PostCount.Should().Be(1);
        }

        [TestMethod]
        public void NormalizeFactory_SuccessAndNullResultPaths_PreserveContract()
        {
            var control = new TrackingControl();
            var messenger = new TrackingMessenger();
            CoreWebView2Environment environment = Uninitialized<CoreWebView2Environment>();
            var normalized = BreadcrumbPopupUiOperations.NormalizeFactory(_ =>
                Task.FromResult(Tuple.Create<Control, IWebViewMessenger>(control, messenger)));
            Tuple<Control, IWebViewMessenger, Task> created = normalized(environment)
                .GetAwaiter().GetResult();
            Func<Task> nullResult = () => BreadcrumbPopupUiOperations.NormalizeFactory(_ =>
                    Task.FromResult<Tuple<Control, IWebViewMessenger>>(null))(environment);
            created.Item1.Should().BeSameAs(control);
            created.Item2.Should().BeSameAs(messenger);
            created.Item3.Should().BeSameAs(Task.CompletedTask);
            nullResult.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*returned no surface*").GetAwaiter().GetResult();
            messenger.Dispose();
            control.Dispose();
        }

        private static BreadcrumbUiDispatcher CreateOwnerOnlyDispatcher(Action<Exception> sink)
        {
            ConstructorInfo constructor = typeof(BreadcrumbUiDispatcher).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(SynchronizationContext), typeof(Action<Exception>), typeof(int?) },
                null
            );
            return (BreadcrumbUiDispatcher)constructor.Invoke(
                new object[] { null, sink, Environment.CurrentManagedThreadId }
            );
        }

        private static T WithContext<T>(SynchronizationContext context, Func<T> action)
        {
            SynchronizationContext previous = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                return action();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        private static void VerifyFactoryFailure(
            string stage,
            int controlDisposals,
            bool initializationTaskFailure,
            bool cleanupFailure,
            params string[] expectedCalls
        )
        {
            using (var harness = new SurfaceHarness())
            {
                var primary = new InvalidOperationException(stage);
                harness.StageFailure = primary;
                if (initializationTaskFailure)
                    harness.Initialization = Task.FromException(primary);
                else
                    harness.FailStage = stage;
                if (cleanupFailure)
                    harness.CleanupFailure = new InvalidOperationException("cleanup");
                harness.CaptureFailure().Should().BeSameAs(primary);
                harness.Calls.Should().Equal(expectedCalls);
                harness.AssertFailureCounts(primary, controlDisposals);
            }
        }

        private static T Uninitialized<T>() where T : class =>
            (T)FormatterServices.GetUninitializedObject(typeof(T));

        private sealed class SurfaceHarness : IDisposable
        {
            internal SurfaceHarness()
            {
                Context = new PumpSynchronizationContext();
                Operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(Context, Errors.Add),
                    () => Stage("create", Control),
                    (initializer, control, environment) => Stage("initialize", Initialization),
                    control => Stage("core", Core),
                    (core, control, html) => Stage("navigate",
                        Tuple.Create<IWebViewMessenger, Task>(Messenger, Readiness)),
                    DisposeSurface
                );
            }
            internal PumpSynchronizationContext Context { get; }
            internal BreadcrumbPopupUiOperations Operations { get; }
            internal TrackingControl Control { get; } = new TrackingControl();
            internal TrackingMessenger Messenger { get; } = new TrackingMessenger();
            internal CoreWebView2 Core { get; } = Uninitialized<CoreWebView2>();
            internal List<string> Calls { get; } = new List<string>();
            internal List<int> Threads { get; } = new List<int>();
            internal List<Exception> Errors { get; } = new List<Exception>();
            internal Task Initialization { get; set; } = Task.CompletedTask;
            internal Task Readiness { get; set; } = Task.CompletedTask;
            internal string FailStage { get; set; }
            internal Exception StageFailure { get; set; }
            internal Exception CleanupFailure { get; set; }
            internal Tuple<Control, IWebViewMessenger, Task> Create() => Context.Drain(
                Factory()(Uninitialized<CoreWebView2Environment>())
            );
            internal Exception CaptureFailure()
            {
                Task creating = Factory()(Uninitialized<CoreWebView2Environment>());
                Action observe = () => Context.Drain(creating);
                return observe.Should().Throw<InvalidOperationException>().Which;
            }
            internal void AssertOwnerThreads() => Threads.Should().OnlyContain(
                threadId => threadId == Context.OwnerThreadId
            );
            internal void AssertFailureCounts(Exception failure, int controlDisposals)
            {
                AssertOwnerThreads();
                Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
                Control.DisposeCount.Should().Be(controlDisposals);
                Messenger.DisposeCount.Should().Be(0);
            }
            public void Dispose()
            {
                if (!Control.IsDisposed)
                    Control.Dispose();
                if (Messenger.DisposeCount == 0)
                    Messenger.Dispose();
            }
            private Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger, Task>>> Factory() =>
                BreadcrumbWebViewSurfaceFactory.Create(
                    new Mock<IWebViewCoreInitializer>(MockBehavior.Strict).Object,
                    "<html></html>",
                    Operations
                );
            private T Stage<T>(string name, T value)
            {
                Calls.Add(name);
                Threads.Add(Environment.CurrentManagedThreadId);
                if (name == FailStage)
                    throw StageFailure;
                return value;
            }
            private void DisposeSurface(Control control, IWebViewMessenger messenger)
            {
                Stage("cleanup", true);
                try
                {
                    (messenger as IDisposable)?.Dispose();
                }
                finally
                {
                    control?.Dispose();
                }
                if (CleanupFailure != null)
                    throw CleanupFailure;
            }
        }

        private sealed class PumpSynchronizationContext : SynchronizationContext
        {
            private readonly Queue<Tuple<SendOrPostCallback, object>> _pending =
                new Queue<Tuple<SendOrPostCallback, object>>();
            private readonly SemaphoreSlim _available = new SemaphoreSlim(0);
            internal PumpSynchronizationContext() =>
                OwnerThreadId = Environment.CurrentManagedThreadId;
            internal int OwnerThreadId { get; }
            internal int PostCount { get; private set; }
            public override void Post(SendOrPostCallback callback, object state)
            {
                lock (_pending)
                    _pending.Enqueue(Tuple.Create(callback, state));
                PostCount++;
                _available.Release();
            }
            internal T Drain<T>(Task<T> operation)
            {
                Drain((Task)operation);
                return operation.GetAwaiter().GetResult();
            }
            internal void Drain(Task operation)
            {
                while (!operation.IsCompleted)
                {
                    if (!DrainOne())
                        Task.WhenAny(operation, _available.WaitAsync()).GetAwaiter().GetResult();
                }
                while (DrainOne()) { }
                operation.GetAwaiter().GetResult();
            }
            private bool DrainOne()
            {
                Tuple<SendOrPostCallback, object> work;
                lock (_pending)
                {
                    if (_pending.Count == 0)
                        return false;
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

        private sealed class TrackingControl : Panel
        {
            internal int DisposeCount { get; private set; }
            protected override void Dispose(bool disposing)
            {
                if (disposing && !IsDisposed)
                    DisposeCount++;
                base.Dispose(disposing);
            }
        }

        private sealed class TrackingMessenger : IWebViewMessenger, IDisposable
        {
            internal int DisposeCount { get; private set; }
            public event EventHandler<string> MessageReceived { add { } remove { } }
            public void PostJson(string json) { }
            public void Dispose() => DisposeCount++;
        }
    }
}
