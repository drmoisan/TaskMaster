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

        [TestMethod]
        public async Task OverlappingUpgrades_CurrentCompletionPostsOnceAndStaleCompletionPostsNothing()
        {
            // Arrange
            var staleGate = Completion();
            var currentGate = Completion();
            FolderTreeNodeKey staleKey = Key("stale", StalePath);
            FolderTreeNodeKey currentKey = Key("current", CurrentPath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            Configure(provider, StalePath, staleGate.Task, staleKey);
            Configure(provider, CurrentPath, currentGate.Task, currentKey);
            var messenger = new TrackingMessenger();
            var coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
            coordinator.SetSuggestions(new[] { Scored(StalePath) });
            Task staleUpgrade = coordinator.SuggestionsUpgrade;
            coordinator.SetSuggestions(new[] { Scored(CurrentPath) });
            Task currentUpgrade = coordinator.SuggestionsUpgrade;
            messenger.Clear();

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
            FolderTreeNodeKey key = Key("stale", StalePath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            Configure(provider, StalePath, gate.Task, key);
            var messenger = new TrackingMessenger();
            var coordinator = new BreadcrumbBridgeCoordinator(messenger, provider.Object);
            coordinator.SetSuggestions(new[] { Scored(StalePath) });
            Task staleUpgrade = coordinator.SuggestionsUpgrade;
            coordinator.Clear();
            messenger.Clear();

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
                FolderTreeNodeKey staleKey = Key("stale", StalePath);
                FolderTreeNodeKey currentKey = Key("current", CurrentPath);
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                Configure(provider, StalePath, staleGate.Task, staleKey);
                Configure(provider, CurrentPath, Task.FromResult(currentKey), currentKey);
                var messenger = new TrackingMessenger();
                scope.Viewer.InitializeBreadcrumbPipeline(provider.Object);
                scope.Viewer.AttachBreadcrumbMessenger(messenger);
                BreadcrumbBridgeCoordinator coordinator = scope.Viewer.BreadcrumbCoordinator;
                coordinator.SetSuggestions(new[] { Scored(StalePath) });
                Task staleUpgrade = coordinator.SuggestionsUpgrade;
                scope.Viewer.ResetBreadcrumb();
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
                    .NotBeNull("coordinator disposal must own and invalidate outstanding work");
                lifetime.Dispose();
                lifetime.Dispose();
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

        private static TaskCompletionSource<FolderTreeNodeKey> Completion() =>
            new TaskCompletionSource<FolderTreeNodeKey>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

        private static void Configure(
            Mock<IFolderHierarchyProvider> provider,
            string path,
            Task<FolderTreeNodeKey> resolution,
            FolderTreeNodeKey key
        )
        {
            provider
                .Setup(value => value.ResolveLeafKeyAsync(path, It.IsAny<CancellationToken>()))
                .Returns(resolution);
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
