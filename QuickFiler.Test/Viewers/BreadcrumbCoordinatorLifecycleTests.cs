using System;
using System.Collections.Generic;
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
    /// <summary>Failure-first cancellation and generation contracts for coordinator upgrades.</summary>
    [TestClass]
    public sealed class BreadcrumbCoordinatorLifecycleTests
    {
        private const string StalePath = "\\Inbox\\Stale";
        private const string CurrentPath = "\\Inbox\\Current";

        private SynchronizationContext _previousContext;

        [TestInitialize]
        public void InstallOwningContext()
        {
            // The coordinator captures its synchronization boundary at construction. The dedicated
            // queued-completion test installs an owner-thread pump when dispatch ordering matters.
            _previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestCleanup]
        public void RestoreOwningContext() =>
            SynchronizationContext.SetSynchronizationContext(_previousContext);

        [TestMethod]
        public async Task OverlappingUpgrades_CurrentCompletionPostsOnceAndStaleCompletionPostsNothing()
        {
            // Arrange
            var staleGate = Completion();
            var currentGate = Completion();
            CancellationToken staleToken = default(CancellationToken);
            FolderTreeNodeKey staleKey = Key("stale", StalePath);
            FolderTreeNodeKey currentKey = Key("current", CurrentPath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            Configure(provider, StalePath, staleGate.Task, staleKey, token => staleToken = token);
            Configure(provider, CurrentPath, currentGate.Task, currentKey);
            var messenger = new TrackingMessenger();
            var coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
            coordinator.SetSuggestions(new[] { Scored(StalePath) });
            Task staleUpgrade = coordinator.SuggestionsUpgrade;
            coordinator.SetSuggestions(new[] { Scored(CurrentPath) });
            Task currentUpgrade = coordinator.SuggestionsUpgrade;
            messenger.Clear();

            staleToken.IsCancellationRequested.Should().BeTrue();
            Action observeCanceledSource = () => staleToken.WaitHandle.WaitOne(0);
            observeCanceledSource
                .Should()
                .NotThrow("the source must remain valid until its provider operation settles");

            // Act
            currentGate.SetResult(currentKey);
            await currentUpgrade.ConfigureAwait(false);
            int currentPostCount = messenger.Posted.Count;
            staleGate.SetResult(staleKey);
            await staleUpgrade.ConfigureAwait(false);

            // Assert
            currentPostCount.Should().Be(1, "the current upgrade publishes one render update");
            messenger.Posted.Should().HaveCount(currentPostCount);
            coordinator.GetFolderItems().Should().Equal(CurrentPath);
        }

        [TestMethod]
        public async Task Clear_InvalidatesLateSuccessfulUpgradeBeforeAnyPostOrCallback()
        {
            // Arrange
            var gate = Completion();
            CancellationToken token = default(CancellationToken);
            FolderTreeNodeKey key = Key("stale", StalePath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            Configure(provider, StalePath, gate.Task, key, value => token = value);
            var messenger = new TrackingMessenger();
            var coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
            coordinator.SetSuggestions(new[] { Scored(StalePath) });
            Task staleUpgrade = coordinator.SuggestionsUpgrade;
            coordinator.Clear();
            messenger.Clear();

            token.IsCancellationRequested.Should().BeTrue();

            // Act
            gate.SetResult(key);
            await staleUpgrade.ConfigureAwait(false);

            // Assert
            messenger.Posted.Should().BeEmpty("clear invalidates every earlier population");
            coordinator.GetFolderItems().Should().BeEmpty();
        }

        [TestMethod]
        public async Task ViewerResetThenReuse_InvalidatesLateFailureWithoutDuplicatingCurrentState()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                var staleGate = Completion();
                CancellationToken staleToken = default(CancellationToken);
                FolderTreeNodeKey staleKey = Key("stale", StalePath);
                FolderTreeNodeKey currentKey = Key("current", CurrentPath);
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                Configure(
                    provider,
                    StalePath,
                    staleGate.Task,
                    staleKey,
                    token => staleToken = token
                );
                Configure(provider, CurrentPath, Task.FromResult(currentKey), currentKey);
                var messenger = new TrackingMessenger();
                scope.Viewer.InitializeBreadcrumbPipeline(provider.Object);
                scope.Viewer.AttachBreadcrumbMessenger(messenger);
                BreadcrumbBridgeCoordinator coordinator = scope.Viewer.BreadcrumbCoordinator;
                coordinator.SetSuggestions(new[] { Scored(StalePath) });
                Task staleUpgrade = coordinator.SuggestionsUpgrade;
                scope.Viewer.ResetBreadcrumb();
                staleToken.IsCancellationRequested.Should().BeTrue();
                coordinator.SetSuggestions(new[] { Scored(CurrentPath) });
                await coordinator.SuggestionsUpgrade.ConfigureAwait(false);
                messenger.Clear();

                // Act
                staleGate.SetException(new InvalidOperationException("late stale failure"));
                await staleUpgrade.ConfigureAwait(false);

                // Assert
                messenger
                    .Posted.Should()
                    .BeEmpty("pooled viewer reuse cannot receive a stale completion");
                coordinator.GetFolderItems().Should().Equal(CurrentPath);
            }
        }

        [TestMethod]
        public async Task Dispose_InvalidatesLateSuccessAndUnsubscribesBeforePostOrCallback()
        {
            // Arrange
            var gate = Completion();
            CancellationToken token = default(CancellationToken);
            FolderTreeNodeKey key = Key("stale", StalePath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            Configure(provider, StalePath, gate.Task, key, value => token = value);
            var messenger = new TrackingMessenger();
            var coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
            coordinator.SetSuggestions(new[] { Scored(StalePath) });
            Task staleUpgrade = coordinator.SuggestionsUpgrade;
            messenger.Clear();
            IDisposable lifetime = (object)coordinator as IDisposable;

            // Act
            try
            {
                lifetime
                    .Should()
                    .NotBeNull("coordinator disposal must own and invalidate outstanding work");
                lifetime.Dispose();
                lifetime.Dispose();
                token.IsCancellationRequested.Should().BeTrue();
            }
            finally
            {
                gate.TrySetResult(key);
                await staleUpgrade.ConfigureAwait(false);
            }

            // Assert
            messenger.Posted.Should().BeEmpty();
            messenger.SubscriberCount.Should().Be(0);
        }

        [TestMethod]
        public async Task Dispose_InvalidatesLateFailureWithoutPostCallbackOrErrorMutation()
        {
            // Arrange
            var gate = Completion();
            FolderTreeNodeKey key = Key("stale", StalePath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            Configure(provider, StalePath, gate.Task, key);
            var messenger = new TrackingMessenger();
            var coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
            coordinator.SetSuggestions(new[] { Scored(StalePath) });
            Task staleUpgrade = coordinator.SuggestionsUpgrade;
            messenger.Clear();
            IDisposable lifetime = (object)coordinator as IDisposable;

            // Act
            try
            {
                lifetime
                    .Should()
                    .NotBeNull("late failure requires an idempotent coordinator lifetime boundary");
                lifetime.Dispose();
            }
            finally
            {
                gate.TrySetException(new InvalidOperationException("late disposed failure"));
                await staleUpgrade.ConfigureAwait(false);
            }

            // Assert
            messenger.Posted.Should().BeEmpty();
            messenger.SubscriberCount.Should().Be(0);
        }

        [TestMethod]
        public async Task CurrentProviderCancellation_PropagatesWithoutPublishingAnUpgrade()
        {
            // Arrange
            FolderTreeNodeKey key = Key("current", CurrentPath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(value =>
                    value.ResolveLeafKeyAsync(CurrentPath, It.IsAny<CancellationToken>())
                )
                .Returns(
                    (string path, CancellationToken token) =>
                        Task.FromException<FolderTreeNodeKey>(new OperationCanceledException(token))
                );
            var messenger = new TrackingMessenger();
            var coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
            coordinator.SetSuggestions(new[] { Scored(CurrentPath) });
            Task upgrade = coordinator.SuggestionsUpgrade;
            messenger.Clear();

            // Act
            Func<Task> awaitUpgrade = async () => await upgrade.ConfigureAwait(false);

            // Assert
            await awaitUpgrade.Should().ThrowAsync<OperationCanceledException>();
            messenger
                .Posted.Should()
                .BeEmpty("a current provider cancellation cannot publish an upgrade");
            coordinator.GetFolderItems().Should().Equal(CurrentPath);
        }

        [TestMethod]
        public void DisposedCoordinator_RejectsPopulationAndClearRemainsSafe()
        {
            // Arrange
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var messenger = new TrackingMessenger();
            var coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
            IDisposable lifetime = (object)coordinator as IDisposable;
            lifetime.Should().NotBeNull();
            lifetime.Dispose();

            messenger.Clear();
            Action populateAfterDisposal = () =>
                coordinator.SetSuggestions(new[] { Scored(CurrentPath) });
            Action clearAfterDisposal = () => coordinator.Clear();

            // Assert
            populateAfterDisposal.Should().Throw<ObjectDisposedException>();
            clearAfterDisposal
                .Should()
                .NotThrow("clearing after disposal must invalidate nothing and stay safe");
            coordinator.GetFolderItems().Should().BeEmpty();
            messenger.Posted.Should().BeEmpty();
            messenger.SubscriberCount.Should().Be(0);
        }

        [TestMethod]
        public async Task AsyncPopulation_SupersededCompletionDoesNotPublishAgain()
        {
            var staleGate = Completion();
            var currentGate = Completion();
            CancellationToken staleToken = default(CancellationToken);
            FolderTreeNodeKey staleKey = Key("stale", StalePath);
            FolderTreeNodeKey currentKey = Key("current", CurrentPath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            Configure(provider, StalePath, staleGate.Task, staleKey, token => staleToken = token);
            Configure(provider, CurrentPath, currentGate.Task, currentKey);
            var messenger = new TrackingMessenger();
            var coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);

            Task stale = coordinator.SetSuggestionsAsync(
                new[] { Scored(StalePath) },
                CancellationToken.None
            );
            Task current = coordinator.SetSuggestionsAsync(
                new[] { Scored(CurrentPath) },
                CancellationToken.None
            );
            staleToken.IsCancellationRequested.Should().BeTrue();
            currentGate.SetResult(currentKey);
            await current.ConfigureAwait(false);
            int currentPostCount = messenger.Posted.Count;

            staleGate.SetResult(staleKey);
            await stale.ConfigureAwait(false);

            messenger.Posted.Should().HaveCount(currentPostCount);
            coordinator.GetFolderItems().Should().Equal(CurrentPath);
        }

        [TestMethod]
        public async Task AddItems_InvalidatesLateUpgradeBeforeDuplicatePost()
        {
            var gate = Completion();
            CancellationToken token = default(CancellationToken);
            FolderTreeNodeKey key = Key("stale", StalePath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            Configure(provider, StalePath, gate.Task, key, value => token = value);
            var messenger = new TrackingMessenger();
            var coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
            coordinator.SetSuggestions(new[] { Scored(StalePath) });
            Task stale = coordinator.SuggestionsUpgrade;

            coordinator.AddItems(new[] { "Plain" });
            messenger.Clear();
            token.IsCancellationRequested.Should().BeTrue();
            gate.SetResult(key);
            await stale.ConfigureAwait(false);

            messenger.Posted.Should().BeEmpty();
            coordinator.GetFolderItems().Should().Contain("Plain");
        }

        [TestMethod]
        public void QueuedCompletion_DisposedBeforeOwnerDrain_DoesNotPublish()
        {
            var previous = SynchronizationContext.Current;
            var context =
                new BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                var gate = Completion();
                FolderTreeNodeKey key = Key("current", CurrentPath);
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                Configure(provider, CurrentPath, gate.Task, key);
                var messenger = new TrackingMessenger();
                var coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
                coordinator.SetSuggestions(new[] { Scored(CurrentPath) });
                Task upgrade = coordinator.SuggestionsUpgrade;
                context.DrainAll();
                messenger.Clear();

                Task.Run(() => gate.SetResult(key)).GetAwaiter().GetResult();
                context.WaitForPost();
                coordinator.Dispose();
                context.DrainUntil(upgrade);

                messenger.Posted.Should().BeEmpty();
                context.ExceptionSnapshot.Should().BeEmpty();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        private static TaskCompletionSource<FolderTreeNodeKey> Completion() =>
            new TaskCompletionSource<FolderTreeNodeKey>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

        private static void Configure(
            Mock<IFolderHierarchyProvider> provider,
            string path,
            Task<FolderTreeNodeKey> resolution,
            FolderTreeNodeKey key,
            Action<CancellationToken> captureToken = null
        )
        {
            provider
                .Setup(value => value.ResolveLeafKeyAsync(path, It.IsAny<CancellationToken>()))
                .Returns(
                    (string ignored, CancellationToken token) =>
                    {
                        captureToken?.Invoke(token);
                        return resolution;
                    }
                );
            provider
                .Setup(value => value.GetAncestorChainAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[]
                    {
                        new FolderBreadcrumbSegment(
                            key,
                            path.Substring(path.LastIndexOf('\\') + 1),
                            path,
                            false
                        ),
                    }
                );
        }

        private static FolderTreeNodeKey Key(string entryId, string path) =>
            new FolderTreeNodeKey("store", entryId, path);

        private static FolderRow Scored(string path) =>
            new FolderRow(path, FolderRowKind.Suggestion, new FolderScore(path, 100, 0.5));

        private sealed class TrackingMessenger : IWebViewMessenger
        {
            private EventHandler<string> _messageReceived;

            internal int SubscriberCount { get; private set; }
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
                    if (!ReferenceEquals(_messageReceived, value))
                    {
                        throw new InvalidOperationException(
                            "The exact subscribed handler is required."
                        );
                    }
                    _messageReceived -= value;
                    SubscriberCount--;
                }
            }

            public void PostJson(string json) => Posted.Add(json);

            internal void Clear() => Posted.Clear();
        }

        private sealed class ViewerScope : IDisposable
        {
            private readonly SynchronizationContext _previous;

            internal ViewerScope()
            {
                _previous = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                Viewer = new QuickFiler.ItemViewer();
            }

            internal QuickFiler.ItemViewer Viewer { get; }

            public void Dispose()
            {
                Viewer.Dispose();
                SynchronizationContext.SetSynchronizationContext(_previous);
            }
        }
    }
}
