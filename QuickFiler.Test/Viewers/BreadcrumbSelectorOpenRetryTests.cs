using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;
using CapturingSynchronizationContext = QuickFiler.Test.Viewers.BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first contracts for mouse-open rollback, retry, and keyboard equivalence.</summary>
    [TestClass]
    public sealed class BreadcrumbSelectorOpenRetryTests
    {
        [TestMethod]
        public void MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly()
        {
            var firstOpen = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var failure = new InvalidOperationException(
                "Cross-thread operation not valid: Control 'L0vhBreadcrumb_WebView2' accessed "
                    + "from a thread other than the thread it was created on."
            );
            using (var harness = new SelectorOpenHarness(firstOpen.Task, Task.FromResult(true)))
            {
                harness.ToggleFromCollapsedSurface();
                Task.Run(() => firstOpen.SetException(failure)).GetAwaiter().GetResult();
                harness.Context.DrainUntil(harness.Viewer.BreadcrumbOpenTask);

                harness.ToggleFromCollapsedSurface();
                harness.Context.DrainUntil(harness.Viewer.BreadcrumbOpenTask);

                using (new AssertionScope())
                {
                    harness.Context.ExceptionSnapshot.Should().BeEmpty();
                    harness.ErrorSnapshot.Should().ContainSingle().Which.Should().BeSameAs(failure);
                    harness.Host.Requests.Should().HaveCount(2);
                    harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeTrue();
                    harness.Host.IsOpen.Should().BeTrue();
                }
            }
        }

        [TestMethod]
        public void SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle()
        {
            Tuple<Rectangle, Rectangle, Size> keyboardRequest;
            using (var keyboard = new SelectorOpenHarness(Task.FromResult(true)))
            {
                keyboard.Viewer.SetFolderDroppedDown(true);
                keyboard.Context.DrainUntil(keyboard.Viewer.BreadcrumbOpenTask);
                keyboardRequest = keyboard.Host.Requests.Single();
            }

            Tuple<Rectangle, Rectangle, Size> mouseRequest;
            using (var mouse = new SelectorOpenHarness(Task.FromResult(true)))
            {
                mouse.ToggleFromCollapsedSurface();
                mouse.Context.DrainUntil(mouse.Viewer.BreadcrumbOpenTask);
                mouseRequest = mouse.Host.Requests.Single();
            }

            mouseRequest.Item1.Should().Be(keyboardRequest.Item1);
            mouseRequest.Item2.Should().Be(keyboardRequest.Item2);
            mouseRequest.Item3.Should().Be(keyboardRequest.Item3);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        public void Placement_StaleCurrentCheck_StopsSubsequentMutations(int rejectedCheck)
        {
            var context = new CapturingSynchronizationContext();
            var errors = new List<Exception>();
            var operations = new BreadcrumbPopupUiOperations(
                new BreadcrumbUiDispatcher(context, errors.Add)
            );
            using (var dropDown = new ToolStripDropDown())
            using (var control = new Panel { Size = new Size(21, 22) })
            using (var host = new ToolStripControlHost(control) { Size = new Size(23, 24) })
            {
                dropDown.Size = new Size(25, 26);
                Size originalHostSize = host.Size;
                Size originalControlSize = control.Size;
                Size originalDropDownSize = dropDown.Size;
                int currentChecks = 0;
                Task<BreadcrumbPopupPlacementResult?> placement = operations.PlaceSurfaceAsync(
                    dropDown,
                    host,
                    control,
                    new Rectangle(120, 240, 390, 25),
                    new Rectangle(0, 0, 1920, 1040),
                    new Size(390, 180),
                    () => ++currentChecks != rejectedCheck
                );
                context.DrainUntil(placement);

                placement.Result.Should().BeNull();
                currentChecks.Should().Be(rejectedCheck);
                if (rejectedCheck <= 2)
                {
                    host.Size.Should().Be(originalHostSize);
                    control.Size.Should().Be(originalControlSize);
                }
                dropDown.Size.Should().Be(originalDropDownSize);
                errors.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void HostedCleanup_HostDisposeFailure_PreservesPrimaryAndDisposesAllOnce()
        {
            var context = new CapturingSynchronizationContext();
            var errors = new List<Exception>();
            var operations = new BreadcrumbPopupUiOperations(
                new BreadcrumbUiDispatcher(context, errors.Add)
            );
            var failure = new InvalidOperationException("host dispose failed");
            using (var dropDown = new ToolStripDropDown())
            {
                var control = new TrackingResourceControl();
                var host = new ThrowingControlHost(control, failure);
                var messenger = new DisposableMessenger();
                dropDown.Items.Add(host);

                Task cleanup = operations.DisposeHostedSurfaceAsync(
                    dropDown,
                    host,
                    control,
                    messenger
                );
                Action act = () => context.DrainUntil(cleanup);

                act.Should().Throw<InvalidOperationException>().Which.Should().BeSameAs(failure);
                errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
                dropDown.Items.Count.Should().Be(0);
                host.DisposeCount.Should().Be(1);
                control.DisposeCount.Should().Be(1);
                messenger.DisposeCount.Should().Be(1);
            }
        }

        [TestMethod]
        public void Dispose_WhenResetAndOpenWorkAreQueued_HasNoLateActivity()
        {
            int creatorThread = Environment.CurrentManagedThreadId;
            var context = new CapturingSynchronizationContext();
            var errors = new SynchronizedRecorder<Exception>();
            var operations = new BreadcrumbPopupUiOperations(
                new BreadcrumbUiDispatcher(context, errors.Add)
            );
            var readiness = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var surface = new TrackingResourceControl();
            var messenger = new DisposableMessenger();
            int readyCount = 0;
            int showCount = 0;
            int focusCount = 0;
            int cancelCount = 0;
            int anchorFocusCount = 0;
            int nativeCloseCount = 0;
            var operationThreads = new List<int>();
            using (var anchor = new Panel())
            using (
                var host = new BreadcrumbDropDownHost(
                    anchor,
                    (CoreWebView2Environment)
                        FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment)),
                    environment =>
                        Task.FromResult(
                            Tuple.Create<Control, IWebViewMessenger, Task>(
                                surface,
                                messenger,
                                readiness.Task
                            )
                        ),
                    () => RecordOperation(operationThreads, ref focusCount),
                    () => RecordOperation(operationThreads, ref anchorFocusCount),
                    () => RecordOperation(operationThreads, ref cancelCount),
                    (dropDown, owner, point) => RecordOperation(operationThreads, ref showCount),
                    operations,
                    (dropDown, reason) => RecordOperation(operationThreads, ref nativeCloseCount)
                )
            )
            {
                host.PopupMessengerReady += (sender, args) =>
                    RecordOperation(operationThreads, ref readyCount);
                Task<bool> opening = BreadcrumbSelectorToggleUiBoundaryTests.InvokeAmbientNull(() =>
                    host.OpenAsync(
                        new Rectangle(120, 240, 390, 25),
                        new Rectangle(0, 0, 1920, 1040),
                        new Size(390, 180)
                    )
                );
                context.WaitForPost();
                context.DrainOne();
                Task.Run(() => readiness.SetResult(true)).GetAwaiter().GetResult();
                while (showCount == 0)
                {
                    context.WaitForPost();
                    context.DrainOne();
                }
                context.WaitForPost();
                int operationsBeforeDispose = operationThreads.Count;
                Task.Run(host.Reset).GetAwaiter().GetResult();
                Task.Run(host.Dispose).GetAwaiter().GetResult();
                context.DrainUntil(opening);

                opening.Result.Should().BeFalse();
                operationThreads.Should().HaveCount(operationsBeforeDispose);
                readyCount.Should().Be(1);
                showCount.Should().Be(1);
                focusCount.Should().Be(0);
                cancelCount.Should().Be(0);
                anchorFocusCount.Should().Be(0);
                nativeCloseCount.Should().Be(0);
                host.DropDown.Items.Count.Should().Be(0);
                host.IsOpen.Should().BeFalse();
                surface.DisposeCount.Should().Be(1);
                messenger.DisposeCount.Should().Be(1);
                errors.Snapshot().Should().BeEmpty();
                context.ExceptionSnapshot.Should().BeEmpty();
                context.PendingCount.Should().Be(0);
                operationThreads.Should().OnlyContain(thread => thread == creatorThread);
                context
                    .ExecutedThreadSnapshot.Should()
                    .OnlyContain(thread => thread == creatorThread);
            }
        }

        private sealed class SelectorOpenHarness : IDisposable
        {
            private readonly SynchronizedRecorder<Exception> _errors =
                new SynchronizedRecorder<Exception>();
            private readonly SynchronizationContext _previousContext;

            internal SelectorOpenHarness(params Task<bool>[] openResults)
            {
                _previousContext = SynchronizationContext.Current;
                Context = new CapturingSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(Context);
                Viewer = new QuickFiler.ItemViewer();
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                var operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(Context, _errors.Add)
                );
                Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                Viewer.SetFolderItems(new[] { "A", "B" });
                Viewer.SetFolderSelectedIndex(0);
                Host = new RecordingDropDownHost(openResults, CancelSelection);
                Viewer.ConfigureBreadcrumbDropDown(Host, () => AnchorBounds, () => WorkingArea);
                Viewer.AttachBreadcrumbMessenger(Messenger);
            }

            internal QuickFiler.ItemViewer Viewer { get; }
            internal CapturingSynchronizationContext Context { get; }
            internal Exception[] ErrorSnapshot => _errors.Snapshot();
            internal RecordingDropDownHost Host { get; }
            internal ToggleMessenger Messenger { get; } = new ToggleMessenger();
            internal Rectangle AnchorBounds { get; } = new Rectangle(120, 240, 390, 25);
            internal Rectangle WorkingArea { get; } = new Rectangle(0, 0, 1920, 1040);

            internal void ToggleFromCollapsedSurface() =>
                Messenger.Receive("{\"type\":\"selectorToggle\"}");

            public void Dispose()
            {
                if (!Viewer.IsDisposed)
                {
                    Viewer.Dispose();
                }
                SynchronizationContext.SetSynchronizationContext(_previousContext);
            }

            private void CancelSelection() => Viewer.BreadcrumbCoordinator.CancelSelector();
        }

        private sealed class RecordingDropDownHost : IBreadcrumbDropDownHost
        {
            private readonly Queue<Task<bool>> _openResults;
            private readonly Action _cancelSelection;

            internal RecordingDropDownHost(
                IEnumerable<Task<bool>> openResults,
                Action cancelSelection
            )
            {
                _openResults = new Queue<Task<bool>>(openResults);
                _cancelSelection = cancelSelection;
            }

            public bool IsOpen { get; private set; }
            public IWebViewMessenger PopupMessenger => null;
            internal List<Tuple<Rectangle, Rectangle, Size>> Requests { get; } =
                new List<Tuple<Rectangle, Rectangle, Size>>();

            public event EventHandler PopupMessengerReady
            {
                add { }
                remove { }
            }

            public Task<bool> OpenAsync(
                Rectangle anchorScreenBounds,
                Rectangle workingArea,
                Size desiredSize
            )
            {
                Requests.Add(Tuple.Create(anchorScreenBounds, workingArea, desiredSize));
                Task<bool> result = _openResults.Dequeue();
                return ObserveOpenAsync(result);
            }

            public bool Close(BreadcrumbDropDownCloseReason reason)
            {
                if (!IsOpen)
                {
                    return false;
                }
                IsOpen = false;
                if (reason == BreadcrumbDropDownCloseReason.Uncommitted)
                {
                    _cancelSelection();
                }
                return true;
            }

            public void SetTheme(string theme) { }

            public void Reset() => IsOpen = false;

            public void Dispose() => IsOpen = false;

            private async Task<bool> ObserveOpenAsync(Task<bool> opening)
            {
                bool opened = await opening;
                IsOpen = opened;
                return opened;
            }
        }

        private sealed class SynchronizedRecorder<T>
        {
            private readonly object _sync = new object();
            private readonly List<T> _items = new List<T>();

            internal void Add(T item)
            {
                lock (_sync)
                    _items.Add(item);
            }

            internal T[] Snapshot()
            {
                lock (_sync)
                    return _items.ToArray();
            }
        }

        private static void RecordOperation(List<int> threads, ref int count)
        {
            threads.Add(Environment.CurrentManagedThreadId);
            count++;
        }

        internal sealed class DisposableMessenger : IWebViewMessenger, IDisposable
        {
            internal int DisposeCount { get; private set; }
            public event EventHandler<string> MessageReceived
            {
                add { }
                remove { }
            }

            public void PostJson(string json) { }

            public void Dispose() => DisposeCount++;
        }

        internal sealed class FailOnceSynchronizationContext : SynchronizationContext
        {
            internal int PostCount { get; private set; }

            public override void Post(SendOrPostCallback callback, object state)
            {
                PostCount++;
                if (PostCount == 1)
                    throw new InvalidOperationException("first schedule failed");
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

        private sealed class ToggleMessenger : IWebViewMessenger
        {
            private EventHandler<string> _messageReceived;

            public event EventHandler<string> MessageReceived
            {
                add => _messageReceived += value;
                remove => _messageReceived -= value;
            }

            public void PostJson(string json) { }

            internal void Receive(string json) => _messageReceived?.Invoke(this, json);
        }

        private sealed class TrackingResourceControl : Panel
        {
            internal int DisposeCount { get; private set; }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    DisposeCount++;
                base.Dispose(disposing);
            }
        }

        private sealed class ThrowingControlHost : ToolStripControlHost
        {
            private readonly Exception _failure;

            internal ThrowingControlHost(Control control, Exception failure)
                : base(control) => _failure = failure;

            internal int DisposeCount { get; private set; }

            protected override void Dispose(bool disposing)
            {
                if (disposing && !IsDisposed)
                    DisposeCount++;
                base.Dispose(disposing);
                if (disposing)
                    throw _failure;
            }
        }
    }
}
