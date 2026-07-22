using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first correlated readiness contracts for the collapsed surface.</summary>
    [TestClass]
    public sealed class BreadcrumbCollapsedSurfaceReadinessTests
    {
        [TestMethod]
        public async Task AttachAsync_PendingAndUnrelatedNavigation_DefersReadyPublicationUntilExactSuccess()
        {
            // Arrange
            using (var harness = new CollapsedHarness())
            {
                int detachCount = 0;
                var readiness = new BreadcrumbNavigationReadiness("Collapsed", () => detachCount++);
                readiness.BeginNavigation(() => { });
                readiness.NavigationStarted(41);

                // Act
                Task<bool> attachment = harness.Controller.AttachAsync(harness.Surface, readiness);

                // Assert pending and unrelated completion
                attachment.IsCompleted.Should().BeFalse();
                harness.Controller.ReadyMessenger.Should().BeNull();
                readiness.NavigationCompleted(40, true, null);
                attachment
                    .IsCompleted.Should()
                    .BeFalse(
                        "an unrelated NavigationCompleted event cannot release the target document"
                    );
                detachCount.Should().Be(0);

                // Act exact completion
                readiness.NavigationCompleted(41, true, null);
                (await attachment.ConfigureAwait(false)).Should().BeTrue();

                // Assert exact-success publication
                harness.Controller.ReadyMessenger.Should().BeSameAs(harness.Surface);
                detachCount.Should().Be(1);
                (
                    await harness
                        .Controller.AttachAsync(harness.Surface, Task.CompletedTask)
                        .ConfigureAwait(false)
                )
                    .Should()
                    .BeTrue();
            }
        }

        [TestMethod]
        public async Task AttachAsync_ExactNavigationFailure_LeavesNoReadyMessenger()
        {
            // Arrange
            using (var harness = new CollapsedHarness())
            {
                int detachCount = 0;
                var readiness = new BreadcrumbNavigationReadiness("Collapsed", () => detachCount++);
                readiness.BeginNavigation(() => { });
                readiness.NavigationStarted(51);
                Task<bool> attachment = harness.Controller.AttachAsync(harness.Surface, readiness);

                // Act
                readiness.NavigationCompleted(51, false, "ConnectionAborted");

                // Assert
                (await attachment.ConfigureAwait(false))
                    .Should()
                    .BeFalse();
                harness.Controller.ReadyMessenger.Should().BeNull();
                harness.Surface.DisposeCount.Should().Be(1);
                detachCount.Should().Be(1);
            }
        }

        [TestMethod]
        public async Task Reset_PendingNavigation_CancelsDetachesAndRejectsLateSuccess()
        {
            // Arrange
            using (var harness = new CollapsedHarness())
            {
                int detachCount = 0;
                var readiness = new BreadcrumbNavigationReadiness("Collapsed", () => detachCount++);
                readiness.BeginNavigation(() => { });
                readiness.NavigationStarted(71);
                Task<bool> attachment = harness.Controller.AttachAsync(harness.Surface, readiness);

                // Act
                harness.Controller.Reset();

                // Assert cancellation and late completion
                (await attachment.ConfigureAwait(false))
                    .Should()
                    .BeFalse();
                detachCount.Should().Be(1);
                readiness.NavigationCompleted(71, true, null);
                harness.Controller.ReadyMessenger.Should().BeNull();
                harness.Surface.DisposeCount.Should().Be(1);
            }
        }

        [TestMethod]
        public async Task Dispose_PendingNavigation_CancelsDetachesAndRejectsLateSuccess()
        {
            // Arrange
            using (var harness = new CollapsedHarness())
            {
                int detachCount = 0;
                var readiness = new BreadcrumbNavigationReadiness("Collapsed", () => detachCount++);
                readiness.BeginNavigation(() => { });
                readiness.NavigationStarted(81);
                Task<bool> attachment = harness.Controller.AttachAsync(harness.Surface, readiness);

                // Act
                harness.Controller.Dispose();
                harness.Controller.Dispose();

                // Assert cancellation and late completion
                (await attachment.ConfigureAwait(false))
                    .Should()
                    .BeFalse();
                detachCount.Should().Be(1);
                readiness.NavigationCompleted(81, true, null);
                harness.Controller.ReadyMessenger.Should().BeNull();
                harness.Surface.DisposeCount.Should().Be(1);
            }
        }

        [TestMethod]
        public async Task LaterNavigation_InvalidatesEarlierGenerationAndPublishesOnlyCurrentMessenger()
        {
            // Arrange
            using (var harness = new CollapsedHarness())
            {
                int staleDetachCount = 0;
                int currentDetachCount = 0;
                var staleSurface = new TrackingMessenger();
                var staleReadiness = new BreadcrumbNavigationReadiness(
                    "Collapsed",
                    () => staleDetachCount++
                );
                var currentReadiness = new BreadcrumbNavigationReadiness(
                    "Collapsed",
                    () => currentDetachCount++
                );
                staleReadiness.BeginNavigation(() => { });
                staleReadiness.NavigationStarted(91);
                currentReadiness.BeginNavigation(() => { });
                currentReadiness.NavigationStarted(92);
                Task<bool> stale = harness.Controller.AttachAsync(staleSurface, staleReadiness);
                Task<bool> current = harness.Controller.AttachAsync(
                    harness.Surface,
                    currentReadiness
                );

                // Act
                staleReadiness.NavigationCompleted(91, true, null);
                currentReadiness.NavigationCompleted(92, true, null);

                // Assert
                (await stale.ConfigureAwait(false))
                    .Should()
                    .BeFalse();
                (await current.ConfigureAwait(false)).Should().BeTrue();
                staleSurface.DisposeCount.Should().Be(1);
                staleDetachCount.Should().Be(1);
                currentDetachCount.Should().Be(1);
                harness.Controller.ReadyMessenger.Should().BeSameAs(harness.Surface);
                harness.Surface.DisposeCount.Should().Be(0);
            }
        }

        [TestMethod]
        public async Task ViewerAttachment_PendingCachesAndReplaysCurrentStateExactlyOnce()
        {
            // Arrange
            using (var harness = new ViewerIntegrationHarness())
            {
                var surface = new TrackingMessenger();
                var readiness = Readiness(301);
                Task<bool> first = harness.Viewer.AttachBreadcrumbMessengerWhenReadyAsync(
                    surface,
                    readiness
                );
                Task<bool> repeated = harness.Viewer.AttachBreadcrumbMessengerWhenReadyAsync(
                    surface,
                    readiness
                );
                const string render = "{\"type\":\"render\",\"rows\":[{\"percentText\":\"73%\"}]}";
                const string selector =
                    "{\"type\":\"selectorView\",\"mode\":\"collapsed\",\"isOpen\":false}";
                const string theme = "{\"type\":\"themeChange\",\"theme\":\"dark\"}";

                // Act while pending
                harness.Hub.PostJson(render);
                harness.Hub.PostJson(selector);
                harness.Hub.PostJson(theme);
                readiness.NavigationCompleted(300, true, null);

                // Assert pending, then exact success
                first.IsCompleted.Should().BeFalse();
                repeated.IsCompleted.Should().BeFalse();
                surface.SubscriberCount.Should().Be(0);
                surface.Posted.Should().BeEmpty();
                readiness.NavigationCompleted(301, true, null);
                (await first.ConfigureAwait(false)).Should().BeTrue();
                (await repeated.ConfigureAwait(false)).Should().BeTrue();
                surface.SubscriberCount.Should().Be(1);
                surface.Posted.Should().Equal(render, selector, theme);

                // Act and assert ready reattachment remains idempotent
                (
                    await harness
                        .Viewer.AttachBreadcrumbMessengerWhenReadyAsync(surface, readiness)
                        .ConfigureAwait(false)
                )
                    .Should()
                    .BeTrue();
                surface.SubscriberCount.Should().Be(1);
                surface.Posted.Should().Equal(render, selector, theme);
            }
        }

        [TestMethod]
        public async Task ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment()
        {
            // Arrange exact failure
            using (var harness = new ViewerIntegrationHarness())
            {
                var failedSurface = new TrackingMessenger();
                var failedReadiness = Readiness(311);
                Task<bool> failed = harness.Viewer.AttachBreadcrumbMessengerWhenReadyAsync(
                    failedSurface,
                    failedReadiness
                );

                // Act and assert failure cleanup
                failedReadiness.NavigationCompleted(311, false, "ConnectionAborted");
                (await failed.ConfigureAwait(false)).Should().BeFalse();
                failedSurface.SubscriberCount.Should().Be(0);
                failedSurface.DisposeCount.Should().Be(1);

                // Arrange and act reset while pending
                var resetSurface = new TrackingMessenger();
                var resetReadiness = Readiness(321);
                Task<bool> reset = harness.Viewer.AttachBreadcrumbMessengerWhenReadyAsync(
                    resetSurface,
                    resetReadiness
                );
                harness.Viewer.ResetBreadcrumb();
                resetReadiness.NavigationCompleted(321, true, null);

                // Assert reset rejection and pooled reuse
                (await reset.ConfigureAwait(false))
                    .Should()
                    .BeFalse();
                resetSurface.SubscriberCount.Should().Be(0);
                resetSurface.DisposeCount.Should().Be(1);
                var reusedSurface = new TrackingMessenger();
                var reusedReadiness = Readiness(331);
                Task<bool> reused = harness.Viewer.AttachBreadcrumbMessengerWhenReadyAsync(
                    reusedSurface,
                    reusedReadiness
                );
                reusedReadiness.NavigationCompleted(331, true, null);
                (await reused.ConfigureAwait(false)).Should().BeTrue();
                reusedSurface.SubscriberCount.Should().Be(1);

                // Act and assert ready reset and pending disposal
                harness.Viewer.ResetBreadcrumb();
                reusedSurface.SubscriberCount.Should().Be(0);
                reusedSurface.DisposeCount.Should().Be(1);
                var disposedSurface = new TrackingMessenger();
                var disposedReadiness = Readiness(341);
                Task<bool> disposed = harness.Viewer.AttachBreadcrumbMessengerWhenReadyAsync(
                    disposedSurface,
                    disposedReadiness
                );
                harness.Viewer.Dispose();
                disposedReadiness.NavigationCompleted(341, true, null);
                (await disposed.ConfigureAwait(false)).Should().BeFalse();
                disposedSurface.SubscriberCount.Should().Be(0);
                disposedSurface.DisposeCount.Should().Be(1);
            }
        }

        [TestMethod]
        public async Task NavigationReadiness_UnrelatedCompletionCannotReleaseExactNavigation()
        {
            // Arrange
            int detachCount = 0;
            int navigationCount = 0;
            using (
                var readiness = new BreadcrumbNavigationReadiness("Collapsed", () => detachCount++)
            )
            {
                readiness.NavigationStarted(99);
                readiness.BeginNavigation(() => navigationCount++);
                readiness.NavigationStarted(101);
                readiness.NavigationStarted(102);

                // Act unrelated completion
                readiness.NavigationCompleted(102, true, null);

                // Assert pending, then exact success
                readiness.Completion.IsCompleted.Should().BeFalse();
                detachCount.Should().Be(0);
                navigationCount.Should().Be(1);
                readiness.NavigationCompleted(101, true, null);
                await readiness.Completion.ConfigureAwait(false);
                detachCount.Should().Be(1);
                readiness.NavigationCompleted(101, false, "LateFailure");
                detachCount.Should().Be(1);
            }
        }

        [TestMethod]
        public void NavigationReadiness_SynchronousSuccessDetachesBeforeNavigationReturns()
        {
            // Arrange
            int detachCount = 0;
            var readiness = new BreadcrumbNavigationReadiness("Popup", () => detachCount++);

            // Act
            readiness.BeginNavigation(() =>
            {
                readiness.NavigationStarted(151);
                readiness.NavigationCompleted(151, true, null);
            });

            // Assert
            readiness.Completion.Status.Should().Be(TaskStatus.RanToCompletion);
            detachCount.Should().Be(1);
            readiness.Dispose();
            detachCount.Should().Be(1);
        }

        [TestMethod]
        public async Task NavigationReadiness_FailureAndSynchronousExceptionDetachEveryPath()
        {
            // Arrange asynchronous failure
            int failureDetachCount = 0;
            var failure = new BreadcrumbNavigationReadiness(
                "Collapsed",
                () => failureDetachCount++
            );
            failure.BeginNavigation(() => { });
            failure.NavigationStarted(202);

            // Act asynchronous failure
            failure.NavigationCompleted(202, false, "ConnectionAborted");

            // Assert translated failure and detachment
            Func<Task> awaitFailure = async () => await failure.Completion.ConfigureAwait(false);
            await awaitFailure
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Collapsed navigation failed with status 'ConnectionAborted'.");
            failureDetachCount.Should().Be(1);
            failure.Dispose();

            // Arrange and act synchronous navigation exception
            int synchronousDetachCount = 0;
            var synchronous = new BreadcrumbNavigationReadiness(
                "Collapsed",
                () => synchronousDetachCount++
            );
            var expected = new InvalidOperationException("NavigateToString rejected");
            Action begin = () => synchronous.BeginNavigation(() => throw expected);

            // Assert exact exception, cancellation, and one detachment
            begin.Should().Throw<InvalidOperationException>().Which.Should().BeSameAs(expected);
            synchronous.Completion.IsCanceled.Should().BeTrue();
            synchronousDetachCount.Should().Be(1);
            synchronous.Dispose();
        }

        private static BreadcrumbNavigationReadiness Readiness(ulong navigationId)
        {
            var readiness = new BreadcrumbNavigationReadiness("Collapsed", () => { });
            readiness.BeginNavigation(() => { });
            readiness.NavigationStarted(navigationId);
            return readiness;
        }

        private sealed class CollapsedHarness : IDisposable
        {
            internal BreadcrumbCollapsedSurfaceController Controller { get; } =
                new BreadcrumbCollapsedSurfaceController();
            internal TrackingMessenger Surface { get; } = new TrackingMessenger();

            public void Dispose()
            {
                Controller.Dispose();
            }
        }

        private sealed class ViewerIntegrationHarness : IDisposable
        {
            private readonly SynchronizationContext _previous;

            internal ViewerIntegrationHarness()
            {
                _previous = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                Viewer = new QuickFiler.ItemViewer();
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                Viewer.InitializeBreadcrumbPipeline(provider.Object);
                FieldInfo field = typeof(QuickFiler.ItemViewer).GetField(
                    "_breadcrumbHub",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
                Hub = (BreadcrumbMessengerHub)field.GetValue(Viewer);
            }

            internal QuickFiler.ItemViewer Viewer { get; }
            internal BreadcrumbMessengerHub Hub { get; }

            public void Dispose()
            {
                Viewer.Dispose();
                SynchronizationContext.SetSynchronizationContext(_previous);
            }
        }

        private sealed class TrackingMessenger : IWebViewMessenger, IDisposable
        {
            private EventHandler<string> _messageReceived;

            internal int DisposeCount { get; private set; }
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

            public void Dispose()
            {
                if (DisposeCount == 0)
                {
                    DisposeCount++;
                }
            }
        }
    }
}
