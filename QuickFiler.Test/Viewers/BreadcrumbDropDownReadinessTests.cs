using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first popup document-readiness contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbDropDownReadinessTests
    {
        [TestMethod]
        public void OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess()
        {
            // Arrange
            ConstructorInfo constructor = RequireReadinessAwareConstructor();
            using (var harness = new ReadinessHarness(constructor))
            {
                harness.CacheSelectorStateAndOpenSession();

                // Act
                Task<bool> opening = harness.OpenAsync();

                // Assert pending readiness
                opening.IsCompleted.Should().BeFalse();
                harness.ReadyEventCount.Should().Be(0);
                harness.AttachmentCount.Should().Be(0);
                harness.PopupMessenger.SubscriberCount.Should().Be(0);
                harness.PopupMessenger.Posted.Should().BeEmpty();
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.Host.PopupMessenger.Should().BeNull();

                // Act
                harness.Readiness.SetResult(true);
                bool opened = harness.DrainUntil(opening);

                // Assert successful readiness
                opened.Should().BeTrue();
                harness.FactoryCount.Should().Be(1);
                harness.ReadyEventCount.Should().Be(1);
                harness.AttachmentCount.Should().Be(1);
                harness.PopupMessenger.SubscriberCount.Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "render").Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "themeChange").Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "selectorView").Should().Be(1);
                harness.PopupMessenger.Posted.Should().HaveCount(3);
                harness
                    .PopupMessenger.Posted.Single(message =>
                        message.Contains("\"type\":\"selectorView\"")
                    )
                    .Should()
                    .Contain("\"mode\":\"expanded\"");
                harness.ShowCount.Should().Be(1);
                harness.FocusPendingCount.Should().Be(1);
                harness.Host.PopupMessenger.Should().BeSameAs(harness.PopupMessenger);
                harness.Host.IsOpen.Should().BeTrue();
            }
        }

        [TestMethod]
        public void OpenAsync_ReadinessFailureRollsBackDisposesPartialSurfaceAndReturnsFocusOnce()
        {
            // Arrange
            ConstructorInfo constructor = RequireReadinessAwareConstructor();
            using (var harness = new ReadinessHarness(constructor))
            {
                harness.CacheSelectorStateAndOpenSession();
                int selectionPublications = 0;
                harness.Coordinator.SelectionChanged += (sender, args) => selectionPublications++;
                var failure = new InvalidOperationException("document readiness failed");

                // Act
                Task<bool> opening = harness.OpenAsync();
                harness.Readiness.SetException(failure);
                bool opened = harness.DrainUntil(opening);

                // Assert
                opened.Should().BeFalse();
                harness.Coordinator.GetSelectedFolder().Should().Be("A");
                harness.Coordinator.CommittedIdentity.Should().Be("plain:0:A");
                harness.Coordinator.PendingIdentity.Should().BeNull();
                harness.Coordinator.IsSelectorOpen.Should().BeFalse();
                harness.CancelCount.Should().Be(1);
                selectionPublications.Should().Be(0);
                harness.Surface.DisposeCount.Should().Be(1);
                harness.PopupMessenger.DisposeCount.Should().Be(1);
                harness.ReadyEventCount.Should().Be(0);
                harness.AttachmentCount.Should().Be(0);
                harness.PopupMessenger.SubscriberCount.Should().Be(0);
                harness.PopupMessenger.Posted.Should().BeEmpty();
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.FocusAnchorCount.Should().Be(1);
                harness.Host.PopupMessenger.Should().BeNull();
                harness.Host.IsOpen.Should().BeFalse();
                harness.Host.DropDown.Items.Count.Should().Be(0);
                harness.Host.LastInitializationException.Should().BeSameAs(failure);

                harness
                    .Readiness.TrySetException(new Exception("duplicate completion"))
                    .Should()
                    .BeFalse();
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                selectionPublications.Should().Be(0);
            }
        }

        [TestMethod]
        public void CaptureCurrent_ControlledContext_CreatesOperationsWithoutInvokingWebView()
        {
            // Arrange
            SynchronizationContext previous = SynchronizationContext.Current;
            BreadcrumbPopupUiOperations operations;

            // Act
            try
            {
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                operations = BreadcrumbPopupUiOperations.CaptureCurrent();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }

            // Assert
            operations.Should().NotBeNull();
        }

        [DataTestMethod]
        [DataRow(0, "initializer")]
        [DataRow(1, "html")]
        [DataRow(2, "operations")]
        [DataRow(3, "initializer")]
        [DataRow(4, "html")]
        public void SurfaceFactory_InvalidArgumentsFailBeforeUiContextCapture(
            int kind,
            string parameter
        )
        {
            var initializer = new Mock<IWebViewCoreInitializer>().Object;
            BreadcrumbPopupUiOperations operations = CreateNoOpOperations(_ => { });
            Action[] actions =
            {
                () => BreadcrumbWebViewSurfaceFactory.Create(null, "html"),
                () => BreadcrumbWebViewSurfaceFactory.Create(initializer, null),
                () => BreadcrumbWebViewSurfaceFactory.Create(initializer, "html", null),
                () => BreadcrumbWebViewSurfaceFactory.Create(null, "html", operations),
                () => BreadcrumbWebViewSurfaceFactory.Create(initializer, null, operations),
            };
            SynchronizationContext previous = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                actions[kind].Should().Throw<ArgumentNullException>().WithParameterName(parameter);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        [TestMethod]
        public void RunAsync_NullAction_ThrowsArgumentNullException()
        {
            // Arrange
            BreadcrumbPopupUiOperations operations = CreateNoOpOperations(_ => { });

            // Act
            Action runNull = () => operations.RunAsync((Action)null);

            // Assert
            runNull.Should().Throw<ArgumentNullException>().WithParameterName("action");
        }

        [TestMethod]
        public void DisposeSurfaceAsync_NullSurface_ReturnsCompletedTask()
        {
            // Arrange
            BreadcrumbPopupUiOperations operations = CreateNoOpOperations(_ => { });

            // Act
            Task cleanup = operations.DisposeSurfaceAsync(null, null);

            // Assert
            cleanup.Should().BeSameAs(Task.CompletedTask);
        }

        [TestMethod]
        public async Task ObserveReadinessAsync_CancellationRethrowsWithoutReporting()
        {
            // Arrange
            var observed = new List<Exception>();
            BreadcrumbPopupUiOperations operations = CreateNoOpOperations(observed.Add);
            var readiness = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            Task observation = operations.ObserveReadinessAsync(readiness.Task);

            // Act
            readiness.SetCanceled();
            Func<Task> observeCancellation = () => observation;

            // Assert
            await observeCancellation.Should().ThrowAsync<OperationCanceledException>();
            observed.Should().BeEmpty();
        }

        [TestMethod]
        public async Task ObserveInitializationAsync_CancellationReportsIdenticalExceptionOnce()
        {
            // Arrange
            var observed = new List<Exception>();
            BreadcrumbPopupUiOperations operations = CreateNoOpOperations(observed.Add);
            var initialization = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            Task observation = operations.ObserveInitializationAsync(initialization.Task);

            // Act
            initialization.SetCanceled();
            Func<Task> observeCancellation = () => observation;
            OperationCanceledException thrown = (
                await observeCancellation.Should().ThrowAsync<OperationCanceledException>()
            ).Which;

            // Assert
            observed.Should().ContainSingle().Which.Should().BeSameAs(thrown);
        }

        private static BreadcrumbPopupUiOperations CreateNoOpOperations(
            Action<Exception> errorSink
        ) =>
            new BreadcrumbPopupUiOperations(
                new BreadcrumbUiDispatcher(new SynchronizationContext(), errorSink),
                () => null,
                (initializer, control, environment) => Task.CompletedTask,
                control => null,
                (core, control, html) =>
                    Tuple.Create<IWebViewMessenger, Task>(null, Task.CompletedTask),
                (control, messenger) => { }
            );

        private static ConstructorInfo RequireReadinessAwareConstructor()
        {
            Type factoryType = typeof(Func<
                CoreWebView2Environment,
                Task<Tuple<Control, IWebViewMessenger, Task>>
            >);
            Type[] parameters =
            {
                typeof(Control),
                typeof(CoreWebView2Environment),
                factoryType,
                typeof(Action),
                typeof(Action),
                typeof(Action),
                typeof(Action<ToolStripDropDown, Control, Point>),
            };
            ConstructorInfo contract = typeof(BreadcrumbDropDownHost).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                parameters,
                null
            );
            contract
                .Should()
                .NotBeNull(
                    "the popup host requires a readiness-aware surface contract before it can "
                        + "defer messenger exposure, cached replay, show, and focus"
                );
            return typeof(BreadcrumbDropDownHost).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                parameters.Concat(new[] { typeof(BreadcrumbPopupUiOperations) }).ToArray(),
                null
            );
        }

        private static int CountType(IEnumerable<string> messages, string type) =>
            messages.Count(message => message.Contains("\"type\":\"" + type + "\""));

        private sealed class ReadinessHarness : IDisposable
        {
            private readonly SynchronizationContext _previousContext;
            private readonly PumpSynchronizationContext _context;
            private readonly BreadcrumbMessengerHub _hub;
            private readonly Panel _anchor;

            internal ReadinessHarness(ConstructorInfo constructor)
            {
                _previousContext = SynchronizationContext.Current;
                _context = new PumpSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(_context);
                _hub = new BreadcrumbMessengerHub();
                _anchor = new Panel();
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                Coordinator = new BreadcrumbBridgeCoordinator(
                    _hub,
                    provider.Object,
                    BreadcrumbUiDispatcher.CreateForCurrentThreadTests()
                );
                var environment = (CoreWebView2Environment)
                    FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                Func<
                    CoreWebView2Environment,
                    Task<Tuple<Control, IWebViewMessenger, Task>>
                > factory = CreateSurfaceAsync;
                Action<ToolStripDropDown, Control, Point> show = (dropDown, owner, point) =>
                    ShowCount++;
                var operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(_context, _ => { })
                );
                Host = (BreadcrumbDropDownHost)
                    constructor.Invoke(
                        new object[]
                        {
                            _anchor,
                            environment,
                            factory,
                            new Action(() => FocusPendingCount++),
                            new Action(() => FocusAnchorCount++),
                            new Action(() =>
                            {
                                CancelCount++;
                                Coordinator.CancelSelector();
                            }),
                            show,
                            operations,
                        }
                    );
                Host.PopupMessengerReady += OnPopupMessengerReady;
            }

            internal BreadcrumbDropDownHost Host { get; }
            internal BreadcrumbBridgeCoordinator Coordinator { get; }
            internal TaskCompletionSource<bool> Readiness { get; } =
                new TaskCompletionSource<bool>();
            internal TrackingControl Surface { get; } = new TrackingControl();
            internal TrackingMessenger PopupMessenger { get; } = new TrackingMessenger();
            internal int FactoryCount { get; private set; }
            internal int ReadyEventCount { get; private set; }
            internal int AttachmentCount { get; private set; }
            internal int ShowCount { get; private set; }
            internal int FocusPendingCount { get; private set; }
            internal int FocusAnchorCount { get; private set; }
            internal int CancelCount { get; private set; }

            internal void CacheSelectorStateAndOpenSession()
            {
                Coordinator.AddItems(new[] { "A", "B" });
                Coordinator.SelectRow(0);
                Coordinator.SetTheme("dark");
                Coordinator.OpenSelector().Should().BeTrue();
                Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down).Should().BeTrue();
                Coordinator.GetSelectedFolder().Should().Be("A");
                Coordinator.PendingIdentity.Should().Be("plain:1:B");
            }

            internal Task<bool> OpenAsync() =>
                Host.OpenAsync(
                    new Rectangle(120, 240, 390, 25),
                    new Rectangle(0, 0, 1920, 1040),
                    new Size(390, 180)
                );

            internal bool DrainUntil(Task<bool> operation) => _context.DrainUntil(operation);

            public void Dispose()
            {
                Host.PopupMessengerReady -= OnPopupMessengerReady;
                Host.Dispose();
                _context.DrainAll();
                _hub.Dispose();
                _anchor.Dispose();
                SynchronizationContext.SetSynchronizationContext(_previousContext);
            }

            private Task<Tuple<Control, IWebViewMessenger, Task>> CreateSurfaceAsync(
                CoreWebView2Environment environment
            )
            {
                FactoryCount++;
                return Task.FromResult(
                    Tuple.Create<Control, IWebViewMessenger, Task>(
                        Surface,
                        PopupMessenger,
                        Readiness.Task
                    )
                );
            }

            private void OnPopupMessengerReady(object sender, EventArgs args)
            {
                ReadyEventCount++;
                IWebViewMessenger messenger = Host.PopupMessenger;
                if (
                    messenger != null
                    && _hub.Attach(messenger, BreadcrumbSelectorViewMode.Expanded)
                )
                {
                    AttachmentCount++;
                }
            }
        }

        private sealed class PumpSynchronizationContext : SynchronizationContext
        {
            private readonly Queue<Tuple<SendOrPostCallback, object>> _pending =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Post(SendOrPostCallback callback, object state)
            {
                lock (_pending)
                    _pending.Enqueue(Tuple.Create(callback, state));
            }

            internal T DrainUntil<T>(Task<T> operation)
            {
                while (!operation.IsCompleted)
                    if (!DrainOne())
                        Thread.Yield();
                while (DrainOne()) { }
                return operation.GetAwaiter().GetResult();
            }

            internal void DrainAll()
            {
                while (DrainOne()) { }
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
                work.Item1(work.Item2);
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
            private EventHandler<string> _messageReceived;

            internal int SubscriberCount { get; private set; }
            internal int DisposeCount { get; private set; }
            internal List<string> Posted { get; } = new List<string>();

            public event EventHandler<string> MessageReceived
            {
                add
                {
                    _messageReceived += value;
                    SubscriberCount++;
                }
                remove
                {
                    _messageReceived -= value;
                    SubscriberCount--;
                }
            }

            public void PostJson(string json) => Posted.Add(json);

            public void Dispose() => DisposeCount = 1;
        }
    }
}
