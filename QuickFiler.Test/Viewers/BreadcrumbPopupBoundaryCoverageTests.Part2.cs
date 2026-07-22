using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Moq;
using QuickFiler.Viewers;
using CapturingContext = QuickFiler.Test.Viewers.BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Continuation partial of <see cref="BreadcrumbPopupBoundaryCoverageTests"/>; the shared
    /// factory/readiness harness helpers and remaining cases live in the sibling primary partial so
    /// each file stays under the 500-line limit. Deterministic; no Outlook, live WebView2, timers, or
    /// temp files.
    /// </summary>
    public sealed partial class BreadcrumbPopupBoundaryCoverageTests
    {
        [TestMethod]
        public void InjectedFactory_CreateFailure_ReportsOnceWithoutCleanup() =>
            VerifyFactoryFailure("create", 0, false, false, "create");

        [TestMethod]
        public void InjectedFactory_InitializationFailure_DisposesControlOnce() =>
            VerifyFactoryFailure("initialize", 1, true, false, "create", "initialize", "cleanup");

        [TestMethod]
        public void InjectedFactory_CoreFailure_DisposesControlOnce() =>
            VerifyFactoryFailure(
                "core",
                1,
                false,
                false,
                "create",
                "initialize",
                "core",
                "cleanup"
            );

        [TestMethod]
        public void InjectedFactory_NavigationFailure_DisposesControlOnce() =>
            VerifyFactoryFailure(
                "navigate",
                1,
                false,
                false,
                "create",
                "initialize",
                "core",
                "navigate",
                "cleanup"
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
                    .Should()
                    .Throw<ArgumentNullException>()
                    .WithParameterName("navigate");
                readiness.BeginNavigation(() => { });
                ((Action)(() => readiness.BeginNavigation(() => { })))
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("*already*");
                readiness.Cancel();
                ((Action)(() => readiness.BeginNavigation(() => { })))
                    .Should()
                    .Throw<ObjectDisposedException>();
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
            var readiness = new BreadcrumbNavigationReadiness(
                "Popup",
                () =>
                {
                    detaches++;
                    throw new InvalidOperationException("detach");
                }
            );
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
                null,
                BreadcrumbPopupUiOperations.CaptureCurrentOrTests
            );
            testOperations
                .PostAsync(() => testThread = Environment.CurrentManagedThreadId)
                .GetAwaiter()
                .GetResult();
            BreadcrumbPopupUiOperations captured = WithContext(
                context,
                BreadcrumbPopupUiOperations.CaptureCurrentOrTests
            );
            Task post = Task.Run(() =>
                captured.PostAsync(() => capturedThread = Environment.CurrentManagedThreadId)
            );
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
                Task.FromResult(Tuple.Create<Control, IWebViewMessenger>(control, messenger))
            );
            Tuple<Control, IWebViewMessenger, Task> created = normalized(environment)
                .GetAwaiter()
                .GetResult();
            Func<Task> nullResult = () =>
                BreadcrumbPopupUiOperations.NormalizeFactory(_ =>
                    Task.FromResult<Tuple<Control, IWebViewMessenger>>(null)
                )(environment);
            created.Item1.Should().BeSameAs(control);
            created.Item2.Should().BeSameAs(messenger);
            created.Item3.Should().BeSameAs(Task.CompletedTask);
            nullResult
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*returned no surface*")
                .GetAwaiter()
                .GetResult();
            messenger.Dispose();
            control.Dispose();
        }

        /// <summary>Lifetime 292-301 and 324: a lease superseded after install declines retention and disposes
        /// the exact installed host, control, and messenger exactly once.</summary>
        [TestMethod]
        public void OpenAsync_LeaseSupersededDuringInstall_DisposesInstalledSurfaceExactlyOnce()
        {
            using (var probe = new PopupLifecycleProbe())
            {
                probe.OnItemAdded = probe.Lifetime.Dispose;
                probe.Open().Should().BeFalse();
                ((ToolStripControlHost)probe.AddedItem)
                    .Control.Should()
                    .BeNull("the exact installed control host was disposed");
                probe.Host.DropDown.Items.Count.Should().Be(0);
                probe.Surface.DisposeCount.Should().Be(1, "the installed control is disposed once");
                probe
                    .MessengerDisposeCount.Should()
                    .Be(1, "the installed messenger is disposed once");
                probe.Host.InstalledControlHost.Should().BeNull();
                probe.Host.InstalledPopupControl.Should().BeNull();
                probe.Host.InstalledPopupMessenger.Should().BeNull();
                probe
                    .ReadyCount.Should()
                    .Be(0, "declined retention publishes no messenger readiness");
            }
        }

        /// <summary>Lifetime 315: a creation failure whose post-failure disposal succeeds disposes the owned
        /// surface exactly once, emits no cleanup report, and preserves the creation failure.</summary>
        [TestMethod]
        public void OpenAsync_CreationFailsAndCleanupSucceeds_DisposesOwnedSurfaceWithoutReport()
        {
            using (var probe = new PopupLifecycleProbe())
            {
                var creationFailure = new InvalidOperationException("surface creation");
                var owned = new TrackingMessenger();
                probe.FactoryFailure = creationFailure;
                probe.Host.InstalledPopupMessenger = owned;
                probe.Open().Should().BeFalse();
                owned.DisposeCount.Should().Be(1, "post-failure disposal ran exactly once");
                probe.Host.InstalledPopupMessenger.Should().BeNull();
                probe.Errors.Should().BeEmpty("a successful cleanup emits no report");
                probe.Host.LastInitializationException.Should().BeSameAs(creationFailure);
            }
        }

        /// <summary>Lifetime 310-313 with 315: a failing post-failure disposal reports its secondary exactly
        /// once and never replaces the primary creation failure.</summary>
        [TestMethod]
        public void OpenAsync_CleanupDispatchFails_ReportsSecondaryOnceAndPreservesPrimary()
        {
            using (var probe = new PopupLifecycleProbe())
            {
                var creationFailure = new InvalidOperationException("readiness");
                var cleanupFailure = new InvalidOperationException("cleanup dispatch");
                var readiness = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
                probe.Readiness = readiness.Task;
                probe.MessengerDisposal = () => probe.EnqueuePostFailure(cleanupFailure);
                Task<bool> opening = probe.OpenAsync();
                int dispatches = 0;
                probe.DrainUntil(
                    opening,
                    () =>
                    {
                        if (++dispatches == 1)
                            readiness.SetException(creationFailure);
                    }
                );
                Exception primary = probe.Host.LastInitializationException;
                opening.Result.Should().BeFalse();
                probe.Errors.Should().ContainSingle().Which.Should().BeSameAs(cleanupFailure);
                primary
                    .Should()
                    .BeSameAs(creationFailure, "the secondary never replaces the primary");
                probe.MessengerDisposeCount.Should().Be(1);
            }
        }

        /// <summary>Lifetime: a failing open failure-recovery dispatch is reported exactly once by
        /// HandleOpenFailureAsync's internal catch; the open task completes unfaulted false and is cleared.</summary>
        [TestMethod]
        public void OpenAsync_RecoveryDispatchFails_ReportsOnceAndClearsStoredOpenTask()
        {
            using (var probe = new PopupLifecycleProbe())
            {
                var kickoffFailure = new InvalidOperationException("kickoff dispatch");
                var recoveryFailure = new InvalidOperationException("recovery dispatch");
                probe.EnqueuePostFailure(kickoffFailure);
                probe.EnqueuePostFailure(recoveryFailure);
                Task<bool> opening = probe.OpenAsync();
                probe.DrainUntil(opening);
                opening.Status.Should().Be(TaskStatus.RanToCompletion);
                opening.Result.Should().BeFalse();
                probe.Errors.Should().Equal(kickoffFailure, recoveryFailure);
                probe
                    .Host.LastInitializationException.Should()
                    .BeNull("recovery never reached the host");
                probe.StoredOpenTask.Should().BeNull("the stored current open task is cleared");
            }
        }

        /// <summary>Host 413: a native closed notification whose scheduled body drains after the host already
        /// left the open state performs no late close work at all.</summary>
        [TestMethod]
        public void NativeClosedCallback_HostClosedBeforeDrain_PerformsNoLateCloseWork()
        {
            using (var probe = new PopupLifecycleProbe())
            {
                probe.Open().Should().BeTrue();
                int cancels = probe.CancelCount;
                int focusAnchors = probe.FocusAnchorCount;
                int nativeCloses = probe.NativeCloseCount;
                probe.RaiseNativeClosed();
                probe.Host.OpenState = false;
                probe.DrainAll();
                probe.CancelCount.Should().Be(cancels, "the late body returns before cancelling");
                probe.FocusAnchorCount.Should().Be(focusAnchors, "no anchor focus is returned");
                probe.NativeCloseCount.Should().Be(nativeCloses);
                probe.Host.IsOpen.Should().BeFalse("the observed open state is unchanged");
            }
        }

        /// <summary>Shared deterministic popup-lifecycle harness: it is the owner synchronization boundary and
        /// delegates queueing to the capturing context, so scheduling drains step by step and a chosen dispatch
        /// can be made to fail. No Outlook, live WebView2, timers, sleeps, retries, or temp files.</summary>
        private sealed class PopupLifecycleProbe : SynchronizationContext, IDisposable
        {
            private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
            private static readonly object[] ClosedArguments =
            {
                null,
                new ToolStripDropDownClosedEventArgs(ToolStripDropDownCloseReason.AppClicked),
            };
            private readonly CapturingContext _queue = new CapturingContext();
            private readonly ConcurrentQueue<Exception> _postFailures =
                new ConcurrentQueue<Exception>();
            private readonly ConcurrentQueue<Exception> _errors = new ConcurrentQueue<Exception>();
            private readonly Panel _anchor = new Panel();

            internal PopupLifecycleProbe()
            {
                var messenger = new Mock<IWebViewMessenger>();
                messenger
                    .As<IDisposable>()
                    .Setup(value => value.Dispose())
                    .Callback(() =>
                    {
                        MessengerDisposeCount++;
                        MessengerDisposal();
                    });
                Messenger = messenger.Object;
                Host = new BreadcrumbDropDownHost(
                    _anchor,
                    Uninitialized<CoreWebView2Environment>(),
                    CreateSurfaceAsync,
                    () => { },
                    () => FocusAnchorCount++,
                    () => CancelCount++,
                    (popup, owner, location) => { },
                    new BreadcrumbPopupUiOperations(
                        new BreadcrumbUiDispatcher(this, _errors.Enqueue)
                    ),
                    (popup, reason) => NativeCloseCount++
                );
                Lifetime = (BreadcrumbDropDownOpenLifetime)
                    typeof(BreadcrumbDropDownHost).GetField("_openLifetime", Hidden).GetValue(Host);
                Host.PopupMessengerReady += (sender, args) => ReadyCount++;
                Host.DropDown.ItemAdded += (sender, args) =>
                {
                    AddedItem = args.Item;
                    OnItemAdded();
                };
            }

            internal BreadcrumbDropDownHost Host { get; }
            internal BreadcrumbDropDownOpenLifetime Lifetime { get; }
            internal TrackingControl Surface { get; } = new TrackingControl();
            internal IWebViewMessenger Messenger { get; }
            internal ToolStripItem AddedItem { get; private set; }
            internal Exception FactoryFailure { get; set; }
            internal Task Readiness { get; set; } = Task.CompletedTask;
            internal Action OnItemAdded { get; set; } = () => { };
            internal Action MessengerDisposal { get; set; } = () => { };
            internal int MessengerDisposeCount { get; private set; }
            internal int FocusAnchorCount { get; private set; }
            internal int CancelCount { get; private set; }
            internal int NativeCloseCount { get; private set; }
            internal int ReadyCount { get; private set; }
            internal Exception[] Errors => _errors.ToArray();
            internal object StoredOpenTask =>
                typeof(BreadcrumbDropDownOpenLifetime)
                    .GetField("_openTask", Hidden)
                    .GetValue(Lifetime);

            /// <summary>Queues one scheduling failure; the next dispatch attempt throws it instead.</summary>
            internal void EnqueuePostFailure(Exception failure) => _postFailures.Enqueue(failure);

            public override void Post(SendOrPostCallback callback, object state)
            {
                if (_postFailures.TryDequeue(out Exception failure))
                    throw failure;
                _queue.Post(callback, state);
            }

            internal void DrainAll() => _queue.DrainAll();

            internal void DrainUntil(Task operation, Action afterDispatch = null) =>
                _queue.DrainUntil(operation, afterDispatch);

            internal Task<bool> OpenAsync() =>
                Host.OpenAsync(
                    new Rectangle(120, 240, 390, 25),
                    new Rectangle(0, 0, 1920, 1040),
                    new Size(390, 180)
                );

            internal bool Open()
            {
                Task<bool> opening = OpenAsync();
                DrainUntil(opening);
                return opening.GetAwaiter().GetResult();
            }

            internal void RaiseNativeClosed() =>
                typeof(BreadcrumbDropDownHost)
                    .GetMethod("OnDropDownClosed", Hidden)
                    .Invoke(Host, ClosedArguments);

            public void Dispose()
            {
                MessengerDisposal = () => { };
                OnItemAdded = () => { };
                Host.Dispose();
                DrainAll();
                Host.DropDown.Dispose();
                Surface.Dispose();
                _anchor.Dispose();
            }

            private Task<Tuple<Control, IWebViewMessenger, Task>> CreateSurfaceAsync(
                CoreWebView2Environment environment
            ) =>
                FactoryFailure != null
                    ? Task.FromException<Tuple<Control, IWebViewMessenger, Task>>(FactoryFailure)
                    : Task.FromResult(
                        Tuple.Create<Control, IWebViewMessenger, Task>(
                            Surface,
                            Messenger,
                            Readiness
                        )
                    );
        }
    }
}
