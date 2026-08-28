using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    [TestClass]
    public sealed class BreadcrumbItemViewerLifecycleCoordinatorTests
    {
        [TestMethod]
        public void HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder()
        {
            using (var fixture = new LifecycleFixture())
            {
                var first = new RecordingHost();
                var second = new RecordingHost();
                var firstMessenger = new Mock<IWebViewMessenger>();
                var secondMessenger = new Mock<IWebViewMessenger>();
                first.PopupMessengerValue = firstMessenger.Object;
                second.PopupMessengerValue = secondMessenger.Object;

                fixture.Coordinator.ConfigureHost(first, FixtureAnchor, FixtureWorkingArea);
                fixture.Queue.DrainOnCreatorThread();
                first.RaisePopupMessengerReady();
                fixture.Queue.DrainOnCreatorThread();
                fixture.Coordinator.ConfigureHost(second, FixtureAnchor, FixtureWorkingArea);
                fixture.Queue.DrainOnCreatorThread();
                second.RaisePopupMessengerReady();
                fixture.Queue.DrainOnCreatorThread();

                first.EventOperations.Should().Equal("add", "remove");
                second.EventOperations.Should().Equal("add");
                firstMessenger.VerifyRemove(
                    value => value.MessageReceived -= It.IsAny<EventHandler<string>>(),
                    Times.Once
                );
                secondMessenger.VerifyAdd(
                    value => value.MessageReceived += It.IsAny<EventHandler<string>>(),
                    Times.Once
                );
            }
        }

        [TestMethod]
        public void CandidateFailure_CleansMessengerAndReadiness()
        {
            using (var fixture = new LifecycleFixture())
            {
                int detached = 0;
                var readiness = BreadcrumbPopupUiOperations.CreateDispatchedReadiness(
                    fixture.Dispatcher,
                    "Collapsed",
                    () => detached++
                );

                Action create = () =>
                    BreadcrumbPopupLifecycleOperations.CreateNavigationSurface(
                        readiness,
                        () => throw new InvalidOperationException("messenger")
                    );

                create.Should().Throw<InvalidOperationException>().WithMessage("messenger");
                fixture.Queue.DrainOnCreatorThread();
                detached.Should().Be(1);
                readiness.Completion.IsCanceled.Should().BeTrue();
            }
        }

        [TestMethod]
        public void ResetDispose_LateCallbackDoesNotReattach()
        {
            using (var fixture = new LifecycleFixture())
            {
                var host = new RecordingHost();
                var messenger = new Mock<IWebViewMessenger>();
                fixture.Coordinator.ConfigureHost(host, FixtureAnchor, FixtureWorkingArea);
                fixture.Queue.DrainOnCreatorThread();
                host.PopupMessengerValue = messenger.Object;
                host.RaisePopupMessengerReady();
                fixture.Coordinator.Reset();
                fixture.Queue.DrainOnCreatorThread();

                messenger.VerifyAdd(
                    value => value.MessageReceived += It.IsAny<EventHandler<string>>(),
                    Times.Never
                );
                fixture.Coordinator.Dispose();
            }
        }

        [TestMethod]
        public void SelectorDelegation_UsesCoordinator()
        {
            using (var fixture = new LifecycleFixture())
            {
                int focusCalls = 0;

                fixture.Coordinator.SetDroppedDown(true, () => focusCalls++);
                fixture.Queue.DrainOnCreatorThread();

                focusCalls.Should().Be(1);
            }
        }

        [TestMethod]
        public void QueuedGeometryAndFocusGuards_RunOnCreatorThread()
        {
            using (var fixture = new LifecycleFixture())
            {
                int focusThread = 0;
                fixture.Coordinator.SetDroppedDown(
                    true,
                    () => focusThread = Environment.CurrentManagedThreadId
                );

                fixture.Queue.DrainOnCreatorThread();

                fixture
                    .Queue.CallbackThreads.Should()
                    .ContainSingle()
                    .Which.Should()
                    .Be(fixture.Queue.CreatorThreadId);
                focusThread.Should().Be(fixture.Queue.CreatorThreadId);
            }
        }

        [TestMethod]
        public void SetBridgeCoordinator_SameReference_DoesNotDuplicateSubscriptions()
        {
            using (var fixture = new LifecycleFixture())
            {
                BreadcrumbBridgeCoordinator bridge = fixture.CreateBridge();
                fixture.Coordinator.SetBridgeCoordinator(bridge);
                fixture.Coordinator.SetBridgeCoordinator(bridge);

                bridge.AddItems(new[] { "A", "B" });
                fixture.Queue.DrainOnCreatorThread();
                bridge.SelectRow(1);
                fixture.Queue.DrainOnCreatorThread();

                fixture.Coordinator.BridgeCoordinator.Should().BeSameAs(bridge);
                fixture.SelectionChangedCount.Should().Be(1);
            }
        }

        [TestMethod]
        public void AttachCollapsedMessenger_Null_ThrowsArgumentNullException()
        {
            using (var fixture = new LifecycleFixture())
            {
                Action attach = () => fixture.Coordinator.AttachCollapsedMessenger(null);

                attach.Should().Throw<ArgumentNullException>().WithParameterName("messenger");
            }
        }

        [TestMethod]
        public void AttachCollapsedMessenger_SameReference_ReusesHubAttachment()
        {
            using (var fixture = new LifecycleFixture())
            {
                var messenger = new Mock<IWebViewMessenger>();

                fixture.Coordinator.AttachCollapsedMessenger(messenger.Object);
                fixture.Coordinator.AttachCollapsedMessenger(messenger.Object);

                messenger.VerifyAdd(
                    value => value.MessageReceived += It.IsAny<EventHandler<string>>(),
                    Times.Once
                );
            }
        }

        [TestMethod]
        public void AttachCollapsedMessenger_ReplacementDetachesPrevious()
        {
            using (var fixture = new LifecycleFixture())
            {
                var first = new Mock<IWebViewMessenger>();
                var second = new Mock<IWebViewMessenger>();

                fixture.Coordinator.AttachCollapsedMessenger(first.Object);
                fixture.Coordinator.AttachCollapsedMessenger(second.Object);

                first.VerifyRemove(
                    value => value.MessageReceived -= It.IsAny<EventHandler<string>>(),
                    Times.Once
                );
                second.VerifyAdd(
                    value => value.MessageReceived += It.IsAny<EventHandler<string>>(),
                    Times.Once
                );
            }
        }

        [TestMethod]
        public void DisposedCoordinator_SetBridgeCoordinatorThrows()
        {
            using (var fixture = new LifecycleFixture())
            {
                BreadcrumbBridgeCoordinator bridge = fixture.CreateBridge();
                fixture.Coordinator.SetBridgeCoordinator(bridge);
                fixture.Coordinator.Dispose();

                Action setBridge = () => fixture.Coordinator.SetBridgeCoordinator(bridge);

                setBridge.Should().Throw<ObjectDisposedException>();
            }
        }

        [TestMethod]
        public void PresentSearchResults_NoOpenCoordinator_BridgeReceivesItemsWithoutThrow()
        {
            // Issue #438 R1: covers the null-_openCoordinator arm of PresentSearchResults
            // (ConfigureHost is never called), asserting the bridge coordinator still receives
            // the search-result items and no exception is thrown.
            using (var fixture = new LifecycleFixture())
            {
                BreadcrumbBridgeCoordinator bridge = fixture.CreateBridge();
                fixture.Coordinator.SetBridgeCoordinator(bridge);

                Action act = () =>
                    fixture.Coordinator.PresentSearchResults(new[] { "Alpha", "Beta" });

                act.Should().NotThrow();
                fixture.Queue.DrainOnCreatorThread();
                bridge.GetFolderItems().Should().Equal("Alpha", "Beta");
            }
        }

        [TestMethod]
        public void PresentSearchResults_NoBridgeCoordinator_IsDeterministicNoOp()
        {
            // Issue #438 R1: covers the null-_bridgeCoordinator arm of PresentSearchResults
            // (neither ConfigureHost nor SetBridgeCoordinator is called), asserting the call is a
            // deterministic no-op: no exception, no bridge coordinator attached, no selection
            // change raised.
            using (var fixture = new LifecycleFixture())
            {
                Action act = () => fixture.Coordinator.PresentSearchResults(new[] { "Gamma" });

                act.Should().NotThrow();
                fixture.Coordinator.BridgeCoordinator.Should().BeNull();
                fixture.SelectionChangedCount.Should().Be(0);
            }
        }

        /// <summary>
        /// Issue #488 defect D2: a theme set while the host-configuration post is still queued must
        /// not be lost. The coordinator retains the last theme and replays it onto the host in
        /// <c>ConfigureHost</c>'s newly-adopted branch, so the theme reaches the host that is
        /// ultimately adopted rather than the null host that was current when it was set.
        /// </summary>
        /// <remarks>
        /// Deterministic and single-threaded. <c>ConfigureHost</c> is queued and deliberately not
        /// drained before <c>SetTheme</c> runs, which is the state that makes the defect observable:
        /// at that moment <c>DropDownHost</c> is null, so the coordinator's direct forward reaches
        /// nothing. The assertion is an exact-sequence equality rather than a containment check, so a
        /// duplicated or additional replay would also fail it. No second thread, no sleep, no timer
        /// delay, and no wall-clock wait is used.
        /// </remarks>
        [TestMethod]
        public void ConfigureHostQueued_SetThemeBeforeDrain_ReplaysThemeOntoAdoptedHost()
        {
            // Arrange
            using (var fixture = new LifecycleFixture())
            {
                var host = new RecordingHost();
                fixture.Coordinator.ConfigureHost(host, FixtureAnchor, FixtureWorkingArea);

                // Act
                fixture.Coordinator.SetTheme("dark");
                fixture.Queue.DrainOnCreatorThread();

                // Assert
                host.ThemesApplied.Should().Equal("dark");
            }
        }

        private static Rectangle FixtureAnchor() => new Rectangle(10, 20, 30, 40);

        private static Rectangle FixtureWorkingArea() => new Rectangle(0, 0, 1920, 1080);

        private sealed class LifecycleFixture : IDisposable
        {
            internal LifecycleFixture()
            {
                Queue = new QueuedCreatorThreadSynchronizationContext();
                Dispatcher = new BreadcrumbUiDispatcher(Queue, _ => { });
                Hub = new BreadcrumbMessengerHub();
                Coordinator = new BreadcrumbItemViewerLifecycleCoordinator(
                    Hub,
                    new BreadcrumbCollapsedAttachment(
                        Hub,
                        new BreadcrumbCollapsedSurfaceController()
                    ),
                    new BreadcrumbPopupUiOperations(Dispatcher),
                    () => SelectionChangedCount++,
                    _ => { },
                    _ => { }
                );
            }

            internal QueuedCreatorThreadSynchronizationContext Queue { get; }
            internal BreadcrumbUiDispatcher Dispatcher { get; }
            internal BreadcrumbMessengerHub Hub { get; }
            internal BreadcrumbItemViewerLifecycleCoordinator Coordinator { get; }
            internal int SelectionChangedCount { get; private set; }

            internal BreadcrumbBridgeCoordinator CreateBridge()
            {
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                return new BreadcrumbBridgeCoordinator(Hub, provider.Object, Dispatcher);
            }

            public void Dispose() => Coordinator.Dispose();
        }

        private sealed class RecordingHost : IBreadcrumbDropDownHost
        {
            private EventHandler _popupMessengerReady;

            internal List<string> EventOperations { get; } = new List<string>();
            internal IWebViewMessenger PopupMessengerValue { get; set; }

            /// <summary>
            /// Issue #488 defect D2: the themes this host received, in the order received.
            /// </summary>
            internal List<string> ThemesApplied { get; } = new List<string>();

            public bool IsOpen => false;
            public IWebViewMessenger PopupMessenger => PopupMessengerValue;

            public event EventHandler PopupMessengerReady
            {
                add
                {
                    EventOperations.Add("add");
                    _popupMessengerReady += value;
                }
                remove
                {
                    EventOperations.Add("remove");
                    _popupMessengerReady -= value;
                }
            }

            public Task<bool> OpenAsync(
                Rectangle anchorScreenBounds,
                Rectangle workingArea,
                Size desiredSize
            ) => OpenAsync(anchorScreenBounds, workingArea, desiredSize, takeFocus: true);

            /// <summary>
            /// Issue #438: the focus intent recorded for each open request, in request order.
            /// </summary>
            internal List<bool> RequestedTakeFocus { get; } = new List<bool>();

            // Issue #438: additive 4-parameter overload delegating to the existing body, plus the
            // recorded focus intent. No existing behavior changes.
            public Task<bool> OpenAsync(
                Rectangle anchorScreenBounds,
                Rectangle workingArea,
                Size desiredSize,
                bool takeFocus
            )
            {
                RequestedTakeFocus.Add(takeFocus);
                return Task.FromResult(false);
            }

            public bool Close(BreadcrumbDropDownCloseReason reason) => true;

            public void SetTheme(string theme) => ThemesApplied.Add(theme);

            public void Reset() { }

            public void Dispose() { }

            internal void RaisePopupMessengerReady() =>
                _popupMessengerReady?.Invoke(this, EventArgs.Empty);
        }

        private sealed class QueuedCreatorThreadSynchronizationContext : SynchronizationContext
        {
            private readonly Queue<Tuple<SendOrPostCallback, object>> _callbacks =
                new Queue<Tuple<SendOrPostCallback, object>>();

            internal QueuedCreatorThreadSynchronizationContext()
            {
                CreatorThreadId = Environment.CurrentManagedThreadId;
            }

            internal int CreatorThreadId { get; }
            internal List<int> CallbackThreads { get; } = new List<int>();

            public override void Post(SendOrPostCallback callback, object state) =>
                _callbacks.Enqueue(Tuple.Create(callback, state));

            internal void DrainOnCreatorThread()
            {
                Environment.CurrentManagedThreadId.Should().Be(CreatorThreadId);
                while (_callbacks.Count > 0)
                {
                    Tuple<SendOrPostCallback, object> callback = _callbacks.Dequeue();
                    CallbackThreads.Add(Environment.CurrentManagedThreadId);
                    callback.Item1(callback.Item2);
                }
            }
        }
    }
}
