using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using CapturingSynchronizationContext = QuickFiler.Test.Viewers.BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #502 (I-502.2): the coordinator-level assertion that a superseded population never leaves
    /// <see cref="BreadcrumbBridgeCoordinator.SuggestionsUpgrade"/> pointing at an earlier call's
    /// still-incomplete task. Reaches the population body through the <c>SetSuggestionsCore</c>
    /// internal seam (SR-5), which <c>[assembly: InternalsVisibleTo("QuickFiler.Test")]</c> makes a
    /// direct call. Deterministic: one thread, an explicitly drained synchronization context, and a
    /// gating <see cref="TaskCompletionSource{TResult}"/> that is deliberately never completed. No
    /// second thread, no timer, no wall-clock wait, no temporary file.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbBridgeCoordinatorSupersessionTests
    {
        private const string SuggestionPath = "\\Inbox\\Current";

        [TestMethod]
        public void SetSuggestionsCore_SupersededLeaseReplacesStaleSuggestionsUpgrade()
        {
            SynchronizationContext previous = SynchronizationContext.Current;
            var context = new CapturingSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                // Arrange: make SuggestionsUpgrade a genuinely PENDING task first.
                //
                // This ordering is load-bearing. The property's initial value is Task.CompletedTask, a
                // process-wide singleton, so a handle captured before this arrangement would be
                // unconditionally reference-equal to any later Task.CompletedTask assignment and the
                // inequality assertion below could never fail.
                var gate = new TaskCompletionSource<FolderTreeNodeKey>(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
                FolderTreeNodeKey key = Key(SuggestionPath);
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                Configure(provider, SuggestionPath, gate.Task, key);
                var messenger = new Mock<IWebViewMessenger>();
                var coordinator = new BreadcrumbBridgeCoordinator(
                    messenger.Object,
                    provider.Object
                );

                coordinator.SetSuggestions(new[] { Scored(SuggestionPath) });
                context.DrainAll();
                Task captured = coordinator.SuggestionsUpgrade;

                // The gating source is never completed, so the population task cannot finish. This
                // assertion is what makes the reference-inequality assertion below meaningful.
                captured
                    .IsCompleted.Should()
                    .BeFalse(
                        "the captured handle must be a genuinely incomplete population task, not the Task.CompletedTask singleton"
                    );

                BreadcrumbCoordinatorUpgradeLifetime lifetime = GetLifetime(coordinator);
                BreadcrumbUpgradeLease dead = lifetime.BeginPopulation();
                lifetime.Invalidate().Should().BeTrue();

                // Act: drive the population body with an already-superseded lease.
                coordinator.SetSuggestionsCore(new[] { Scored(SuggestionPath) }, dead);

                // Assert
                Task replaced = coordinator.SuggestionsUpgrade;
                replaced
                    .Should()
                    .NotBeSameAs(
                        captured,
                        "a superseded population must replace the stale handle, not leave it in place"
                    );
                replaced
                    .IsCompleted.Should()
                    .BeTrue("the replacement handle must already be completed (I-502.2)");
                dead.Settled.Should().BeTrue("the superseded lease must be settled (I-502.3)");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        /// <summary>
        /// Issue #502 (I-502.4): a superseded <c>AddItems</c> must skip the append entirely and settle
        /// its lease rather than leak the cancellation source. Unlike <c>SetSuggestionsCore</c> the skip
        /// exposes no replaced handle, so the settled lease and the untouched collaborators are the only
        /// observable evidence. Reaches the body through the <c>AddItemsCore</c> seam (SR-5) because a
        /// lease taken by the public entry point is current by construction. Deterministic: one thread,
        /// no timer, no wall-clock wait, no temporary file.
        /// </summary>
        [TestMethod]
        public void AddItemsCore_SupersededLeaseSkipsAppendAndSettlesTheLease()
        {
            SynchronizationContext previous = SynchronizationContext.Current;
            var context = new CapturingSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                // Arrange
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                var messenger = new Mock<IWebViewMessenger>();
                var coordinator = new BreadcrumbBridgeCoordinator(
                    messenger.Object,
                    provider.Object
                );

                BreadcrumbCoordinatorUpgradeLifetime lifetime = GetLifetime(coordinator);
                BreadcrumbUpgradeLease dead = lifetime.BeginPopulation();
                lifetime
                    .Invalidate()
                    .Should()
                    .BeTrue("the arrangement must actually supersede the lease before the act");
                messenger.Invocations.Clear();

                // Act
                coordinator.AddItemsCore(new[] { "Alpha" }, dead);

                // Assert
                dead.Settled.Should()
                    .BeTrue(
                        "a superseded AddItems must settle its lease so no CancellationTokenSource leaks (I-502.3)"
                    );
                messenger
                    .Invocations.Should()
                    .BeEmpty("the skipped append must not render or post to the surface (I-502.4)");
                provider.VerifyNoOtherCalls();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        private static BreadcrumbCoordinatorUpgradeLifetime GetLifetime(
            BreadcrumbBridgeCoordinator coordinator
        )
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo field = typeof(BreadcrumbBridgeCoordinator).GetField(
                "_upgradeLifetime",
                Flags
            );
            field.Should().NotBeNull();
            var lifetime = field.GetValue(coordinator) as BreadcrumbCoordinatorUpgradeLifetime;
            lifetime.Should().NotBeNull();
            return lifetime;
        }

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

        private static FolderTreeNodeKey Key(string path) =>
            new FolderTreeNodeKey("store", "current", path);

        private static FolderRow Scored(string path) =>
            new FolderRow(path, FolderRowKind.Suggestion, new FolderScore(path, 100, 0.5));
    }
}
